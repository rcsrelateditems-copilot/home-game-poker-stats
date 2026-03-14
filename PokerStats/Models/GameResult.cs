namespace PokerStats.Models;

public class GameResult
{
    /// <summary>Player ID.</summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>Final finishing position (1 = first place / winner).</summary>
    public int Placement { get; set; }

    /// <summary>Amount paid out to this player. Zero for players who did not cash.</summary>
    public decimal Payout { get; set; }
}
