using PokerStats.Models;
using PokerStats.WinForms.Forms;
using PokerStats.WinForms.Services;

namespace PokerStats.WinForms.Tabs;

/// <summary>
/// Shows the game history and allows adding a new game via the Add Game dialog.
/// </summary>
public class GamesTab : UserControl
{
    private readonly DataGridView _grid = new();
    private DataService? _dataService;

    public GamesTab()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(8);

        var title = new Label
        {
            Text = "Game History",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 4, 0, 4),
        };

        var toolbar = new Panel { Height = 38, Dock = DockStyle.Top };
        var addBtn = new Button
        {
            Text = "➕  Add Game",
            AutoSize = true,
            Location = new Point(0, 4),
        };
        addBtn.Click += AddGame_Click;
        toolbar.Controls.Add(addBtn);

        ConfigureGrid();
        Controls.Add(_grid);
        Controls.Add(toolbar);
        Controls.Add(title);
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 255);
        _grid.Font = new Font("Segoe UI", 9);
        _grid.RowHeadersVisible = false;
    }

    /// <summary>Reloads the game list from the database.</summary>
    public void Refresh(DataService dataService)
    {
        _dataService = dataService;
        LoadGrid(dataService.Load());
    }

    private void LoadGrid(Database db)
    {
        _grid.Columns.Clear();
        _grid.Rows.Clear();

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Buy-In" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Players" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Knockouts" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Winner" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Notes" });

        foreach (var g in db.Games.OrderByDescending(g => g.Date))
        {
            var winner = g.Results.OrderBy(r => r.Placement).FirstOrDefault();
            var winnerName = winner != null
                ? (db.Players.FirstOrDefault(p => p.Id == winner.PlayerId)?.Name ?? "?")
                : "?";

            _grid.Rows.Add(
                g.Date.ToString("yyyy-MM-dd"),
                $"${g.BuyIn:F2}",
                g.Results.Count,
                g.Knockouts.Count,
                winnerName,
                g.Notes ?? ""
            );
        }

        _grid.AutoResizeColumns();
    }

    private void AddGame_Click(object? sender, EventArgs e)
    {
        if (_dataService == null) return;

        var db = _dataService.Load();
        if (db.Players.Count < 2)
        {
            MessageBox.Show("Add at least 2 players before recording a game.", "Not Enough Players",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new AddGameDialog(db.Players);
        if (dlg.ShowDialog(this) != DialogResult.OK || dlg.Result == null) return;

        db.Games.Add(dlg.Result);
        _dataService.Save(db);
        LoadGrid(db);
        OnDataChanged();
    }

    /// <summary>Raised after a game is successfully saved.</summary>
    public event EventHandler? DataChanged;

    private void OnDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);
}
