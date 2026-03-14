namespace PokerStats.Models;

/// <summary>
/// Represents a single poker tournament session.
/// </summary>
public class Game
{
    /// <summary>Unique identifier for this game.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Date the tournament was played.</summary>
    public DateTime Date { get; set; } = DateTime.UtcNow;

    /// <summary>Optional description or venue name.</summary>
    public string? Notes { get; set; }

    /// <summary>Buy-in amount per player in dollars (or the table's currency).</summary>
    public decimal BuyIn { get; set; }

    /// <summary>Final results for every player who participated in this game.</summary>
    public List<GameResult> Results { get; set; } = [];

    /// <summary>Knockout events that occurred during this game.</summary>
    public List<KnockoutRecord> Knockouts { get; set; } = [];
}
