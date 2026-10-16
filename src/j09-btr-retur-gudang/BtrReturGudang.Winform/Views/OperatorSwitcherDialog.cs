using System;
using System.Drawing;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;

namespace BtrReturGudang.Winform.Views
{
    /// <summary>
    /// Modal dialog for switching the active shift operator name.
    /// Persists operator name to workstation registry.
    /// Traceable to: ARCHITECTURE §5.2 SCR-01, TD-007, P3-S07.
    /// </summary>
    public class OperatorSwitcherDialog : Form
    {
        private readonly IRegistryHelper _registryHelper;
        private TextBox _txtOperator;
        private Button _btnSave;
        private Button _btnCancel;

        public string SelectedOperatorName { get; private set; }

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
            ClientSize = new Size(460, 260);
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
                Text = "Perbarui identitas operator aktif untuk sesi retur workstation ini.",
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

            var lblPrompt = new Label
            {
                Text = "Nama Operator / Petugas Gudang:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(20, 16),
                AutoSize = true
            };

            string currentOp = _registryHelper.LastOperatorName ?? string.Empty;
            _txtOperator = new TextBox
            {
                Location = new Point(20, 40),
                Width = 388,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                Text = currentOp
            };

            var lblHint = new Label
            {
                Text = string.IsNullOrWhiteSpace(currentOp) 
                    ? "Belum ada operator tersimpan di registry workstation ini." 
                    : $"Operator aktif sebelumnya: \"{currentOp}\"",
                Font = UiTheme.FontSubtitle,
                ForeColor = UiTheme.Slate500,
                Location = new Point(20, 75),
                AutoSize = true
            };

            // 3. Action Buttons
            _btnSave = new Button
            {
                Text = "Simpan Operator",
                DialogResult = DialogResult.OK,
                Location = new Point(194, 110),
                Width = 130,
                Height = 36
            };
            UiTheme.ApplyPrimaryButton(_btnSave, 36);
            _btnSave.Click += OnSaveClick;

            _btnCancel = new Button
            {
                Text = "Batal",
                DialogResult = DialogResult.Cancel,
                Location = new Point(332, 110),
                Width = 76,
                Height = 36
            };
            UiTheme.ApplyOutlineButton(_btnCancel, 36);

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            pnlBody.Controls.Add(lblPrompt);
            pnlBody.Controls.Add(_txtOperator);
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

            _registryHelper.LastOperatorName = newOperator;
            SelectedOperatorName = newOperator;
        }
    }
}
