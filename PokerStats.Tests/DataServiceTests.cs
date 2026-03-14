using System.Text.Json;
using PokerStats.Models;
using PokerStats.Services;
using Xunit;

namespace PokerStats.Tests;

public class DataServiceTests : IDisposable
{
    private readonly string _tempFile;

    public DataServiceTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"pokerstats_test_{Guid.NewGuid()}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    [Fact]
    public void Load_ReturnsEmptyDatabase_WhenFileDoesNotExist()
    {
        var svc = new DataService(_tempFile);
        var db = svc.Load();

        Assert.NotNull(db);
        Assert.Empty(db.Players);
        Assert.Empty(db.Games);
    }

    [Fact]
    public void SaveAndLoad_RoundTrips_PlayerData()
    {
        var svc = new DataService(_tempFile);
        var db = new Database();
        db.Players.Add(new Player { Id = "p1", Name = "Alice", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) });

        svc.Save(db);
        var loaded = svc.Load();

        Assert.Single(loaded.Players);
        Assert.Equal("Alice", loaded.Players[0].Name);
        Assert.Equal("p1", loaded.Players[0].Id);
    }

    [Fact]
    public void SaveAndLoad_RoundTrips_GameData()
    {
        var svc = new DataService(_tempFile);
        var db = new Database();
        db.Games.Add(new Game
        {
            Id = "g1",
            Date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            BuyIn = 25,
            Notes = "Saturday game",
            Results =
            [
                new GameResult { PlayerId = "p1", Placement = 1, Payout = 75 },
                new GameResult { PlayerId = "p2", Placement = 2, Payout = 25 },
            ],
            Knockouts =
            [
                new KnockoutRecord { EliminatedPlayerId = "p2", KnockedOutByPlayerId = "p1" }
            ]
        });

        svc.Save(db);
        var loaded = svc.Load();

        Assert.Single(loaded.Games);
        var game = loaded.Games[0];
        Assert.Equal("g1", game.Id);
        Assert.Equal(25m, game.BuyIn);
        Assert.Equal("Saturday game", game.Notes);
        Assert.Equal(2, game.Results.Count);
        Assert.Single(game.Knockouts);
        Assert.Equal("p1", game.Knockouts[0].KnockedOutByPlayerId);
    }

    [Fact]
    public void Load_ReturnsEmptyDatabase_WhenFileIsInvalidJson()
    {
        File.WriteAllText(_tempFile, "not valid json {{{");
        var svc = new DataService(_tempFile);
        var db = svc.Load();

        Assert.NotNull(db);
        Assert.Empty(db.Players);
        Assert.Empty(db.Games);
    }
}
