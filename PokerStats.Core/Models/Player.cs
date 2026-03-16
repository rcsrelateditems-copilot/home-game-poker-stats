namespace PokerStats.Models;

/// <summary>
/// Represents a registered player in the poker league.
/// </summary>
public class Player
{
    /// <summary>Unique identifier for this player.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Display name of the player.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the player was registered.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
