using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;

namespace BtrReturGudang.Winform.Views
{
    /// <summary>
    /// Modal dialog for switching the active shift operator identity (display name + registered Google email).
    /// Persists the operator name and Google email to the workstation registry.
    /// The Google email is the Cloud session actor (X-Session-Actor); the name is display/attribution only.
    /// Traceable to: ARCHITECTURE §5.2 SCR-01, §5.2 SCR-07, TD-007, TD-012, P2-S04.
    /// </summary>
    public class OperatorSwitcherDialog : Form
    {
        private static readonly Regex EmailShapeRegex =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private readonly IRegistryHelper _registryHelper;
        private TextBox _txtOperator;
        private TextBox _txtOperatorEmail;
        private Button _btnSave;
        private Button _btnCancel;

        public string SelectedOperatorName { get; private set; }
        public string SelectedOperatorEmail { get; private set; }

        // Public test accessors
        public TextBox OperatorNameBox => _txtOperator;
        public TextBox OperatorEmailBox => _txtOperatorEmail;
        public Button SaveButton => _btnSave;

        public OperatorSwitcherDialog(IRegistryHelper registryHelper)
        {
            _registryHelper = registryHelper ?? throw new ArgumentNullException(nameof(registryHelper));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Ganti Operator Shift";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(680, 240);
            BackColor = UiTheme.Slate100;
            Font = UiTheme.FontBody;

            // 1. Top Header Banner
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = UiTheme.Slate900,
                Padding = new Padding(20, 10, 20, 10)
            };

            var lblTitle = new Label
            {
                Text = "👤 Ganti Operator Shift",
                Font = UiTheme.FontSectionTitle,
                ForeColor = Color.White,
                Location = new Point(20, 10),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Perbarui identitas operator aktif (nama tampilan & Google email) untuk sesi retur workstation ini.",
                Font = UiTheme.FontSubtitle,
                ForeColor = UiTheme.Slate400,
                Location = new Point(20, 34),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);

            // 2. Card Body Container
            var pnlBody = UiTheme.CreateCard(new Padding(16, 12, 16, 14), new Padding(20, 16, 20, 16));
            pnlBody.Dock = DockStyle.Fill;

            // Name input (left)
            var lblPrompt = new Label
            {
                Text = "Nama Operator / Petugas Gudang:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(20, 18),
                AutoSize = true
            };

            string currentOp = _registryHelper.LastOperatorName ?? string.Empty;
            _txtOperator = new TextBox
            {
                Location = new Point(20, 42),
                Width = 300,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                Text = currentOp
            };

            // Google email input (right, beside the existing name input)
            var lblEmailPrompt = new Label
            {
                Text = "Google Email Operator:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(350, 18),
                AutoSize = true
            };

            string currentEmail = _registryHelper.LastOperatorEmail ?? string.Empty;
            _txtOperatorEmail = new TextBox
            {
                Location = new Point(350, 42),
                Width = 310,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                Text = currentEmail
            };

            var lblHint = new Label
            {
                Text = string.IsNullOrWhiteSpace(currentOp)
                    ? "Belum ada operator tersimpan di registry workstation ini."
                    : $"Operator aktif sebelumnya: \"{currentOp}\"",
                Font = UiTheme.FontSubtitle,
                ForeColor = UiTheme.Slate500,
                Location = new Point(20, 80),
                AutoSize = true
            };

            // 3. Action Buttons
            _btnSave = new Button
            {
                Text = "Simpan Operator",
                DialogResult = DialogResult.OK,
                Location = new Point(410, 116),
                Width = 130,
                Height = 36
            };
            UiTheme.ApplyPrimaryButton(_btnSave, 36);
            _btnSave.Click += OnSaveClick;

            _btnCancel = new Button
            {
                Text = "Batal",
                DialogResult = DialogResult.Cancel,
                Location = new Point(548, 116),
                Width = 76,
                Height = 36
            };
            UiTheme.ApplyOutlineButton(_btnCancel, 36);

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            pnlBody.Controls.Add(lblPrompt);
            pnlBody.Controls.Add(_txtOperator);
            pnlBody.Controls.Add(lblEmailPrompt);
            pnlBody.Controls.Add(_txtOperatorEmail);
            pnlBody.Controls.Add(lblHint);
            pnlBody.Controls.Add(_btnSave);
            pnlBody.Controls.Add(_btnCancel);

            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);

            Shown += (s, e) =>
            {
                _txtOperator.Focus();
                _txtOperator.SelectAll();
            };
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            string newOperator = _txtOperator.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(newOperator))
            {
                MessageBox.Show(
                    "Nama operator tidak boleh kosong.",
                    "Validasi Operator",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            string newEmail = _txtOperatorEmail.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(newEmail) || !EmailShapeRegex.IsMatch(newEmail))
            {
                MessageBox.Show(
                    "Google Email Operator harus diisi dengan format email yang valid (contoh: nama@perusahaan.com).",
                    "Validasi Operator",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            _registryHelper.LastOperatorName = newOperator;
            _registryHelper.LastOperatorEmail = newEmail;
            SelectedOperatorName = newOperator;
            SelectedOperatorEmail = newEmail;
        }
    }
}
