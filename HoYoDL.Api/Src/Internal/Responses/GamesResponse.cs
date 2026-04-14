using System.Text.Json.Serialization;

using HoYoDL.Api.Internal.Dtos;

namespace HoYoDL.Api.Internal.Responses;

internal sealed class GamesResponse {
    [JsonPropertyName("games")]
    public required IReadOnlyList<GameDto> Games { get; init; }
}