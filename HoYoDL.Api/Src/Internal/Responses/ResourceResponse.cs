using System.Text.Json.Serialization;

using HoYoDL.Api.Internal.Dtos;

namespace HoYoDL.Api.Internal.Responses;

internal sealed class ResourceResponse {
    [JsonPropertyName("tag")]
    public required string Tag { get; init; }

    [JsonPropertyName("manifests")]
    public required IReadOnlyList<PackageDto> Packages { get; init; }
}