using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;

namespace BtrReturGudang.Winform.Views
{
    /// <summary>
    /// SCR-04: On-the-spot modal dialog for quick barcode registration of unknown barcodes.
    /// Triggered automatically when ScannerWedgeListener captures an unmapped barcode.
    /// Prominently displays the unmapped barcode, provides product picker from BTRG_Barang,
    /// configures unit and conversion multiplier, persists local mapping and queues upload,
    /// and immediately transfers the resolved item into ReturnOrderEntryView intake.
    /// Traceable to: ARCHITECTURE §5.2 SCR-04, TD-006, TD-009, P3-S09.
    /// </summary>
    public class QuickBarcodeRegistrationDialog : Form
    {
        private readonly IBarcodeService _barcodeService;
        private readonly IBarcodeRepo _barcodeRepo;
        private readonly IReferenceRepo _referenceRepo;
        private readonly IRegistryHelper _registryHelper;
        private readonly IAudioHelper _audioHelper;

        // UI Controls
        private TextBox _txtBarcode;
        private TextBox _txtSearchItem;
        private ListBox _lstItems;
        private Label _lblSelectedItem;
        private ComboBox _cboSatuan;
        private NumericUpDown _numPengali;
        private Button _btnRegisterAndUse;
        private Button _btnCancel;

        // State
        private List<Barang> _filteredItems = new List<Barang>();
        private Barang _selectedBarang;
        private BarcodeLookupResult _registeredResult;

        // Public Properties for Testing & Integration
        public BarcodeLookupResult RegisteredResult => _registeredResult;
        public string BarcodeValue => _txtBarcode?.Text?.Trim();
        public Barang SelectedBarang => _selectedBarang;
        public string SelectedSatuan => _cboSatuan?.SelectedItem?.ToString();
        public decimal SelectedPengali => _numPengali?.Value ?? 1m;
        public TextBox BarcodeTextBox => _txtBarcode;
        public TextBox SearchItemTextBox => _txtSearchItem;
        public ListBox ItemsListBox => _lstItems;
        public ComboBox SatuanComboBox => _cboSatuan;
        public NumericUpDown PengaliNumeric => _numPengali;
        public Button RegisterButton => _btnRegisterAndUse;
        public Button CancelButtonControl => _btnCancel;

        public QuickBarcodeRegistrationDialog(
            string unmappedBarcode = null,
            IBarcodeService barcodeService = null,
            IBarcodeRepo barcodeRepo = null,
            IReferenceRepo referenceRepo = null,
            IRegistryHelper registryHelper = null,
            IAudioHelper audioHelper = null)
        {
            _barcodeService = barcodeService;
            _barcodeRepo = barcodeRepo;
            _referenceRepo = referenceRepo;
            _registryHelper = registryHelper;
            _audioHelper = audioHelper;

            InitializeComponent(unmappedBarcode);

            Load += async (s, e) =>
            {
                // Trigger 400Hz buzz warning feedback upon showing unknown barcode modal
                _audioHelper?.PlayWarning();
                await SearchItemsAsync(string.Empty);
                _txtSearchItem?.Focus();
            };
        }

        public QuickBarcodeRegistrationDialog() : this(null, null, null, null, null, null)
        {
        }

        private void InitializeComponent(string initialBarcode)
        {
            Text = "Registrasi Barcode Cepat (SCR-04)";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(540, 560);
            BackColor = UiTheme.Slate100;
            Font = UiTheme.FontBody;
            KeyPreview = true;

            // 1. Top Header Banner with Warning Tone Styling
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.FromArgb(76, 12, 12), // Deep Rose-950
                Padding = new Padding(24, 12, 24, 12)
            };

            var lblHeaderTitle = new Label
            {
                Text = "⚠️ Barcode Belum Terdaftar",
                Font = UiTheme.FontSectionTitle,
                ForeColor = Color.FromArgb(254, 205, 211), // Rose-200
                Location = new Point(24, 10),
                AutoSize = true
            };

            var lblHeaderSubtitle = new Label
            {
                Text = "Tautkan ke Master Barang agar item langsung masuk ke dokumen retur aktif.",
                Font = UiTheme.FontSubtitle,
                ForeColor = Color.FromArgb(253, 164, 175), // Rose-300
                Location = new Point(24, 36),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(lblHeaderSubtitle);

            // Card body container
            var pnlBody = UiTheme.CreateCard(new Padding(20), new Padding(16, 12, 16, 16));
            pnlBody.Dock = DockStyle.Fill;

            // 2. Barcode Prominent Read-Only Display
            var lblBarcode = new Label
            {
                Text = "Barcode Terdeteksi (Read-Only):",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(16, 14),
                AutoSize = true
            };

            _txtBarcode = new TextBox
            {
                Text = initialBarcode ?? string.Empty,
                ReadOnly = true,
                BackColor = UiTheme.RoseLight,
                ForeColor = UiTheme.RoseText,
                Font = UiTheme.FontMonospaceBold,
                Location = new Point(16, 36),
                Width = 472
            };

            // 3. Product Search & Selection
            var lblSearch = new Label
            {
                Text = "Cari Master Barang (Kode / Nama):",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(16, 72),
                AutoSize = true
            };

            _txtSearchItem = new TextBox
            {
                Font = UiTheme.FontBody,
                Location = new Point(16, 94),
                Width = 472
            };
            _txtSearchItem.TextChanged += async (s, e) => await SearchItemsAsync(_txtSearchItem.Text);
            _txtSearchItem.KeyDown += OnSearchKeyDown;

            _lstItems = new ListBox
            {
                Font = UiTheme.FontBody,
                Location = new Point(16, 126),
                Width = 472,
                Height = 115,
                BorderStyle = BorderStyle.FixedSingle
            };
            _lstItems.SelectedIndexChanged += OnItemSelected;
            _lstItems.KeyDown += OnListKeyDown;

            _lblSelectedItem = new Label
            {
                Text = "Barang Terpilih: (Belum dipilih)",
                Font = UiTheme.FontBodyBold,
                ForeColor = UiTheme.Primary,
                Location = new Point(16, 250),
                AutoSize = true
            };

            // 4. Packaging Unit and Multiplier Strip
            var lblSatuan = new Label
            {
                Text = "Satuan Kemasan:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(16, 280),
                AutoSize = true
            };

            _cboSatuan = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.FontBody,
                Location = new Point(16, 304),
                Width = 160
            };
            _cboSatuan.Items.AddRange(new object[] { "Dus", "Pak", "Pcs", "Ball", "Lusin" });
            _cboSatuan.SelectedIndex = 0;

            var lblPengali = new Label
            {
                Text = "Pengali (Konversi ke Pcs):",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(200, 280),
                AutoSize = true
            };

            _numPengali = new NumericUpDown
            {
                DecimalPlaces = 0,
                Minimum = 1,
                Maximum = 10000,
                Value = 1,
                Font = UiTheme.FontBodyBold,
                Location = new Point(200, 304),
                Width = 160
            };

            // 5. Action Buttons
            _btnRegisterAndUse = new Button
            {
                Text = "Simpan & Gunakan",
                Font = UiTheme.FontBodyBold,
                BackColor = UiTheme.Emerald,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(216, 396),
                Width = 175,
                Height = 40,
                Cursor = Cursors.Hand
            };
            _btnRegisterAndUse.FlatAppearance.BorderSize = 0;
            _btnRegisterAndUse.Click += async (s, e) => await OnRegisterClickAsync();

            _btnCancel = new Button
            {
                Text = "Batal",
                DialogResult = DialogResult.Cancel,
                Font = UiTheme.FontBody,
                BackColor = UiTheme.White,
                ForeColor = UiTheme.Slate700,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(399, 396),
                Width = 90,
                Height = 40,
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderColor = UiTheme.Slate300;
            _btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            pnlBody.Controls.Add(lblBarcode);
            pnlBody.Controls.Add(_txtBarcode);
            pnlBody.Controls.Add(lblSearch);
            pnlBody.Controls.Add(_txtSearchItem);
            pnlBody.Controls.Add(_lstItems);
            pnlBody.Controls.Add(_lblSelectedItem);
            pnlBody.Controls.Add(lblSatuan);
            pnlBody.Controls.Add(_cboSatuan);
            pnlBody.Controls.Add(lblPengali);
            pnlBody.Controls.Add(_numPengali);
            pnlBody.Controls.Add(_btnRegisterAndUse);
            pnlBody.Controls.Add(_btnCancel);

            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);

            AcceptButton = _btnRegisterAndUse;
            CancelButton = _btnCancel;
        }

        public async Task SearchItemsAsync(string keyword)
        {
            if (_referenceRepo == null) return;

            try
            {
                var items = await _referenceRepo.SearchBarangByNameOrCodeAsync(keyword ?? string.Empty);
                _filteredItems = items?.Take(50).ToList() ?? new List<Barang>();

                _lstItems.Items.Clear();
                foreach (var item in _filteredItems)
                {
                    _lstItems.Items.Add($"[{item.BrgId}] {item.BrgName}");
                }

                if (_lstItems.Items.Count > 0 && _selectedBarang == null)
                {
                    _lstItems.SelectedIndex = 0;
                }
            }
            catch
            {
            }
        }

        public void SetItems(IEnumerable<Barang> items)
        {
            _filteredItems = items?.ToList() ?? new List<Barang>();
            _lstItems.Items.Clear();
            foreach (var item in _filteredItems)
            {
                _lstItems.Items.Add($"[{item.BrgId}] {item.BrgName}");
            }

            if (_lstItems.Items.Count > 0)
            {
                _lstItems.SelectedIndex = 0;
            }
        }

        private void OnItemSelected(object sender, EventArgs e)
        {
            if (_lstItems.SelectedIndex >= 0 && _lstItems.SelectedIndex < _filteredItems.Count)
            {
                _selectedBarang = _filteredItems[_lstItems.SelectedIndex];
                _lblSelectedItem.Text = $"Barang Terpilih: [{_selectedBarang.BrgId}] {_selectedBarang.BrgName}";

                // Auto-set unit dropdown with item units
                var units = _selectedBarang.GetAvailableUnits().ToList();
                if (units.Count > 0)
                {
                    _cboSatuan.Items.Clear();
                    foreach (var u in units)
                    {
                        _cboSatuan.Items.Add(u);
                    }
                    _cboSatuan.SelectedIndex = 0;
                }
            }
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (_lstItems.Items.Count > 0)
                {
                    int next = Math.Min(_lstItems.SelectedIndex + 1, _lstItems.Items.Count - 1);
                    _lstItems.SelectedIndex = next;
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (_lstItems.Items.Count > 0)
                {
                    int prev = Math.Max(_lstItems.SelectedIndex - 1, 0);
                    _lstItems.SelectedIndex = prev;
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                _cboSatuan.Focus();
            }
        }

        private void OnListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                _cboSatuan.Focus();
            }
        }

        public async Task<bool> OnRegisterClickAsync()
        {
            string barcode = _txtBarcode?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(barcode))
            {
                _audioHelper?.PlayWarning();
                MessageBox.Show("Barcode tidak boleh kosong.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_selectedBarang == null)
            {
                _audioHelper?.PlayWarning();
                MessageBox.Show("Silakan pilih barang master yang akan ditautkan.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string warehouseCode = _registryHelper?.WarehouseCode ?? "GDG-PST";
            string operatorName = _registryHelper?.LastOperatorName ?? "OPERATOR";
            string satuan = _cboSatuan.SelectedItem?.ToString() ?? "Dus";
            decimal pengali = _numPengali.Value;

            try
            {
                if (_barcodeService != null)
                {
                    await _barcodeService.QuickRegisterUnmappedBarcodeAsync(
                        barcode,
                        _selectedBarang.BrgId,
                        satuan,
                        warehouseCode,
                        operatorName,
                        pengali);
                }
                else if (_barcodeRepo != null)
                {
                    var barcodeObj = new Barcode(barcode, _selectedBarang.BrgId, _selectedBarang.BrgName, satuan, pengali, true);
                    await _barcodeRepo.InsertBarcodeAsync(barcodeObj);

                    var reg = BarcodeRegistration.Create(barcode, _selectedBarang.BrgId, satuan, warehouseCode, operatorName);
                    await _barcodeRepo.QueueRegistrationAsync(reg);
                }

                _registeredResult = BarcodeLookupResult.Hit(
                    rawInput: barcode,
                    normalizedBarcode: barcode,
                    brgId: _selectedBarang.BrgId,
                    brgName: _selectedBarang.BrgName,
                    satuan: satuan,
                    pengali: pengali,
                    isActive: true);

                _audioHelper?.PlaySuccess();
                DialogResult = DialogResult.OK;
                Close();
                return true;
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                MessageBox.Show($"Gagal mendaftarkan barcode: {ex.Message}", "Error Registrasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
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
