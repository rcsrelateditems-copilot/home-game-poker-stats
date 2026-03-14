using PokerStats.WinForms.Services;

namespace PokerStats.WinForms.Forms;

/// <summary>
/// Modal dialog for registering a new player.
/// </summary>
public class AddPlayerDialog : Form
{
    private readonly TextBox _nameBox = new();

    /// <summary>The player name entered by the user. Valid only when <see cref="DialogResult"/> is OK.</summary>
    public string PlayerName => _nameBox.Text.Trim();

    public AddPlayerDialog()
    {
        Text = "Add Player";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(320, 110);

        var label = new Label
        {
            Text = "Player name:",
            Location = new Point(12, 16),
            AutoSize = true,
        };

        _nameBox.Location = new Point(12, 36);
        _nameBox.Width = 294;
        _nameBox.MaxLength = 100;

        var btnOk = new Button
        {
            Text = "Add",
            DialogResult = DialogResult.OK,
            Location = new Point(150, 70),
            Width = 75,
        };
        btnOk.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_nameBox.Text))
            {
                MessageBox.Show("Please enter a player name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        };

        var btnCancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(232, 70),
            Width = 75,
        };

        AcceptButton = btnOk;
        CancelButton = btnCancel;
        Controls.AddRange([label, _nameBox, btnOk, btnCancel]);
    }
}
