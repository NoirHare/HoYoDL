using System.Text.Json.Serialization;

namespace HoYoDL.Api.Internal.Dtos;

internal sealed class BranchesDto {
    [JsonPropertyName("main")]
    public required BranchDto Main { get; init; }

    [JsonPropertyName("pre_download")]
    public required BranchDto? PreDownload { get; init; }
}

internal sealed class BranchDto {
    [JsonPropertyName("branch")]
    public required string Id { get; init; }

    [JsonPropertyName("package_id")]
    public required string PackageId { get; init; }

    [JsonPropertyName("password")]
    public required string Password { get; init; }
}