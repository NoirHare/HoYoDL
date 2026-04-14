using HoYoDL.Api;
using HoYoDL.Api.Models;

using Umrab.Options;

namespace HoYoDL.Handlers;

internal static class GameHandler {
    public static async Task RunAsync(ParseResult result) {
        Region region = result.GetValue(Commands.GameRegionOption, () => Region.China);
        string language = result.GetValue(Commands.GameLanguageOption, () => "zh-cn");
        using HttpClient client = new();
        HoYoApi api = new(client, region);
        IReadOnlyList<Game> games = await api.GetGamesAsync(language);
        foreach (Game game in games) {
            Console.WriteLine($"{game.Id}  {game.Name}");
        }
    }
}