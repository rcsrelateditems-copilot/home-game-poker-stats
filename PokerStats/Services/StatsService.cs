using PokerStats.Models;

namespace PokerStats.Services;

public record PlayerStats(
    string PlayerId,
    string PlayerName,
    int GamesPlayed,
    int Wins,
    int TopThree,
    int InTheMoney,
    double WinRate,
    double TopThreeRate,
    double InTheMoneyRate,
    double AvgPlacement,
    decimal TotalPayout,
    decimal TotalBuyIn,
    decimal NetProfit,
    double Roi,
    int TotalKnockouts,
    int TimesKnockedOut,
    double KdRatio,
    int LongestWinStreak,
    int LongestTopThreeStreak,
    int CurrentWinStreak,
    int CurrentTopThreeStreak
);

public record HeadToHeadRecord(
    string OpponentId,
    string OpponentName,
    int GamesAgainst,
    int WinsAgainst,
    int KnockoutsDealtToOpponent,
    int KnockoutsReceivedFromOpponent,
    double WinRateAgainst
);

public record KnockoutLeaderEntry(
    string PlayerId,
    string PlayerName,
    int TotalKnockouts,
    int TimesEliminated,
    double KdRatio
);

public class StatsService
{
    private readonly Database _db;

    public StatsService(Database db)
    {
        _db = db;
    }

    public PlayerStats GetPlayerStats(string playerId)
    {
        var player = _db.Players.FirstOrDefault(p => p.Id == playerId)
            ?? throw new ArgumentException($"Player '{playerId}' not found.");

        var gamesWithPlayer = _db.Games
            .Where(g => g.Results.Any(r => r.PlayerId == playerId))
            .OrderBy(g => g.Date)
            .ToList();

        int gamesPlayed = gamesWithPlayer.Count;
        int wins = gamesWithPlayer.Count(g => g.Results.Any(r => r.PlayerId == playerId && r.Placement == 1));
        int topThree = gamesWithPlayer.Count(g => g.Results.Any(r => r.PlayerId == playerId && r.Placement <= 3));
        int inTheMoney = gamesWithPlayer.Count(g => g.Results.Any(r => r.PlayerId == playerId && r.Payout > 0));

        double winRate = gamesPlayed > 0 ? (double)wins / gamesPlayed * 100 : 0;
        double topThreeRate = gamesPlayed > 0 ? (double)topThree / gamesPlayed * 100 : 0;
        double itmRate = gamesPlayed > 0 ? (double)inTheMoney / gamesPlayed * 100 : 0;

        double avgPlacement = gamesPlayed > 0
            ? gamesWithPlayer.Average(g => g.Results.First(r => r.PlayerId == playerId).Placement)
            : 0;

        decimal totalPayout = gamesWithPlayer.Sum(g => g.Results.Where(r => r.PlayerId == playerId).Sum(r => r.Payout));
        decimal totalBuyIn = gamesWithPlayer.Sum(g => g.BuyIn);
        decimal netProfit = totalPayout - totalBuyIn;
        double roi = totalBuyIn > 0 ? (double)(netProfit / totalBuyIn) * 100 : 0;

        int totalKnockouts = _db.Games.Sum(g => g.Knockouts.Count(k => k.KnockedOutByPlayerId == playerId));
        int timesKnockedOut = _db.Games.Sum(g => g.Knockouts.Count(k => k.EliminatedPlayerId == playerId));
        double kdRatio = timesKnockedOut > 0 ? (double)totalKnockouts / timesKnockedOut : totalKnockouts;

        var (longestWinStreak, currentWinStreak) = CalculateStreak(
            gamesWithPlayer, playerId, r => r.Placement == 1);
        var (longestTopThreeStreak, currentTopThreeStreak) = CalculateStreak(
            gamesWithPlayer, playerId, r => r.Placement <= 3);

        return new PlayerStats(
            playerId, player.Name, gamesPlayed, wins, topThree, inTheMoney,
            winRate, topThreeRate, itmRate, avgPlacement,
            totalPayout, totalBuyIn, netProfit, roi,
            totalKnockouts, timesKnockedOut, kdRatio,
            longestWinStreak, longestTopThreeStreak,
            currentWinStreak, currentTopThreeStreak
        );
    }

    private static (int longest, int current) CalculateStreak(
        List<Game> orderedGames, string playerId, Func<GameResult, bool> predicate)
    {
        int longest = 0;
        int current = 0;
        int running = 0;

        foreach (var game in orderedGames)
        {
            var result = game.Results.FirstOrDefault(r => r.PlayerId == playerId);
            if (result != null && predicate(result))
            {
                running++;
                if (running > longest) longest = running;
            }
            else
            {
                running = 0;
            }
        }

        current = running;
        return (longest, current);
    }

    public List<PlayerStats> GetLeaderboard()
    {
        return _db.Players
            .Select(p => GetPlayerStats(p.Id))
            .Where(s => s.GamesPlayed > 0)
            .OrderByDescending(s => s.Wins)
            .ThenByDescending(s => s.NetProfit)
            .ThenByDescending(s => s.TopThree)
            .ThenBy(s => s.AvgPlacement)
            .ToList();
    }

    public List<HeadToHeadRecord> GetHeadToHead(string playerId)
    {
        var player = _db.Players.FirstOrDefault(p => p.Id == playerId)
            ?? throw new ArgumentException($"Player '{playerId}' not found.");

        var gamesWithPlayer = _db.Games
            .Where(g => g.Results.Any(r => r.PlayerId == playerId))
            .ToList();

        var opponentIds = gamesWithPlayer
            .SelectMany(g => g.Results.Select(r => r.PlayerId))
            .Where(id => id != playerId)
            .Distinct()
            .ToList();

        var records = new List<HeadToHeadRecord>();

        foreach (var opponentId in opponentIds)
        {
            var opponent = _db.Players.FirstOrDefault(p => p.Id == opponentId);
            if (opponent == null) continue;

            var sharedGames = gamesWithPlayer
                .Where(g => g.Results.Any(r => r.PlayerId == opponentId))
                .ToList();

            int gamesAgainst = sharedGames.Count;
            int winsAgainst = sharedGames.Count(g =>
            {
                var myResult = g.Results.FirstOrDefault(r => r.PlayerId == playerId);
                var oppResult = g.Results.FirstOrDefault(r => r.PlayerId == opponentId);
                return myResult != null && oppResult != null && myResult.Placement < oppResult.Placement;
            });

            int knockoutsDealt = _db.Games.Sum(g =>
                g.Knockouts.Count(k => k.KnockedOutByPlayerId == playerId && k.EliminatedPlayerId == opponentId));
            int knockoutsReceived = _db.Games.Sum(g =>
                g.Knockouts.Count(k => k.KnockedOutByPlayerId == opponentId && k.EliminatedPlayerId == playerId));

            double winRateAgainst = gamesAgainst > 0 ? (double)winsAgainst / gamesAgainst * 100 : 0;

            records.Add(new HeadToHeadRecord(
                opponentId, opponent.Name, gamesAgainst, winsAgainst,
                knockoutsDealt, knockoutsReceived, winRateAgainst));
        }

        return records.OrderByDescending(r => r.WinRateAgainst).ToList();
    }

    public List<KnockoutLeaderEntry> GetKnockoutLeaderboard()
    {
        return _db.Players
            .Select(p =>
            {
                int kills = _db.Games.Sum(g => g.Knockouts.Count(k => k.KnockedOutByPlayerId == p.Id));
                int deaths = _db.Games.Sum(g => g.Knockouts.Count(k => k.EliminatedPlayerId == p.Id));
                double kd = deaths > 0 ? (double)kills / deaths : kills;
                return new KnockoutLeaderEntry(p.Id, p.Name, kills, deaths, kd);
            })
            .Where(e => e.TotalKnockouts > 0 || e.TimesEliminated > 0)
            .OrderByDescending(e => e.TotalKnockouts)
            .ThenByDescending(e => e.KdRatio)
            .ToList();
    }

    public Dictionary<string, List<(DateTime Date, decimal NetProfitToDate)>> GetEarningsTrends()
    {
        var trends = new Dictionary<string, List<(DateTime, decimal)>>();

        foreach (var player in _db.Players)
        {
            var gamesWithPlayer = _db.Games
                .Where(g => g.Results.Any(r => r.PlayerId == player.Id))
                .OrderBy(g => g.Date)
                .ToList();

            if (gamesWithPlayer.Count == 0) continue;

            decimal running = 0;
            var series = new List<(DateTime, decimal)>();

            foreach (var game in gamesWithPlayer)
            {
                var result = game.Results.First(r => r.PlayerId == player.Id);
                running += result.Payout - game.BuyIn;
                series.Add((game.Date, running));
            }

            trends[player.Name] = series;
        }

        return trends;
    }

    public (double mean, double stdDev, int min, int max) GetPlacementDistribution(string playerId)
    {
        var placements = _db.Games
            .Where(g => g.Results.Any(r => r.PlayerId == playerId))
            .Select(g => g.Results.First(r => r.PlayerId == playerId).Placement)
            .ToList();

        if (placements.Count == 0)
            return (0, 0, 0, 0);

        double mean = placements.Average();
        double variance = placements.Average(p => Math.Pow(p - mean, 2));
        double stdDev = Math.Sqrt(variance);

        return (mean, stdDev, placements.Min(), placements.Max());
    }

    public List<(string PlayerName, decimal AvgPayout, decimal MaxPayout, int CashCount)> GetPayoutStats()
    {
        return _db.Players
            .Select(p =>
            {
                var payouts = _db.Games
                    .Where(g => g.Results.Any(r => r.PlayerId == p.Id))
                    .Select(g => g.Results.First(r => r.PlayerId == p.Id).Payout)
                    .ToList();

                var cashes = payouts.Where(pay => pay > 0).ToList();
                decimal avgPayout = cashes.Count > 0 ? cashes.Average() : 0;
                decimal maxPayout = cashes.Count > 0 ? cashes.Max() : 0;

                return (p.Name, avgPayout, maxPayout, cashes.Count);
            })
            .Where(x => x.Count > 0)
            .OrderByDescending(x => x.avgPayout)
            .ToList();
    }
}
