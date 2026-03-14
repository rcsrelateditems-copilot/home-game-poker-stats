namespace PokerStats.Models;

/// <summary>
/// Describes a single knockout event — one player eliminating another.
/// </summary>
public class KnockoutRecord
{
    /// <summary>Identifier of the player who was knocked out.</summary>
    public string EliminatedPlayerId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the player who delivered the knockout.
    /// <see langword="null"/> when the eliminator is unknown or was not tracked.
    /// </summary>
    public string? KnockedOutByPlayerId { get; set; }
}
