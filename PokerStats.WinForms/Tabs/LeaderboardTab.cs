using PokerStats.Models;
using PokerStats.Services;
using PokerStats.WinForms.Services;

namespace PokerStats.WinForms.Tabs;

/// <summary>
/// Shows the full player leaderboard ranked by wins and net profit.
/// </summary>
public class LeaderboardTab : UserControl
{
    private readonly DataGridView _grid = new();

    public LeaderboardTab()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(8);

        var label = new Label
        {
            Text = "Player Leaderboard",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 4, 0, 8),
        };

        ConfigureGrid();

        Controls.Add(_grid);
        Controls.Add(label);
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
        _grid.Font = new Font("Segoe UI", 9);
        _grid.RowHeadersVisible = false;
    }

    /// <summary>Reloads leaderboard data from the database.</summary>
    public void Refresh(DataService dataService)
    {
        var db = dataService.Load();
        var svc = new StatsService(db);
        var leaderboard = svc.GetLeaderboard();

        _grid.Columns.Clear();
        _grid.Rows.Clear();

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rank", HeaderText = "#", Width = 40 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Player" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Games", HeaderText = "Games" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Wins", HeaderText = "Wins" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "WinRate", HeaderText = "Win %" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Top3", HeaderText = "Top 3 %" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ITM", HeaderText = "ITM %" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "AvgPlace", HeaderText = "Avg Place" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Net", HeaderText = "Net Profit" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ROI", HeaderText = "ROI %" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "KD", HeaderText = "K/D" });

        int rank = 1;
        foreach (var s in leaderboard)
        {
            _grid.Rows.Add(
                rank++,
                s.PlayerName,
                s.GamesPlayed,
                s.Wins,
                $"{s.WinRate:F1}",
                $"{s.TopThreeRate:F1}",
                $"{s.InTheMoneyRate:F1}",
                $"{s.AvgPlacement:F2}",
                $"${s.NetProfit:F2}",
                $"{s.Roi:F1}",
                $"{s.KdRatio:F2}"
            );
        }

        _grid.AutoResizeColumns();
    }
}
