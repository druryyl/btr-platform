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
    /// SCR-08: Keyboard-driven customer search modal dialog.
    /// Filters customer master records in BTRG_Customer by code, name, or wilayah,
    /// and passes selected customer back to intake.
    /// Traceable to: FEATURE BTR-RETUR-DESKTOP-001, TD-011, TD-013.
    /// </summary>
    public class ManualCustomerSearchDialog : Form
    {
        private readonly IReferenceRepo _referenceRepo;
        private readonly IAudioHelper _audioHelper;

        // UI Controls
        private TextBox _txtSearch;
        private DoubleBufferedDataGridView _dgvCustomers;
        private Label _lblSelectedCustomer;
        private Label _lblStatus;
        private Button _btnSelect;
        private Button _btnCancel;

        // State
        private List<Customer> _filteredCustomers = new List<Customer>();
        private Customer _selectedCustomer;
        private readonly string _initialKeyword;

        // Public Properties for Inspection and Testing
        public Customer SelectedCustomer => _selectedCustomer;
        public TextBox SearchBox => _txtSearch;
        public DoubleBufferedDataGridView GridView => _dgvCustomers;
        public Button SelectButton => _btnSelect;
        public Button CancelButtonControl => _btnCancel;
        public IReadOnlyList<Customer> FilteredCustomers => _filteredCustomers;

        public ManualCustomerSearchDialog(
            IReferenceRepo referenceRepo = null,
            IAudioHelper audioHelper = null,
            string initialKeyword = null)
        {
            _referenceRepo = referenceRepo;
            _audioHelper = audioHelper;
            _initialKeyword = initialKeyword ?? string.Empty;

            InitializeComponent();

            Load += async (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(_initialKeyword))
                {
                    _txtSearch.Text = _initialKeyword;
                    _txtSearch.SelectAll();
                }
                await PerformSearchAsync(_txtSearch.Text);
                _txtSearch?.Focus();
            };
        }

        public ManualCustomerSearchDialog() : this(null, null, null)
        {
        }

        private void InitializeComponent()
        {
            Text = "Pencarian Customer";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(760, 480);
            BackColor = UiTheme.Slate100;
            Font = UiTheme.FontBody;
            KeyPreview = true;

            // 1. Header Area
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = UiTheme.Slate900,
                Padding = new Padding(20, 4, 20, 4)
            };

            var lblTitle = new Label
            {
                Text = "🔍 Cari Master Customer",
                Font = UiTheme.FontSectionTitle,
                ForeColor = Color.White,
                Location = new Point(20, 4),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Ketik kode, nama, atau wilayah customer. Gunakan panah Atas/Bawah dan tekan Enter untuk memilih.",
                Font = UiTheme.FontSubtitle,
                ForeColor = UiTheme.Slate400,
                Location = new Point(20, 24),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);

            // 2. Search Input Strip
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = UiTheme.Slate50,
                Padding = new Padding(20, 10, 20, 10)
            };

            var lblSearch = new Label
            {
                Text = "Cari Customer:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(20, 16),
                AutoSize = true
            };

            _txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(130, 12),
                Width = 490
            };
            _txtSearch.TextChanged += async (s, e) => await PerformSearchAsync(_txtSearch.Text);
            _txtSearch.KeyDown += OnSearchKeyDown;

            var btnSearch = new Button
            {
                Text = "Cari",
                Font = UiTheme.FontLabel,
                BackColor = UiTheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(630, 11),
                Width = 105,
                Height = 30,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += async (s, e) => await PerformSearchAsync(_txtSearch.Text);

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(_txtSearch);
            pnlSearch.Controls.Add(btnSearch);

            // 3. Bottom Footer Strip
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                BackColor = UiTheme.Slate50,
                Padding = new Padding(20, 8, 20, 8)
            };
            pnlFooter.Paint += (s, e) =>
            {
                using (var pen = new Pen(UiTheme.Slate200, 1))
                {
                    e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
                }
            };

            _lblSelectedCustomer = new Label
            {
                Text = "Customer Terpilih: (Belum dipilih)",
                Font = UiTheme.FontBodyBold,
                ForeColor = UiTheme.Primary,
                Location = new Point(20, 12),
                AutoSize = true
            };

            _lblStatus = new Label
            {
                Text = "0 customer ditemukan",
                Font = UiTheme.FontSubtitle,
                ForeColor = UiTheme.Slate500,
                Location = new Point(20, 38),
                AutoSize = true
            };

            _btnSelect = new Button
            {
                Text = "Pilih (Enter)",
                Font = UiTheme.FontBodyBold,
                BackColor = UiTheme.Emerald,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(510, 18),
                Width = 115,
                Height = 38,
                Cursor = Cursors.Hand
            };
            _btnSelect.FlatAppearance.BorderSize = 0;
            _btnSelect.Click += (s, e) => ConfirmSelection();

            _btnCancel = new Button
            {
                Text = "Batal (Esc)",
                DialogResult = DialogResult.Cancel,
                Font = UiTheme.FontBody,
                BackColor = UiTheme.White,
                ForeColor = UiTheme.Slate700,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(635, 18),
                Width = 100,
                Height = 38,
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderColor = UiTheme.Slate300;
            _btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            pnlFooter.Controls.Add(_lblSelectedCustomer);
            pnlFooter.Controls.Add(_lblStatus);
            pnlFooter.Controls.Add(_btnSelect);
            pnlFooter.Controls.Add(_btnCancel);

            // 4. Center Grid Area
            var pnlGrid = UiTheme.CreateCard(new Padding(10), new Padding(20, 10, 20, 10));
            pnlGrid.Dock = DockStyle.Fill;

            _dgvCustomers = new DoubleBufferedDataGridView
            {
                Dock = DockStyle.Fill
            };
            UiTheme.StyleDataGrid(_dgvCustomers);
            BuildColumns();

            _dgvCustomers.SelectionChanged += OnGridSelectionChanged;
            _dgvCustomers.CellDoubleClick += (s, e) => ConfirmSelection();
            _dgvCustomers.KeyDown += OnGridKeyDown;

            pnlGrid.Controls.Add(_dgvCustomers);

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
            _dgvCustomers.Columns.Clear();

            var colCustCode = new DataGridViewTextBoxColumn
            {
                Name = "colCustCode",
                HeaderText = "Kode",
                DataPropertyName = "CustomerCode",
                Width = 115,
                ReadOnly = true,
                DefaultCellStyle = { Font = UiTheme.FontMonospaceBold }
            };

            var colCustName = new DataGridViewTextBoxColumn
            {
                Name = "colCustName",
                HeaderText = "Nama Customer",
                DataPropertyName = "CustomerName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 220,
                ReadOnly = true
            };

            var colWilayah = new DataGridViewTextBoxColumn
            {
                Name = "colWilayah",
                HeaderText = "Wilayah / Kota",
                DataPropertyName = "Wilayah",
                Width = 130,
                ReadOnly = true
            };

            var colAlamat = new DataGridViewTextBoxColumn
            {
                Name = "colAlamat",
                HeaderText = "Alamat",
                DataPropertyName = "Alamat",
                Width = 200,
                ReadOnly = true
            };

            _dgvCustomers.Columns.AddRange(colCustCode, colCustName, colWilayah, colAlamat);
        }

        public async Task PerformSearchAsync(string keyword)
        {
            if (_referenceRepo == null) return;

            try
            {
                var results = await _referenceRepo.SearchCustomerAsync(keyword ?? string.Empty);
                var items = results?.ToList() ?? new List<Customer>();
                SetCustomers(items);
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        public void SetCustomers(IEnumerable<Customer> customers)
        {
            _filteredCustomers = customers?.ToList() ?? new List<Customer>();
            _dgvCustomers.Rows.Clear();

            foreach (var item in _filteredCustomers)
            {
                var displayCode = !string.IsNullOrWhiteSpace(item.CustomerCode) ? item.CustomerCode : item.CustomerId;
                int rowIndex = _dgvCustomers.Rows.Add(
                    displayCode,
                    item.CustomerName,
                    item.Wilayah ?? "-",
                    item.Alamat ?? "-");
                _dgvCustomers.Rows[rowIndex].Tag = item;
            }

            _lblStatus.Text = $"{_filteredCustomers.Count} customer ditemukan";

            if (_dgvCustomers.Rows.Count > 0)
            {
                _dgvCustomers.CurrentCell = _dgvCustomers.Rows[0].Cells[0];
                _dgvCustomers.Rows[0].Selected = true;
                UpdateSelectedItem(_filteredCustomers[0]);
            }
            else
            {
                _selectedCustomer = null;
                _lblSelectedCustomer.Text = "Customer Terpilih: (Belum dipilih)";
            }
        }

        private void OnGridSelectionChanged(object sender, EventArgs e)
        {
            if (_dgvCustomers.CurrentRow != null && _dgvCustomers.CurrentRow.Tag is Customer item)
            {
                UpdateSelectedItem(item);
            }
        }

        private void UpdateSelectedItem(Customer item)
        {
            _selectedCustomer = item;
            var displayCode = !string.IsNullOrWhiteSpace(item.CustomerCode) ? item.CustomerCode : item.CustomerId;
            var wilayahText = !string.IsNullOrWhiteSpace(item.Wilayah) ? $" - {item.Wilayah}" : string.Empty;
            _lblSelectedCustomer.Text = $"Customer Terpilih: [{displayCode}] {item.CustomerName}{wilayahText}";
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (_dgvCustomers.Rows.Count > 0)
                {
                    int currentIndex = _dgvCustomers.CurrentRow != null
                        ? _dgvCustomers.CurrentRow.Index
                        : (_dgvCustomers.SelectedRows.Count > 0 ? _dgvCustomers.SelectedRows[0].Index : 0);
                    int nextIndex = Math.Min(currentIndex + 1, _dgvCustomers.Rows.Count - 1);
                    _dgvCustomers.CurrentCell = _dgvCustomers.Rows[nextIndex].Cells[0];
                    _dgvCustomers.Rows[nextIndex].Selected = true;
                    if (_dgvCustomers.Rows[nextIndex].Tag is Customer cust)
                    {
                        UpdateSelectedItem(cust);
                    }
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (_dgvCustomers.Rows.Count > 0)
                {
                    int currentIndex = _dgvCustomers.CurrentRow != null
                        ? _dgvCustomers.CurrentRow.Index
                        : (_dgvCustomers.SelectedRows.Count > 0 ? _dgvCustomers.SelectedRows[0].Index : 0);
                    int prevIndex = Math.Max(currentIndex - 1, 0);
                    _dgvCustomers.CurrentCell = _dgvCustomers.Rows[prevIndex].Cells[0];
                    _dgvCustomers.Rows[prevIndex].Selected = true;
                    if (_dgvCustomers.Rows[prevIndex].Tag is Customer cust)
                    {
                        UpdateSelectedItem(cust);
                    }
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
            if (_selectedCustomer == null && _dgvCustomers.CurrentRow != null)
            {
                _selectedCustomer = _dgvCustomers.CurrentRow.Tag as Customer;
            }

            if (_selectedCustomer == null)
            {
                _audioHelper?.PlayWarning();
                MessageBox.Show("Silakan pilih customer terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
