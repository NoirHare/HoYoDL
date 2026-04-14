using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using HoYoDL.Api;
using HoYoDL.Api.Models;
using HoYoDL.Handlers.Contexts;
using HoYoDL.Logging;
using HoYoDL.Utilities;

using Microsoft.Win32.SafeHandles;

using Umrab.Options;

using ZstdSharp;

namespace HoYoDL.Handlers;

internal static class DownloadHandler {
    public static async Task RunAsync(ParseResult result, CancellationToken ct = default) {
        string gameId = result.GetRequiredValue(Commands.DownloadGameIdArgument);
        string target = result.GetRequiredValue(Commands.DownloadTargetArgument);

        Region region = result.GetValue(Commands.DownloadRegionOption, () => Region.China);
        string audio = result.GetValue(Commands.DownloadAudioOption, () => "zh-cn");
        bool predownload = result.GetValue(Commands.DownloadPredownloadOption, () => false);
        string cache = result.GetValue(Commands.DownloadCacheOption, () => Path.Combine(target, "cache"));
        bool merge = result.GetValue(Commands.DownloadMergeOption, () => false);
        bool delete = result.GetValue(Commands.DownloadDeleteOption, () => false);

        target = Path.GetFullPath(target);
        cache = cache != null ? Path.GetFullPath(cache) : Path.Combine(target, "cache");

        using Logger logger = new();
        using HttpClient client = new();
        HoYoApi api = new(client, region);

        // get branch info
        Branches branches = await api.GetBranchesAsync(gameId, ct);
        Branch branch = predownload
            ? branches.PreDownload ?? throw new Exception("There are currently no predownload branches available")
            : branches.Main;

        // get resource info
        Resource resource = await api.GetResourceAsync(branch.Id, branch.PackageId, branch.Password, ct);

        // get package info
        List<Package> packages = [];
        foreach (Package pkg in resource.Packages) {
            if (pkg.Id == "game"
             || pkg.Id.All(c => '0' <= c && c <= '9')
             || pkg.Id.Contains(audio, StringComparison.OrdinalIgnoreCase)) {
                packages.Add(pkg);
            }
        }

        // get chunk info
        List<VerifyContext> verifyContexts = [];
        foreach (Package package in packages) {
            foreach (HoYoFile file in await api.GetFilesAsync(package.ManifestDownloadPrefix, package.ManifestId, ct)) {
                string path = Path.Combine(target, file.Path);
                LazyLease<SafeFileHandle>? lease = File.Exists(path)
                    ? new LazyLease<SafeFileHandle>(
                        () => File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.None),
                        file.Chunks.Count
                    )
                    : null;
                foreach (HoYoChunk chunk in file.Chunks) {
                    verifyContexts.Add(new VerifyContext(package, file, chunk, lease));
                }
            }
        }

        // verify chunk
        ConcurrentBag<DownloadContext> downloadContexts = [];
        ConcurrentDictionary<HoYoFile, ConcurrentBag<HoYoChunk>> mergeChunks = [];
        await ProcessAsync(
            logger,
            verifyContexts, Environment.ProcessorCount, async (context, tracker, ct) => {
                logger.Log($"Verifying {context.File.Path} {context.Chunk.Offset}");

                try {
                    if (await VerifyFileChunkAsync(context, ct)) return;
                    if (merge) mergeChunks.GetOrAdd(context.File, _ => []).Add(context.Chunk);
                    if (await VerifyCacheChunkAsync(context, cache, ct)) return;
                    downloadContexts.Add(new DownloadContext(context.Package, context.File, context.Chunk));
                } finally { tracker.Report(1); }
            },
            "Verify", verifyContexts.Count,
            ct: ct
        );

        // download chunk
        await ProcessAsync(
            logger,
            downloadContexts, 32, async (context, tracker, ct) => {
                logger.Log($"Downloading {context.File.Path} {context.Chunk.Offset}");

                string path = Path.Combine(cache, $"{context.File.Path}_{context.Chunk.Offset}");
                string? directory = Path.GetDirectoryName(path);
                if (directory != null) Directory.CreateDirectory(directory);

                using Stream chunkStream = await api.GetChunkAsync(
                    context.Package.ChunkDownloadPrefix,
                    context.Chunk.Id,
                    ct
                );
                Pipe pipe = new();
                Task task = FillLoopAsync(chunkStream, pipe.Writer, tracker, ct);
                using Stream pipeStream = pipe.Reader.AsStream();
                using FileStream file = File.OpenWrite(path);
                await new DecompressionStream(pipeStream).CopyToAsync(file, ct);
            },
            "Download", downloadContexts.Sum(c => (long)c.Chunk.CompressedSize), fromat: FormatSize,
            ct: ct
        );

        // merge chunk
        if (!merge) return;
        List<MergeContext> mergeContexts = [];
        foreach ((HoYoFile file, ConcurrentBag<HoYoChunk> chunks) in mergeChunks) {
            string path = Path.Combine(target, file.Path);
            string? directory = Path.GetDirectoryName(path);
            if (directory != null) Directory.CreateDirectory(directory);

            LazyLease<SafeFileHandle> lease = new(
                () => {
                    SafeFileHandle handle = File.OpenHandle(
                        path,
                        FileMode.OpenOrCreate,
                        FileAccess.Write,
                        FileShare.None,
                        FileOptions.Asynchronous
                    );
                    if ((ulong)RandomAccess.GetLength(handle) < file.Size) {
                        RandomAccess.SetLength(handle, (long)file.Size);
                    }
                    return handle;
                },
                chunks.Count
            );
            foreach (HoYoChunk chunk in chunks) {
                mergeContexts.Add(new MergeContext(file, chunk, lease));
            }
        }
        await ProcessAsync(
            logger,
            mergeContexts, 128, async (context, tracker, ct) => {
                logger.Log($"Merging {context.File.Path} {context.Chunk.Offset}");
                string cachePath = Path.Combine(cache, $"{context.File.Path}_{context.Chunk.Offset}");
                {
                    using LazyLease<SafeFileHandle>.Handle handle = context.Lease.Acquire();
                    using FileStream cacheStream = File.OpenRead(cachePath);
                    using IMemoryOwner<byte> bufferOwner = MemoryPool<byte>.Shared.Rent(1024 * 1024);
                    Memory<byte> buffer = bufferOwner.Memory[..(1024 * 1024)];
                    long writeOffset = (long)context.Chunk.Offset;
                    int read;
                    while ((read = await cacheStream.ReadAsync(buffer, ct)) > 0) {
                        await RandomAccess.WriteAsync(handle, buffer[..read], writeOffset, ct);
                        writeOffset += read;
                    }
                    tracker.Report(1);
                }
                if (delete) File.Delete(cachePath);
            },
            "Merge", mergeContexts.Count,
            ct: ct
        );

        if (delete) Directory.Delete(cache, true);
    }

    private static async Task ProcessAsync<T>(Logger logger, IEnumerable<T> enumerable, int parallelism, Func<T, ProgressTracker, CancellationToken, ValueTask> body, string label, long total, Func<double, string>? fromat = null, CancellationToken ct = default) {
        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        ProgressTracker tracker = new(total);
        Task task = ProgressTask(logger, tracker, label, fromat, ct: cts.Token);

        ParallelOptions options = new() {
            MaxDegreeOfParallelism = parallelism,
            CancellationToken = ct,
        };
        await Parallel.ForEachAsync(enumerable, options, (context, ct) => body(context, tracker, ct));

        cts.Cancel();
        await task;
    }

    private static async Task ProgressTask(Logger logger, ProgressTracker tracker, string label, Func<double, string>? formatSpeed = null, CancellationToken ct = default) {
        try {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(ct)) {
                ProgressTracker.Snapshot snap = tracker.GetSnapshot();
                string speed = formatSpeed != null ? formatSpeed(snap.Speed) : $"{snap.Speed:F2}";
                logger.SetPin("[{0}] {1}.{2}% | {3}/s | ETA {4}",
                    label, snap.Percentage / 100, snap.Percentage % 100, speed, snap.Eta);
            }
        } catch (OperationCanceledException) { }
        logger.ClearPin();
    }

    private static async Task<bool> VerifyFileChunkAsync(VerifyContext context, CancellationToken ct) {
        HoYoChunk chunk = context.Chunk;
        LazyLease<SafeFileHandle>? lease = context.Lease;

        ulong chunkOffset = chunk.Offset;
        ulong chunkSize = chunk.UncompressedSize;

        // file doesn't exist
        if (lease == null) return false;

        // file exist
        using LazyLease<SafeFileHandle>.Handle handle = lease.Acquire();
        long length = RandomAccess.GetLength(handle);
        if ((ulong)length < chunkOffset + chunkSize) return false;

        using IMemoryOwner<byte> contentOwner = MemoryPool<byte>.Shared.Rent((int)chunkSize);
        Memory<byte> content = contentOwner.Memory[..(int)chunkSize];
        int read = await RandomAccess.ReadAsync(handle, content, (long)chunkOffset, ct);
        if ((ulong)read != chunkSize) return false;
        byte[] md5 = ArrayPool<byte>.Shared.Rent(16);
        try {
            MD5.HashData(content.Span, md5);
            if (md5.AsSpan(0, 16).SequenceEqual(Convert.FromHexString(chunk.Md5))) return true;
        } finally { ArrayPool<byte>.Shared.Return(md5); }
        return false;
    }

    private static async Task<bool> VerifyCacheChunkAsync(VerifyContext context, string cache, CancellationToken ct = default) {
        string path = Path.Combine(cache, $"{context.File.Path}_{context.Chunk.Offset}");
        if (!File.Exists(path)) return false;

        using FileStream stream = File.OpenRead(path);
        if ((ulong)stream.Length != context.Chunk.UncompressedSize) return false;

        using IMemoryOwner<byte> memory = MemoryPool<byte>.Shared.Rent(16);
        await MD5.HashDataAsync(stream, memory.Memory, ct);
        return memory.Memory.Span[..16].SequenceEqual(Convert.FromHexString(context.Chunk.Md5));
    }

    private static async Task FillLoopAsync(Stream input, PipeWriter output, ProgressTracker tracker, CancellationToken ct = default) {
        try {
            int read;
            while ((read = await input.ReadAsync(output.GetMemory(), ct)) > 0) {
                output.Advance(read);
                tracker.Report(read);
                await output.FlushAsync(ct);
            }
            await output.CompleteAsync();
        } catch (Exception ex) { await output.CompleteAsync(ex); }
    }

    private static string FormatSize(double bytes) {
        return bytes switch {
            >= 1024 * 1024 * 1024 => $"{bytes / (1024 * 1024 * 1024):F2} GB",
            >= 1024 * 1024 => $"{bytes / (1024 * 1024):F2} MB",
            >= 1024 => $"{bytes / 1024:F2} KB",
            _ => $"{bytes:F0} B",
        };
    }
}
