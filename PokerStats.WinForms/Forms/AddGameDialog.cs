using PokerStats.Models;

namespace PokerStats.WinForms.Forms;

/// <summary>
/// Three-step modal dialog for recording a new tournament game.
/// Step 1 — Date, buy-in, notes and player selection.
/// Step 2 — Placement and payout entry for each selected player.
/// Step 3 — Optional knockout recording.
/// </summary>
public class AddGameDialog : Form
{
    // ── shared data ──────────────────────────────────────────────────────────
    private readonly List<Player> _allPlayers;

    // ── step 1 controls ──────────────────────────────────────────────────────
    private readonly DateTimePicker _datePicker = new() { Format = DateTimePickerFormat.Short };
    private readonly NumericUpDown _buyInBox = new() { Minimum = 1, Maximum = 10_000, Value = 20, DecimalPlaces = 2 };
    private readonly TextBox _notesBox = new() { Width = 340, MaxLength = 200 };
    private readonly CheckedListBox _playerList = new() { Height = 180, Width = 340, CheckOnClick = true };

    // ── step 2 controls ──────────────────────────────────────────────────────
    private readonly DataGridView _resultsGrid = new();

    // ── step 3 controls ──────────────────────────────────────────────────────
    private readonly DataGridView _knockoutsGrid = new();

    // ── wizard panels ────────────────────────────────────────────────────────
    private readonly Panel _step1Panel = new();
    private readonly Panel _step2Panel = new();
    private readonly Panel _step3Panel = new();
    private int _currentStep = 1;

    // ── navigation buttons ───────────────────────────────────────────────────
    private readonly Button _backBtn = new() { Text = "◀ Back", Width = 80 };
    private readonly Button _nextBtn = new() { Text = "Next ▶", Width = 80 };
    private readonly Button _finishBtn = new() { Text = "✔ Save Game", Width = 100, Visible = false };
    private readonly Button _cancelBtn = new() { Text = "Cancel", Width = 80, DialogResult = DialogResult.Cancel };

    /// <summary>The completed game. Valid only when <see cref="DialogResult"/> is OK.</summary>
    public Game? Result { get; private set; }

    public AddGameDialog(List<Player> players)
    {
        _allPlayers = players;

        Text = "Add Game";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 460);
        CancelButton = _cancelBtn;

        BuildStep1();
        BuildStep2();
        BuildStep3();
        BuildNavBar();

        _step1Panel.Visible = true;
        _step2Panel.Visible = false;
        _step3Panel.Visible = false;

        Controls.AddRange([_step1Panel, _step2Panel, _step3Panel]);

        _backBtn.Click += (_, _) => Navigate(-1);
        _nextBtn.Click += (_, _) => Navigate(+1);
        _finishBtn.Click += Finish_Click;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Build steps
    // ─────────────────────────────────────────────────────────────────────────

    private void BuildStep1()
    {
        _step1Panel.Dock = DockStyle.Fill;

        var title = StepTitle("Step 1 of 3 — Game Details & Players");

        var table = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Location = new Point(12, 44),
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        AddRow(table, "Date:", _datePicker);
        AddRow(table, "Buy-In ($):", _buyInBox);
        AddRow(table, "Notes:", _notesBox);

        var playersLabel = new Label { Text = "Players (check who played):", AutoSize = true };
        _playerList.Items.Clear();
        foreach (var p in _allPlayers.OrderBy(p => p.Name))
            _playerList.Items.Add(p.Name);

        _step1Panel.Controls.AddRange([title, table, playersLabel, _playerList]);

        playersLabel.Location = new Point(12, table.Bottom + 8);
        _playerList.Location = new Point(12, playersLabel.Bottom + 4);
    }

    private void BuildStep2()
    {
        _step2Panel.Dock = DockStyle.Fill;

        var title = StepTitle("Step 2 of 3 — Placements & Payouts");
        var hint = new Label
        {
            Text = "Enter placement (1 = winner) and payout for each player. Leave payout 0 if they didn't cash.",
            Location = new Point(12, 40),
            Width = 454,
            Height = 32,
            ForeColor = Color.DimGray,
        };

        _resultsGrid.Location = new Point(12, 78);
        _resultsGrid.Width = 454;
        _resultsGrid.Height = 280;
        _resultsGrid.AllowUserToAddRows = false;
        _resultsGrid.AllowUserToDeleteRows = false;
        _resultsGrid.RowHeadersVisible = false;
        _resultsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _resultsGrid.Font = new Font("Segoe UI", 9);

        _resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Player",
            HeaderText = "Player",
            ReadOnly = true,
            FillWeight = 40,
        });
        _resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Placement",
            HeaderText = "Placement (1=1st)",
            FillWeight = 30,
        });
        _resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Payout",
            HeaderText = "Payout ($)",
            FillWeight = 30,
        });

        _step2Panel.Controls.AddRange([title, hint, _resultsGrid]);
    }

    private void BuildStep3()
    {
        _step3Panel.Dock = DockStyle.Fill;

        var title = StepTitle("Step 3 of 3 — Knockouts (optional)");
        var hint = new Label
        {
            Text = "Record who knocked out whom. Leave \"Knocked Out By\" blank if unknown. Click a row to add; use the minus button to remove.",
            Location = new Point(12, 40),
            Width = 454,
            Height = 40,
            ForeColor = Color.DimGray,
        };

        _knockoutsGrid.Location = new Point(12, 86);
        _knockoutsGrid.Width = 454;
        _knockoutsGrid.Height = 240;
        _knockoutsGrid.AllowUserToDeleteRows = true;
        _knockoutsGrid.RowHeadersVisible = true;
        _knockoutsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _knockoutsGrid.Font = new Font("Segoe UI", 9);

        var eliminatedCol = new DataGridViewComboBoxColumn
        {
            Name = "Eliminated",
            HeaderText = "Eliminated Player",
            FillWeight = 50,
        };
        var knockedByCol = new DataGridViewComboBoxColumn
        {
            Name = "KnockedBy",
            HeaderText = "Knocked Out By",
            FillWeight = 50,
        };

        _knockoutsGrid.Columns.Add(eliminatedCol);
        _knockoutsGrid.Columns.Add(knockedByCol);

        var addRowBtn = new Button
        {
            Text = "➕ Add Knockout Row",
            AutoSize = true,
            Location = new Point(12, _knockoutsGrid.Bottom + 8),
        };
        addRowBtn.Click += (_, _) => _knockoutsGrid.Rows.Add();

        _step3Panel.Controls.AddRange([title, hint, _knockoutsGrid, addRowBtn]);
    }

    private void BuildNavBar()
    {
        var navBar = new Panel { Height = 42, Dock = DockStyle.Bottom };
        _cancelBtn.Location = new Point(8, 8);
        _backBtn.Location = new Point(292, 8);
        _nextBtn.Location = new Point(380, 8);
        _finishBtn.Location = new Point(368, 8);

        _backBtn.Enabled = false;
        navBar.Controls.AddRange([_cancelBtn, _backBtn, _nextBtn, _finishBtn]);
        Controls.Add(navBar);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Navigation
    // ─────────────────────────────────────────────────────────────────────────

    private void Navigate(int direction)
    {
        if (direction > 0 && !ValidateCurrentStep()) return;

        _currentStep += direction;

        _step1Panel.Visible = _currentStep == 1;
        _step2Panel.Visible = _currentStep == 2;
        _step3Panel.Visible = _currentStep == 3;

        _backBtn.Enabled = _currentStep > 1;
        _nextBtn.Visible = _currentStep < 3;
        _finishBtn.Visible = _currentStep == 3;

        if (_currentStep == 2) PopulateResultsGrid();
        if (_currentStep == 3) PopulateKnockoutsGrid();
    }

    private bool ValidateCurrentStep()
    {
        if (_currentStep == 1)
        {
            if (_playerList.CheckedItems.Count < 2)
            {
                MessageBox.Show("Select at least 2 players.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
        else if (_currentStep == 2)
        {
            if (!ValidatePlacements()) return false;
        }
        return true;
    }

    private void PopulateResultsGrid()
    {
        _resultsGrid.Rows.Clear();
        int placement = 1;
        foreach (string name in _playerList.CheckedItems)
        {
            var idx = _resultsGrid.Rows.Add(name, placement, "0");
            _resultsGrid.Rows[idx].Tag = name;
            placement++;
        }
    }

    private void PopulateKnockoutsGrid()
    {
        var names = _playerList.CheckedItems.Cast<string>().OrderBy(n => n).ToArray();
        var blankPlusNames = new[] { "" }.Concat(names).ToArray();

        var elCol = (DataGridViewComboBoxColumn)_knockoutsGrid.Columns["Eliminated"]!;
        var koCol = (DataGridViewComboBoxColumn)_knockoutsGrid.Columns["KnockedBy"]!;

        elCol.Items.Clear();
        elCol.Items.AddRange(names);

        koCol.Items.Clear();
        koCol.Items.AddRange(blankPlusNames);

        _knockoutsGrid.Rows.Clear();
        // Pre-populate one row per participant (they each get knocked out once in a typical game)
        foreach (var name in names)
            _knockoutsGrid.Rows.Add(name, "");
    }

    private bool ValidatePlacements()
    {
        var placements = new List<int>();
        for (int i = 0; i < _resultsGrid.Rows.Count; i++)
        {
            var placementCell = _resultsGrid.Rows[i].Cells["Placement"];
            var payoutCell = _resultsGrid.Rows[i].Cells["Payout"];

            if (!int.TryParse(placementCell.Value?.ToString(), out int p) || p < 1)
            {
                MessageBox.Show($"Row {i + 1}: placement must be a positive integer.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!decimal.TryParse(payoutCell.Value?.ToString(), out decimal payout) || payout < 0)
            {
                MessageBox.Show($"Row {i + 1}: payout must be zero or a positive amount.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            placements.Add(p);
        }

        if (placements.Distinct().Count() != placements.Count)
        {
            MessageBox.Show("Each player must have a unique placement.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Finish
    // ─────────────────────────────────────────────────────────────────────────

    private void Finish_Click(object? sender, EventArgs e)
    {
        var game = new Game
        {
            Date = _datePicker.Value.Date,
            BuyIn = _buyInBox.Value,
            Notes = string.IsNullOrWhiteSpace(_notesBox.Text) ? null : _notesBox.Text.Trim(),
        };

        // Results
        for (int i = 0; i < _resultsGrid.Rows.Count; i++)
        {
            var playerName = _resultsGrid.Rows[i].Cells["Player"].Value?.ToString() ?? "";
            var player = _allPlayers.First(p => p.Name == playerName);
            int.TryParse(_resultsGrid.Rows[i].Cells["Placement"].Value?.ToString(), out int placement);
            decimal.TryParse(_resultsGrid.Rows[i].Cells["Payout"].Value?.ToString(), out decimal payout);

            game.Results.Add(new GameResult { PlayerId = player.Id, Placement = placement, Payout = payout });
        }

        // Knockouts (skip empty rows)
        foreach (DataGridViewRow row in _knockoutsGrid.Rows)
        {
            if (row.IsNewRow) continue;
            var eliminatedName = row.Cells["Eliminated"].Value?.ToString();
            var knockedByName = row.Cells["KnockedBy"].Value?.ToString();

            if (string.IsNullOrWhiteSpace(eliminatedName)) continue;

            var eliminated = _allPlayers.FirstOrDefault(p => p.Name == eliminatedName);
            if (eliminated == null) continue;
            var knockedBy = string.IsNullOrWhiteSpace(knockedByName) ? null :
                _allPlayers.FirstOrDefault(p => p.Name == knockedByName);

            game.Knockouts.Add(new KnockoutRecord
            {
                EliminatedPlayerId = eliminated.Id,
                KnockedOutByPlayerId = knockedBy?.Id,
            });
        }

        Result = game;
        DialogResult = DialogResult.OK;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static Label StepTitle(string text) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", 11, FontStyle.Bold),
        Location = new Point(12, 10),
        AutoSize = true,
    };

    private static void AddRow(TableLayoutPanel table, string labelText, Control control)
    {
        table.Controls.Add(new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left });
        table.Controls.Add(control);
    }
}
