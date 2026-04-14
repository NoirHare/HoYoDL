using System.Text.Json.Serialization;

namespace HoYoDL.Api.Internal.Dtos;

internal sealed class GameDto {
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("display")]
    public required GameDisplayDTO Display { get; init; }
}

internal sealed class GameDisplayDTO {
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}