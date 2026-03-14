using System.Text.Json;
using PokerStats.Models;

namespace PokerStats.Services;

public class DataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _filePath;

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
            Console.Error.WriteLine($"Warning: Could not parse data file at {_filePath}. Starting fresh.");
            return new Database();
        }
    }

    public void Save(Database db)
    {
        var json = JsonSerializer.Serialize(db, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    public string FilePath => _filePath;
}
