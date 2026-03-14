namespace PokerStats.Models;

public class KnockoutRecord
{
    /// <summary>Player ID of the player who was knocked out.</summary>
    public string EliminatedPlayerId { get; set; } = string.Empty;

    /// <summary>Player ID of the player who delivered the knockout. Null if unknown/untracked.</summary>
    public string? KnockedOutByPlayerId { get; set; }
}
