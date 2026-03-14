# home-game-poker-stats

A .NET console application for tracking home poker tournament statistics over time. Record every game's results including placements, payouts, and knockouts, then analyze performance with rich statistical reports.

## Features

- **Player management** – register players once, reuse across all games
- **Game recording** – interactive wizard to enter finishing order, payouts, and knockout events
- **Comprehensive statistics:**
  - Overall leaderboard (wins, win%, top-3%, ITM%, avg placement, net profit, ROI)
  - Per-player deep-dive (win streaks, placement distribution, K/D ratio)
  - Head-to-head records against every opponent
  - Knockout leaderboard (kills, deaths, K/D ratio)
  - Payout statistics (average cash, biggest cash)
  - Cumulative earnings trends over time
- **Persistent JSON storage** – all data saved to a local file automatically

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Getting Started

```bash
# Clone the repository
git clone <repo-url>
cd home-game-poker-stats

# Build
dotnet build PokerStats.sln

# Run tests
dotnet test PokerStats.sln

# Run the application
dotnet run --project PokerStats -- help
```

## Usage

### Register Players

```bash
dotnet run --project PokerStats -- add-player Alice
dotnet run --project PokerStats -- add-player Bob
dotnet run --project PokerStats -- add-player Charlie
dotnet run --project PokerStats -- list-players
```

### Record a Game

```bash
dotnet run --project PokerStats -- add-game
```

The interactive wizard will ask for:
1. Game date
2. Buy-in amount per player
3. Optional notes/venue
4. Which players participated
5. Finishing order (1st through last)
6. Payouts for top-3 finishers
7. Optional knockout tracking (who knocked out whom)

### View Game History

```bash
dotnet run --project PokerStats -- list-games
```

### Statistics Commands

| Command | Description |
|---|---|
| `stats` | Show all statistics at once |
| `stats leaderboard` | Overall standings ranked by wins and profit |
| `stats knockouts` | Knockout leaderboard (kills / deaths / K/D ratio) |
| `stats earnings` | Cumulative net profit trend per player |
| `stats payouts` | Average and biggest cash per player |
| `stats player <name>` | Full individual breakdown for one player |

#### Example: Per-Player Stats

```
=== Stats for Alice ===

--- Performance ---
  Games played:          10
  Wins (1st place):      3  (30.0%)
  Top 3 finishes:        7  (70.0%)
  In the money (payout): 6  (60.0%)
  Avg placement:         2.40
  Placement std dev:     1.02
  Best finish:           #1
  Worst finish:          #5

--- Earnings ---
  Total buy-in spent:    $200.00
  Total payout earned:   $320.00
  Net profit/loss:       $120.00
  ROI:                   60.0%

--- Knockouts ---
  Total knockouts dealt: 14
  Times knocked out:     7
  K/D ratio:             2.00

--- Streaks ---
  Longest win streak:       2
  Longest top-3 streak:     4
  Current win streak:       1
  Current top-3 streak:     3

--- Head-to-Head ---
  Opponent             Games   Wins    Win%     KO Dealt   KO Recv
  ...
```

## Data Storage

All data is stored in a JSON file at:

- **Linux/macOS:** `~/.config/PokerStats/data.json`
- **Windows:** `%APPDATA%\PokerStats\data.json`

You can back up or share this file to move your stats between machines.

## Project Structure

```
PokerStats/
  Models/
    Player.cs          – Player entity
    Game.cs            – Tournament game entity
    GameResult.cs      – Per-player placement and payout
    KnockoutRecord.cs  – Knockout event (eliminated by whom)
    Database.cs        – Root data container
  Services/
    DataService.cs     – JSON load/save persistence
    StatsService.cs    – All statistical calculations
  Commands/
    PlayerCommands.cs  – add-player, list-players
    GameCommands.cs    – add-game, list-games
    StatsCommands.cs   – stats sub-commands
  Program.cs           – CLI entry point / command routing

PokerStats.Tests/
  StatsServiceTests.cs – Unit tests for statistics logic
  DataServiceTests.cs  – Unit tests for persistence layer
```

