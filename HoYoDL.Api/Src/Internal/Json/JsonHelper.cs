using System.Text.Json;
using System.Text.Json.Serialization;

using HoYoDL.Api.Internal.Responses;

namespace HoYoDL.Api.Internal.Json;

internal static partial class JsonHelper {
    [JsonSerializable(typeof(HoYoResult<GamesResponse>))]
    [JsonSerializable(typeof(HoYoResult<BranchesResponse>))]
    [JsonSerializable(typeof(HoYoResult<ResourceResponse>))]
    private sealed partial class Context : JsonSerializerContext;

    public static ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken ct = default) {
        ValueTask<object?> vt = JsonSerializer.DeserializeAsync(stream, typeof(T), Context.Default, ct);

        return vt.IsCompleted ? ValueTask.FromResult((T?)vt.Result) : Await(vt);

        static async ValueTask<T?> Await(ValueTask<object?> vt) => (T?)await vt;
    }
}