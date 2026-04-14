using System.Text.Json.Serialization;

namespace HoYoDL.Api.Internal;

internal class HoYoResult<T> {
    [JsonPropertyName("retcode")]
    public required long Retcode { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("data")]
    public required T? Data { get; init; }
}