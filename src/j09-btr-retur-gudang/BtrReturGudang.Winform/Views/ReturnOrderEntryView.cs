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
using BtrReturGudang.Winform.Services;

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
        private readonly OperatorSessionValidator _sessionValidator;

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
            IRegistryHelper registryHelper,
            OperatorSessionValidator sessionValidator = null)
        {
            _returnOrderService = returnOrderService;
            _barcodeService = barcodeService;
            _referenceRepo = referenceRepo;
            _audioHelper = audioHelper;
            _registryHelper = registryHelper;
            _sessionValidator = sessionValidator;

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
            BackColor = UiTheme.Slate100;
            Font = UiTheme.FontBody;

            // Root TableLayoutPanel layout for responsive, professional arrangement
            var tblMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(16, 12, 16, 8)
            };

            // Define row styles: Header (auto), Intake Card (auto), Grid (fill)
            tblMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // 1. Header (Customer, Driver, Salesman, Notes)
            var pnlHeader = BuildHeaderArea();
            tblMain.Controls.Add(pnlHeader, 0, 0);

            // 2. Intake Control Card (Scanner Banner + Ergonomic Quantity Strip)
            var pnlIntakeCard = BuildIntakeCard();
            tblMain.Controls.Add(pnlIntakeCard, 0, 1);

            // 3. Grid + Summary Footer Container
            var pnlGridContainer = BuildGridArea();
            tblMain.Controls.Add(pnlGridContainer, 0, 2);

            // 4. Action Toolbar (Bottom)
            var pnlToolbar = BuildActionToolbar();

            // Assemble controls with proper docking:
            // In WinForms, docked controls are evaluated in reverse child index order.
            // pnlToolbar (DockStyle.Bottom) at index 1 is evaluated first -> takes bottom 54px.
            // tblMain (DockStyle.Fill) at index 0 is evaluated last -> fills remaining space above pnlToolbar.
            Controls.Add(tblMain);
            Controls.Add(pnlToolbar);
            Controls.SetChildIndex(tblMain, 0);
            Controls.SetChildIndex(pnlToolbar, 1);
        }

        #region Header Construction

        private Panel BuildHeaderArea()
        {
            var pnl = UiTheme.CreateCard(new Padding(18, 12, 18, 14), new Padding(0, 0, 0, 10));
            pnl.Dock = DockStyle.Top;
            pnl.AutoSize = true;

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 4,
                RowCount = 3,
                AutoSize = true
            };

            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // Section Title Header
            var pnlTitle = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 28,
                Margin = new Padding(0, 0, 0, 10)
            };

            var lblTitle = new Label
            {
                Text = "📄 INFORMASI DOKUMEN & CUSTOMER",
                Font = UiTheme.FontCardTitle,
                ForeColor = UiTheme.Slate800,
                AutoSize = true,
                Location = new Point(0, 2)
            };
            pnlTitle.Controls.Add(lblTitle);

            tbl.Controls.Add(pnlTitle, 0, 0);
            tbl.SetColumnSpan(pnlTitle, 4);

            // Row 1: Customer
            var lblCustomer = new Label
            {
                Text = "Customer *:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            _cboCustomer = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.FontBody,
                DropDownStyle = ComboBoxStyle.DropDown
            };

            if (System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.STA)
            {
                _cboCustomer.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                _cboCustomer.AutoCompleteSource = AutoCompleteSource.ListItems;
            }

            tbl.Controls.Add(lblCustomer, 0, 1);
            tbl.Controls.Add(_cboCustomer, 1, 1);

            // Row 1: Driver
            var lblDriver = new Label
            {
                Text = "Driver Pengirim:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(16, 0, 0, 0)
            };

            _cboDriver = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.FontBody,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            tbl.Controls.Add(lblDriver, 2, 1);
            tbl.Controls.Add(_cboDriver, 3, 1);

            // Row 2: Salesman
            var lblSalesman = new Label
            {
                Text = "Salesman:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 0, 0)
            };

            _cboSalesPerson = new ComboBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.FontBody,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 8, 0, 0)
            };

            tbl.Controls.Add(lblSalesman, 0, 2);
            tbl.Controls.Add(_cboSalesPerson, 1, 2);

            // Row 2: Catatan / Keterangan
            var lblNotes = new Label
            {
                Text = "Catatan Retur:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(16, 8, 0, 0)
            };

            _txtKeterangan = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.FontBody,
                Margin = new Padding(0, 8, 0, 0)
            };

            tbl.Controls.Add(lblNotes, 2, 2);
            tbl.Controls.Add(_txtKeterangan, 3, 2);

            pnl.Controls.Add(tbl);
            return pnl;
        }

        #endregion

        #region Intake Card (Scanner Banner + Ergonomic Quantity Strip)

        private Panel BuildIntakeCard()
        {
            var pnlCard = UiTheme.CreateCard(new Padding(14), new Padding(0, 0, 0, 10));
            pnlCard.Dock = DockStyle.Top;
            pnlCard.AutoSize = true;

            var tblCard = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true
            };
            tblCard.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tblCard.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // 1. Scanner Intake Focus Banner
            _pnlScannerBanner = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 64,
                BackColor = UiTheme.Slate900,
                Padding = new Padding(16, 10, 16, 10),
                Margin = new Padding(0, 0, 0, 10)
            };

            _lblBannerTitle = new Label
            {
                Text = "Menunggu scan barcode scanner USB atau tekan F2 untuk pencarian produk...",
                Font = UiTheme.FontSectionTitle,
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                AutoSize = true
            };

            _lblBannerDetails = new Label
            {
                Text = "Barcode: -  |  Satuan Default: -  |  Pengali: -",
                Font = UiTheme.FontMonospace,
                ForeColor = UiTheme.Slate400,
                Dock = DockStyle.Bottom,
                AutoSize = true
            };

            _lblBannerStatus = new Label
            {
                Text = "STATUS: STANDBY",
                Font = UiTheme.FontBadge,
                ForeColor = UiTheme.Emerald,
                BackColor = Color.FromArgb(20, 35, 30),
                Padding = new Padding(8, 4, 8, 4),
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.TopRight
            };

            _pnlScannerBanner.Controls.Add(_lblBannerTitle);
            _pnlScannerBanner.Controls.Add(_lblBannerDetails);
            _pnlScannerBanner.Controls.Add(_lblBannerStatus);

            // 2. Ergonomic Quantity Strip (TableLayoutPanel for clean non-wrapping layout)
            var pnlQuantityStrip = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 2,
                AutoSize = true,
                BackColor = UiTheme.Slate50,
                Padding = new Padding(14, 10, 14, 10)
            };

            pnlQuantityStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            pnlQuantityStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135F));
            pnlQuantityStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135F));
            pnlQuantityStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlQuantityStrip.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));

            // Labels row (Row 0)
            var lblSatuan = new Label
            {
                Text = "Satuan:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate600,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };

            var lblBagus = new Label
            {
                Text = "Qty BAGUS (Baik):",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.EmeraldDark,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };

            var lblRusak = new Label
            {
                Text = "Qty RUSAK (BS):",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Rose,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };

            var lblItemNote = new Label
            {
                Text = "Keterangan Item:",
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate600,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };

            var lblSpacer = new Label { Text = "", Dock = DockStyle.Fill };

            pnlQuantityStrip.Controls.Add(lblSatuan, 0, 0);
            pnlQuantityStrip.Controls.Add(lblBagus, 1, 0);
            pnlQuantityStrip.Controls.Add(lblRusak, 2, 0);
            pnlQuantityStrip.Controls.Add(lblItemNote, 3, 0);
            pnlQuantityStrip.Controls.Add(lblSpacer, 4, 0);

            // Controls row (Row 1)
            _cboSatuan = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.FontLabel,
                Margin = new Padding(0, 4, 10, 0)
            };

            _numQtyBagus = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 999999,
                DecimalPlaces = 0,
                Font = new Font("Segoe UI Bold", 11F),
                ForeColor = UiTheme.EmeraldDark,
                BackColor = UiTheme.EmeraldLight,
                TextAlign = HorizontalAlignment.Right,
                Margin = new Padding(0, 4, 10, 0)
            };
            _numQtyBagus.KeyDown += OnQuantityKeyDown;

            _numQtyRusak = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 999999,
                DecimalPlaces = 0,
                Font = new Font("Segoe UI Bold", 11F),
                ForeColor = UiTheme.Rose,
                BackColor = UiTheme.RoseLight,
                TextAlign = HorizontalAlignment.Right,
                Margin = new Padding(0, 4, 10, 0)
            };
            _numQtyRusak.KeyDown += OnQuantityKeyDown;

            _txtItemKeterangan = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = UiTheme.FontBody,
                Margin = new Padding(0, 4, 10, 0)
            };
            _txtItemKeterangan.KeyDown += OnQuantityKeyDown;

            _btnAddItem = new Button
            {
                Text = "+ Tambah [Enter]",
                Font = UiTheme.FontBodyBold,
                ForeColor = Color.White,
                BackColor = UiTheme.Primary,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Height = 32,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 0, 0)
            };
            _btnAddItem.FlatAppearance.BorderSize = 0;
            _btnAddItem.Click += (s, e) => CommitQuantityStripToGrid();

            pnlQuantityStrip.Controls.Add(_cboSatuan, 0, 1);
            pnlQuantityStrip.Controls.Add(_numQtyBagus, 1, 1);
            pnlQuantityStrip.Controls.Add(_numQtyRusak, 2, 1);
            pnlQuantityStrip.Controls.Add(_txtItemKeterangan, 3, 1);
            pnlQuantityStrip.Controls.Add(_btnAddItem, 4, 1);

            tblCard.Controls.Add(_pnlScannerBanner, 0, 0);
            tblCard.Controls.Add(pnlQuantityStrip, 0, 1);

            pnlCard.Controls.Add(tblCard);

            return pnlCard;
        }

        #endregion

        #region Grid Area Construction

        private Panel BuildGridArea()
        {
            var pnlContainer = UiTheme.CreateCard(new Padding(14), new Padding(0, 0, 0, 0));
            pnlContainer.Dock = DockStyle.Fill;

            // Section Title
            var lblGridTitle = new Label
            {
                Text = "📦 DAFTAR BARANG FISIK RETUR",
                Font = UiTheme.FontCardTitle,
                ForeColor = UiTheme.Slate800,
                Dock = DockStyle.Top,
                Height = 26
            };

            // Summary Footer
            var pnlFooter = BuildSummaryFooter();

            // DataGridView setup
            _dgvItems = new DoubleBufferedDataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = _itemsList
            };

            UiTheme.StyleDataGrid(_dgvItems);
            ConfigureGridColumns();
            _dgvItems.CellFormatting += OnGridCellFormatting;
            _dgvItems.CellContentClick += OnGridCellContentClick;
            _dgvItems.KeyDown += OnGridKeyDown;

            pnlContainer.Controls.Add(_dgvItems);
            pnlContainer.Controls.Add(pnlFooter);
            pnlContainer.Controls.Add(lblGridTitle);
            pnlContainer.Controls.SetChildIndex(_dgvItems, 0);
            pnlContainer.Controls.SetChildIndex(pnlFooter, 1);
            pnlContainer.Controls.SetChildIndex(lblGridTitle, 2);

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
                Width = 50,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            var colBarcode = new DataGridViewTextBoxColumn
            {
                Name = "colBarcode",
                HeaderText = "Barcode",
                DataPropertyName = "Barcode",
                Width = 140,
                ReadOnly = true,
                DefaultCellStyle = { Font = UiTheme.FontMonospace }
            };

            var colBrgCode = new DataGridViewTextBoxColumn
            {
                Name = "colBrgCode",
                HeaderText = "Kode Barang",
                DataPropertyName = "BrgId",
                Width = 120,
                ReadOnly = true,
                DefaultCellStyle = { Font = UiTheme.FontMonospace }
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
                Width = 80,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N0", Font = UiTheme.FontBodyBold }
            };

            var colSatuan = new DataGridViewTextBoxColumn
            {
                Name = "colSatId",
                HeaderText = "Satuan",
                DataPropertyName = "SatId",
                Width = 85,
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
                Width = 80,
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
            var pnl = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                BackColor = UiTheme.Slate900,
                Padding = new Padding(16, 12, 16, 12),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            _lblSummaryItems = new Label
            {
                Text = "Total Item: 0 baris",
                Font = UiTheme.FontBodyBold,
                ForeColor = UiTheme.Slate200,
                AutoSize = true,
                Margin = new Padding(0, 0, 36, 0)
            };

            _lblSummaryBagus = new Label
            {
                Text = "Total Qty Bagus: 0",
                Font = UiTheme.FontBodyBold,
                ForeColor = UiTheme.Emerald,
                AutoSize = true,
                Margin = new Padding(0, 0, 36, 0)
            };

            _lblSummaryRusak = new Label
            {
                Text = "Total Qty Rusak: 0",
                Font = UiTheme.FontBodyBold,
                ForeColor = Color.FromArgb(251, 113, 133), // Rose-400
                AutoSize = true,
                Margin = new Padding(0, 0, 36, 0)
            };

            _lblSummaryTotal = new Label
            {
                Text = "Total Kuantitas: 0",
                Font = UiTheme.FontBodyBold,
                ForeColor = Color.FromArgb(147, 197, 253), // Blue-300
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
            var pnl = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 54,
                BackColor = UiTheme.Slate100,
                Padding = new Padding(16, 9, 16, 9),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            pnl.Paint += (s, e) =>
            {
                using (var pen = new Pen(UiTheme.Slate300, 1))
                {
                    e.Graphics.DrawLine(pen, 0, 0, pnl.Width, 0);
                }
            };

            _btnSaveDraft = new Button
            {
                Text = "F9  Simpan Draft",
                Font = UiTheme.FontBodyBold,
                ForeColor = Color.White,
                BackColor = UiTheme.Slate700,
                FlatStyle = FlatStyle.Flat,
                Width = 150,
                Height = 36,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 12, 0)
            };
            _btnSaveDraft.FlatAppearance.BorderSize = 0;
            _btnSaveDraft.Click += async (s, e) => await SaveDraftAsync();

            _btnSaveAndUpload = new Button
            {
                Text = "F10  Simpan & Upload",
                Font = UiTheme.FontBodyBold,
                ForeColor = Color.White,
                BackColor = UiTheme.Emerald,
                FlatStyle = FlatStyle.Flat,
                Width = 185,
                Height = 36,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 12, 0)
            };
            _btnSaveAndUpload.FlatAppearance.BorderSize = 0;
            _btnSaveAndUpload.Click += async (s, e) => await SaveAndUploadAsync();

            _btnReset = new Button
            {
                Text = "Esc  Reset Form",
                Font = UiTheme.FontBodyBold,
                ForeColor = UiTheme.Slate700,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 140,
                Height = 36,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 12, 0)
            };
            _btnReset.FlatAppearance.BorderColor = UiTheme.Slate300;
            _btnReset.FlatAppearance.BorderSize = 1;
            _btnReset.Click += (s, e) => ResetForm(promptUser: true);

            _btnManualSearch = new Button
            {
                Text = "F2  Cari Barang",
                Font = UiTheme.FontBodyBold,
                ForeColor = Color.White,
                BackColor = UiTheme.Amber,
                FlatStyle = FlatStyle.Flat,
                Width = 145,
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
                    _cboCustomer.Items.Add(new CustomerItem(cust.CustomerId, cust.CustomerName, cust.CustomerCode, cust.Wilayah));
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
            var (custId, custName, customerCode, wilayah) = GetSelectedCustomer();
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

            try
            {
                if (string.IsNullOrWhiteSpace(CurrentReturnOrderId))
                {
                    order = await _returnOrderService.CreateAsync(
                        custId,
                        custName,
                        warehouseCode,
                        operatorName,
                        driver?.Id,
                        driver?.Name,
                        salesman?.Id,
                        salesman?.Name,
                        notes,
                        customerCode,
                        wilayah,
                        _itemsList);

                    CurrentReturnOrderId = order.ReturnOrderId;
                }
                else
                {
                    order = await _returnOrderService.UpdateAsync(
                        CurrentReturnOrderId,
                        custId,
                        custName,
                        driver?.Id,
                        driver?.Name,
                        salesman?.Id,
                        salesman?.Name,
                        notes,
                        customerCode,
                        wilayah,
                        _itemsList);
                }
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                if (!SuppressMessageBoxes)
                {
                    MessageBox.Show($"Gagal menyimpan draft Return Order: {ex.Message}", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return null;
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

            // Pre-empt Cloud upload when the configured operator identity is unresolved so the
            // Cloud refusal message is surfaced instead of an opaque HTTP 409.
            // Traceable to: ARCHITECTURE §4 TD-012, §5.2 SCR-07, P2-S05.
            if (_sessionValidator != null && _sessionValidator.IsUnresolved)
            {
                _audioHelper?.PlayWarning();
                var refusal = _sessionValidator.RefusalMessage;
                if (!SuppressMessageBoxes)
                {
                    MessageBox.Show(
                        $"Return Order '{CurrentReturnOrderId}' tersimpan sebagai Draft LOKAL.\r\nUpload Cloud dibatalkan: {refusal}",
                        "Operator Belum Terverifikasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                return SyncResultDto.Fail("ReturnOrder", refusal);
            }

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

        private (string CustId, string CustName, string CustomerCode, string Wilayah) GetSelectedCustomer()
        {
            if (_cboCustomer.SelectedItem is CustomerItem cust)
            {
                return (cust.CustId, cust.CustName, cust.CustomerCode, cust.Wilayah);
            }

            var text = _cboCustomer.Text.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return ("CUST-MANUAL", text, null, null);
            }

            return (null, null, null, null);
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
            public string CustomerCode { get; }
            public string Wilayah { get; }

            public CustomerItem(string custId, string custName, string customerCode = null, string wilayah = null)
            {
                CustId = custId;
                CustName = custName;
                CustomerCode = customerCode;
                Wilayah = wilayah;
            }

            public override string ToString()
            {
                var codePart = !string.IsNullOrWhiteSpace(CustomerCode) ? $"[{CustomerCode}] " : string.Empty;
                var regionPart = !string.IsNullOrWhiteSpace(Wilayah) ? $" - {Wilayah}" : string.Empty;
                return $"{codePart}{CustName}{regionPart} ({CustId})";
            }
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
