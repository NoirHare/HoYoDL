using System;
using System.Threading.Tasks;

using HoYoDL.Handlers;

using Umrab.Options;

namespace HoYoDL;

internal class Program {
    private static async Task Main(string[] args) {
        ParseResult? sub;
        try {
            ParseResult result = Commands.RootCommand.Parse(args);
            if (result.GetValue(Commands.RootHelpOption, () => false)) {
                Commands.PrintHelp();
                return;
            }

            if (result.SubResult == null) {
                Commands.PrintHelp();
                return;
            }

            sub = result.SubResult;
        } catch (Exception e) {
            Console.WriteLine(e);
            Console.WriteLine();
            Commands.PrintHelp();
            return;
        }

        if (sub.Command == Commands.GameCommand) await GameHandler.RunAsync(sub);
        else if (sub.Command == Commands.DownloadCommand) await DownloadHandler.RunAsync(sub);
    }
}