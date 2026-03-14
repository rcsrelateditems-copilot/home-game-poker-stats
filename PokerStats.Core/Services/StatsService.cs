using PokerStats.Models;

namespace PokerStats.Services;

/// <summary>
/// Comprehensive statistics for a single player across all recorded games.
/// </summary>
/// <param name="PlayerId">Unique identifier of the player.</param>
/// <param name="PlayerName">Display name of the player.</param>
/// <param name="GamesPlayed">Total number of games the player participated in.</param>
/// <param name="Wins">Number of first-place finishes.</param>
/// <param name="TopThree">Number of top-three finishes.</param>
/// <param name="InTheMoney">Number of games where the player received a cash payout.</param>
/// <param name="WinRate">Percentage of games won (0–100).</param>
/// <param name="TopThreeRate">Percentage of games finishing in the top three (0–100).</param>
/// <param name="InTheMoneyRate">Percentage of games where a cash payout was received (0–100).</param>
/// <param name="AvgPlacement">Mean finishing position across all games (lower is better).</param>
/// <param name="TotalPayout">Sum of all cash payouts received.</param>
/// <param name="AvgPayoutPerGame">Average cash payout per game played (including non-cashing games).</param>
/// <param name="AvgPayoutPerCash">Average cash payout for games where the player finished in the money.</param>
/// <param name="BiggestPayout">Largest single-game payout received.</param>
/// <param name="TotalBuyIn">Total buy-in cost across all games.</param>
/// <param name="NetProfit">Total payout minus total buy-in (positive means profitable).</param>
/// <param name="Roi">Return on investment as a percentage: (NetProfit / TotalBuyIn) × 100.</param>
/// <param name="TotalKnockouts">Total number of opponents this player has knocked out.</param>
/// <param name="TimesKnockedOut">Total number of times this player was knocked out by an opponent.</param>
/// <param name="KdRatio">Kill/death ratio: knockouts dealt divided by times knocked out.</param>
/// <param name="LongestWinStreak">Longest consecutive win streak recorded for this player.</param>
/// <param name="LongestTopThreeStreak">Longest consecutive top-three streak recorded for this player.</param>
/// <param name="CurrentWinStreak">Current active win streak (games at the end of the record).</param>
/// <param name="CurrentTopThreeStreak">Current active top-three streak (games at the end of the record).</param>
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
    decimal AvgPayoutPerGame,
    decimal AvgPayoutPerCash,
    decimal BiggestPayout,
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

/// <summary>
/// Head-to-head record between two players across all shared games.
/// </summary>
/// <param name="OpponentId">Unique identifier of the opponent.</param>
/// <param name="OpponentName">Display name of the opponent.</param>
/// <param name="GamesAgainst">Number of games both players appeared in together.</param>
/// <param name="WinsAgainst">Number of those shared games where the primary player finished ahead of this opponent.</param>
/// <param name="KnockoutsDealtToOpponent">Number of times the primary player knocked out this opponent.</param>
/// <param name="KnockoutsReceivedFromOpponent">Number of times this opponent knocked out the primary player.</param>
/// <param name="WinRateAgainst">Win rate against this opponent as a percentage (0–100).</param>
public record HeadToHeadRecord(
    string OpponentId,
    string OpponentName,
    int GamesAgainst,
    int WinsAgainst,
    int KnockoutsDealtToOpponent,
    int KnockoutsReceivedFromOpponent,
    double WinRateAgainst
);

/// <summary>
/// A single player's entry on the knockout leaderboard.
/// </summary>
/// <param name="PlayerId">Unique identifier of the player.</param>
/// <param name="PlayerName">Display name of the player.</param>
/// <param name="TotalKnockouts">Total number of opponents knocked out.</param>
/// <param name="TimesEliminated">Total number of times this player was eliminated.</param>
/// <param name="KdRatio">Kill/death ratio: knockouts dealt divided by times eliminated.</param>
public record KnockoutLeaderEntry(
    string PlayerId,
    string PlayerName,
    int TotalKnockouts,
    int TimesEliminated,
    double KdRatio
);

/// <summary>
/// Placement distribution statistics for a single player.
/// </summary>
/// <param name="Mean">Mean (average) finishing position across all games.</param>
/// <param name="StdDev">Standard deviation of finishing positions (spread from the mean).</param>
/// <param name="Min">Best (lowest) finishing position recorded.</param>
/// <param name="Max">Worst (highest) finishing position recorded.</param>
public record PlacementDistribution(double Mean, double StdDev, int Min, int Max);

/// <summary>
/// A single data point in a player's cumulative earnings timeline.
/// </summary>
/// <param name="Date">The date of the game.</param>
/// <param name="NetProfitToDate">Cumulative net profit (payouts minus buy-ins) up to and including this game.</param>
public record EarningsTrendPoint(DateTime Date, decimal NetProfitToDate);

/// <summary>
/// Payout summary for a single player.
/// </summary>
/// <param name="PlayerName">Display name of the player.</param>
/// <param name="AvgPayout">Average payout across games where the player cashed.</param>
/// <param name="MaxPayout">Largest single-game cash payout received.</param>
/// <param name="CashCount">Number of games where the player received a payout.</param>
public record PayoutStat(string PlayerName, decimal AvgPayout, decimal MaxPayout, int CashCount);

/// <summary>
/// Calculates statistics for players and games stored in a <see cref="Database"/>.
/// Construct a new instance whenever the underlying database changes.
/// </summary>
public class StatsService
{
    private readonly Database _db;

    /// <summary>
    /// Initialises the service with the database to query.
    /// </summary>
    /// <param name="db">The league database containing players and games.</param>
    public StatsService(Database db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns a complete statistical profile for the specified player.
    /// </summary>
    /// <param name="playerId">The <see cref="Player.Id"/> to look up.</param>
    /// <returns>A <see cref="PlayerStats"/> record with all computed metrics.</returns>
    /// <exception cref="ArgumentException">Thrown when no player with <paramref name="playerId"/> exists.</exception>
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
        decimal avgPayoutPerGame = gamesPlayed > 0 ? totalPayout / gamesPlayed : 0;
        var cashPayouts = gamesWithPlayer
            .Select(g => g.Results.First(r => r.PlayerId == playerId).Payout)
            .Where(p => p > 0)
            .ToList();
        decimal avgPayoutPerCash = cashPayouts.Count > 0 ? cashPayouts.Average() : 0;
        decimal biggestPayout = cashPayouts.Count > 0 ? cashPayouts.Max() : 0;
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
            totalPayout, avgPayoutPerGame, avgPayoutPerCash, biggestPayout,
            totalBuyIn, netProfit, roi,
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

    /// <summary>
    /// Returns all players ranked by wins, then net profit, then top-three count,
    /// then average placement. Players with no recorded games are excluded.
    /// </summary>
    /// <returns>Ordered list of <see cref="PlayerStats"/>, best first.</returns>
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

    /// <summary>
    /// Returns the head-to-head record for <paramref name="playerId"/> against every
    /// opponent they have shared a game with, ordered by win rate (descending).
    /// </summary>
    /// <param name="playerId">The <see cref="Player.Id"/> of the primary player.</param>
    /// <returns>List of <see cref="HeadToHeadRecord"/>, one per opponent.</returns>
    /// <exception cref="ArgumentException">Thrown when no player with <paramref name="playerId"/> exists.</exception>
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

    /// <summary>
    /// Returns every player who has at least one knockout or elimination recorded,
    /// ranked by total knockouts then kill/death ratio (descending).
    /// </summary>
    /// <returns>Ordered list of <see cref="KnockoutLeaderEntry"/>.</returns>
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

    /// <summary>
    /// Returns the cumulative net profit timeline for every player, keyed by player name.
    /// Each entry in the list represents one game in chronological order.
    /// Players with no recorded games are excluded.
    /// </summary>
    /// <returns>
    /// Dictionary mapping player name → ordered list of <see cref="EarningsTrendPoint"/>.
    /// </returns>
    public Dictionary<string, List<EarningsTrendPoint>> GetEarningsTrends()
    {
        var trends = new Dictionary<string, List<EarningsTrendPoint>>();

        foreach (var player in _db.Players)
        {
            var gamesWithPlayer = _db.Games
                .Where(g => g.Results.Any(r => r.PlayerId == player.Id))
                .OrderBy(g => g.Date)
                .ToList();

            if (gamesWithPlayer.Count == 0) continue;

            decimal running = 0;
            var series = new List<EarningsTrendPoint>();

            foreach (var game in gamesWithPlayer)
            {
                var result = game.Results.First(r => r.PlayerId == player.Id);
                running += result.Payout - game.BuyIn;
                series.Add(new EarningsTrendPoint(game.Date, running));
            }

            trends[player.Name] = series;
        }

        return trends;
    }

    /// <summary>
    /// Returns the placement distribution statistics for the specified player.
    /// Returns a zeroed-out <see cref="PlacementDistribution"/> if the player has no games.
    /// </summary>
    /// <param name="playerId">The <see cref="Player.Id"/> to analyse.</param>
    /// <returns>
    /// A <see cref="PlacementDistribution"/> containing mean, standard deviation,
    /// best placement, and worst placement.
    /// </returns>
    public PlacementDistribution GetPlacementDistribution(string playerId)
    {
        var placements = _db.Games
            .Where(g => g.Results.Any(r => r.PlayerId == playerId))
            .Select(g => g.Results.First(r => r.PlayerId == playerId).Placement)
            .ToList();

        if (placements.Count == 0)
            return new PlacementDistribution(0, 0, 0, 0);

        double mean = placements.Average();
        double variance = placements.Average(p => Math.Pow(p - mean, 2));
        double stdDev = Math.Sqrt(variance);

        return new PlacementDistribution(mean, stdDev, placements.Min(), placements.Max());
    }

    /// <summary>
    /// Returns payout statistics for every player who has cashed at least once,
    /// ordered by average payout (descending).
    /// </summary>
    /// <returns>List of <see cref="PayoutStat"/>, one per qualifying player.</returns>
    public List<PayoutStat> GetPayoutStats()
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

                return new PayoutStat(p.Name, avgPayout, maxPayout, cashes.Count);
            })
            .Where(x => x.CashCount > 0)
            .OrderByDescending(x => x.AvgPayout)
            .ToList();
    }
}
