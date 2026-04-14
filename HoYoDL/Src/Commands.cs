using System;

using HoYoDL.Api;

using Umrab.Options;

namespace HoYoDL;

public static class Commands {
    public static readonly Command RootCommand;
    public static readonly Option<bool> RootHelpOption;

    public static readonly Command GameCommand;
    public static readonly Option<Region> GameRegionOption;
    public static readonly Option<string> GameLanguageOption;

    public static readonly Command DownloadCommand;
    public static readonly Option<Region> DownloadRegionOption;
    public static readonly Option<string> DownloadAudioOption;
    public static readonly Option<bool> DownloadPredownloadOption;
    public static readonly Option<string> DownloadCacheOption;
    public static readonly Option<bool> DownloadMergeOption;
    public static readonly Option<bool> DownloadDeleteOption;
    public static readonly Argument<string> DownloadGameIdArgument;
    public static readonly Argument<string> DownloadTargetArgument;

    static Commands() {
        RootCommand = new Command("")
            .Add(RootHelpOption = new Option<bool>(
                "help",
                ['h'],
                isRequired: false,
                isFlag: true,
                BoolOptionsConverter
            ))
            .Add(GameCommand = new Command("game", ['g'])
                .Add(GameRegionOption = new Option<Region>(
                    "region",
                    ['r'],
                    isRequired: false,
                    isFlag: false,
                    RegionOptionsConverter))
                .Add(GameLanguageOption = new Option<string>(
                    "language",
                    ['l'],
                    isRequired: false,
                    isFlag: false,
                    StringOptionsConverter
                ))
            )
            .Add(DownloadCommand = new Command("download", ['d'])
                .Add(DownloadRegionOption = new Option<Region>(
                    "region",
                    ['r'],
                    isRequired: false,
                    isFlag: false,
                    RegionOptionsConverter
                ))
                .Add(DownloadAudioOption = new Option<string>(
                    "audio",
                    ['a'],
                    isRequired: false,
                    isFlag: false,
                    StringOptionsConverter
                ))
                .Add(DownloadPredownloadOption = new Option<bool>(
                    "predownload",
                    ['p'],
                    isRequired: false,
                    isFlag: true,
                    BoolOptionsConverter
                ))
                .Add(DownloadCacheOption = new Option<string>(
                    "cache",
                    ['c'],
                    isRequired: false,
                    isFlag: false,
                    StringOptionsConverter
                ))
                .Add(DownloadMergeOption = new Option<bool>(
                    "merge",
                    ['m'],
                    isRequired: false,
                    isFlag: true,
                    BoolOptionsConverter
                ))
                .Add(DownloadDeleteOption = new Option<bool>(
                    "delete",
                    ['d'],
                    isRequired: false,
                    isFlag: true,
                    BoolOptionsConverter
                ))
                .Add(DownloadGameIdArgument = new Argument<string>(
                    isRequired: true,
                    StringArgumentConverter
                ))
                .Add(DownloadTargetArgument = new Argument<string>(
                    isRequired: true,
                    StringArgumentConverter
                ))
            );
    }

    public static void PrintHelp() => Console.WriteLine("""
        HoYoDL - HoYoverse Game Downloader
        Usage:
          HoYoDL [options]
          HoYoDL <command> [options]
        Options:
          -h, --help                    Show this help message
        Commands:
          game, g                       List available games
          download, d                   Download a game
        game [options]:
          -r, --region <region>         Target region (global, china)
          -l, --language <language>     Language for game info
        download [options] <game_id> <target>:
          <game_id>                     The ID of the game to download
          <target>                      The target download directory
          -r, --region <region>         Target region (global, china)
          -a, --audio <language>        Audio language to download
          -p, --predownload             Download predownload version if available
          -c, --cache <path>            Cache directory for downloaded chunks
          -m, --merge                   Merge chunks to target directory after download
          -d, --delete                  Delete cache after merging
        """);

    private static bool BoolOptionsConverter(ReadOnlySpan<char> value, bool previous) => true;
    private static string StringOptionsConverter(ReadOnlySpan<char> value, string? previous) => value.ToString();
    private static Region RegionOptionsConverter(ReadOnlySpan<char> value, Region? previous) => value switch {
        "global" => Region.Global,
        "china" => Region.China,
        _ => throw new Exception($""), // TODO
    };

    private static string StringArgumentConverter(ReadOnlySpan<char> value) => value.ToString();
}