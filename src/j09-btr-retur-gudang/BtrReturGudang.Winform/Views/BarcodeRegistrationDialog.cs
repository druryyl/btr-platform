using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;

namespace BtrReturGudang.Winform.Views
{
    /// <summary>
    /// Modal dialog for registering a new barcode or linking an unmapped barcode to a master product.
    /// Traceable to: ARCHITECTURE §5.2 SCR-04/SCR-06, TD-006, P3-S10.
    /// </summary>
    public class BarcodeRegistrationDialog : Form
    {
        private readonly IBarcodeService _barcodeService;
        private readonly IBarcodeRepo _barcodeRepo;
        private readonly IReferenceRepo _referenceRepo;
        private readonly IRegistryHelper _registryHelper;
        private readonly IAudioHelper _audioHelper;

        private TextBox _txtBarcode;
        private TextBox _txtSearchItem;
        private ListBox _lstItems;
        private Label _lblSelectedItem;
        private ComboBox _cboSatuan;
        private NumericUpDown _numPengali;
        private Button _btnSave;
        private Button _btnCancel;

        private List<Barang> _filteredItems = new List<Barang>();
        private Barang _selectedBarang = null;

        public string RegisteredBarcode => _txtBarcode?.Text?.Trim();
        public string RegisteredBrgId => _selectedBarang?.BrgId;
        public string RegisteredBrgName => _selectedBarang?.BrgName;
        public string RegisteredSatuan => _cboSatuan?.SelectedItem?.ToString();
        public decimal RegisteredPengali => _numPengali?.Value ?? 1m;

        public BarcodeRegistrationDialog(
            string initialBarcode = null,
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

            InitializeComponent(initialBarcode);
            Load += async (s, e) => await SearchItemsAsync(string.Empty);
        }

        private void InitializeComponent(string initialBarcode)
        {
            Text = "Pendaftaran Barcode Baru";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 520);
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

            // Title Header
            var lblTitle = new Label
            {
                Text = "Registrasi Barcode Produk",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(24, 16),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Tautkan barcode ke master barang dan tentukan satuan konversinya.",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(24, 44),
                AutoSize = true
            };

            // Barcode input
            var lblBarcode = new Label
            {
                Text = "Nilai Barcode:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Location = new Point(24, 76),
                AutoSize = true
            };

            _txtBarcode = new TextBox
            {
                Text = initialBarcode ?? string.Empty,
                Font = new Font("Consolas", 11F, FontStyle.Bold, GraphicsUnit.Point),
                Location = new Point(24, 98),
                Width = 472
            };

            // Product lookup
            var lblSearch = new Label
            {
                Text = "Cari Master Barang (Nama / Kode):",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Location = new Point(24, 136),
                AutoSize = true
            };

            _txtSearchItem = new TextBox
            {
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(24, 158),
                Width = 472
            };
            _txtSearchItem.TextChanged += async (s, e) => await SearchItemsAsync(_txtSearchItem.Text);

            _lstItems = new ListBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(24, 190),
                Width = 472,
                Height = 110
            };
            _lstItems.SelectedIndexChanged += OnItemSelected;

            _lblSelectedItem = new Label
            {
                Text = "Barang Terpilih: (Belum dipilih)",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235), // Blue-600
                Location = new Point(24, 308),
                AutoSize = true
            };

            // Unit & Multiplier Strip
            var lblSatuan = new Label
            {
                Text = "Satuan:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Location = new Point(24, 338),
                AutoSize = true
            };

            _cboSatuan = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(24, 360),
                Width = 140
            };
            _cboSatuan.Items.AddRange(new object[] { "Dus", "Pak", "Pcs", "Ball", "Lusin" });
            _cboSatuan.SelectedIndex = 0;

            var lblPengali = new Label
            {
                Text = "Pengali (Konversi ke Pcs):",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Location = new Point(190, 338),
                AutoSize = true
            };

            _numPengali = new NumericUpDown
            {
                DecimalPlaces = 0,
                Minimum = 1,
                Maximum = 10000,
                Value = 1,
                Location = new Point(190, 360),
                Width = 140
            };

            // Buttons
            _btnSave = new Button
            {
                Text = "Simpan & Daftarkan",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(37, 99, 235), // Blue-600
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(240, 440),
                Width = 150,
                Height = 38,
                Cursor = Cursors.Hand
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (s, e) => await OnSaveClickAsync();

            _btnCancel = new Button
            {
                Text = "Batal",
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(402, 440),
                Width = 94,
                Height = 38,
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderSize = 0;

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            Controls.Add(lblTitle);
            Controls.Add(lblSubtitle);
            Controls.Add(lblBarcode);
            Controls.Add(_txtBarcode);
            Controls.Add(lblSearch);
            Controls.Add(_txtSearchItem);
            Controls.Add(_lstItems);
            Controls.Add(_lblSelectedItem);
            Controls.Add(lblSatuan);
            Controls.Add(_cboSatuan);
            Controls.Add(lblPengali);
            Controls.Add(_numPengali);
            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);
        }

        private async Task SearchItemsAsync(string keyword)
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

        private void OnItemSelected(object sender, EventArgs e)
        {
            if (_lstItems.SelectedIndex >= 0 && _lstItems.SelectedIndex < _filteredItems.Count)
            {
                _selectedBarang = _filteredItems[_lstItems.SelectedIndex];
                _lblSelectedItem.Text = $"Barang Terpilih: [{_selectedBarang.BrgId}] {_selectedBarang.BrgName}";

                // Auto-set satuan options if item provides them
                if (!string.IsNullOrWhiteSpace(_selectedBarang.Satuan1))
                {
                    _cboSatuan.SelectedItem = _selectedBarang.Satuan1;
                }
            }
        }

        private async Task OnSaveClickAsync()
        {
            string rawBarcode = _txtBarcode.Text?.Trim();
            if (string.IsNullOrWhiteSpace(rawBarcode))
            {
                MessageBox.Show("Barcode tidak boleh kosong.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedBarang == null)
            {
                MessageBox.Show("Silakan pilih barang master yang akan ditautkan.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string warehouseCode = _registryHelper?.WarehouseCode ?? "GDG-PST";
            string operatorName = _registryHelper?.LastOperatorName ?? "Operator-1";
            string satuan = _cboSatuan.SelectedItem?.ToString() ?? "Dus";
            decimal pengali = _numPengali.Value;

            try
            {
                if (_barcodeService != null)
                {
                    await _barcodeService.QuickRegisterUnmappedBarcodeAsync(
                        rawBarcode,
                        _selectedBarang.BrgId,
                        satuan,
                        warehouseCode,
                        operatorName,
                        pengali);
                }
                else if (_barcodeRepo != null)
                {
                    var barcodeObj = new Barcode(rawBarcode, _selectedBarang.BrgId, _selectedBarang.BrgName, satuan, pengali, true);
                    await _barcodeRepo.InsertBarcodeAsync(barcodeObj);

                    var reg = BarcodeRegistration.Create(rawBarcode, _selectedBarang.BrgId, satuan, warehouseCode, operatorName);
                    await _barcodeRepo.QueueRegistrationAsync(reg);
                }

                _audioHelper?.PlaySuccess();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                MessageBox.Show($"Gagal mendaftarkan barcode: {ex.Message}", "Error Registrasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
