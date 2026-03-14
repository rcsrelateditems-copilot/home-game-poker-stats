using PokerStats.Models;
using PokerStats.WinForms.Forms;
using PokerStats.WinForms.Services;

namespace PokerStats.WinForms.Tabs;

/// <summary>
/// Displays all registered players and allows adding new ones.
/// </summary>
public class PlayersTab : UserControl
{
    private readonly DataGridView _grid = new();
    private DataService? _dataService;

    public PlayersTab()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(8);

        var title = new Label
        {
            Text = "Players",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 4, 0, 4),
        };

        var toolbar = new Panel { Height = 38, Dock = DockStyle.Top };
        var addBtn = new Button
        {
            Text = "➕  Add Player",
            AutoSize = true,
            Location = new Point(0, 4),
        };
        addBtn.Click += AddPlayer_Click;
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
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
        _grid.Font = new Font("Segoe UI", 9);
        _grid.RowHeadersVisible = false;
    }

    /// <summary>Reloads the player list from the database.</summary>
    public void Refresh(DataService dataService)
    {
        _dataService = dataService;
        LoadGrid(dataService.Load());
    }

    private void LoadGrid(Database db)
    {
        _grid.Columns.Clear();
        _grid.Rows.Clear();

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name" });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Member Since" });

        foreach (var p in db.Players.OrderBy(p => p.Name))
            _grid.Rows.Add(p.Name, p.CreatedAt.ToString("yyyy-MM-dd"));

        _grid.AutoResizeColumns();
    }

    private void AddPlayer_Click(object? sender, EventArgs e)
    {
        if (_dataService == null) return;

        using var dlg = new AddPlayerDialog();
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var db = _dataService.Load();

        if (db.Players.Any(p => p.Name.Equals(dlg.PlayerName, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show($"A player named \"{dlg.PlayerName}\" already exists.", "Duplicate",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        db.Players.Add(new Player { Name = dlg.PlayerName });
        _dataService.Save(db);
        LoadGrid(db);

        // Notify MainForm so other tabs (player pickers) can update
        OnDataChanged();
    }

    /// <summary>Raised after a player is successfully added.</summary>
    public event EventHandler? DataChanged;

    private void OnDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);
}
