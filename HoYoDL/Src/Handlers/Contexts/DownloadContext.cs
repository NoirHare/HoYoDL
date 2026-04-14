using HoYoDL.Api.Models;

namespace HoYoDL.Handlers.Contexts;

internal class DownloadContext(Package package, HoYoFile file, HoYoChunk chunk) {
    public Package Package { get; } = package;
    public HoYoFile File { get; } = file;
    public HoYoChunk Chunk { get; } = chunk;
}
