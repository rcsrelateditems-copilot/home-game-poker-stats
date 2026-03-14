using PokerStats.WinForms.Services;
using PokerStats.WinForms.Tabs;

namespace PokerStats.WinForms.Forms;

/// <summary>
/// The application's main window. Hosts a tab control with all views and
/// a status bar showing the current data file path.
/// </summary>
public class MainForm : Form
{
    private readonly DataService _dataService = new();
    private readonly TabControl _tabs = new();
    private readonly ToolStripStatusLabel _statusLabel = new();

    // ── tab user controls ─────────────────────────────────────────────────────
    private readonly LeaderboardTab _leaderboardTab = new();
    private readonly PlayerStatsTab _playerStatsTab = new();
    private readonly KnockoutsTab _knockoutsTab = new();
    private readonly EarningsTab _earningsTab = new();
    private readonly PayoutStatsTab _payoutStatsTab = new();
    private readonly PlayersTab _playersTab = new();
    private readonly GamesTab _gamesTab = new();

    public MainForm()
    {
        Text = "Poker Stats";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(900, 620);
        MinimumSize = new Size(700, 500);
        Font = new Font("Segoe UI", 9);

        BuildToolbar();
        BuildTabs();
        BuildStatusBar();

        // Wire DataChanged events so stats tabs refresh after writes
        _playersTab.DataChanged += (_, _) => RefreshAll();
        _gamesTab.DataChanged += (_, _) => RefreshAll();

        // Refresh the active tab when the user switches
        _tabs.SelectedIndexChanged += (_, _) => RefreshActiveTab();

        Load += (_, _) => RefreshAll();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Build UI
    // ─────────────────────────────────────────────────────────────────────────

    private void BuildToolbar()
    {
        var strip = new ToolStrip { Dock = DockStyle.Top };

        var refreshBtn = new ToolStripButton("🔄  Reload All")
        {
            ToolTipText = "Reload all data from disk",
            DisplayStyle = ToolStripItemDisplayStyle.Text,
        };
        refreshBtn.Click += (_, _) => RefreshAll();

        strip.Items.Add(refreshBtn);
        Controls.Add(strip);
    }

    private void BuildTabs()
    {
        _tabs.Dock = DockStyle.Fill;

        AddTab("🏆 Leaderboard", _leaderboardTab);
        AddTab("📊 Player Stats", _playerStatsTab);
        AddTab("💀 Knockouts", _knockoutsTab);
        AddTab("📈 Earnings", _earningsTab);
        AddTab("💰 Payout Stats", _payoutStatsTab);
        AddTab("👥 Players", _playersTab);
        AddTab("🎮 Games", _gamesTab);

        Controls.Add(_tabs);
    }

    private void AddTab(string title, UserControl content)
    {
        var page = new TabPage(title) { Padding = new Padding(0) };
        page.Controls.Add(content);
        _tabs.TabPages.Add(page);
    }

    private void BuildStatusBar()
    {
        var statusStrip = new StatusStrip();
        _statusLabel.Text = $"Data file: {_dataService.FilePath}";
        statusStrip.Items.Add(_statusLabel);
        Controls.Add(statusStrip);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Data refresh
    // ─────────────────────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        RefreshActiveTab();
        _statusLabel.Text = $"Data file: {_dataService.FilePath}";
    }

    private void RefreshActiveTab()
    {
        switch (_tabs.SelectedIndex)
        {
            case 0: _leaderboardTab.Refresh(_dataService); break;
            case 1: _playerStatsTab.Refresh(_dataService); break;
            case 2: _knockoutsTab.Refresh(_dataService); break;
            case 3: _earningsTab.Refresh(_dataService); break;
            case 4: _payoutStatsTab.Refresh(_dataService); break;
            case 5: _playersTab.Refresh(_dataService); break;
            case 6: _gamesTab.Refresh(_dataService); break;
        }
    }
}
