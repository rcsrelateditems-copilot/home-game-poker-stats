using PokerStats.Services;
using PokerStats.WinForms.Services;

namespace PokerStats.WinForms.Tabs;

/// <summary>
/// Shows average and maximum payout statistics per player.
/// </summary>
public class PayoutStatsTab : UserControl
{
    private readonly DataGridView _grid = new();

    public PayoutStatsTab()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(8);

        var label = new Label
        {
            Text = "Payout Statistics",
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
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 255, 240);
        _grid.Font = new Font("Segoe UI", 9);
        _grid.RowHeadersVisible = false;
    }

    /// <summary>Reloads payout statistics from the database.</summary>
    public void Refresh(DataService dataService)
    {
        var db = dataService.Load();
        var svc = new StatsService(db);
        var stats = svc.GetPayoutStats();

        _grid.Columns.Clear();
        _grid.Rows.Clear();

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Player" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cashes" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Avg Cash Payout" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Biggest Payout" });

        foreach (var s in stats)
            _grid.Rows.Add(s.PlayerName, s.CashCount, $"${s.AvgPayout:F2}", $"${s.MaxPayout:F2}");

        _grid.AutoResizeColumns();
    }
}
