using PokerStats.Models;
using PokerStats.Services;
using Xunit;

namespace PokerStats.Tests;

public class StatsServiceTests
{
    private static Database BuildDatabase()
    {
        var alice = new Player { Id = "alice", Name = "Alice" };
        var bob = new Player { Id = "bob", Name = "Bob" };
        var charlie = new Player { Id = "charlie", Name = "Charlie" };

        var game1 = new Game
        {
            Id = "g1",
            Date = new DateTime(2024, 1, 1),
            BuyIn = 20,
            Results =
            [
                new GameResult { PlayerId = "alice", Placement = 1, Payout = 60 },
                new GameResult { PlayerId = "bob", Placement = 2, Payout = 30 },
                new GameResult { PlayerId = "charlie", Placement = 3, Payout = 10 },
            ],
            Knockouts =
            [
                new KnockoutRecord { EliminatedPlayerId = "charlie", KnockedOutByPlayerId = "alice" },
                new KnockoutRecord { EliminatedPlayerId = "bob", KnockedOutByPlayerId = "alice" },
            ]
        };

        var game2 = new Game
        {
            Id = "g2",
            Date = new DateTime(2024, 2, 1),
            BuyIn = 20,
            Results =
            [
                new GameResult { PlayerId = "bob", Placement = 1, Payout = 60 },
                new GameResult { PlayerId = "alice", Placement = 2, Payout = 30 },
                new GameResult { PlayerId = "charlie", Placement = 3, Payout = 10 },
            ],
            Knockouts =
            [
                new KnockoutRecord { EliminatedPlayerId = "charlie", KnockedOutByPlayerId = "bob" },
                new KnockoutRecord { EliminatedPlayerId = "alice", KnockedOutByPlayerId = "bob" },
            ]
        };

        var game3 = new Game
        {
            Id = "g3",
            Date = new DateTime(2024, 3, 1),
            BuyIn = 20,
            Results =
            [
                new GameResult { PlayerId = "alice", Placement = 1, Payout = 60 },
                new GameResult { PlayerId = "charlie", Placement = 2, Payout = 30 },
                new GameResult { PlayerId = "bob", Placement = 3, Payout = 0 },
            ],
            Knockouts =
            [
                new KnockoutRecord { EliminatedPlayerId = "bob", KnockedOutByPlayerId = "charlie" },
                new KnockoutRecord { EliminatedPlayerId = "charlie", KnockedOutByPlayerId = "alice" },
            ]
        };

        return new Database
        {
            Players = [alice, bob, charlie],
            Games = [game1, game2, game3]
        };
    }

    [Fact]
    public void GetPlayerStats_Alice_WinsCorrect()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var stats = svc.GetPlayerStats("alice");

        Assert.Equal(3, stats.GamesPlayed);
        Assert.Equal(2, stats.Wins);
        Assert.Equal(3, stats.TopThree);
    }

    [Fact]
    public void GetPlayerStats_Alice_WinRateCorrect()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var stats = svc.GetPlayerStats("alice");

        Assert.Equal(2.0 / 3.0 * 100, stats.WinRate, precision: 5);
    }

    [Fact]
    public void GetPlayerStats_Alice_NetProfitCorrect()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var stats = svc.GetPlayerStats("alice");

        // Game1: +40, Game2: +10, Game3: +40 => net = 90
        // TotalBuyIn = 60, TotalPayout = 150
        Assert.Equal(60m, stats.TotalBuyIn);
        Assert.Equal(150m, stats.TotalPayout);
        Assert.Equal(90m, stats.NetProfit);
    }

    [Fact]
    public void GetPlayerStats_Alice_PayoutStatsCorrect()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var stats = svc.GetPlayerStats("alice");

        // All 3 games paid out (60, 30, 60) => AvgPayoutPerGame = 50, AvgPayoutPerCash = 50, Biggest = 60
        Assert.Equal(50m, stats.AvgPayoutPerGame);
        Assert.Equal(50m, stats.AvgPayoutPerCash);
        Assert.Equal(60m, stats.BiggestPayout);
    }

    [Fact]
    public void GetPlayerStats_Bob_PayoutStatsWithMiss()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var stats = svc.GetPlayerStats("bob");

        // Bob: g1 payout=30, g2 payout=60, g3 payout=0
        // AvgPayoutPerGame = 90/3 = 30, AvgPayoutPerCash = 90/2 = 45, Biggest = 60
        Assert.Equal(30m, stats.AvgPayoutPerGame);
        Assert.Equal(45m, stats.AvgPayoutPerCash);
        Assert.Equal(60m, stats.BiggestPayout);
    }

    [Fact]
    public void GetPlayerStats_Alice_KnockoutsCorrect()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var stats = svc.GetPlayerStats("alice");

        // Alice knocked out: charlie(g1), bob(g1), charlie(g3) = 3
        Assert.Equal(3, stats.TotalKnockouts);
        // Alice was knocked out by bob in g2
        Assert.Equal(1, stats.TimesKnockedOut);
    }

    [Fact]
    public void GetPlayerStats_Alice_StreakCorrect()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var stats = svc.GetPlayerStats("alice");

        // Games: win, loss, win => longest win streak = 1, current win streak = 1
        Assert.Equal(1, stats.LongestWinStreak);
        Assert.Equal(1, stats.CurrentWinStreak);
        // All 3 are top-3 => longest and current top-3 streak = 3
        Assert.Equal(3, stats.LongestTopThreeStreak);
        Assert.Equal(3, stats.CurrentTopThreeStreak);
    }

    [Fact]
    public void GetLeaderboard_OrderedByWins()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var leaderboard = svc.GetLeaderboard();

        // Alice has 2 wins, Bob has 1 win, Charlie has 0
        Assert.Equal("Alice", leaderboard[0].PlayerName);
        Assert.Equal("Bob", leaderboard[1].PlayerName);
        Assert.Equal("Charlie", leaderboard[2].PlayerName);
    }

    [Fact]
    public void GetKnockoutLeaderboard_OrderedByKills()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var leaders = svc.GetKnockoutLeaderboard();

        // Alice: 3 kills, Bob: 2 kills, Charlie: 1 kill
        Assert.Equal("Alice", leaders[0].PlayerName);
        Assert.Equal(3, leaders[0].TotalKnockouts);
    }

    [Fact]
    public void GetHeadToHead_Alice_VsBob()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var h2h = svc.GetHeadToHead("alice");
        var vsBob = h2h.FirstOrDefault(h => h.OpponentName == "Bob");

        Assert.NotNull(vsBob);
        Assert.Equal(3, vsBob.GamesAgainst);
        // Game1: Alice 1st, Bob 2nd => Alice wins
        // Game2: Bob 1st, Alice 2nd => Alice loses
        // Game3: Alice 1st, Bob 3rd => Alice wins
        Assert.Equal(2, vsBob.WinsAgainst);
    }

    [Fact]
    public void GetPlacementDistribution_Alice()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var (mean, stdDev, min, max) = svc.GetPlacementDistribution("alice");

        // Placements: 1, 2, 1 => mean = 4/3
        Assert.Equal((1.0 + 2.0 + 1.0) / 3.0, mean, precision: 5);
        Assert.Equal(1, min);
        Assert.Equal(2, max);
    }

    [Fact]
    public void GetEarningsTrends_Alice_CumulativeNet()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var trends = svc.GetEarningsTrends();

        Assert.True(trends.ContainsKey("Alice"));
        var aliceSeries = trends["Alice"];
        // Game1: payout 60 - buyin 20 = +40 cumulative => 40
        // Game2: payout 30 - buyin 20 = +10 cumulative => 50
        // Game3: payout 60 - buyin 20 = +40 cumulative => 90
        Assert.Equal(3, aliceSeries.Count);
        Assert.Equal(40m, aliceSeries[0].NetProfitToDate);
        Assert.Equal(50m, aliceSeries[1].NetProfitToDate);
        Assert.Equal(90m, aliceSeries[2].NetProfitToDate);
    }

    [Fact]
    public void GetPayoutStats_ReturnsCorrectCashCount()
    {
        var db = BuildDatabase();
        var svc = new StatsService(db);
        var payoutStats = svc.GetPayoutStats();

        // Bob cashed in g1 (30) and g2 (60) but not g3 (0 payout)
        var bobStats = payoutStats.FirstOrDefault(p => p.PlayerName == "Bob");
        Assert.Equal(2, bobStats.CashCount);
        Assert.Equal(45m, bobStats.AvgPayout); // (30+60)/2
    }
}
