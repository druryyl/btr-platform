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
            ClientSize = new Size(380, 180);
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);

            var lblPrompt = new Label
            {
                Text = "Masukkan nama operator yang sedang bertugas:",
                Location = new Point(24, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(15, 23, 42)
            };

            _txtOperator = new TextBox
            {
                Location = new Point(24, 52),
                Width = 330,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
                Text = _registryHelper.LastOperatorName ?? string.Empty
            };

            _btnSave = new Button
            {
                Text = "Simpan",
                DialogResult = DialogResult.OK,
                Location = new Point(168, 110),
                Width = 90,
                Height = 36,
                BackColor = Color.FromArgb(37, 99, 235), // Blue-600
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += OnSaveClick;

            _btnCancel = new Button
            {
                Text = "Batal",
                DialogResult = DialogResult.Cancel,
                Location = new Point(266, 110),
                Width = 88,
                Height = 36,
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat
            };
            _btnCancel.FlatAppearance.BorderSize = 0;

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            Controls.Add(lblPrompt);
            Controls.Add(_txtOperator);
            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);
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
