namespace PokerStats.Models;

/// <summary>
/// Records a single player's finishing position and payout for one game.
/// </summary>
public class GameResult
{
    /// <summary>Identifier of the player this result belongs to.</summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>Final finishing position — 1 is first place (the winner).</summary>
    public int Placement { get; set; }

    /// <summary>
    /// Amount paid out to this player. Zero means the player did not cash
    /// (finished outside the paid positions).
    /// </summary>
    public decimal Payout { get; set; }
}
