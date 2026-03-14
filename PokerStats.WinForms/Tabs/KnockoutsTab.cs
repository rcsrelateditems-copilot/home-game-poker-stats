using PokerStats.Services;
using PokerStats.WinForms.Services;

namespace PokerStats.WinForms.Tabs;

/// <summary>
/// Shows the knockout leaderboard ranked by total knockouts and K/D ratio.
/// </summary>
public class KnockoutsTab : UserControl
{
    private readonly DataGridView _grid = new();

    public KnockoutsTab()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(8);

        var label = new Label
        {
            Text = "Knockout Leaderboard",
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
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 248, 240);
        _grid.Font = new Font("Segoe UI", 9);
        _grid.RowHeadersVisible = false;
    }

    /// <summary>Reloads knockout data from the database.</summary>
    public void Refresh(DataService dataService)
    {
        var db = dataService.Load();
        var svc = new StatsService(db);
        var leaders = svc.GetKnockoutLeaderboard();

        _grid.Columns.Clear();
        _grid.Rows.Clear();

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "#", Width = 40 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Player" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Knockouts" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Times Eliminated" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "K/D Ratio" });

        int rank = 1;
        foreach (var e in leaders)
            _grid.Rows.Add(rank++, e.PlayerName, e.TotalKnockouts, e.TimesEliminated, $"{e.KdRatio:F2}");

        _grid.AutoResizeColumns();
    }
}
