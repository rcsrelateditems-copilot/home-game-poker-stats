using PokerStats.Models;
using PokerStats.Services;

namespace PokerStats.Commands;

public static class PlayerCommands
{
    public static void AddPlayer(DataService dataService, string name)
    {
        var db = dataService.Load();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.Error.WriteLine("Error: Player name cannot be empty.");
            return;
        }

        if (db.Players.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            Console.Error.WriteLine($"Error: A player named '{name}' already exists.");
            return;
        }

        var player = new Player { Name = name.Trim() };
        db.Players.Add(player);
        dataService.Save(db);

        Console.WriteLine($"Player '{player.Name}' added (ID: {player.Id}).");
    }

    public static void ListPlayers(DataService dataService)
    {
        var db = dataService.Load();

        if (db.Players.Count == 0)
        {
            Console.WriteLine("No players registered yet. Use 'add-player <name>' to add players.");
            return;
        }

        Console.WriteLine($"{"#",-4} {"Name",-30} {"ID",-36} {"Joined",-12}");
        Console.WriteLine(new string('-', 86));

        for (int i = 0; i < db.Players.Count; i++)
        {
            var p = db.Players[i];
            Console.WriteLine($"{i + 1,-4} {p.Name,-30} {p.Id,-36} {p.CreatedAt:yyyy-MM-dd}");
        }
    }
}
