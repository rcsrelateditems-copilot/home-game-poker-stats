using PokerStats.Services;

namespace PokerStats.Commands;

public static class StatsCommands
{
    public static void ShowLeaderboard(DataService dataService)
    {
        var db = dataService.Load();
        var statsService = new StatsService(db);
        var leaderboard = statsService.GetLeaderboard();

        if (leaderboard.Count == 0)
        {
            Console.WriteLine("No stats available yet. Record some games first.");
            return;
        }

        Console.WriteLine("=== Overall Leaderboard ===");
        Console.WriteLine();
        Console.WriteLine($"{"Rank",-5} {"Name",-20} {"Games",-7} {"Wins",-6} {"Win%",-7} {"Top3%",-7} {"ITM%",-7} {"AvgPos",-7} {"Payout$",-11} {"Net $",-10} {"ROI%",-8}");
        Console.WriteLine(new string('-', 102));

        for (int i = 0; i < leaderboard.Count; i++)
        {
            var s = leaderboard[i];
            Console.WriteLine(
                $"{i + 1,-5} {s.PlayerName,-20} {s.GamesPlayed,-7} {s.Wins,-6} {s.WinRate,6:F1}% {s.TopThreeRate,6:F1}% {s.InTheMoneyRate,6:F1}% {s.AvgPlacement,7:F2} {s.TotalPayout,10:F2} {s.NetProfit,9:F2} {s.Roi,7:F1}%");
        }

        Console.WriteLine();
    }

    public static void ShowPlayerStats(DataService dataService, string playerName)
    {
        var db = dataService.Load();

        var player = db.Players.FirstOrDefault(p =>
            p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));

        if (player == null)
        {
            Console.Error.WriteLine($"Error: Player '{playerName}' not found.");
            Console.Error.WriteLine("Use 'list-players' to see all registered players.");
            return;
        }

        var statsService = new StatsService(db);
        var stats = statsService.GetPlayerStats(player.Id);
        var (mean, stdDev, min, max) = statsService.GetPlacementDistribution(player.Id);
        var headToHead = statsService.GetHeadToHead(player.Id);

        Console.WriteLine($"=== Stats for {stats.PlayerName} ===");
        Console.WriteLine();

        Console.WriteLine("--- Performance ---");
        Console.WriteLine($"  Games played:          {stats.GamesPlayed}");
        Console.WriteLine($"  Wins (1st place):      {stats.Wins}  ({stats.WinRate:F1}%)");
        Console.WriteLine($"  Top 3 finishes:        {stats.TopThree}  ({stats.TopThreeRate:F1}%)");
        Console.WriteLine($"  In the money (payout): {stats.InTheMoney}  ({stats.InTheMoneyRate:F1}%)");
        Console.WriteLine($"  Avg placement:         {stats.AvgPlacement:F2}");
        Console.WriteLine($"  Placement std dev:     {stdDev:F2}");
        Console.WriteLine($"  Best finish:           #{min}");
        Console.WriteLine($"  Worst finish:          #{max}");
        Console.WriteLine();

        Console.WriteLine("--- Earnings ---");
        Console.WriteLine($"  Total buy-in spent:    ${stats.TotalBuyIn:F2}");
        Console.WriteLine($"  Total payout earned:   ${stats.TotalPayout:F2}");
        Console.WriteLine($"  Avg payout per game:   ${stats.AvgPayoutPerGame:F2}");
        Console.WriteLine($"  Avg payout per cash:   ${stats.AvgPayoutPerCash:F2}");
        Console.WriteLine($"  Biggest single payout: ${stats.BiggestPayout:F2}");
        Console.WriteLine($"  Net profit/loss:       ${stats.NetProfit:F2}");
        Console.WriteLine($"  ROI:                   {stats.Roi:F1}%");
        Console.WriteLine();

        Console.WriteLine("--- Knockouts ---");
        Console.WriteLine($"  Total knockouts dealt: {stats.TotalKnockouts}");
        Console.WriteLine($"  Times knocked out:     {stats.TimesKnockedOut}");
        Console.WriteLine($"  K/D ratio:             {stats.KdRatio:F2}");
        Console.WriteLine();

        Console.WriteLine("--- Streaks ---");
        Console.WriteLine($"  Longest win streak:       {stats.LongestWinStreak}");
        Console.WriteLine($"  Longest top-3 streak:     {stats.LongestTopThreeStreak}");
        Console.WriteLine($"  Current win streak:       {stats.CurrentWinStreak}");
        Console.WriteLine($"  Current top-3 streak:     {stats.CurrentTopThreeStreak}");
        Console.WriteLine();

        if (headToHead.Count > 0)
        {
            Console.WriteLine("--- Head-to-Head ---");
            Console.WriteLine($"  {"Opponent",-20} {"Games",-7} {"Wins",-7} {"Win%",-8} {"KO Dealt",-10} {"KO Recv"}");
            Console.WriteLine("  " + new string('-', 65));
            foreach (var h in headToHead)
            {
                Console.WriteLine(
                    $"  {h.OpponentName,-20} {h.GamesAgainst,-7} {h.WinsAgainst,-7} {h.WinRateAgainst,7:F1}% {h.KnockoutsDealtToOpponent,-10} {h.KnockoutsReceivedFromOpponent}");
            }
            Console.WriteLine();
        }
    }

    public static void ShowKnockoutLeaderboard(DataService dataService)
    {
        var db = dataService.Load();
        var statsService = new StatsService(db);
        var leaders = statsService.GetKnockoutLeaderboard();

        if (leaders.Count == 0)
        {
            Console.WriteLine("No knockout data available yet.");
            return;
        }

        Console.WriteLine("=== Knockout Leaderboard ===");
        Console.WriteLine();
        Console.WriteLine($"{"Rank",-5} {"Name",-20} {"Kills",-8} {"Deaths",-8} {"K/D Ratio"}");
        Console.WriteLine(new string('-', 55));

        for (int i = 0; i < leaders.Count; i++)
        {
            var e = leaders[i];
            Console.WriteLine($"{i + 1,-5} {e.PlayerName,-20} {e.TotalKnockouts,-8} {e.TimesEliminated,-8} {e.KdRatio:F2}");
        }

        Console.WriteLine();
    }

    public static void ShowEarningsTrends(DataService dataService)
    {
        var db = dataService.Load();
        var statsService = new StatsService(db);
        var trends = statsService.GetEarningsTrends();

        if (trends.Count == 0)
        {
            Console.WriteLine("No earnings data available yet.");
            return;
        }

        Console.WriteLine("=== Earnings Trends (Cumulative Net Profit) ===");
        Console.WriteLine();

        foreach (var (playerName, series) in trends.OrderBy(t => t.Key))
        {
            Console.WriteLine($"  {playerName}:");
            foreach (var (date, net) in series)
            {
                var bar = net >= 0
                    ? new string('+', Math.Min((int)(net / 5), 40))
                    : new string('-', Math.Min((int)(-net / 5), 40));
                Console.WriteLine($"    {date:yyyy-MM-dd}  {net,8:F2}  {bar}");
            }
            Console.WriteLine();
        }
    }

    public static void ShowPayoutStats(DataService dataService)
    {
        var db = dataService.Load();
        var statsService = new StatsService(db);
        var payoutStats = statsService.GetPayoutStats();

        if (payoutStats.Count == 0)
        {
            Console.WriteLine("No payout data available yet.");
            return;
        }

        Console.WriteLine("=== Payout Statistics ===");
        Console.WriteLine();
        Console.WriteLine($"{"Name",-20} {"Cashes",-8} {"Avg Cash",-12} {"Biggest Cash"}");
        Console.WriteLine(new string('-', 55));

        foreach (var (name, avgPayout, maxPayout, cashCount) in payoutStats)
        {
            Console.WriteLine($"{name,-20} {cashCount,-8} ${avgPayout,-11:F2} ${maxPayout:F2}");
        }

        Console.WriteLine();
    }

    public static void ShowAllStats(DataService dataService)
    {
        ShowLeaderboard(dataService);
        ShowKnockoutLeaderboard(dataService);
        ShowPayoutStats(dataService);
        ShowEarningsTrends(dataService);
    }
}
