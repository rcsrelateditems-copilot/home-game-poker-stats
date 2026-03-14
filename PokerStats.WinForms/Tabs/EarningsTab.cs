using PokerStats.Services;
using PokerStats.WinForms.Services;

namespace PokerStats.WinForms.Tabs;

/// <summary>
/// Shows cumulative net profit over time for every player.
/// </summary>
public class EarningsTab : UserControl
{
    private readonly DataGridView _grid = new();

    public EarningsTab()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(8);

        var label = new Label
        {
            Text = "Earnings Trends (Cumulative Net Profit)",
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
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 255, 240);
        _grid.Font = new Font("Segoe UI", 9);
        _grid.RowHeadersVisible = false;
    }

    /// <summary>Reloads earnings trend data from the database.</summary>
    public void Refresh(DataService dataService)
    {
        var db = dataService.Load();
        var svc = new StatsService(db);
        var trends = svc.GetEarningsTrends();

        _grid.Columns.Clear();
        _grid.Rows.Clear();

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Player" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Net Profit To Date" });

        foreach (var (playerName, series) in trends.OrderBy(t => t.Key))
        {
            foreach (var point in series)
            {
                var row = _grid.Rows[_grid.Rows.Add(playerName, point.Date.ToString("yyyy-MM-dd"), $"${point.NetProfitToDate:F2}")];
                row.DefaultCellStyle.ForeColor = point.NetProfitToDate >= 0 ? Color.DarkGreen : Color.DarkRed;
            }
        }

        _grid.AutoResizeColumns();
    }
}
