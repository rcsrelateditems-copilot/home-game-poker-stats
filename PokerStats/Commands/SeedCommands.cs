using PokerStats.Models;
using PokerStats.Services;

namespace PokerStats.Commands;

public static class SeedCommands
{
    private static readonly string[] PlayerNames =
    [
        "Alice", "Bob", "Charlie", "Dave", "Eve", "Frank", "Grace", "Henry"
    ];

    public static void SeedData(DataService dataService)
    {
        var db = dataService.Load();

        if (db.Players.Count > 0 || db.Games.Count > 0)
        {
            Console.Error.WriteLine("Error: Database already contains data. Seed only works on an empty database.");
            Console.Error.WriteLine($"Data file: {dataService.FilePath}");
            return;
        }

        var rng = new Random(42);

        var players = PlayerNames
            .Select(name => new Player { Id = name.ToLowerInvariant(), Name = name })
            .ToList();
        db.Players.AddRange(players);

        var startDate = new DateTime(2023, 1, 7);

        for (int i = 0; i < 100; i++)
        {
            var gameDate = startDate.AddDays(i * 7 + rng.Next(0, 3));
            decimal buyIn = (rng.Next(2, 6) * 10); // $20, $30, $40, or $50

            // Pick 5–8 random players for this game
            int playerCount = rng.Next(5, 9);
            var gamePlayers = players.OrderBy(_ => rng.Next()).Take(playerCount).ToList();

            decimal prizePool = buyIn * gamePlayers.Count;
            var orderedPlayers = gamePlayers.OrderBy(_ => rng.Next()).ToList();

            var results = orderedPlayers.Select((p, idx) =>
            {
                int placement = idx + 1;
                decimal payout = placement switch
                {
                    1 => Math.Round(prizePool * 0.50m, 2),
                    2 => Math.Round(prizePool * 0.30m, 2),
                    3 => Math.Round(prizePool * 0.20m, 2),
                    _ => 0m
                };
                return new GameResult { PlayerId = p.Id, Placement = placement, Payout = payout };
            }).ToList();

            // Generate knockouts (70% chance of having knockout records)
            var knockouts = new List<KnockoutRecord>();
            if (rng.Next(0, 10) < 7)
            {
                // Players eliminated in reverse finishing order (last place first)
                var eliminationOrder = orderedPlayers.AsEnumerable().Reverse().ToList();
                foreach (var eliminated in eliminationOrder.Skip(1)) // Winner never gets knocked out
                {
                    // 80% chance knockout is attributed to another player still in the game
                    string? knockedOutBy = null;
                    if (rng.Next(0, 10) < 8)
                    {
                        var survivorsAtTime = orderedPlayers
                            .Where(p => orderedPlayers.IndexOf(p) < orderedPlayers.IndexOf(eliminated))
                            .ToList();
                        if (survivorsAtTime.Count > 0)
                            knockedOutBy = survivorsAtTime[rng.Next(0, survivorsAtTime.Count)].Id;
                    }
                    knockouts.Add(new KnockoutRecord
                    {
                        EliminatedPlayerId = eliminated.Id,
                        KnockedOutByPlayerId = knockedOutBy
                    });
                }
            }

            db.Games.Add(new Game
            {
                Id = Guid.NewGuid().ToString(),
                Date = gameDate,
                BuyIn = buyIn,
                Results = results,
                Knockouts = knockouts
            });
        }

        dataService.Save(db);

        Console.WriteLine($"Seeded {players.Count} players and {db.Games.Count} games successfully.");
        Console.WriteLine($"Data written to: {dataService.FilePath}");
    }
}
