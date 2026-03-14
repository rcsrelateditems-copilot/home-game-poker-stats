using PokerStats.Models;
using PokerStats.Services;
using PokerStats.WinForms.Services;

namespace PokerStats.WinForms.Tabs;

/// <summary>
/// Shows detailed statistics for a selected player, including stat cards
/// and a head-to-head record against every opponent.
/// </summary>
public class PlayerStatsTab : UserControl
{
    private readonly ComboBox _playerPicker = new();
    private readonly FlowLayoutPanel _statsPanel = new();
    private readonly DataGridView _h2hGrid = new();
    private readonly Label _noDataLabel = new();
    private DataService? _dataService;

    public PlayerStatsTab()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(8);

        var title = new Label
        {
            Text = "Player Statistics",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 4, 0, 4),
        };

        var pickerPanel = new Panel { Height = 40, Dock = DockStyle.Top };
        var pickerLabel = new Label
        {
            Text = "Select player:",
            AutoSize = true,
            Location = new Point(0, 10),
        };
        _playerPicker.DropDownStyle = ComboBoxStyle.DropDownList;
        _playerPicker.Location = new Point(90, 6);
        _playerPicker.Width = 200;
        _playerPicker.SelectedIndexChanged += (_, _) => UpdateStats();
        pickerPanel.Controls.AddRange([pickerLabel, _playerPicker]);

        _statsPanel.Dock = DockStyle.Top;
        _statsPanel.AutoSize = true;
        _statsPanel.FlowDirection = FlowDirection.LeftToRight;
        _statsPanel.WrapContents = true;
        _statsPanel.Padding = new Padding(0, 4, 0, 8);

        var h2hLabel = new Label
        {
            Text = "Head-to-Head Records",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 8, 0, 4),
        };

        ConfigureH2HGrid();

        _noDataLabel.Text = "Select a player above to view stats.";
        _noDataLabel.ForeColor = Color.Gray;
        _noDataLabel.AutoSize = true;
        _noDataLabel.Dock = DockStyle.Top;

        var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scrollPanel.Controls.Add(_h2hGrid);
        scrollPanel.Controls.Add(h2hLabel);
        scrollPanel.Controls.Add(_noDataLabel);
        scrollPanel.Controls.Add(_statsPanel);
        scrollPanel.Controls.Add(pickerPanel);
        scrollPanel.Controls.Add(title);

        Controls.Add(scrollPanel);
    }

    private void ConfigureH2HGrid()
    {
        _h2hGrid.Dock = DockStyle.Top;
        _h2hGrid.Height = 200;
        _h2hGrid.ReadOnly = true;
        _h2hGrid.AllowUserToAddRows = false;
        _h2hGrid.AllowUserToDeleteRows = false;
        _h2hGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _h2hGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        _h2hGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
        _h2hGrid.Font = new Font("Segoe UI", 9);
        _h2hGrid.RowHeadersVisible = false;
    }

    /// <summary>Reloads player list and refreshes current selection from the database.</summary>
    public void Refresh(DataService dataService)
    {
        _dataService = dataService;
        var db = dataService.Load();
        var selected = _playerPicker.SelectedItem as string;

        _playerPicker.Items.Clear();
        foreach (var p in db.Players.OrderBy(p => p.Name))
            _playerPicker.Items.Add(p.Name);

        if (selected != null && _playerPicker.Items.Contains(selected))
            _playerPicker.SelectedItem = selected;
        else if (_playerPicker.Items.Count > 0)
            _playerPicker.SelectedIndex = 0;

        UpdateStats();
    }

    private void UpdateStats()
    {
        _statsPanel.Controls.Clear();
        _h2hGrid.Columns.Clear();
        _h2hGrid.Rows.Clear();
        _noDataLabel.Visible = true;

        if (_dataService == null || _playerPicker.SelectedItem == null) return;

        var db = _dataService.Load();
        var playerName = _playerPicker.SelectedItem.ToString()!;
        var player = db.Players.FirstOrDefault(p => p.Name == playerName);
        if (player == null) return;

        var svc = new StatsService(db);
        var stats = svc.GetPlayerStats(player.Id);
        var dist = svc.GetPlacementDistribution(player.Id);

        _noDataLabel.Visible = false;

        AddStatGroup("Performance",
            ("Games Played", stats.GamesPlayed.ToString()),
            ("Wins (1st)", $"{stats.Wins}"),
            ("Win Rate", $"{stats.WinRate:F1}%"),
            ("Top 3", $"{stats.TopThree}  ({stats.TopThreeRate:F1}%)"),
            ("In the Money", $"{stats.InTheMoney}  ({stats.InTheMoneyRate:F1}%)"),
            ("Avg Placement", $"{stats.AvgPlacement:F2}"),
            ("Best Finish", $"#{dist.Min}"),
            ("Worst Finish", $"#{dist.Max}"),
            ("Placement StdDev", $"{dist.StdDev:F2}")
        );

        AddStatGroup("Earnings",
            ("Total Buy-In", $"${stats.TotalBuyIn:F2}"),
            ("Total Payout", $"${stats.TotalPayout:F2}"),
            ("Net Profit", $"${stats.NetProfit:F2}"),
            ("ROI", $"{stats.Roi:F1}%"),
            ("Avg Payout/Game", $"${stats.AvgPayoutPerGame:F2}"),
            ("Avg Payout/Cash", $"${stats.AvgPayoutPerCash:F2}"),
            ("Biggest Payout", $"${stats.BiggestPayout:F2}")
        );

        AddStatGroup("Knockouts & Streaks",
            ("Knockouts Dealt", stats.TotalKnockouts.ToString()),
            ("Times Eliminated", stats.TimesKnockedOut.ToString()),
            ("K/D Ratio", $"{stats.KdRatio:F2}"),
            ("Longest Win Streak", stats.LongestWinStreak.ToString()),
            ("Current Win Streak", stats.CurrentWinStreak.ToString()),
            ("Longest Top-3 Streak", stats.LongestTopThreeStreak.ToString()),
            ("Current Top-3 Streak", stats.CurrentTopThreeStreak.ToString())
        );

        var h2h = svc.GetHeadToHead(player.Id);
        _h2hGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Opponent" });
        _h2hGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Games" });
        _h2hGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Wins" });
        _h2hGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Win %" });
        _h2hGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "KOs Dealt" });
        _h2hGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "KOs Received" });
        foreach (var r in h2h)
            _h2hGrid.Rows.Add(r.OpponentName, r.GamesAgainst, r.WinsAgainst,
                $"{r.WinRateAgainst:F1}%", r.KnockoutsDealtToOpponent, r.KnockoutsReceivedFromOpponent);

        _h2hGrid.AutoResizeColumns();
    }

    private void AddStatGroup(string groupTitle, params (string Label, string Value)[] stats)
    {
        var group = new GroupBox
        {
            Text = groupTitle,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 12, 8),
        };

        var table = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(4),
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        foreach (var (lbl, val) in stats)
        {
            table.Controls.Add(new Label
            {
                Text = lbl + ":",
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray,
                Margin = new Padding(2, 2, 8, 2),
            });
            table.Controls.Add(new Label
            {
                Text = val,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(2),
            });
        }

        group.Controls.Add(table);
        _statsPanel.Controls.Add(group);
    }
}
