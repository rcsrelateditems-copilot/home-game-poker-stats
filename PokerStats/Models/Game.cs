namespace PokerStats.Models;

public class Game
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Date the tournament was played.</summary>
    public DateTime Date { get; set; } = DateTime.UtcNow;

    /// <summary>Optional description or venue name.</summary>
    public string? Notes { get; set; }

    /// <summary>Buy-in amount per player.</summary>
    public decimal BuyIn { get; set; }

    /// <summary>Ordered list of final results for all players in this game.</summary>
    public List<GameResult> Results { get; set; } = [];

    /// <summary>List of knockout events that occurred during this game.</summary>
    public List<KnockoutRecord> Knockouts { get; set; } = [];
}
