namespace PokerStats.Models;

/// <summary>
/// The root data container holding all players and game history.
/// Serialize and deserialize this type to persist the full league database.
/// </summary>
public class Database
{
    /// <summary>All registered players in the league.</summary>
    public List<Player> Players { get; set; } = [];

    /// <summary>All recorded tournament games, in any order.</summary>
    public List<Game> Games { get; set; } = [];
}
