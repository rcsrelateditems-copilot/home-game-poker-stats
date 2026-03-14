using PokerStats.Commands;
using PokerStats.Services;

var dataService = new DataService();

if (args.Length == 0)
{
    PrintHelp();
    return 0;
}

switch (args[0].ToLowerInvariant())
{
    case "add-player":
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: pokerstats add-player <name>");
            return 1;
        }
        PlayerCommands.AddPlayer(dataService, string.Join(" ", args[1..]));
        break;

    case "list-players":
        PlayerCommands.ListPlayers(dataService);
        break;

    case "add-game":
        GameCommands.AddGame(dataService);
        break;

    case "list-games":
        GameCommands.ListGames(dataService);
        break;

    case "seed-data":
        SeedCommands.SeedData(dataService);
        break;

    case "stats":
        if (args.Length >= 2)
        {
            // stats for a specific sub-command or player
            switch (args[1].ToLowerInvariant())
            {
                case "leaderboard":
                    StatsCommands.ShowLeaderboard(dataService);
                    break;
                case "knockouts":
                    StatsCommands.ShowKnockoutLeaderboard(dataService);
                    break;
                case "earnings":
                    StatsCommands.ShowEarningsTrends(dataService);
                    break;
                case "payouts":
                    StatsCommands.ShowPayoutStats(dataService);
                    break;
                case "player":
                    if (args.Length < 3)
                    {
                        Console.Error.WriteLine("Usage: pokerstats stats player <name>");
                        return 1;
                    }
                    StatsCommands.ShowPlayerStats(dataService, string.Join(" ", args[2..]));
                    break;
                default:
                    // Treat the argument as a player name
                    StatsCommands.ShowPlayerStats(dataService, string.Join(" ", args[1..]));
                    break;
            }
        }
        else
        {
            StatsCommands.ShowAllStats(dataService);
        }
        break;

    case "help":
    case "--help":
    case "-h":
        PrintHelp();
        break;

    default:
        Console.Error.WriteLine($"Unknown command: '{args[0]}'. Run 'pokerstats help' for usage.");
        return 1;
}

return 0;

static void PrintHelp()
{
    Console.WriteLine("Home Game Poker Stats Tracker");
    Console.WriteLine("==============================");
    Console.WriteLine();
    Console.WriteLine("USAGE:");
    Console.WriteLine("  pokerstats <command> [options]");
    Console.WriteLine();
    Console.WriteLine("COMMANDS:");
    Console.WriteLine("  add-player <name>           Register a new player");
    Console.WriteLine("  list-players                List all registered players");
    Console.WriteLine("  add-game                    Record a new tournament (interactive wizard)");
    Console.WriteLine("  list-games                  List all recorded games");
    Console.WriteLine("  seed-data                   Populate database with 100 random sample games");
    Console.WriteLine("  stats                       Show all statistics");
    Console.WriteLine("  stats leaderboard           Overall leaderboard (wins, profit, ROI)");
    Console.WriteLine("  stats knockouts             Knockout leaderboard (K/D ratios)");
    Console.WriteLine("  stats earnings              Cumulative earnings trend per player");
    Console.WriteLine("  stats payouts               Payout statistics per player");
    Console.WriteLine("  stats player <name>         Detailed stats for one player including:");
    Console.WriteLine("                                - Win rate, top-3%, in-the-money%");
    Console.WriteLine("                                - Average placement & std deviation");
    Console.WriteLine("                                - Net profit, ROI");
    Console.WriteLine("                                - Knockout K/D ratio");
    Console.WriteLine("                                - Win & top-3 streaks");
    Console.WriteLine("                                - Head-to-head records vs all opponents");
    Console.WriteLine("  help                        Show this help");
    Console.WriteLine();
    Console.WriteLine("DATA FILE:");
    Console.WriteLine($"  {new DataService().FilePath}");
    Console.WriteLine();
    Console.WriteLine("EXAMPLES:");
    Console.WriteLine("  pokerstats add-player Alice");
    Console.WriteLine("  pokerstats add-player Bob");
    Console.WriteLine("  pokerstats add-game");
    Console.WriteLine("  pokerstats stats");
    Console.WriteLine("  pokerstats stats player Alice");
    Console.WriteLine("  pokerstats stats leaderboard");
}
