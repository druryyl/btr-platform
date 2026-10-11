using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Winform.Controls;

namespace BtrReturGudang.Winform.Views
{
    /// <summary>
    /// SCR-03: Keyboard-driven manual item search modal dialog (F2 fallback).
    /// Filters product master records in BTRG_Barang by item code or name,
    /// allows packaging unit selection, and passes selected product back to intake.
    /// Traceable to: ARCHITECTURE §5.2 SCR-03, TD-011, P3-S09.
    /// </summary>
    public class ManualItemSearchDialog : Form
    {
        private readonly IReferenceRepo _referenceRepo;
        private readonly IAudioHelper _audioHelper;

        // UI Controls
        private TextBox _txtSearch;
        private DoubleBufferedDataGridView _dgvItems;
        private ComboBox _cboSatuan;
        private Label _lblSelectedItem;
        private Label _lblStatus;
        private Button _btnSelect;
        private Button _btnCancel;

        // State
        private List<Barang> _filteredItems = new List<Barang>();
        private Barang _selectedBarang;

        // Public Properties for Inspection and Testing
        public Barang SelectedBarang => _selectedBarang;
        public string SelectedSatuan => _cboSatuan?.SelectedItem?.ToString();
        public TextBox SearchBox => _txtSearch;
        public DoubleBufferedDataGridView GridView => _dgvItems;
        public ComboBox SatuanComboBox => _cboSatuan;
        public Button SelectButton => _btnSelect;
        public Button CancelButtonControl => _btnCancel;
        public IReadOnlyList<Barang> FilteredItems => _filteredItems;

        public ManualItemSearchDialog(IReferenceRepo referenceRepo = null, IAudioHelper audioHelper = null)
        {
            _referenceRepo = referenceRepo;
            _audioHelper = audioHelper;

            InitializeComponent();

            Load += async (s, e) =>
            {
                await PerformSearchAsync(string.Empty);
                _txtSearch?.Focus();
            };
        }

        public ManualItemSearchDialog() : this(null, null)
        {
        }

        private void InitializeComponent()
        {
            Text = "Pencarian Barang Manual (F2)";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(680, 520);
            BackColor = Color.FromArgb(248, 250, 252); // Slate-50
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            KeyPreview = true;

            // 1. Header Area
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(15, 23, 42), // Slate-900
                Padding = new Padding(20, 12, 20, 12)
            };

            var lblTitle = new Label
            {
                Text = "🔍 Cari Master Barang",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.White,
                Location = new Point(20, 12),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Ketik kode atau nama barang. Gunakan panah Atas/Bawah dan tekan Enter untuk memilih.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(148, 163, 184), // Slate-400
                Location = new Point(20, 38),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);

            // 2. Search Input Strip
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.FromArgb(241, 245, 249), // Slate-100
                Padding = new Padding(20, 10, 20, 10)
            };

            var lblSearch = new Label
            {
                Text = "Cari Barang:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(20, 15),
                AutoSize = true
            };

            _txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(106, 12),
                Width = 440
            };
            _txtSearch.TextChanged += async (s, e) => await PerformSearchAsync(_txtSearch.Text);
            _txtSearch.KeyDown += OnSearchKeyDown;

            var btnSearch = new Button
            {
                Text = "Cari",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(37, 99, 235), // Blue-600
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(556, 11),
                Width = 100,
                Height = 28,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += async (s, e) => await PerformSearchAsync(_txtSearch.Text);

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(_txtSearch);
            pnlSearch.Controls.Add(btnSearch);

            // 3. Bottom Footer Strip (Unit selector + Selection details + Action buttons)
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 84,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(20, 8, 20, 8)
            };

            _lblSelectedItem = new Label
            {
                Text = "Barang Terpilih: (Belum dipilih)",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235), // Blue-600
                Location = new Point(20, 10),
                AutoSize = true
            };

            var lblSatuan = new Label
            {
                Text = "Satuan:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(20, 44),
                AutoSize = true
            };

            _cboSatuan = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(76, 41),
                Width = 120
            };
            _cboSatuan.Items.AddRange(new object[] { "Dus", "Pak", "Pcs" });
            _cboSatuan.SelectedIndex = 0;

            _lblStatus = new Label
            {
                Text = "0 barang ditemukan",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(210, 45),
                AutoSize = true
            };

            _btnSelect = new Button
            {
                Text = "Pilih (Enter)",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(16, 185, 129), // Emerald-500
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(430, 26),
                Width = 120,
                Height = 38,
                Cursor = Cursors.Hand
            };
            _btnSelect.FlatAppearance.BorderSize = 0;
            _btnSelect.Click += (s, e) => ConfirmSelection();

            _btnCancel = new Button
            {
                Text = "Batal (Esc)",
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(560, 26),
                Width = 100,
                Height = 38,
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            pnlFooter.Controls.Add(_lblSelectedItem);
            pnlFooter.Controls.Add(lblSatuan);
            pnlFooter.Controls.Add(_cboSatuan);
            pnlFooter.Controls.Add(_lblStatus);
            pnlFooter.Controls.Add(_btnSelect);
            pnlFooter.Controls.Add(_btnCancel);

            // 4. Center Grid Area
            var pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 10, 20, 10)
            };

            _dgvItems = new DoubleBufferedDataGridView
            {
                Dock = DockStyle.Fill
            };
            BuildColumns();

            _dgvItems.SelectionChanged += OnGridSelectionChanged;
            _dgvItems.CellDoubleClick += (s, e) => ConfirmSelection();
            _dgvItems.KeyDown += OnGridKeyDown;

            pnlGrid.Controls.Add(_dgvItems);

            // Assemble Form
            Controls.Add(pnlGrid);
            Controls.Add(pnlFooter);
            Controls.Add(pnlSearch);
            Controls.Add(pnlHeader);

            AcceptButton = _btnSelect;
            CancelButton = _btnCancel;
        }

        private void BuildColumns()
        {
            _dgvItems.Columns.Clear();

            var colCode = new DataGridViewTextBoxColumn
            {
                Name = "colBrgId",
                HeaderText = "Kode Barang",
                DataPropertyName = "BrgId",
                Width = 120,
                ReadOnly = true
            };

            var colName = new DataGridViewTextBoxColumn
            {
                Name = "colBrgName",
                HeaderText = "Nama Barang",
                DataPropertyName = "BrgName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 220,
                ReadOnly = true
            };

            var colSat1 = new DataGridViewTextBoxColumn
            {
                Name = "colSat1",
                HeaderText = "Satuan 1",
                DataPropertyName = "Satuan1",
                Width = 80,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            var colSat2 = new DataGridViewTextBoxColumn
            {
                Name = "colSat2",
                HeaderText = "Satuan 2",
                DataPropertyName = "Satuan2",
                Width = 80,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            var colSat3 = new DataGridViewTextBoxColumn
            {
                Name = "colSat3",
                HeaderText = "Satuan 3",
                DataPropertyName = "Satuan3",
                Width = 80,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            _dgvItems.Columns.AddRange(colCode, colName, colSat1, colSat2, colSat3);
        }

        public async Task PerformSearchAsync(string keyword)
        {
            if (_referenceRepo == null) return;

            try
            {
                var results = await _referenceRepo.SearchBarangByNameOrCodeAsync(keyword ?? string.Empty);
                var items = results?.ToList() ?? new List<Barang>();
                SetItems(items);
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        public void SetItems(IEnumerable<Barang> items)
        {
            _filteredItems = items?.ToList() ?? new List<Barang>();
            _dgvItems.Rows.Clear();

            foreach (var item in _filteredItems)
            {
                int rowIndex = _dgvItems.Rows.Add(
                    item.BrgId,
                    item.BrgName,
                    item.Satuan1 ?? "-",
                    item.Satuan2 ?? "-",
                    item.Satuan3 ?? "-");
                _dgvItems.Rows[rowIndex].Tag = item;
            }

            _lblStatus.Text = $"{_filteredItems.Count} barang ditemukan";

            if (_dgvItems.Rows.Count > 0)
            {
                _dgvItems.Rows[0].Selected = true;
                UpdateSelectedItem(_filteredItems[0]);
            }
            else
            {
                _selectedBarang = null;
                _lblSelectedItem.Text = "Barang Terpilih: (Belum dipilih)";
            }
        }

        private void OnGridSelectionChanged(object sender, EventArgs e)
        {
            if (_dgvItems.CurrentRow != null && _dgvItems.CurrentRow.Tag is Barang item)
            {
                UpdateSelectedItem(item);
            }
        }

        private void UpdateSelectedItem(Barang item)
        {
            _selectedBarang = item;
            _lblSelectedItem.Text = $"Barang Terpilih: [{item.BrgId}] {item.BrgName}";

            // Populate unit options
            _cboSatuan.Items.Clear();
            var units = item.GetAvailableUnits().ToList();
            if (units.Count == 0)
            {
                units.Add("Dus");
                units.Add("Pak");
                units.Add("Pcs");
            }

            foreach (var u in units)
            {
                _cboSatuan.Items.Add(u);
            }

            _cboSatuan.SelectedIndex = 0;
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (_dgvItems.Rows.Count > 0)
                {
                    int nextIndex = _dgvItems.CurrentRow != null
                        ? Math.Min(_dgvItems.CurrentRow.Index + 1, _dgvItems.Rows.Count - 1)
                        : 0;
                    _dgvItems.CurrentCell = _dgvItems.Rows[nextIndex].Cells[0];
                    _dgvItems.Rows[nextIndex].Selected = true;
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (_dgvItems.Rows.Count > 0)
                {
                    int prevIndex = _dgvItems.CurrentRow != null
                        ? Math.Max(_dgvItems.CurrentRow.Index - 1, 0)
                        : 0;
                    _dgvItems.CurrentCell = _dgvItems.Rows[prevIndex].Cells[0];
                    _dgvItems.Rows[prevIndex].Selected = true;
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ConfirmSelection();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void OnGridKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ConfirmSelection();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        public void ConfirmSelection()
        {
            if (_selectedBarang == null && _dgvItems.CurrentRow != null)
            {
                _selectedBarang = _dgvItems.CurrentRow.Tag as Barang;
            }

            if (_selectedBarang == null)
            {
                _audioHelper?.PlayWarning();
                MessageBox.Show("Silakan pilih barang terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _audioHelper?.PlaySuccess();
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
