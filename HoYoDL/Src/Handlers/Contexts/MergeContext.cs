using HoYoDL.Api.Models;
using HoYoDL.Utilities;

using Microsoft.Win32.SafeHandles;

namespace HoYoDL.Handlers.Contexts;

internal class MergeContext(HoYoFile file, HoYoChunk chunk, LazyLease<SafeFileHandle> lease) {
    public HoYoFile File { get; } = file;
    public HoYoChunk Chunk { get; } = chunk;
    public LazyLease<SafeFileHandle> Lease { get; } = lease;
}
