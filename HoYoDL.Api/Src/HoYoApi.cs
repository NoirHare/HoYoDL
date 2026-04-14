using HoYoDL.Api.Exceptions;
using HoYoDL.Api.Internal;
using HoYoDL.Api.Internal.Dtos;
using HoYoDL.Api.Internal.Extensions;
using HoYoDL.Api.Internal.Json;
using HoYoDL.Api.Internal.Responses;
using HoYoDL.Api.Internal.Utilities;
using HoYoDL.Api.Models;

using ZstdSharp;

namespace HoYoDL.Api;

public class HoYoApi(HttpClient client, Region region) {
    private readonly HttpClient _client = client;
    private readonly Region _region = region;

    private async Task<T> GetApiAsync<T>(Uri url, CancellationToken ct = default) {
        using HttpRequestMessage request = new();
        request.Method = HttpMethod.Get;
        request.RequestUri = url;

        using HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        using Stream network = await response.Content.ReadAsStreamAsync(ct);

        HoYoResult<T>? result = await JsonHelper.DeserializeAsync<HoYoResult<T>>(network, ct);
        if (result == null) {
            throw new HoYoSerializationException($"Failed to deserialize API response to {typeof(T).Name}.");
        }
        if (result.Retcode != 0) {
            throw new HoYoApiException(result.Retcode, result.Message);
        }
        if (result.Data is null) {
            throw new HoYoApiException($"API returned success (retcode: 0) but data is null.");
        }

        return result.Data;
    }

    private Task<T> GetApiAsync<T>(string host, string path, IEnumerable<KeyValuePair<string, string>> queries, CancellationToken ct = default) {
        UrlBuilder builder = Url.Https(host).Path(path);
        foreach (KeyValuePair<string, string> query in queries) {
            builder.Query(query.Key, query.Value);
        }
        return GetApiAsync<T>(builder.ToUri(), ct);
    }

    private Task<T> GetHypApiAsync<T>(string path, IEnumerable<KeyValuePair<string, string>> querys, CancellationToken ct = default)
        => GetApiAsync<T>(_region.HypApiHost, path, querys, ct);

    private Task<T> GetTakumiApiAsync<T>(string path, IEnumerable<KeyValuePair<string, string>> querys, CancellationToken ct = default)
        => GetApiAsync<T>(_region.TakumiApiHost, path, querys, ct);

    public async Task<IReadOnlyList<Game>> GetGamesAsync(string language, CancellationToken ct = default) {
        GamesResponse response = await GetHypApiAsync<GamesResponse>(
            "/hyp/hyp-connect/api/getGames",
            [
                KeyValuePair.Create("launcher_id", _region.LauncherId),
                KeyValuePair.Create("language", language),
            ],
            ct
        );

        return response.Games.ToModels();
    }

    public async Task<Branches> GetBranchesAsync(string gameId, CancellationToken ct = default) {
        BranchesResponse response = await GetHypApiAsync<BranchesResponse>(
            "/hyp/hyp-connect/api/getGameBranches",
            [
                KeyValuePair.Create("launcher_id", _region.LauncherId),
                KeyValuePair.Create("game_ids[]", gameId),
            ],
            ct
        );

        if (response.Branches.Count == 0) throw new HoYoResourceNotFoundException("Game", gameId);

        return response.Branches[0].ToModel();
    }

    public async Task<Resource> GetResourceAsync(string branchId, string packageId, string password, CancellationToken ct = default) {
        ResourceResponse response = await GetTakumiApiAsync<ResourceResponse>(
            "/downloader/sophon_chunk/api/getBuild",
            [
                KeyValuePair.Create("branch", branchId),
                KeyValuePair.Create("package_id", packageId),
                KeyValuePair.Create("password", password),
            ],
            ct
        );

        return response.ToModel();
    }

    public async Task<IReadOnlyList<HoYoFile>> GetFilesAsync(string prefix, string id, CancellationToken ct = default) {
        using HttpRequestMessage request = new();
        request.Method = HttpMethod.Get;
        request.RequestUri = new($"{prefix}/{id}");

        using HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        using Stream network = await response.Content.ReadAsStreamAsync(ct);
        using DecompressionStream decompression = new(network);

        ManifestDto manifest = LightProto.Serializer.Deserialize<ManifestDto>(decompression);
        return manifest.Files.ToModels();
    }

    public async Task<Stream> GetChunkAsync(string prefix, string id, CancellationToken ct = default) {
        using HttpRequestMessage request = new();
        request.Method = HttpMethod.Get;
        request.RequestUri = new($"{prefix}/{id}");

        HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        return new WithOwnerStream(await response.Content.ReadAsStreamAsync(ct), response);
    }
}