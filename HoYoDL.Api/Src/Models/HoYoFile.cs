namespace HoYoDL.Api.Models;

public sealed class HoYoFile {
    public required string Path { get; init; }
    public required ulong Size { get; init; }
    public required IReadOnlyList<HoYoChunk> Chunks { get; init; }
}

public sealed class HoYoChunk {
    public required string Id { get; init; }
    public required ulong Offset { get; init; }
    public required string Md5 { get; init; }
    public required ulong CompressedSize { get; init; }
    public required ulong UncompressedSize { get; init; }
}