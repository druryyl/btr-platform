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
            ClientSize = new Size(580, 480);
            BackColor = UiTheme.Slate100;
            Font = UiTheme.FontBody;

            // 1. Title Header
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = UiTheme.Slate900,
                Padding = new Padding(20, 4, 20, 4)
            };

            var lblTitle = new Label
            {
                Text = "🏷️ Registrasi Barcode Produk",
                Font = UiTheme.FontSectionTitle,
                ForeColor = Color.White,
                Location = new Point(20, 4),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Tautkan barcode ke master barang dan tentukan satuan konversinya.",
                Font = UiTheme.FontSubtitle,
                ForeColor = UiTheme.Slate400,
                Location = new Point(20, 24),
                AutoSize = true
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);

            // Card body container
            var pnlBody = UiTheme.CreateCard(new Padding(16, 10, 16, 10), new Padding(16, 8, 16, 12));
            pnlBody.Dock = DockStyle.Fill;

            // Barcode input
            var lblBarcode = new Label
            {
                Text = "Nilai Barcode:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(16, 10),
                AutoSize = true
            };

            _txtBarcode = new TextBox
            {
                Text = initialBarcode ?? string.Empty,
                Font = UiTheme.FontMonospaceBold,
                Location = new Point(16, 30),
                Width = 516
            };

            // Product lookup
            var lblSearch = new Label
            {
                Text = "Cari Master Barang (Nama / Kode):",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(16, 60),
                AutoSize = true
            };

            _txtSearchItem = new TextBox
            {
                Font = UiTheme.FontBody,
                Location = new Point(16, 80),
                Width = 516
            };
            _txtSearchItem.TextChanged += async (s, e) => await SearchItemsAsync(_txtSearchItem.Text);

            _lstItems = new ListBox
            {
                Font = UiTheme.FontBody,
                Location = new Point(16, 108),
                Width = 516,
                Height = 100,
                BorderStyle = BorderStyle.FixedSingle
            };
            _lstItems.SelectedIndexChanged += OnItemSelected;

            _lblSelectedItem = new Label
            {
                Text = "Barang Terpilih: (Belum dipilih)",
                Font = UiTheme.FontBodyBold,
                ForeColor = UiTheme.Primary,
                Location = new Point(16, 214),
                AutoSize = true
            };

            // Unit & Multiplier Strip
            var lblSatuan = new Label
            {
                Text = "Satuan:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(16, 238),
                AutoSize = true
            };

            _cboSatuan = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.FontBody,
                Location = new Point(16, 258),
                Width = 160
            };
            _cboSatuan.Items.AddRange(new object[] { "Dus", "Pak", "Pcs", "Ball", "Lusin" });
            _cboSatuan.SelectedIndex = 0;

            var lblPengali = new Label
            {
                Text = "Pengali (Konversi ke Pcs):",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Location = new Point(200, 238),
                AutoSize = true
            };

            _numPengali = new NumericUpDown
            {
                DecimalPlaces = 0,
                Minimum = 1,
                Maximum = 10000,
                Value = 1,
                Font = UiTheme.FontBodyBold,
                Location = new Point(200, 258),
                Width = 160
            };

            // Buttons
            _btnSave = new Button
            {
                Text = "Simpan & Daftarkan",
                Font = UiTheme.FontBodyBold,
                BackColor = UiTheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(226, 308),
                Width = 185,
                Height = 38,
                Cursor = Cursors.Hand
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += async (s, e) => await OnSaveClickAsync();

            _btnCancel = new Button
            {
                Text = "Batal",
                DialogResult = DialogResult.Cancel,
                Font = UiTheme.FontBody,
                BackColor = UiTheme.White,
                ForeColor = UiTheme.Slate700,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(421, 308),
                Width = 111,
                Height = 38,
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderColor = UiTheme.Slate300;

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

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
            pnlBody.Controls.Add(_btnSave);
            pnlBody.Controls.Add(_btnCancel);

            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
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
