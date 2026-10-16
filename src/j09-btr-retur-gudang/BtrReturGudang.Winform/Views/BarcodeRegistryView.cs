using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;

namespace BtrReturGudang.Winform.Views
{
    /// <summary>
    /// Display model for Barcode Registry view rows.
    /// Combines active catalog barcodes and pending registration queue records.
    /// </summary>
    public class BarcodeDisplayItem
    {
        public string Barcode { get; set; }
        public string BrgId { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; }
        public decimal Pengali { get; set; }
        public string Status { get; set; } // "Aktif Pusat" or "Pending Upload"
        public bool IsPending { get; set; }
    }

    /// <summary>
    /// SCR-06: Barcode Catalog Management &amp; Quick Registration Inspector view.
    /// Provides search filtering across local catalog and registration queues,
    /// colored status badges, and action to register new unmapped barcodes.
    /// Traceable to: ARCHITECTURE §5.2 SCR-06, TD-006, TD-011, P3-S10.
    /// </summary>
    public class BarcodeRegistryView : UserControl
    {
        private readonly IBarcodeRepo _barcodeRepo;
        private readonly IBarcodeService _barcodeService;
        private readonly IReferenceRepo _referenceRepo;
        private readonly IRegistryHelper _registryHelper;
        private readonly IAudioHelper _audioHelper;

        // UI Header Controls
        private Panel _pnlHeader;
        private Label _lblTitle;
        private Label _lblStatus;
        private Button _btnAddBarcode;
        private Button _btnRefresh;

        // UI Search / Filter Controls
        private Panel _pnlFilter;
        private TextBox _txtSearch;
        private Button _btnSearch;
        private ComboBox _cboStatusFilter;
        private Label _lblItemCount;

        // Main Data Grid
        private DataGridView _dgvBarcodes;

        // Cached items
        private List<BarcodeDisplayItem> _allItems = new List<BarcodeDisplayItem>();
        private List<BarcodeDisplayItem> _filteredItems = new List<BarcodeDisplayItem>();

        // Public test accessors
        public DataGridView BarcodesGrid => _dgvBarcodes;
        public TextBox SearchTextBox => _txtSearch;
        public Button SearchButton => _btnSearch;
        public Button AddBarcodeButton => _btnAddBarcode;
        public ComboBox StatusFilterBox => _cboStatusFilter;
        public IReadOnlyList<BarcodeDisplayItem> LoadedItems => _filteredItems;

        public BarcodeRegistryView()
            : this(null, null, null, null, null)
        {
        }

        public BarcodeRegistryView(
            IBarcodeRepo barcodeRepo = null,
            IBarcodeService barcodeService = null,
            IReferenceRepo referenceRepo = null,
            IRegistryHelper registryHelper = null,
            IAudioHelper audioHelper = null)
        {
            _barcodeRepo = barcodeRepo;
            _barcodeService = barcodeService;
            _referenceRepo = referenceRepo;
            _registryHelper = registryHelper;
            _audioHelper = audioHelper;

            InitializeComponent();
            EnableDoubleBuffering(_dgvBarcodes);

            Load += async (s, e) => await LoadBarcodesAsync();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = UiTheme.Slate100;
            Font = UiTheme.FontBody;

            // 1. Top Header
            BuildHeader();

            // 2. Search & Filter Bar
            BuildFilterBar();

            // 3. Grid Container
            BuildDataGrid();

            var pnlGridContainer = UiTheme.CreateCard(new Padding(12), new Padding(24, 16, 24, 24));
            pnlGridContainer.Dock = DockStyle.Fill;
            pnlGridContainer.Controls.Add(_dgvBarcodes);

            Controls.Add(pnlGridContainer);
            Controls.Add(_pnlFilter);
            Controls.Add(_pnlHeader);
        }

        private void BuildHeader()
        {
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = UiTheme.White,
                Padding = new Padding(24, 12, 24, 12)
            };

            _lblTitle = new Label
            {
                Text = "Barcode Registry [F4]",
                Font = UiTheme.FontPageTitle,
                ForeColor = UiTheme.Slate900,
                AutoSize = true,
                Location = new Point(24, 10)
            };

            _lblStatus = new Label
            {
                Text = "Katalog barcode fisik lokal, status sinkronisasi, dan registrasi on-the-spot untuk barang belum terpetakan.",
                Font = UiTheme.FontSubtitle,
                ForeColor = UiTheme.Slate500,
                AutoSize = true,
                Location = new Point(26, 38)
            };

            var pnlHeaderRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Height = 50,
                Padding = new Padding(0, 6, 0, 0)
            };

            _btnAddBarcode = new Button
            {
                Text = "➕ Tambah Barcode Baru",
                Font = UiTheme.FontLabel,
                BackColor = UiTheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 185,
                Height = 34,
                Margin = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand
            };
            _btnAddBarcode.FlatAppearance.BorderSize = 0;
            _btnAddBarcode.Click += OnAddBarcodeClick;

            _btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Font = UiTheme.FontLabel,
                BackColor = UiTheme.Slate100,
                ForeColor = UiTheme.Slate800,
                FlatStyle = FlatStyle.Flat,
                Width = 95,
                Height = 34,
                Cursor = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderColor = UiTheme.Slate300;
            _btnRefresh.Click += async (s, e) => await LoadBarcodesAsync();

            pnlHeaderRight.Controls.Add(_btnAddBarcode);
            pnlHeaderRight.Controls.Add(_btnRefresh);

            _pnlHeader.Controls.Add(_lblTitle);
            _pnlHeader.Controls.Add(_lblStatus);
            _pnlHeader.Controls.Add(pnlHeaderRight);
        }

        private void BuildFilterBar()
        {
            _pnlFilter = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = UiTheme.Slate50,
                Padding = new Padding(24, 10, 24, 10),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            var lblSearch = new Label
            {
                Text = "Cari Barcode / Kode / Nama:",
                AutoSize = true,
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Margin = new Padding(0, 6, 6, 0)
            };

            _txtSearch = new TextBox
            {
                Font = UiTheme.FontBody,
                Width = 260,
                Margin = new Padding(0, 2, 8, 0)
            };
            _txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplyFilter();
                }
            };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();

            _btnSearch = new Button
            {
                Text = "Cari",
                Font = UiTheme.FontLabel,
                BackColor = UiTheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 65,
                Height = 30,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 1, 24, 0)
            };
            _btnSearch.FlatAppearance.BorderSize = 0;
            _btnSearch.Click += (s, e) => ApplyFilter();

            var lblStatus = new Label
            {
                Text = "Filter Status:",
                AutoSize = true,
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate700,
                Margin = new Padding(0, 6, 6, 0)
            };

            _cboStatusFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.FontBody,
                Width = 145,
                Margin = new Padding(0, 2, 20, 0)
            };
            _cboStatusFilter.Items.AddRange(new object[] { "Semua Status", "Aktif Pusat", "Pending Upload" });
            _cboStatusFilter.SelectedIndex = 0;
            _cboStatusFilter.SelectedIndexChanged += (s, e) => ApplyFilter();

            _lblItemCount = new Label
            {
                Text = "0 barcode terdaftar",
                AutoSize = true,
                Font = UiTheme.FontLabel,
                ForeColor = UiTheme.Slate500,
                Margin = new Padding(0, 6, 0, 0)
            };

            _pnlFilter.Controls.Add(lblSearch);
            _pnlFilter.Controls.Add(_txtSearch);
            _pnlFilter.Controls.Add(_btnSearch);
            _pnlFilter.Controls.Add(lblStatus);
            _pnlFilter.Controls.Add(_cboStatusFilter);
            _pnlFilter.Controls.Add(_lblItemCount);
        }

        private void BuildDataGrid()
        {
            _dgvBarcodes = new DataGridView
            {
                Dock = DockStyle.Fill
            };
            UiTheme.StyleDataGrid(_dgvBarcodes);

            _dgvBarcodes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colBarcode",
                HeaderText = "Nilai Barcode",
                Width = 190,
                DefaultCellStyle = { Font = UiTheme.FontMonospaceBold }
            });

            _dgvBarcodes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colBrgId",
                HeaderText = "Kode Barang",
                Width = 130,
                DefaultCellStyle = { Font = UiTheme.FontMonospace }
            });

            _dgvBarcodes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colBrgName",
                HeaderText = "Nama Barang Master",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _dgvBarcodes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSatuan",
                HeaderText = "Satuan",
                Width = 85,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _dgvBarcodes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPengali",
                HeaderText = "Pengali",
                Width = 85,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = UiTheme.FontBodyBold }
            });

            _dgvBarcodes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "Status Barcode",
                Width = 145,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _dgvBarcodes.CellFormatting += OnGridCellFormatting;
        }

        public async Task LoadBarcodesAsync()
        {
            try
            {
                var list = new List<BarcodeDisplayItem>();

                // 1. Fetch active cached barcodes
                if (_barcodeRepo != null)
                {
                    var barcodes = await _barcodeRepo.GetAllBarcodesAsync();
                    if (barcodes != null)
                    {
                        foreach (var b in barcodes)
                        {
                            list.Add(new BarcodeDisplayItem
                            {
                                Barcode = b.BarcodeValue,
                                BrgId = b.BrgId,
                                BrgName = b.BrgName,
                                Satuan = b.Satuan,
                                Pengali = b.Pengali,
                                Status = "Aktif Pusat",
                                IsPending = false
                            });
                        }
                    }

                    // 2. Fetch pending registration requests
                    var pending = await _barcodeRepo.GetPendingRegistrationsAsync();
                    if (pending != null)
                    {
                        foreach (var p in pending)
                        {
                            // If not already in list with higher priority
                            var existing = list.FirstOrDefault(x => x.Barcode.Equals(p.Barcode, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.Status = "Pending Upload";
                                existing.IsPending = true;
                            }
                            else
                            {
                                list.Insert(0, new BarcodeDisplayItem
                                {
                                    Barcode = p.Barcode,
                                    BrgId = p.BrgId,
                                    BrgName = $"[Pending] {p.BrgId}",
                                    Satuan = p.Satuan,
                                    Pengali = 1m,
                                    Status = "Pending Upload",
                                    IsPending = true
                                });
                            }
                        }
                    }
                }
                else
                {
                    list = _allItems ?? new List<BarcodeDisplayItem>();
                }

                _allItems = list;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat katalog barcode: {ex.Message}", "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetBarcodes(IEnumerable<BarcodeDisplayItem> items)
        {
            _allItems = items != null ? items.ToList() : new List<BarcodeDisplayItem>();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string keyword = _txtSearch?.Text?.Trim() ?? string.Empty;
            int statusIndex = _cboStatusFilter?.SelectedIndex ?? 0;

            var query = _allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    (x.Barcode != null && x.Barcode.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (x.BrgId != null && x.BrgId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (x.BrgName != null && x.BrgName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (statusIndex == 1) // Aktif Pusat
            {
                query = query.Where(x => !x.IsPending);
            }
            else if (statusIndex == 2) // Pending Upload
            {
                query = query.Where(x => x.IsPending);
            }

            _filteredItems = query.ToList();
            RenderGrid();
        }

        private void RenderGrid()
        {
            _dgvBarcodes.Rows.Clear();

            foreach (var item in _filteredItems)
            {
                int idx = _dgvBarcodes.Rows.Add(
                    item.Barcode,
                    item.BrgId,
                    item.BrgName,
                    item.Satuan,
                    item.Pengali.ToString("N0"),
                    item.Status);

                _dgvBarcodes.Rows[idx].Tag = item;
            }

            _lblItemCount.Text = $"{_filteredItems.Count} barcode ditemukan";
        }

        private void OnAddBarcodeClick(object sender, EventArgs e)
        {
            using (var dlg = new BarcodeRegistrationDialog(
                initialBarcode: string.Empty,
                barcodeService: _barcodeService,
                barcodeRepo: _barcodeRepo,
                referenceRepo: _referenceRepo,
                registryHelper: _registryHelper,
                audioHelper: _audioHelper))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    Task.Run(async () =>
                    {
                        await LoadBarcodesAsync();
                    });
                }
            }
        }

        private void OnGridCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _dgvBarcodes.Rows.Count) return;

            if (_dgvBarcodes.Columns[e.ColumnIndex].Name == "colStatus" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status.Equals("Aktif Pusat", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = Color.FromArgb(209, 250, 229); // Emerald-100
                    e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87);    // Emerald-700
                    e.CellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
                }
                else if (status.Equals("Pending Upload", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 243, 199); // Amber-100
                    e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);    // Amber-700
                    e.CellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
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
