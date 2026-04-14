using System.Text.Json.Serialization;

using HoYoDL.Api.Internal.Dtos;

namespace HoYoDL.Api.Internal.Responses;

internal sealed class BranchesResponse {
    [JsonPropertyName("game_branches")]
    public required IReadOnlyList<BranchesDto> Branches { get; init; }
}