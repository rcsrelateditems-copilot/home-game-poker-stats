using System.Text.Json;
using PokerStats.Models;

namespace PokerStats.WinForms.Services;

/// <summary>
/// Loads and saves the poker league database to a JSON file.
/// The default path is %APPDATA%\PokerStats\data.json — the same file used
/// by the PokerStats console application.
/// </summary>
public class DataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _filePath;

    /// <summary>
    /// Initialises the service. If <paramref name="filePath"/> is omitted the
    /// platform default path (%APPDATA%\PokerStats\data.json) is used.
    /// </summary>
    public DataService(string? filePath = null)
    {
        _filePath = filePath ?? GetDefaultDataPath();
    }

    private static string GetDefaultDataPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "PokerStats");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "data.json");
    }

    /// <summary>Loads the database from disk. Returns an empty database if the file does not exist.</summary>
    public Database Load()
    {
        if (!File.Exists(_filePath))
            return new Database();

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<Database>(json, JsonOptions) ?? new Database();
        }
        catch (JsonException)
        {
            return new Database();
        }
    }

    /// <summary>Persists the database to disk.</summary>
    public void Save(Database db)
    {
        var json = JsonSerializer.Serialize(db, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    /// <summary>Full path to the data file being used.</summary>
    public string FilePath => _filePath;
}
