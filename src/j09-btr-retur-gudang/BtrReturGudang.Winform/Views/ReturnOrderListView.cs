using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;

namespace BtrReturGudang.Winform.Views
{
    /// <summary>
    /// SCR-05: Return Order History &amp; Inspector view.
    /// Provides date filtering, status sync filtering, colored status badges,
    /// slide-over / split inspector panel, and strict post-sync read-only document locking.
    /// Traceable to: ARCHITECTURE §5.2 SCR-05, TD-010, TD-011, P3-S10.
    /// </summary>
    public class ReturnOrderListView : UserControl
    {
        private readonly IReturnOrderService _orderService;
        private readonly IReturnOrderRepo _orderRepo;
        private readonly ISyncService _syncService;
        private readonly IAudioHelper _audioHelper;

        // UI Header Controls
        private Panel _pnlHeader;
        private Label _lblTitle;
        private Label _lblStatus;
        private Button _btnRefresh;

        // UI Filter Controls
        private Panel _pnlFilter;
        private DateTimePicker _dtpStartDate;
        private DateTimePicker _dtpEndDate;
        private ComboBox _cboStatusFilter;
        private Button _btnApplyFilter;
        private Label _lblSummaryCount;

        // Main Split Body
        private SplitContainer _splitBody;
        private DataGridView _dgvOrders;

        // Inspector Panel Controls
        private Panel _pnlInspector;
        private Panel _pnlLockedBanner;
        private Label _lblLockedBanner;
        private Label _lblInspectorTitle;

        // Header Metadata Labels
        private Label _lblMetaOrderId;
        private Label _lblMetaDate;
        private Label _lblMetaCustomer;
        private Label _lblMetaOperator;
        private Label _lblMetaWarehouse;
        private Label _lblMetaDriver;
        private Label _lblMetaSales;
        private Label _lblMetaNote;
        private Label _lblMetaSyncInfo;

        // Detail Items Grid
        private DataGridView _dgvItems;

        // Inspector Action Buttons
        private Panel _pnlInspectorActions;
        private Button _btnEditOrder;
        private Button _btnDeleteOrder;
        private Button _btnUploadOrder;

        // Cached list
        private List<ReturnOrderDto> _loadedOrders = new List<ReturnOrderDto>();
        private ReturnOrderDto _selectedOrder = null;

        /// <summary>
        /// Event fired when user requests to open a draft order for editing in intake view.
        /// </summary>
        public event EventHandler<string> EditDraftRequested;

        /// <summary>
        /// Public accessors for testing
        /// </summary>
        public DataGridView OrdersGrid => _dgvOrders;
        public DataGridView ItemsGrid => _dgvItems;
        public Panel LockedBanner => _pnlLockedBanner;
        public Button EditButton => _btnEditOrder;
        public Button DeleteButton => _btnDeleteOrder;
        public Button UploadButton => _btnUploadOrder;
        public ComboBox StatusFilterBox => _cboStatusFilter;
        public DateTimePicker StartDatePicker => _dtpStartDate;
        public DateTimePicker EndDatePicker => _dtpEndDate;
        public ReturnOrderDto SelectedOrder => _selectedOrder;
        public IReadOnlyList<ReturnOrderDto> LoadedOrders => _loadedOrders;

        public ReturnOrderListView()
            : this(null, null, null, null)
        {
        }

        public ReturnOrderListView(
            IReturnOrderService orderService = null,
            IReturnOrderRepo orderRepo = null,
            ISyncService syncService = null,
            IAudioHelper audioHelper = null)
        {
            _orderService = orderService;
            _orderRepo = orderRepo;
            _syncService = syncService;
            _audioHelper = audioHelper;

            InitializeComponent();
            EnableDoubleBuffering(_dgvOrders);
            EnableDoubleBuffering(_dgvItems);

            Load += OnViewLoad;
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(241, 245, 249); // Slate-100
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);

            // 1. Top Header
            BuildHeader();

            // 2. Filter Bar
            BuildFilterBar();

            // 3. Body Split (Orders Grid + Detail Inspector)
            BuildSplitBody();

            // Arrange controls
            Controls.Add(_splitBody);
            Controls.Add(_pnlFilter);
            Controls.Add(_pnlHeader);
        }

        private void BuildHeader()
        {
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.White,
                Padding = new Padding(24, 12, 24, 12)
            };

            _lblTitle = new Label
            {
                Text = "Daftar & Riwayat Retur [F3]",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(15, 23, 42), // Slate-900
                AutoSize = true,
                Location = new Point(24, 12)
            };

            _lblStatus = new Label
            {
                Text = "Riwayat fisik retur, verifikasi status sinkronisasi, dan inspeksi proteksi dokumen (Post-Sync Locking).",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(100, 116, 139), // Slate-500
                AutoSize = true,
                Location = new Point(26, 42)
            };

            _btnRefresh = new Button
            {
                Text = "🔄 Muat Ulang",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Width = 110,
                Height = 36,
                Location = new Point(Width - 140, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            _btnRefresh.Click += async (s, e) => await LoadOrdersAsync();

            _pnlHeader.Controls.Add(_lblTitle);
            _pnlHeader.Controls.Add(_lblStatus);
            _pnlHeader.Controls.Add(_btnRefresh);
        }

        private void BuildFilterBar()
        {
            _pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.FromArgb(248, 250, 252), // Slate-50
                Padding = new Padding(24, 10, 24, 10)
            };

            var lblFrom = new Label
            {
                Text = "Dari Tanggal:",
                Location = new Point(24, 16),
                AutoSize = true,
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            _dtpStartDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(106, 13),
                Width = 110,
                Value = DateTime.Today.AddDays(-14)
            };

            var lblTo = new Label
            {
                Text = "Sampai:",
                Location = new Point(226, 16),
                AutoSize = true,
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            _dtpEndDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(282, 13),
                Width = 110,
                Value = DateTime.Today
            };

            var lblStatusFilter = new Label
            {
                Text = "Status Sync:",
                Location = new Point(408, 16),
                AutoSize = true,
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            _cboStatusFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(488, 13),
                Width = 120
            };
            _cboStatusFilter.Items.AddRange(new object[] { "Semua (All)", "Draft", "Synced" });
            _cboStatusFilter.SelectedIndex = 0;

            _btnApplyFilter = new Button
            {
                Text = "Terapkan Filter",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                BackColor = Color.FromArgb(37, 99, 235), // Blue-600
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(620, 12),
                Width = 115,
                Height = 28,
                Cursor = Cursors.Hand
            };
            _btnApplyFilter.FlatAppearance.BorderSize = 0;
            _btnApplyFilter.Click += async (s, e) => await LoadOrdersAsync();

            _lblSummaryCount = new Label
            {
                Text = "0 dokumen ditemukan",
                Location = new Point(750, 16),
                AutoSize = true,
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            _pnlFilter.Controls.Add(lblFrom);
            _pnlFilter.Controls.Add(_dtpStartDate);
            _pnlFilter.Controls.Add(lblTo);
            _pnlFilter.Controls.Add(_dtpEndDate);
            _pnlFilter.Controls.Add(lblStatusFilter);
            _pnlFilter.Controls.Add(_cboStatusFilter);
            _pnlFilter.Controls.Add(_btnApplyFilter);
            _pnlFilter.Controls.Add(_lblSummaryCount);
        }

        private void BuildSplitBody()
        {
            _splitBody = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 620,
                SplitterWidth = 6,
                BackColor = Color.FromArgb(226, 232, 240)
            };

            // Left: Master Return Orders Grid
            BuildOrdersGrid();
            _splitBody.Panel1.Controls.Add(_dgvOrders);
            _splitBody.Panel1.Padding = new Padding(24, 16, 8, 16);
            _splitBody.Panel1.BackColor = Color.FromArgb(241, 245, 249);

            // Right: Inspector Panel
            BuildInspectorPanel();
            _splitBody.Panel2.Controls.Add(_pnlInspector);
            _splitBody.Panel2.Padding = new Padding(8, 16, 24, 16);
            _splitBody.Panel2.BackColor = Color.FromArgb(241, 245, 249);
        }

        private void BuildOrdersGrid()
        {
            _dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
                RowTemplate = { Height = 34 },
                ColumnHeadersHeight = 36,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                GridColor = Color.FromArgb(241, 245, 249),
                EnableHeadersVisualStyles = false
            };

            _dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            _dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            _dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
            _dgvOrders.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254); // Blue-100
            _dgvOrders.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);

            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOrderId",
                HeaderText = "No. Dokumen (ULID)",
                Width = 160
            });

            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDate",
                HeaderText = "Tanggal",
                Width = 85
            });

            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCust",
                HeaderText = "Customer",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTotalQty",
                HeaderText = "Total Qty",
                Width = 75,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "Status Sync",
                Width = 110,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _dgvOrders.SelectionChanged += OnOrderSelectionChanged;
            _dgvOrders.CellFormatting += OnOrdersGridCellFormatting;
        }

        private void BuildInspectorPanel()
        {
            _pnlInspector = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16)
            };

            // Inspector Title
            _lblInspectorTitle = new Label
            {
                Text = "📋 Detail & Inspeksi Dokumen",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Top,
                Height = 30
            };

            // Post-Sync Immutability Banner
            _pnlLockedBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Color.FromArgb(254, 242, 242), // Red-50
                Padding = new Padding(12),
                Visible = false
            };

            _lblLockedBanner = new Label
            {
                Text = "🔒 DOKUMEN TERKUNCI (READ-ONLY)\nStatus telah disinkronkan ke pusat (Synced). Edit dan hapus dinonaktifkan.",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(185, 28, 28), // Red-700
                Dock = DockStyle.Fill
            };
            _pnlLockedBanner.Controls.Add(_lblLockedBanner);

            // Metadata panel
            var pnlMeta = new Panel
            {
                Dock = DockStyle.Top,
                Height = 155,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(12)
            };

            _lblMetaOrderId = new Label { Text = "ID: -", Location = new Point(12, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold) };
            _lblMetaDate = new Label { Text = "Tanggal: -", Location = new Point(12, 32), AutoSize = true };
            _lblMetaCustomer = new Label { Text = "Customer: -", Location = new Point(12, 54), AutoSize = true };
            _lblMetaOperator = new Label { Text = "Operator: -", Location = new Point(12, 76), AutoSize = true };
            _lblMetaWarehouse = new Label { Text = "Depo: -", Location = new Point(240, 76), AutoSize = true };
            _lblMetaDriver = new Label { Text = "Driver: -", Location = new Point(12, 98), AutoSize = true };
            _lblMetaSales = new Label { Text = "Sales: -", Location = new Point(240, 98), AutoSize = true };
            _lblMetaNote = new Label { Text = "Catatan: -", Location = new Point(12, 120), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105) };
            _lblMetaSyncInfo = new Label { Text = "Status: -", Location = new Point(240, 32), AutoSize = true };

            pnlMeta.Controls.Add(_lblMetaOrderId);
            pnlMeta.Controls.Add(_lblMetaDate);
            pnlMeta.Controls.Add(_lblMetaCustomer);
            pnlMeta.Controls.Add(_lblMetaOperator);
            pnlMeta.Controls.Add(_lblMetaWarehouse);
            pnlMeta.Controls.Add(_lblMetaDriver);
            pnlMeta.Controls.Add(_lblMetaSales);
            pnlMeta.Controls.Add(_lblMetaNote);
            pnlMeta.Controls.Add(_lblMetaSyncInfo);

            // Items grid header label
            var lblItemsHeader = new Label
            {
                Text = "Daftar Barang Fisik Retur:",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(30, 41, 59),
                Dock = DockStyle.Top,
                Height = 26,
                Padding = new Padding(0, 6, 0, 0)
            };

            // Bottom Action buttons
            BuildInspectorActions();

            // Line items grid
            BuildItemsGrid();

            // Assemble inspector hierarchy (Reverse Dock order)
            _pnlInspector.Controls.Add(_dgvItems);
            _pnlInspector.Controls.Add(lblItemsHeader);
            _pnlInspector.Controls.Add(pnlMeta);
            _pnlInspector.Controls.Add(_pnlLockedBanner);
            _pnlInspector.Controls.Add(_lblInspectorTitle);
            _pnlInspector.Controls.Add(_pnlInspectorActions);
        }

        private void BuildItemsGrid()
        {
            _dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                RowTemplate = { Height = 28 },
                ColumnHeadersHeight = 30,
                GridColor = Color.FromArgb(241, 245, 249),
                EnableHeadersVisualStyles = false
            };

            _dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            _dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
            _dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);

            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItemNo", HeaderText = "#", Width = 30 });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItemBrg", HeaderText = "Barang", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItemBarcode", HeaderText = "Barcode", Width = 95 });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItemJenis", HeaderText = "Klasifikasi", Width = 75, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItemQty", HeaderText = "Qty", Width = 55, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItemSat", HeaderText = "Sat", Width = 45 });

            _dgvItems.CellFormatting += OnItemsGridCellFormatting;
        }

        private void BuildInspectorActions()
        {
            _pnlInspectorActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(8, 8, 8, 8)
            };

            _btnEditOrder = new Button
            {
                Text = "✏️ Buka / Edit Draft",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(8, 9),
                Width = 135,
                Height = 34,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnEditOrder.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            _btnEditOrder.Click += OnEditOrderClick;

            _btnDeleteOrder = new Button
            {
                Text = "🗑️ Hapus Draft",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                BackColor = Color.FromArgb(254, 242, 242),
                ForeColor = Color.FromArgb(185, 28, 28),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(150, 9),
                Width = 110,
                Height = 34,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnDeleteOrder.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
            _btnDeleteOrder.Click += async (s, e) => await OnDeleteOrderClickAsync();

            _btnUploadOrder = new Button
            {
                Text = "☁️ Upload Ulang",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                BackColor = Color.FromArgb(16, 185, 129), // Emerald-500
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(268, 9),
                Width = 120,
                Height = 34,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnUploadOrder.FlatAppearance.BorderSize = 0;
            _btnUploadOrder.Click += async (s, e) => await OnUploadOrderClickAsync();

            _pnlInspectorActions.Controls.Add(_btnEditOrder);
            _pnlInspectorActions.Controls.Add(_btnDeleteOrder);
            _pnlInspectorActions.Controls.Add(_btnUploadOrder);
        }

        private async void OnViewLoad(object sender, EventArgs e)
        {
            await LoadOrdersAsync();
        }

        /// <summary>
        /// Loads return orders matching current filter criteria.
        /// </summary>
        public async Task LoadOrdersAsync()
        {
            DateTime start = _dtpStartDate.Value.Date;
            DateTime end = _dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1);

            StatusSync? statusFilter = null;
            if (_cboStatusFilter.SelectedIndex == 1) statusFilter = StatusSync.Draft;
            else if (_cboStatusFilter.SelectedIndex == 2) statusFilter = StatusSync.Synced;

            try
            {
                IEnumerable<ReturnOrderDto> orders = null;

                if (_orderService != null)
                {
                    orders = await _orderService.GetOrdersAsync(start, end, statusFilter);
                }
                else if (_orderRepo != null)
                {
                    var domainOrders = await _orderRepo.GetFilteredListAsync(start, end, statusFilter);
                    orders = domainOrders.Select(ReturnOrderDto.FromDomain);
                }
                else
                {
                    // In-memory or fallback mode
                    orders = _loadedOrders ?? new List<ReturnOrderDto>();
                }

                _loadedOrders = orders != null ? orders.ToList() : new List<ReturnOrderDto>();
                RenderOrdersGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Gagal memuat daftar retur: {ex.Message}",
                    "Error Database",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Sets orders directly (useful for testing or in-memory view presentation).
        /// </summary>
        public void SetOrders(IEnumerable<ReturnOrderDto> orders)
        {
            _loadedOrders = orders != null ? orders.ToList() : new List<ReturnOrderDto>();
            RenderOrdersGrid();
        }

        private void RenderOrdersGrid()
        {
            _dgvOrders.Rows.Clear();

            foreach (var order in _loadedOrders)
            {
                decimal totalQty = order.Items?.Sum(i => i.Qty) ?? 0;
                string statusText = order.StatusSync.ToString();

                int rowIndex = _dgvOrders.Rows.Add(
                    order.ReturnOrderId,
                    order.ReturnOrderDate.ToString("yyyy-MM-dd"),
                    order.CustName,
                    totalQty.ToString("N0"),
                    statusText);

                _dgvOrders.Rows[rowIndex].Tag = order;
            }

            _lblSummaryCount.Text = $"{_loadedOrders.Count} dokumen ditemukan";

            if (_dgvOrders.Rows.Count > 0)
            {
                _dgvOrders.Rows[0].Selected = true;
                InspectOrder(_dgvOrders.Rows[0].Tag as ReturnOrderDto);
            }
            else
            {
                ClearInspector();
            }
        }

        private void OnOrderSelectionChanged(object sender, EventArgs e)
        {
            if (_dgvOrders.SelectedRows.Count > 0)
            {
                var order = _dgvOrders.SelectedRows[0].Tag as ReturnOrderDto;
                InspectOrder(order);
            }
            else
            {
                ClearInspector();
            }
        }

        /// <summary>
        /// Inspects the given order and enforces Post-Sync Immutability rules.
        /// </summary>
        public void InspectOrder(ReturnOrderDto order)
        {
            _selectedOrder = order;

            if (order == null)
            {
                ClearInspector();
                return;
            }

            // Update Metadata Labels
            _lblMetaOrderId.Text = $"ID: {order.ReturnOrderId}";
            _lblMetaDate.Text = $"Tanggal: {order.ReturnOrderDate:yyyy-MM-dd}";
            _lblMetaCustomer = SetText(_lblMetaCustomer, $"Customer: {order.CustName} ({order.CustId ?? order.CustomerId})");
            _lblMetaOperator = SetText(_lblMetaOperator, $"Operator: {order.OperatorName}");
            _lblMetaWarehouse = SetText(_lblMetaWarehouse, $"Depo: {order.WarehouseCode}");
            _lblMetaDriver = SetText(_lblMetaDriver, $"Driver: {order.DriverName ?? "-"}");
            _lblMetaSales = SetText(_lblMetaSales, $"Sales: {order.SalesPersonName ?? "-"}");
            _lblMetaNote = SetText(_lblMetaNote, $"Catatan: {order.Keterangan ?? "-"}");

            string syncInfo = order.IsSynced
                ? $"Status: SYNCED ({order.SyncedDate:yyyy-MM-dd HH:mm})"
                : "Status: DRAFT (Lokal)";
            _lblMetaSyncInfo.Text = syncInfo;

            // Render Items Grid
            _dgvItems.Rows.Clear();
            if (order.Items != null)
            {
                int no = 1;
                foreach (var item in order.Items)
                {
                    _dgvItems.Rows.Add(
                        no++,
                        item.BrgName,
                        item.Barcode,
                        item.JenisRetur.ToString(),
                        item.Qty.ToString("N0"),
                        item.SatId ?? item.Satuan);
                }
            }

            // POST-SYNC IMMUTABILITY ENFORCEMENT
            bool isSynced = order.IsSynced;

            if (isSynced)
            {
                // Prominent Read-Only Locking banner displayed
                _pnlLockedBanner.Visible = true;
                _btnEditOrder.Enabled = false;
                _btnDeleteOrder.Enabled = false;
                _btnUploadOrder.Enabled = false;
            }
            else
            {
                // Draft documents allow editing, deletion, and upload
                _pnlLockedBanner.Visible = false;
                _btnEditOrder.Enabled = true;
                _btnDeleteOrder.Enabled = true;
                _btnUploadOrder.Enabled = true;
            }
        }

        private Label SetText(Label lbl, string text)
        {
            lbl.Text = text;
            return lbl;
        }

        private void ClearInspector()
        {
            _selectedOrder = null;
            _lblMetaOrderId.Text = "ID: -";
            _lblMetaDate.Text = "Tanggal: -";
            _lblMetaCustomer.Text = "Customer: -";
            _lblMetaOperator.Text = "Operator: -";
            _lblMetaWarehouse.Text = "Depo: -";
            _lblMetaDriver.Text = "Driver: -";
            _lblMetaSales.Text = "Sales: -";
            _lblMetaNote.Text = "Catatan: -";
            _lblMetaSyncInfo.Text = "Status: -";

            _dgvItems.Rows.Clear();
            _pnlLockedBanner.Visible = false;
            _btnEditOrder.Enabled = false;
            _btnDeleteOrder.Enabled = false;
            _btnUploadOrder.Enabled = false;
        }

        private void OnEditOrderClick(object sender, EventArgs e)
        {
            if (_selectedOrder == null) return;

            if (_selectedOrder.IsSynced)
            {
                MessageBox.Show(
                    "Dokumen telah terkunci karena sudah disinkronkan ke pusat (Status = Synced). Perubahan tidak diizinkan.",
                    "Dokumen Terkunci",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            EditDraftRequested?.Invoke(this, _selectedOrder.ReturnOrderId);
        }

        private async Task OnDeleteOrderClickAsync()
        {
            if (_selectedOrder == null) return;

            if (_selectedOrder.IsSynced)
            {
                MessageBox.Show(
                    "Dokumen telah terkunci karena sudah disinkronkan ke pusat (Status = Synced). Penghapusan tidak diizinkan.",
                    "Dokumen Terkunci",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Apakah Anda yakin ingin menghapus dokumen draft retur [{_selectedOrder.ReturnOrderId}] untuk customer '{_selectedOrder.CustName}'?",
                "Konfirmasi Hapus Draft",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                if (_orderService != null)
                {
                    await _orderService.DeleteDraftAsync(_selectedOrder.ReturnOrderId);
                }
                else if (_orderRepo != null)
                {
                    await _orderRepo.DeleteDraftAsync(_selectedOrder.ReturnOrderId);
                }

                _audioHelper?.PlaySuccess();
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                MessageBox.Show(
                    $"Gagal menghapus draft: {ex.Message}",
                    "Error Hapus Draft",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task OnUploadOrderClickAsync()
        {
            if (_selectedOrder == null) return;

            if (_selectedOrder.IsSynced)
            {
                MessageBox.Show("Dokumen ini sudah tersinkronkan sebelumnya.", "Info Sinkronisasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _btnUploadOrder.Enabled = false;
                _btnUploadOrder.Text = "Mengupload...";

                bool success = false;
                if (_orderService != null)
                {
                    var result = await _orderService.CommitAndUploadAsync(_selectedOrder.ReturnOrderId);
                    success = result != null && result.Success;
                }
                else if (_syncService != null)
                {
                    var syncResult = await _syncService.TriggerDocumentUploadAsync(_selectedOrder.ReturnOrderId);
                    success = syncResult != null && syncResult.Success;
                }

                if (success)
                {
                    _audioHelper?.PlaySuccess();
                    MessageBox.Show(
                        $"Dokumen retur [{_selectedOrder.ReturnOrderId}] berhasil disinkronkan ke pusat!",
                        "Upload Berhasil",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    await LoadOrdersAsync();
                }
                else
                {
                    _audioHelper?.PlayWarning();
                    MessageBox.Show(
                        $"Gagal mengupload dokumen retur [{_selectedOrder.ReturnOrderId}] ke Cloud API relay.",
                        "Upload Gagal",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                MessageBox.Show(
                    $"Kesalahan saat upload: {ex.Message}",
                    "Error Upload",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _btnUploadOrder.Text = "☁️ Upload Ulang";
                _btnUploadOrder.Enabled = _selectedOrder != null && !_selectedOrder.IsSynced;
            }
        }

        private void OnOrdersGridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _dgvOrders.Rows.Count) return;

            if (_dgvOrders.Columns[e.ColumnIndex].Name == "colStatus" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status.Equals("Synced", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = Color.FromArgb(209, 250, 229); // Emerald-100
                    e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87);    // Emerald-700
                    e.CellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
                }
                else if (status.Equals("Draft", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 243, 199); // Amber-100
                    e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);    // Amber-700
                    e.CellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
                }
            }
        }

        private void OnItemsGridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _dgvItems.Rows.Count) return;

            if (_dgvItems.Columns[e.ColumnIndex].Name == "colItemJenis" && e.Value != null)
            {
                string jenis = e.Value.ToString();
                if (jenis.Equals("BAGUS", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = Color.FromArgb(209, 250, 229); // Emerald-100
                    e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87);    // Emerald-700
                    e.CellStyle.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
                }
                else if (jenis.Equals("RUSAK", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 226, 226); // Red-100
                    e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);   // Red-700
                    e.CellStyle.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
                }
            }
        }

        private static void EnableDoubleBuffering(DataGridView dgv)
        {
            try
            {
                typeof(DataGridView).InvokeMember(
                    "DoubleBuffered",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                    null,
                    dgv,
                    new object[] { true });
            }
            catch
            {
            }
        }
    }
}
