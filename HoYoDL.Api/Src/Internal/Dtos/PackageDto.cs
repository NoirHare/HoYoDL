using System.Text.Json.Serialization;

namespace HoYoDL.Api.Internal.Dtos;

internal sealed class PackageDto {
    [JsonPropertyName("matching_field")]
    public required string Id { get; init; }

    [JsonPropertyName("manifest")]
    public required PackageManifestDto Manifest { get; init; }

    [JsonPropertyName("chunk_download")]
    public required PackageDownloadDto ChunkDownload { get; init; }

    [JsonPropertyName("manifest_download")]
    public required PackageDownloadDto ManifestDownload { get; init; }

    [JsonPropertyName("stats")]
    public required PackageStatsDto Stats { get; init; }
}

internal sealed class PackageManifestDto {
    [JsonPropertyName("id")]
    public required string Id { get; init; }
}

internal sealed class PackageDownloadDto {
    [JsonPropertyName("url_prefix")]
    public required string Prefix { get; init; }
}

internal sealed class PackageStatsDto {
    [JsonPropertyName("compressed_size")]
    public required string CompressedSize { get; init; }

    [JsonPropertyName("uncompressed_size")]
    public required string UncompressedSize { get; init; }
}
