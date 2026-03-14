using PokerStats.Models;
using PokerStats.Services;

namespace PokerStats.Commands;

public static class GameCommands
{
    /// <summary>
    /// Interactive game entry wizard.
    /// </summary>
    public static void AddGame(DataService dataService)
    {
        var db = dataService.Load();

        if (db.Players.Count < 2)
        {
            Console.Error.WriteLine("Error: You need at least 2 players registered before adding a game.");
            Console.Error.WriteLine("Use 'add-player <name>' to register players.");
            return;
        }

        Console.WriteLine("=== Add New Tournament Game ===");
        Console.WriteLine();

        // Game date
        Console.Write("Game date (YYYY-MM-DD) [today]: ");
        var dateInput = Console.ReadLine()?.Trim();
        DateTime gameDate;
        if (string.IsNullOrEmpty(dateInput))
            gameDate = DateTime.Today;
        else if (!DateTime.TryParse(dateInput, out gameDate))
        {
            Console.Error.WriteLine("Error: Invalid date format.");
            return;
        }

        // Buy-in
        Console.Write("Buy-in amount per player (e.g. 20): $");
        var buyInInput = Console.ReadLine()?.Trim();
        if (!decimal.TryParse(buyInInput, out decimal buyIn) || buyIn < 0)
        {
            Console.Error.WriteLine("Error: Invalid buy-in amount.");
            return;
        }

        // Notes
        Console.Write("Notes/venue (optional): ");
        var notes = Console.ReadLine()?.Trim();

        // Show player list
        Console.WriteLine();
        Console.WriteLine("Registered players:");
        for (int i = 0; i < db.Players.Count; i++)
            Console.WriteLine($"  {i + 1}. {db.Players[i].Name}");

        // Select participants
        Console.WriteLine();
        Console.Write("Enter player numbers who participated (comma-separated, e.g. 1,3,4): ");
        var participantInput = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(participantInput))
        {
            Console.Error.WriteLine("Error: No participants entered.");
            return;
        }

        var participants = new List<Player>();
        foreach (var token in participantInput.Split(','))
        {
            if (int.TryParse(token.Trim(), out int idx) && idx >= 1 && idx <= db.Players.Count)
                participants.Add(db.Players[idx - 1]);
            else
            {
                Console.Error.WriteLine($"Error: Invalid player number '{token.Trim()}'.");
                return;
            }
        }

        if (participants.Count < 2)
        {
            Console.Error.WriteLine("Error: A game needs at least 2 participants.");
            return;
        }

        // Remove duplicates while preserving order
        participants = participants.Distinct().ToList();

        // Enter finishing order and payouts
        Console.WriteLine();
        Console.WriteLine("Enter finishing order (1st place first):");
        var results = new List<GameResult>();
        var remaining = new List<Player>(participants);

        for (int place = 1; place <= participants.Count; place++)
        {
            Console.WriteLine($"\nPlace #{place}:");
            for (int i = 0; i < remaining.Count; i++)
                Console.WriteLine($"  {i + 1}. {remaining[i].Name}");

            Console.Write($"  Who finished #{place}? ");
            var pickInput = Console.ReadLine()?.Trim();
            if (!int.TryParse(pickInput, out int pick) || pick < 1 || pick > remaining.Count)
            {
                Console.Error.WriteLine("Error: Invalid selection.");
                return;
            }

            var selectedPlayer = remaining[pick - 1];
            decimal payout = 0;

            if (place <= 3)
            {
                Console.Write($"  Payout for {selectedPlayer.Name} (0 if none): $");
                var payoutInput = Console.ReadLine()?.Trim();
                if (!decimal.TryParse(payoutInput, out payout) || payout < 0)
                {
                    Console.Error.WriteLine("Error: Invalid payout amount.");
                    return;
                }
            }

            results.Add(new GameResult
            {
                PlayerId = selectedPlayer.Id,
                Placement = place,
                Payout = payout
            });

            remaining.RemoveAt(pick - 1);
        }

        // Enter knockouts
        Console.WriteLine();
        Console.WriteLine("=== Record Knockouts (optional) ===");
        Console.WriteLine("For each player knocked out, specify who eliminated them.");
        Console.WriteLine("Press Enter to skip knockout tracking for this game.");
        Console.WriteLine();

        var knockouts = new List<KnockoutRecord>();

        // Players who were knocked out = everyone except 1st place
        var eliminatedPlayers = results
            .Where(r => r.Placement > 1)
            .OrderByDescending(r => r.Placement) // start with last place (first knocked out)
            .ToList();

        Console.Write("Track knockouts? (y/n) [n]: ");
        var trackKo = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (trackKo == "y" || trackKo == "yes")
        {
            foreach (var eliminatedResult in eliminatedPlayers)
            {
                var eliminated = participants.First(p => p.Id == eliminatedResult.PlayerId);
                Console.WriteLine($"\nWho knocked out {eliminated.Name}?");

                var killers = participants.Where(p => p.Id != eliminated.Id).ToList();
                for (int i = 0; i < killers.Count; i++)
                    Console.WriteLine($"  {i + 1}. {killers[i].Name}");
                Console.WriteLine($"  {killers.Count + 1}. Unknown/skip");

                Console.Write("  Select: ");
                var koInput = Console.ReadLine()?.Trim();
                if (int.TryParse(koInput, out int koIdx) && koIdx >= 1 && koIdx <= killers.Count)
                {
                    knockouts.Add(new KnockoutRecord
                    {
                        EliminatedPlayerId = eliminated.Id,
                        KnockedOutByPlayerId = killers[koIdx - 1].Id
                    });
                }
                else
                {
                    knockouts.Add(new KnockoutRecord
                    {
                        EliminatedPlayerId = eliminated.Id,
                        KnockedOutByPlayerId = null
                    });
                }
            }
        }

        // Create and save game
        var game = new Game
        {
            Date = gameDate,
            BuyIn = buyIn,
            Notes = string.IsNullOrEmpty(notes) ? null : notes,
            Results = results,
            Knockouts = knockouts
        };

        db.Games.Add(game);
        dataService.Save(db);

        // Summary
        Console.WriteLine();
        Console.WriteLine("=== Game Saved ===");
        Console.WriteLine($"Date:   {game.Date:yyyy-MM-dd}");
        Console.WriteLine($"Buy-in: ${game.BuyIn:F2}");
        Console.WriteLine($"Players: {participants.Count}");
        Console.WriteLine();
        Console.WriteLine("Results:");
        foreach (var r in results)
        {
            var pName = participants.First(p => p.Id == r.PlayerId).Name;
            var payoutStr = r.Payout > 0 ? $"  Payout: ${r.Payout:F2}" : "";
            Console.WriteLine($"  #{r.Placement} {pName}{payoutStr}");
        }

        if (knockouts.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Knockouts:");
            foreach (var ko in knockouts)
            {
                var eliminated = participants.FirstOrDefault(p => p.Id == ko.EliminatedPlayerId)?.Name ?? ko.EliminatedPlayerId;
                var killer = ko.KnockedOutByPlayerId != null
                    ? participants.FirstOrDefault(p => p.Id == ko.KnockedOutByPlayerId)?.Name ?? ko.KnockedOutByPlayerId
                    : "Unknown";
                Console.WriteLine($"  {killer} knocked out {eliminated}");
            }
        }
    }

    public static void ListGames(DataService dataService)
    {
        var db = dataService.Load();

        if (db.Games.Count == 0)
        {
            Console.WriteLine("No games recorded yet. Use 'add-game' to record a tournament.");
            return;
        }

        var orderedGames = db.Games.OrderByDescending(g => g.Date).ToList();

        Console.WriteLine($"Total games recorded: {db.Games.Count}");
        Console.WriteLine();

        foreach (var game in orderedGames)
        {
            Console.WriteLine($"Game ID: {game.Id}");
            Console.WriteLine($"  Date:    {game.Date:yyyy-MM-dd}");
            Console.WriteLine($"  Buy-in:  ${game.BuyIn:F2}");
            if (!string.IsNullOrEmpty(game.Notes))
                Console.WriteLine($"  Notes:   {game.Notes}");
            Console.WriteLine($"  Players: {game.Results.Count}");

            var top3 = game.Results.Where(r => r.Placement <= 3).OrderBy(r => r.Placement).ToList();
            foreach (var r in top3)
            {
                var pName = db.Players.FirstOrDefault(p => p.Id == r.PlayerId)?.Name ?? r.PlayerId;
                var payoutStr = r.Payout > 0 ? $" (${r.Payout:F2})" : "";
                Console.WriteLine($"    #{r.Placement}: {pName}{payoutStr}");
            }

            if (game.Knockouts.Count > 0)
                Console.WriteLine($"  Knockouts tracked: {game.Knockouts.Count(k => k.KnockedOutByPlayerId != null)}");

            Console.WriteLine();
        }
    }
}
