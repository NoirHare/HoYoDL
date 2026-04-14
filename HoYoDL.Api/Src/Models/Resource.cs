namespace HoYoDL.Api.Models;

public sealed class Resource {
    public required string Tag { get; init; }
    public required IReadOnlyList<Package> Packages { get; init; }
}

public sealed class Package {
    public required string Id { get; init; }
    public required string ManifestId { get; init; }
    public required string ManifestDownloadPrefix { get; init; }
    public required string ChunkDownloadPrefix { get; init; }
    public required ulong CompressedSize { get; init; }
    public required ulong UncompressedSize { get; init; }
}