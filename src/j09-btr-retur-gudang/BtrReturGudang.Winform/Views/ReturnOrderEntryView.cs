using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Winform.Controls;

namespace BtrReturGudang.Winform.Views
{
    /// <summary>
    /// SCR-02: Core Return Order Intake &amp; Quantity Strip view.
    /// Hosts customer autocomplete, active scanner intake banner, ergonomic Quantity Strip,
    /// double-buffered intake DataGridView, and action toolbar.
    /// Traceable to: ARCHITECTURE §5.2 SCR-02, §6, §10, TD-011, P3-S08.
    /// </summary>
    public class ReturnOrderEntryView : UserControl, IScannerConsumer
    {
        // Application & Infrastructure Dependencies
        private readonly IReturnOrderService _returnOrderService;
        private readonly IBarcodeService _barcodeService;
        private readonly IReferenceRepo _referenceRepo;
        private readonly IAudioHelper _audioHelper;
        private readonly IRegistryHelper _registryHelper;

        // Active State
        private BarcodeLookupResult _currentScanResult;
        private readonly BindingList<ReturnOrderItemDto> _itemsList = new BindingList<ReturnOrderItemDto>();
        private decimal _totalQtyBagus;
        private decimal _totalQtyRusak;

        // UI Controls - Header
        private ComboBox _cboCustomer;
        private ComboBox _cboDriver;
        private ComboBox _cboSalesPerson;
        private TextBox _txtKeterangan;

        // UI Controls - Scanner Intake Banner
        private Panel _pnlScannerBanner;
        private Label _lblBannerTitle;
        private Label _lblBannerDetails;
        private Label _lblBannerStatus;

        // UI Controls - Ergonomic Quantity Strip
        private ComboBox _cboSatuan;
        private NumericUpDown _numQtyBagus;
        private NumericUpDown _numQtyRusak;
        private TextBox _txtItemKeterangan;
        private Button _btnAddItem;

        // UI Controls - DataGridView
        private DoubleBufferedDataGridView _dgvItems;

        // UI Controls - Summary Footer
        private Label _lblSummaryItems;
        private Label _lblSummaryBagus;
        private Label _lblSummaryRusak;
        private Label _lblSummaryTotal;

        // UI Controls - Action Toolbar
        private Button _btnSaveDraft;
        private Button _btnSaveAndUpload;
        private Button _btnReset;
        private Button _btnManualSearch;

        // Exposed properties for state inspection & testing
        public string CurrentReturnOrderId { get; set; }
        public BindingList<ReturnOrderItemDto> ItemsList => _itemsList;
        public BarcodeLookupResult CurrentScanResult => _currentScanResult;
        public ComboBox CustomerComboBox => _cboCustomer;
        public ComboBox DriverComboBox => _cboDriver;
        public ComboBox SalesPersonComboBox => _cboSalesPerson;
        public TextBox NotesTextBox => _txtKeterangan;
        public NumericUpDown QtyBagusNumeric => _numQtyBagus;
        public NumericUpDown QtyRusakNumeric => _numQtyRusak;
        public ComboBox SatuanComboBox => _cboSatuan;
        public DoubleBufferedDataGridView ItemsGridView => _dgvItems;
        public decimal TotalQtyBagus => _totalQtyBagus;
        public decimal TotalQtyRusak => _totalQtyRusak;
        public int TotalItemsCount => _itemsList.Count;
        public Label BannerTitleLabel => _lblBannerTitle;
        public Label BannerDetailsLabel => _lblBannerDetails;
        public Label SummaryTotalItemsLabel => _lblSummaryItems;
        public Label SummaryQtyBagusLabel => _lblSummaryBagus;
        public Label SummaryQtyRusakLabel => _lblSummaryRusak;

        // Dialog hooks and automation configuration for SCR-03 and SCR-04
        public bool AutoLaunchRegistrationDialog { get; set; } = true;
        public bool ForceShowModals { get; set; } = false;
        public bool SuppressMessageBoxes { get; set; } = false;
        public Func<string, BarcodeLookupResult> QuickRegistrationDialogLauncher { get; set; }
        public Func<Barang> ManualSearchDialogLauncher { get; set; }

        /// <summary>
        /// DI Constructor resolving required service and repository contracts.
        /// </summary>
        public ReturnOrderEntryView(
            IReturnOrderService returnOrderService,
            IBarcodeService barcodeService,
            IReferenceRepo referenceRepo,
            IAudioHelper audioHelper,
            IRegistryHelper registryHelper)
        {
            _returnOrderService = returnOrderService;
            _barcodeService = barcodeService;
            _referenceRepo = referenceRepo;
            _audioHelper = audioHelper;
            _registryHelper = registryHelper;

            InitializeComponent();
            LoadReferenceData();
        }

        /// <summary>
        /// Parameterless constructor for WinForms designer and headless test instances.
        /// </summary>
        public ReturnOrderEntryView() : this(null, null, null, null, null)
        {
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(241, 245, 249); // Slate-100
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

            // Root panel layout
            var pnlMainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // 1. Header (Customer, Driver, Salesman, Notes)
            var pnlHeader = BuildHeaderArea();

            // 2. Intake Control Card (Scanner Banner + Ergonomic Quantity Strip)
            var pnlIntakeCard = BuildIntakeCard();

            // 3. Grid + Summary Footer Container
            var pnlGridContainer = BuildGridArea();

            // 4. Action Toolbar (Bottom)
            var pnlToolbar = BuildActionToolbar();

            // Assemble controls (docking order: Bottom toolbar, Bottom summary, Fill grid, Top intake, Top header)
            pnlMainContainer.Controls.Add(pnlGridContainer);
            pnlMainContainer.Controls.Add(pnlIntakeCard);
            pnlMainContainer.Controls.Add(pnlHeader);

            Controls.Add(pnlMainContainer);
            Controls.Add(pnlToolbar);
        }

        #region Header Construction

        private Panel BuildHeaderArea()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 125,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12),
                Margin = new Padding(0, 0, 0, 12)
            };

            // Section Title
            var lblTitle = new Label
            {
                Text = "Intake Retur Gudang [F1]",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42), // Slate-900
                Location = new Point(16, 10),
                AutoSize = true
            };

            // Row 1: Customer, Driver, Salesman
            var lblCustomer = new Label
            {
                Text = "Customer *:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(16, 42),
                AutoSize = true
            };

            _cboCustomer = new ComboBox
            {
                Location = new Point(95, 38),
                Width = 280,
                Font = new Font("Segoe UI", 9.5F),
                DropDownStyle = ComboBoxStyle.DropDown
            };

            if (System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.STA)
            {
                _cboCustomer.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                _cboCustomer.AutoCompleteSource = AutoCompleteSource.ListItems;
            }

            var lblDriver = new Label
            {
                Text = "Driver:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(395, 42),
                AutoSize = true
            };

            _cboDriver = new ComboBox
            {
                Location = new Point(450, 38),
                Width = 200,
                Font = new Font("Segoe UI", 9.5F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblSalesman = new Label
            {
                Text = "Salesman:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(670, 42),
                AutoSize = true
            };

            _cboSalesPerson = new ComboBox
            {
                Location = new Point(745, 38),
                Width = 200,
                Font = new Font("Segoe UI", 9.5F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Row 2: Catatan / Keterangan
            var lblNotes = new Label
            {
                Text = "Catatan Retur:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(16, 80),
                AutoSize = true
            };

            _txtKeterangan = new TextBox
            {
                Location = new Point(115, 76),
                Width = 830,
                Font = new Font("Segoe UI", 9.5F)
            };

            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblCustomer);
            pnl.Controls.Add(_cboCustomer);
            pnl.Controls.Add(lblDriver);
            pnl.Controls.Add(_cboDriver);
            pnl.Controls.Add(lblSalesman);
            pnl.Controls.Add(_cboSalesPerson);
            pnl.Controls.Add(lblNotes);
            pnl.Controls.Add(_txtKeterangan);

            return pnl;
        }

        #endregion

        #region Intake Card (Scanner Banner + Ergonomic Quantity Strip)

        private Panel BuildIntakeCard()
        {
            var pnlCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = Color.White,
                Padding = new Padding(16, 10, 16, 10),
                Margin = new Padding(0, 8, 0, 12)
            };

            // 1. Scanner Intake Focus Banner
            _pnlScannerBanner = new Panel
            {
                Location = new Point(16, 10),
                Width = 930,
                Height = 62,
                BackColor = Color.FromArgb(30, 41, 59), // Slate-800
                Padding = new Padding(12, 8, 12, 8)
            };

            _lblBannerTitle = new Label
            {
                Text = "Menunggu scan barcode scanner USB atau tekan F2 untuk pencarian produk...",
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(226, 232, 240), // Slate-200
                Location = new Point(12, 8),
                AutoSize = true
            };

            _lblBannerDetails = new Label
            {
                Text = "Barcode: -  |  Satuan Default: -  |  Pengali: -",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184), // Slate-400
                Location = new Point(12, 33),
                AutoSize = true
            };

            _lblBannerStatus = new Label
            {
                Text = "STATUS: STANDBY",
                Font = new Font("Segoe UI Bold", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129), // Emerald
                Location = new Point(780, 8),
                AutoSize = true
            };

            _pnlScannerBanner.Controls.Add(_lblBannerTitle);
            _pnlScannerBanner.Controls.Add(_lblBannerDetails);
            _pnlScannerBanner.Controls.Add(_lblBannerStatus);

            // 2. Ergonomic Quantity Strip
            var pnlQuantityStrip = new Panel
            {
                Location = new Point(16, 80),
                Width = 930,
                Height = 60,
                BackColor = Color.FromArgb(248, 250, 252) // Slate-50
            };

            var lblSatuan = new Label
            {
                Text = "Satuan:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(10, 18),
                AutoSize = true
            };

            _cboSatuan = new ComboBox
            {
                Location = new Point(62, 14),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI Semibold", 9.5F)
            };

            var lblBagus = new Label
            {
                Text = "Qty BAGUS:",
                Font = new Font("Segoe UI Bold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(5, 150, 105), // Emerald-600
                Location = new Point(176, 18),
                AutoSize = true
            };

            _numQtyBagus = new NumericUpDown
            {
                Location = new Point(260, 14),
                Width = 85,
                Minimum = 0,
                Maximum = 999999,
                DecimalPlaces = 0,
                Font = new Font("Segoe UI Bold", 11F),
                ForeColor = Color.FromArgb(5, 150, 105), // Emerald
                BackColor = Color.FromArgb(236, 253, 245), // Emerald-50
                TextAlign = HorizontalAlignment.Right
            };
            _numQtyBagus.KeyDown += OnQuantityKeyDown;

            var lblRusak = new Label
            {
                Text = "Qty RUSAK:",
                Font = new Font("Segoe UI Bold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(225, 29, 72), // Rose-600
                Location = new Point(360, 18),
                AutoSize = true
            };

            _numQtyRusak = new NumericUpDown
            {
                Location = new Point(444, 14),
                Width = 85,
                Minimum = 0,
                Maximum = 999999,
                DecimalPlaces = 0,
                Font = new Font("Segoe UI Bold", 11F),
                ForeColor = Color.FromArgb(225, 29, 72), // Rose
                BackColor = Color.FromArgb(255, 241, 242), // Rose-50
                TextAlign = HorizontalAlignment.Right
            };
            _numQtyRusak.KeyDown += OnQuantityKeyDown;

            var lblItemNote = new Label
            {
                Text = "Ket:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(545, 18),
                AutoSize = true
            };

            _txtItemKeterangan = new TextBox
            {
                Location = new Point(580, 14),
                Width = 180,
                Font = new Font("Segoe UI", 9.5F)
            };
            _txtItemKeterangan.KeyDown += OnQuantityKeyDown;

            _btnAddItem = new Button
            {
                Text = "+ Tambah [Enter]",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(37, 99, 235), // Blue-600
                FlatStyle = FlatStyle.Flat,
                Location = new Point(775, 11),
                Width = 140,
                Height = 34,
                Cursor = Cursors.Hand
            };
            _btnAddItem.FlatAppearance.BorderSize = 0;
            _btnAddItem.Click += (s, e) => CommitQuantityStripToGrid();

            pnlQuantityStrip.Controls.Add(lblSatuan);
            pnlQuantityStrip.Controls.Add(_cboSatuan);
            pnlQuantityStrip.Controls.Add(lblBagus);
            pnlQuantityStrip.Controls.Add(_numQtyBagus);
            pnlQuantityStrip.Controls.Add(lblRusak);
            pnlQuantityStrip.Controls.Add(_numQtyRusak);
            pnlQuantityStrip.Controls.Add(lblItemNote);
            pnlQuantityStrip.Controls.Add(_txtItemKeterangan);
            pnlQuantityStrip.Controls.Add(_btnAddItem);

            pnlCard.Controls.Add(_pnlScannerBanner);
            pnlCard.Controls.Add(pnlQuantityStrip);

            return pnlCard;
        }

        #endregion

        #region Grid Area Construction

        private Panel BuildGridArea()
        {
            var pnlContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 12, 0, 0)
            };

            // Summary Footer
            var pnlFooter = BuildSummaryFooter();

            // DataGridView setup
            _dgvItems = new DoubleBufferedDataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = _itemsList
            };

            ConfigureGridColumns();
            _dgvItems.CellFormatting += OnGridCellFormatting;
            _dgvItems.CellContentClick += OnGridCellContentClick;
            _dgvItems.KeyDown += OnGridKeyDown;

            pnlContainer.Controls.Add(_dgvItems);
            pnlContainer.Controls.Add(pnlFooter);

            return pnlContainer;
        }

        private void ConfigureGridColumns()
        {
            _dgvItems.Columns.Clear();

            var colNo = new DataGridViewTextBoxColumn
            {
                Name = "colNo",
                HeaderText = "No",
                DataPropertyName = "NoUrut",
                Width = 45,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            var colBarcode = new DataGridViewTextBoxColumn
            {
                Name = "colBarcode",
                HeaderText = "Barcode",
                DataPropertyName = "Barcode",
                Width = 130,
                ReadOnly = true
            };

            var colBrgCode = new DataGridViewTextBoxColumn
            {
                Name = "colBrgCode",
                HeaderText = "Kode Barang",
                DataPropertyName = "BrgId",
                Width = 110,
                ReadOnly = true
            };

            var colBrgName = new DataGridViewTextBoxColumn
            {
                Name = "colBrgName",
                HeaderText = "Nama Barang",
                DataPropertyName = "BrgName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 200,
                ReadOnly = true
            };

            var colJenis = new DataGridViewTextBoxColumn
            {
                Name = "colJenisRetur",
                HeaderText = "Jenis Retur",
                DataPropertyName = "JenisRetur",
                Width = 110,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            var colQty = new DataGridViewTextBoxColumn
            {
                Name = "colQty",
                HeaderText = "Qty",
                DataPropertyName = "Qty",
                Width = 70,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N0" }
            };

            var colSatuan = new DataGridViewTextBoxColumn
            {
                Name = "colSatId",
                HeaderText = "Satuan",
                DataPropertyName = "SatId",
                Width = 80,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            var colKeterangan = new DataGridViewTextBoxColumn
            {
                Name = "colKeterangan",
                HeaderText = "Keterangan",
                DataPropertyName = "Keterangan",
                Width = 160,
                ReadOnly = true
            };

            var colAction = new DataGridViewButtonColumn
            {
                Name = "colAction",
                HeaderText = "Aksi",
                Text = "Hapus",
                UseColumnTextForButtonValue = true,
                Width = 75,
                FlatStyle = FlatStyle.Flat
            };

            _dgvItems.Columns.AddRange(
                colNo,
                colBarcode,
                colBrgCode,
                colBrgName,
                colJenis,
                colQty,
                colSatuan,
                colKeterangan,
                colAction);
        }

        private Panel BuildSummaryFooter()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.FromArgb(30, 41, 59), // Slate-800
                Padding = new Padding(16, 8, 16, 8)
            };

            _lblSummaryItems = new Label
            {
                Text = "Total Item: 0 baris",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(226, 232, 240),
                Location = new Point(16, 12),
                AutoSize = true
            };

            _lblSummaryBagus = new Label
            {
                Text = "Total Qty Bagus: 0",
                Font = new Font("Segoe UI Bold", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129), // Emerald
                Location = new Point(250, 11),
                AutoSize = true
            };

            _lblSummaryRusak = new Label
            {
                Text = "Total Qty Rusak: 0",
                Font = new Font("Segoe UI Bold", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(244, 63, 94), // Rose
                Location = new Point(480, 11),
                AutoSize = true
            };

            _lblSummaryTotal = new Label
            {
                Text = "Total Kuantitas: 0",
                Font = new Font("Segoe UI Bold", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(147, 197, 253), // Blue-300
                Location = new Point(720, 11),
                AutoSize = true
            };

            pnl.Controls.Add(_lblSummaryItems);
            pnl.Controls.Add(_lblSummaryBagus);
            pnl.Controls.Add(_lblSummaryRusak);
            pnl.Controls.Add(_lblSummaryTotal);

            return pnl;
        }

        #endregion

        #region Action Toolbar Construction

        private Panel BuildActionToolbar()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Color.FromArgb(15, 23, 42), // Slate-900
                Padding = new Padding(20, 10, 20, 10)
            };

            _btnSaveDraft = new Button
            {
                Text = "F9  Simpan Draft",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(51, 65, 85), // Slate-700
                FlatStyle = FlatStyle.Flat,
                Location = new Point(20, 10),
                Width = 150,
                Height = 36,
                Cursor = Cursors.Hand
            };
            _btnSaveDraft.FlatAppearance.BorderSize = 0;
            _btnSaveDraft.Click += async (s, e) => await SaveDraftAsync();

            _btnSaveAndUpload = new Button
            {
                Text = "F10  Simpan & Upload",
                Font = new Font("Segoe UI Bold", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(16, 185, 129), // Emerald-500
                FlatStyle = FlatStyle.Flat,
                Location = new Point(180, 10),
                Width = 180,
                Height = 36,
                Cursor = Cursors.Hand
            };
            _btnSaveAndUpload.FlatAppearance.BorderSize = 0;
            _btnSaveAndUpload.Click += async (s, e) => await SaveAndUploadAsync();

            _btnReset = new Button
            {
                Text = "Esc  Reset Form",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(203, 213, 225),
                BackColor = Color.FromArgb(30, 41, 59), // Slate-800
                FlatStyle = FlatStyle.Flat,
                Location = new Point(370, 10),
                Width = 140,
                Height = 36,
                Cursor = Cursors.Hand
            };
            _btnReset.FlatAppearance.BorderSize = 0;
            _btnReset.Click += (s, e) => ResetForm(promptUser: true);

            _btnManualSearch = new Button
            {
                Text = "F2  Cari Barang",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(217, 119, 6), // Amber-600
                FlatStyle = FlatStyle.Flat,
                Location = new Point(520, 10),
                Width = 140,
                Height = 36,
                Cursor = Cursors.Hand
            };
            _btnManualSearch.FlatAppearance.BorderSize = 0;
            _btnManualSearch.Click += (s, e) => TriggerManualItemSearch();

            pnl.Controls.Add(_btnSaveDraft);
            pnl.Controls.Add(_btnSaveAndUpload);
            pnl.Controls.Add(_btnReset);
            pnl.Controls.Add(_btnManualSearch);

            return pnl;
        }

        #endregion

        #region Reference Data Loading

        public void LoadReferenceData()
        {
            if (_referenceRepo == null) return;

            try
            {
                // 1. Customers
                var customers = _referenceRepo.SearchCustomer(string.Empty) ?? Enumerable.Empty<Customer>();
                _cboCustomer.Items.Clear();
                foreach (var cust in customers)
                {
                    _cboCustomer.Items.Add(new CustomerItem(cust.CustId, cust.CustName));
                }

                // 2. Drivers
                var drivers = _referenceRepo.GetAllDrivers() ?? Enumerable.Empty<Driver>();
                _cboDriver.Items.Clear();
                _cboDriver.Items.Add(new KeyValueItem(null, "-- Pilih Driver (Opsional) --"));
                foreach (var drv in drivers)
                {
                    _cboDriver.Items.Add(new KeyValueItem(drv.DriverId, drv.DriverName));
                }
                _cboDriver.SelectedIndex = 0;

                // 3. SalesPersons
                var salesPersons = _referenceRepo.GetAllSalesPersons() ?? Enumerable.Empty<SalesPerson>();
                _cboSalesPerson.Items.Clear();
                _cboSalesPerson.Items.Add(new KeyValueItem(null, "-- Pilih Salesman (Opsional) --"));
                foreach (var sp in salesPersons)
                {
                    _cboSalesPerson.Items.Add(new KeyValueItem(sp.SalesPersonId, sp.SalesPersonName));
                }
                _cboSalesPerson.SelectedIndex = 0;
            }
            catch
            {
                // Fallback for disconnected mode
            }
        }

        #endregion

        #region Barcode Scanner Consumption (IScannerConsumer)

        public void OnBarcodeScanned(string normalizedBarcode)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(OnBarcodeScanned), normalizedBarcode);
                return;
            }

            if (string.IsNullOrWhiteSpace(normalizedBarcode))
                return;

            var lookup = _barcodeService?.LookupBarcode(normalizedBarcode);
            if (lookup != null && lookup.Found)
            {
                _currentScanResult = lookup;
                UpdateScannerBannerHit(lookup);
                PopulatePackagingUnits(lookup);

                // Auto-focuses numQtyBagus immediately upon successful barcode scan
                _numQtyBagus.Value = 0;
                _numQtyRusak.Value = 0;
                _numQtyBagus.Focus();
                _numQtyBagus.Select(0, _numQtyBagus.Text.Length);
            }
            else
            {
                _currentScanResult = null;
                UpdateScannerBannerMiss(normalizedBarcode);
                _audioHelper?.PlayWarning();

                // Trigger QuickBarcodeRegistrationDialog (SCR-04) automatically
                if (QuickRegistrationDialogLauncher != null)
                {
                    var registered = QuickRegistrationDialogLauncher(normalizedBarcode);
                    if (registered != null)
                    {
                        ApplyActiveProduct(registered);
                    }
                }
                else if (AutoLaunchRegistrationDialog && (ForceShowModals || (IsHandleCreated && Parent != null)))
                {
                    using (var dlg = new QuickBarcodeRegistrationDialog(
                        unmappedBarcode: normalizedBarcode,
                        barcodeService: _barcodeService,
                        barcodeRepo: null,
                        referenceRepo: _referenceRepo,
                        registryHelper: _registryHelper,
                        audioHelper: _audioHelper))
                    {
                        IWin32Window owner = (Control)FindForm() ?? (IsHandleCreated ? this : null);
                        var res = owner != null ? dlg.ShowDialog(owner) : dlg.ShowDialog();
                        if (res == DialogResult.OK && dlg.RegisteredResult != null)
                        {
                            ApplyActiveProduct(dlg.RegisteredResult);
                        }
                    }
                }
            }
        }

        public void ApplyActiveProduct(BarcodeLookupResult result)
        {
            if (result == null) return;
            _currentScanResult = result;
            UpdateScannerBannerHit(result);
            PopulatePackagingUnits(result);

            _numQtyBagus.Value = 0;
            _numQtyRusak.Value = 0;
            _numQtyBagus.Focus();
            _numQtyBagus.Select(0, _numQtyBagus.Text.Length);
        }

        private void UpdateScannerBannerHit(BarcodeLookupResult result)
        {
            _pnlScannerBanner.BackColor = Color.FromArgb(15, 23, 42); // Slate-900
            _lblBannerTitle.Text = $"📦 [{result.BrgId}] {result.BrgName}";
            _lblBannerTitle.ForeColor = Color.FromArgb(248, 250, 252);

            var barcodeDisplay = !string.IsNullOrWhiteSpace(result.NormalizedBarcode)
                ? result.NormalizedBarcode
                : (!string.IsNullOrWhiteSpace(result.BarcodeValue) ? result.BarcodeValue : "(Manual / Tanpa Barcode)");

            _lblBannerDetails.Text = $"Barcode: {barcodeDisplay}  |  Satuan: {result.Satuan ?? "-"}  |  Pengali: x{result.Pengali:N0}";
            _lblBannerDetails.ForeColor = Color.FromArgb(148, 163, 184);

            _lblBannerStatus.Text = "BARCODE TERDETEKSI";
            _lblBannerStatus.ForeColor = Color.FromArgb(16, 185, 129); // Emerald
        }

        private void UpdateScannerBannerMiss(string barcode)
        {
            _pnlScannerBanner.BackColor = Color.FromArgb(69, 10, 10); // Rose-950
            _lblBannerTitle.Text = $"⚠️ Barcode tidak dikenal: [{barcode}]";
            _lblBannerTitle.ForeColor = Color.FromArgb(254, 205, 211); // Rose-200

            _lblBannerDetails.Text = "Item tidak terdaftar di katalog lokal. Tekan F2 untuk cari barang manual.";
            _lblBannerDetails.ForeColor = Color.FromArgb(253, 164, 175);

            _lblBannerStatus.Text = "TIDAK DITEMUKAN";
            _lblBannerStatus.ForeColor = Color.FromArgb(244, 63, 94); // Rose-500
        }

        private void PopulatePackagingUnits(BarcodeLookupResult lookup)
        {
            _cboSatuan.Items.Clear();

            var units = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(lookup.Satuan))
            {
                units.Add(lookup.Satuan);
            }

            // Query product definition for all alternative packaging units
            if (_referenceRepo != null && !string.IsNullOrWhiteSpace(lookup.BrgId))
            {
                try
                {
                    var barang = _referenceRepo.GetBarangById(lookup.BrgId);
                    if (barang != null)
                    {
                        if (!string.IsNullOrWhiteSpace(barang.Satuan1)) units.Add(barang.Satuan1);
                        if (!string.IsNullOrWhiteSpace(barang.Satuan2)) units.Add(barang.Satuan2);
                        if (!string.IsNullOrWhiteSpace(barang.Satuan3)) units.Add(barang.Satuan3);
                    }
                }
                catch
                {
                    // Fallback
                }
            }

            if (units.Count == 0)
            {
                units.Add("Pcs");
            }

            foreach (var u in units)
            {
                _cboSatuan.Items.Add(u);
            }

            // Select matching unit or first available
            if (!string.IsNullOrWhiteSpace(lookup.Satuan) && _cboSatuan.Items.Contains(lookup.Satuan))
            {
                _cboSatuan.SelectedItem = lookup.Satuan;
            }
            else
            {
                _cboSatuan.SelectedIndex = 0;
            }
        }

        #endregion

        #region Ergonomic Quantity Strip & Item Intake

        private void OnQuantityKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                CommitQuantityStripToGrid();
            }
        }

        public bool CommitQuantityStripToGrid()
        {
            if (_currentScanResult == null)
            {
                _audioHelper?.PlayWarning();
                if (!SuppressMessageBoxes)
                {
                    MessageBox.Show("Silakan scan barcode terlebih dahulu atau pilih item produk.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }

            var qtyBagus = _numQtyBagus.Value;
            var qtyRusak = _numQtyRusak.Value;

            if (qtyBagus <= 0 && qtyRusak <= 0)
            {
                _audioHelper?.PlayWarning();
                return false;
            }

            var satuan = _cboSatuan.SelectedItem?.ToString() ?? _currentScanResult.Satuan ?? "Pcs";
            var itemNotes = _txtItemKeterangan.Text.Trim();

            // Add Qty Bagus line if specified
            if (qtyBagus > 0)
            {
                var rowBagus = new ReturnOrderItemDto
                {
                    ReturnOrderId = CurrentReturnOrderId,
                    NoUrut = _itemsList.Count + 1,
                    BrgId = _currentScanResult.BrgId,
                    BrgCode = _currentScanResult.BrgId,
                    BrgName = _currentScanResult.BrgName,
                    Barcode = _currentScanResult.BarcodeValue,
                    JenisRetur = "BAGUS",
                    Qty = qtyBagus,
                    SatId = satuan,
                    Keterangan = itemNotes
                };
                _itemsList.Add(rowBagus);
            }

            // Add Qty Rusak line if specified
            if (qtyRusak > 0)
            {
                var rowRusak = new ReturnOrderItemDto
                {
                    ReturnOrderId = CurrentReturnOrderId,
                    NoUrut = _itemsList.Count + 1,
                    BrgId = _currentScanResult.BrgId,
                    BrgCode = _currentScanResult.BrgId,
                    BrgName = _currentScanResult.BrgName,
                    Barcode = _currentScanResult.BarcodeValue,
                    JenisRetur = "RUSAK",
                    Qty = qtyRusak,
                    SatId = satuan,
                    Keterangan = itemNotes
                };
                _itemsList.Add(rowRusak);
            }

            // Trigger success audio cue
            _audioHelper?.PlaySuccess();

            // Reset quantity strip
            _numQtyBagus.Value = 0;
            _numQtyRusak.Value = 0;
            _txtItemKeterangan.Text = string.Empty;

            // Update banner & summaries
            UpdateSummaryFooter();
            _lblBannerStatus.Text = "ITEM DITAMBAHKAN";
            _lblBannerStatus.ForeColor = Color.FromArgb(16, 185, 129);

            // Resets focus for the next scan
            _dgvItems.Focus();

            return true;
        }

        public void DeleteItemAt(int index)
        {
            if (index < 0 || index >= _itemsList.Count) return;

            _itemsList.RemoveAt(index);

            // Re-sequence lines
            for (int i = 0; i < _itemsList.Count; i++)
            {
                _itemsList[i].NoUrut = i + 1;
            }
            _dgvItems.Refresh();
            UpdateSummaryFooter();
        }

        public void UpdateSummaryFooter()
        {
            _totalQtyBagus = _itemsList.Where(i => string.Equals(i.JenisRetur, "BAGUS", StringComparison.OrdinalIgnoreCase)).Sum(i => i.Qty);
            _totalQtyRusak = _itemsList.Where(i => string.Equals(i.JenisRetur, "RUSAK", StringComparison.OrdinalIgnoreCase)).Sum(i => i.Qty);
            var totalAll = _totalQtyBagus + _totalQtyRusak;

            _lblSummaryItems.Text = $"Total Item: {_itemsList.Count} baris";
            _lblSummaryBagus.Text = $"Total Qty Bagus: {_totalQtyBagus:N0}";
            _lblSummaryRusak.Text = $"Total Qty Rusak: {_totalQtyRusak:N0}";
            _lblSummaryTotal.Text = $"Total Kuantitas: {totalAll:N0}";
        }

        #endregion

        #region DataGridView Events & Badges

        private void OnGridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _itemsList.Count) return;

            var colName = _dgvItems.Columns[e.ColumnIndex].Name;
            if (colName == "colJenisRetur" && e.Value != null)
            {
                var jenis = e.Value.ToString();
                if (string.Equals(jenis, "BAGUS", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(5, 150, 105);   // Emerald-600
                    e.CellStyle.BackColor = Color.FromArgb(236, 253, 245); // Emerald-50
                    e.CellStyle.Font = new Font(_dgvItems.Font, FontStyle.Bold);
                }
                else if (string.Equals(jenis, "RUSAK", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(225, 29, 72);   // Rose-600
                    e.CellStyle.BackColor = Color.FromArgb(255, 241, 242); // Rose-50
                    e.CellStyle.Font = new Font(_dgvItems.Font, FontStyle.Bold);
                }
            }
        }

        private void OnGridCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _itemsList.Count) return;

            if (_dgvItems.Columns[e.ColumnIndex].Name == "colAction")
            {
                DeleteItemAt(e.RowIndex);
            }
        }

        private void OnGridKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && _dgvItems.CurrentRow != null)
            {
                DeleteItemAt(_dgvItems.CurrentRow.Index);
                e.Handled = true;
            }
        }

        #endregion

        #region Actions (Save Draft, Save & Upload, Reset)

        public async Task<ReturnOrderDto> SaveDraftAsync()
        {
            var (custId, custName) = GetSelectedCustomer();
            if (string.IsNullOrWhiteSpace(custName))
            {
                _audioHelper?.PlayWarning();
                if (!SuppressMessageBoxes)
                {
                    MessageBox.Show("Silakan pilih atau masukkan Customer terlebih dahulu (wajib).", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                _cboCustomer.Focus();
                return null;
            }

            if (_itemsList.Count == 0)
            {
                _audioHelper?.PlayWarning();
                if (!SuppressMessageBoxes)
                {
                    MessageBox.Show("Belum ada item retur yang dimasukkan ke dalam daftar.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return null;
            }

            if (_returnOrderService == null)
            {
                if (!SuppressMessageBoxes)
                {
                    MessageBox.Show("Layanan ReturnOrderService belum terhubung.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return null;
            }

            var driver = _cboDriver.SelectedItem as KeyValueItem;
            var salesman = _cboSalesPerson.SelectedItem as KeyValueItem;
            var notes = _txtKeterangan.Text.Trim();

            var warehouseCode = _registryHelper?.WarehouseCode ?? "GD-BTR";
            if (string.IsNullOrWhiteSpace(warehouseCode)) warehouseCode = "GD-BTR";

            var operatorName = _registryHelper?.LastOperatorName ?? "OPERATOR";
            if (string.IsNullOrWhiteSpace(operatorName)) operatorName = "OPERATOR";

            ReturnOrderDto order;

            if (string.IsNullOrWhiteSpace(CurrentReturnOrderId))
            {
                order = _returnOrderService.Create(
                    custId,
                    custName,
                    warehouseCode,
                    operatorName,
                    driver?.Id,
                    driver?.Name,
                    salesman?.Id,
                    salesman?.Name,
                    notes);

                CurrentReturnOrderId = order.ReturnOrderId;

                foreach (var itm in _itemsList)
                {
                    var jenisEnum = string.Equals(itm.JenisRetur, "RUSAK", StringComparison.OrdinalIgnoreCase)
                        ? JenisRetur.RUSAK
                        : JenisRetur.BAGUS;

                    _returnOrderService.AddItem(
                        CurrentReturnOrderId,
                        itm.BrgId,
                        itm.BrgName,
                        itm.Barcode,
                        jenisEnum,
                        itm.Qty,
                        itm.SatId,
                        itm.Keterangan);
                }

                order = await _returnOrderService.CommitDraftAsync(CurrentReturnOrderId);
            }
            else
            {
                order = await _returnOrderService.CommitDraftAsync(CurrentReturnOrderId);
            }

            _audioHelper?.PlaySuccess();
            if (!SuppressMessageBoxes)
            {
                MessageBox.Show($"Draft Return Order '{CurrentReturnOrderId}' berhasil disimpan secara lokal.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return order;
        }

        public async Task<SyncResultDto> SaveAndUploadAsync()
        {
            var draftOrder = await SaveDraftAsync();
            if (draftOrder == null)
                return SyncResultDto.Fail("ReturnOrder", "Gagal menyimpan draft.");

            if (_returnOrderService == null)
                return SyncResultDto.Fail("ReturnOrder", "Service tidak tersedia.");

            var syncResult = await _returnOrderService.CommitAndUploadAsync(CurrentReturnOrderId);
            if (syncResult != null && syncResult.Success)
            {
                _audioHelper?.PlaySuccess();
                if (!SuppressMessageBoxes)
                {
                    MessageBox.Show($"Return Order '{CurrentReturnOrderId}' berhasil disimpan dan di-upload ke server Cloud!", "Upload Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                ResetForm(promptUser: false);
            }
            else
            {
                _audioHelper?.PlayWarning();
                var err = syncResult?.ErrorMessage ?? "Koneksi offline atau server relay tidak merespon.";
                if (!SuppressMessageBoxes)
                {
                    MessageBox.Show($"Return Order '{CurrentReturnOrderId}' tersimpan sebagai Draft LOKAL.\r\nUpload Cloud belum selesai: {err}\r\nDokumen akan di-upload otomatis saat online.", "Tersimpan Lokal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                ResetForm(promptUser: false);
            }

            return syncResult;
        }

        public void ResetForm(bool promptUser = true)
        {
            if (promptUser && !SuppressMessageBoxes && _itemsList.Count > 0)
            {
                var confirm = MessageBox.Show(
                    "Apakah Anda yakin ingin membatalkan transaksi ini dan mengosongkan form?",
                    "Konfirmasi Reset Form",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return;
            }

            CurrentReturnOrderId = null;
            _cboCustomer.SelectedIndex = -1;
            _cboCustomer.Text = string.Empty;

            if (_cboDriver.Items.Count > 0) _cboDriver.SelectedIndex = 0;
            if (_cboSalesPerson.Items.Count > 0) _cboSalesPerson.SelectedIndex = 0;

            _txtKeterangan.Text = string.Empty;

            _currentScanResult = null;
            _pnlScannerBanner.BackColor = Color.FromArgb(30, 41, 59);
            _lblBannerTitle.Text = "Menunggu scan barcode scanner USB atau tekan F2 untuk pencarian produk...";
            _lblBannerTitle.ForeColor = Color.FromArgb(226, 232, 240);
            _lblBannerDetails.Text = "Barcode: -  |  Satuan Default: -  |  Pengali: -";
            _lblBannerDetails.ForeColor = Color.FromArgb(148, 163, 184);
            _lblBannerStatus.Text = "STATUS: STANDBY";
            _lblBannerStatus.ForeColor = Color.FromArgb(16, 185, 129);

            _cboSatuan.Items.Clear();
            _numQtyBagus.Value = 0;
            _numQtyRusak.Value = 0;
            _txtItemKeterangan.Text = string.Empty;

            _itemsList.Clear();
            UpdateSummaryFooter();
        }

        public void LoadOrder(ReturnOrderDto order)
        {
            if (order == null) return;

            CurrentReturnOrderId = order.ReturnOrderId;
            _txtKeterangan.Text = order.Keterangan ?? string.Empty;

            // Set customer
            for (int i = 0; i < _cboCustomer.Items.Count; i++)
            {
                if (_cboCustomer.Items[i] is CustomerItem cust && cust.CustId == order.CustId)
                {
                    _cboCustomer.SelectedIndex = i;
                    break;
                }
            }
            if (_cboCustomer.SelectedIndex < 0)
            {
                _cboCustomer.Text = order.CustName;
            }

            // Set driver
            if (!string.IsNullOrWhiteSpace(order.DriverId))
            {
                for (int i = 0; i < _cboDriver.Items.Count; i++)
                {
                    if (_cboDriver.Items[i] is KeyValueItem itm && itm.Id == order.DriverId)
                    {
                        _cboDriver.SelectedIndex = i;
                        break;
                    }
                }
            }

            // Set salesman
            if (!string.IsNullOrWhiteSpace(order.SalesPersonId))
            {
                for (int i = 0; i < _cboSalesPerson.Items.Count; i++)
                {
                    if (_cboSalesPerson.Items[i] is KeyValueItem itm && itm.Id == order.SalesPersonId)
                    {
                        _cboSalesPerson.SelectedIndex = i;
                        break;
                    }
                }
            }

            // Load items
            _itemsList.Clear();
            if (order.Items != null)
            {
                foreach (var item in order.Items)
                {
                    _itemsList.Add(item);
                }
            }

            UpdateSummaryFooter();
        }

        public void SetSelectedItem(Barang item, string selectedSatuan = null)
        {
            if (item == null) return;

            string effectiveSatuan = selectedSatuan;
            if (string.IsNullOrWhiteSpace(effectiveSatuan))
            {
                effectiveSatuan = item.Satuan1 ?? "Pcs";
            }

            var lookup = BarcodeLookupResult.Hit(
                rawInput: null,
                normalizedBarcode: null,
                brgId: item.BrgId,
                brgName: item.BrgName,
                satuan: effectiveSatuan,
                pengali: 1m,
                isActive: true);

            ApplyActiveProduct(lookup);
            _audioHelper?.PlaySuccess();
        }

        public void TriggerManualItemSearch()
        {
            if (ManualSearchDialogLauncher != null)
            {
                var item = ManualSearchDialogLauncher();
                if (item != null)
                {
                    SetSelectedItem(item);
                }
                return;
            }

            using (var dlg = new ManualItemSearchDialog(_referenceRepo, _audioHelper))
            {
                IWin32Window owner = (Control)FindForm() ?? (IsHandleCreated ? this : null);
                var result = owner != null ? dlg.ShowDialog(owner) : dlg.ShowDialog();
                if (result == DialogResult.OK && dlg.SelectedBarang != null)
                {
                    SetSelectedItem(dlg.SelectedBarang, dlg.SelectedSatuan);
                }
            }
        }

        private (string CustId, string CustName) GetSelectedCustomer()
        {
            if (_cboCustomer.SelectedItem is CustomerItem cust)
            {
                return (cust.CustId, cust.CustName);
            }

            var text = _cboCustomer.Text.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return ("CUST-MANUAL", text);
            }

            return (null, null);
        }

        #endregion

        #region Keyboard Command Routing

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F9)
            {
                _ = SaveDraftAsync();
                return true;
            }

            if (keyData == Keys.F10)
            {
                _ = SaveAndUploadAsync();
                return true;
            }

            if (keyData == Keys.Escape)
            {
                ResetForm(promptUser: true);
                return true;
            }

            if (keyData == Keys.F2)
            {
                TriggerManualItemSearch();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Helper Value Classes

        private class CustomerItem
        {
            public string CustId { get; }
            public string CustName { get; }

            public CustomerItem(string custId, string custName)
            {
                CustId = custId;
                CustName = custName;
            }

            public override string ToString() => $"{CustName} ({CustId})";
        }

        private class KeyValueItem
        {
            public string Id { get; }
            public string Name { get; }

            public KeyValueItem(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public override string ToString() => Name;
        }

        #endregion
    }
}
