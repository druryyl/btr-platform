using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Winform.Hardware;
using Microsoft.Extensions.DependencyInjection;

namespace BtrReturGudang.Winform.Views
{
    /// <summary>
    /// Navigation module enumeration for application shell.
    /// </summary>
    public enum NavigationModule
    {
        Intake,
        DaftarRetur,
        BarcodeRegistry,
        Pengaturan
    }

    /// <summary>
    /// SCR-01: Main Application Shell Window.
    /// Hosts docked sidebar navigation, dynamic view container (pnlContentHost), workstation header,
    /// operator switcher, global scanner message filter hook, 5-minute background sync timer, and bottom status strip.
    /// Traceable to: ARCHITECTURE §5.2 SCR-01, TD-011, P3-S07.
    /// </summary>
    public class MainForm : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ScannerWedgeListener _scannerListener;
        private readonly ISyncService _syncService;
        private readonly IRegistryHelper _registryHelper;

        // UI Controls
        private Panel _pnlHeader;
        private Panel _pnlSidebar;
        private Panel _pnlContentHost;
        private StatusStrip _statusStrip;

        // Header controls
        private Label _lblAppTitle;
        private Label _lblDepoBadge;
        private Label _lblOperatorBadge;
        private Button _btnSwitchOperator;
        private Panel _pnlOnlineDot;
        private Label _lblOnlineStatus;

        // Sidebar navigation controls
        private Button _btnNavIntake;
        private Button _btnNavDaftar;
        private Button _btnNavBarcode;
        private Button _btnNavSettings;
        private Button _btnManualSync;
        private Label _lblSyncAnimation;

        // Status strip items
        private ToolStripStatusLabel _lblStatusScanner;
        private ToolStripStatusLabel _lblStatusLatency;
        private ToolStripStatusLabel _lblStatusLastSync;
        private ToolStripStatusLabel _lblStatusInfo;

        // Background sync timer (5 minutes = 300,000 ms)
        private Timer _bgSyncTimer;
        private Timer _syncAnimationTimer;
        private int _animationStep = 0;
        private bool _isManualSyncRunning = false;

        private NavigationModule _currentModule = NavigationModule.Intake;
        private Control _activeViewControl = null;

        public MainForm(
            IServiceProvider serviceProvider,
            ScannerWedgeListener scannerListener,
            ISyncService syncService,
            IRegistryHelper registryHelper)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _scannerListener = scannerListener ?? throw new ArgumentNullException(nameof(scannerListener));
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
            _registryHelper = registryHelper ?? throw new ArgumentNullException(nameof(registryHelper));

            InitializeComponent();
            SetupHardwareScannerHook();
            SetupBackgroundSyncTimer();
        }

        private void InitializeComponent()
        {
            Text = "BTR Retur Gudang - Desktop Application";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1280, 800);
            MinimumSize = new Size(1024, 650);
            KeyPreview = true;
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Color.FromArgb(241, 245, 249); // Slate-100

            // 1. Header (Top)
            BuildHeader();

            // 2. Status Strip (Bottom)
            BuildStatusStrip();

            // 3. Sidebar (Left)
            BuildSidebar();

            // 4. Content Host (Fill)
            BuildContentHost();

            // Initial view load
            LoadView(NavigationModule.Intake);
        }

        private void BuildHeader()
        {
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(15, 23, 42), // Slate-900
                Padding = new Padding(20, 0, 20, 0)
            };

            _lblAppTitle = new Label
            {
                Text = "BTR RETUR GUDANG",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 18)
            };

            // Flow panel for right-aligned items
            var pnlRightHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Height = 60,
                Padding = new Padding(0, 14, 0, 0)
            };

            // Depo badge
            string depoCode = !string.IsNullOrWhiteSpace(_registryHelper.WarehouseCode)
                ? _registryHelper.WarehouseCode
                : "GDG-PST";

            _lblDepoBadge = new Label
            {
                Text = $"🏢 DEPO: {depoCode}",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(203, 213, 225), // Slate-300
                BackColor = Color.FromArgb(30, 41, 59),   // Slate-800
                Padding = new Padding(8, 5, 8, 5),
                Margin = new Padding(0, 0, 12, 0),
                AutoSize = true
            };

            // Operator badge
            string operatorName = !string.IsNullOrWhiteSpace(_registryHelper.LastOperatorName)
                ? _registryHelper.LastOperatorName
                : "Operator-1";

            _lblOperatorBadge = new Label
            {
                Text = $"👤 {operatorName}",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(8, 5, 8, 5),
                Margin = new Padding(0, 0, 4, 0),
                AutoSize = true
            };

            // Switch operator button
            _btnSwitchOperator = new Button
            {
                Text = "Ganti",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(51, 65, 85), // Slate-700
                FlatStyle = FlatStyle.Flat,
                Width = 56,
                Height = 28,
                Margin = new Padding(0, 2, 16, 0),
                Cursor = Cursors.Hand
            };
            _btnSwitchOperator.FlatAppearance.BorderSize = 0;
            _btnSwitchOperator.Click += OnSwitchOperatorClick;

            // Live online/offline indicator dot
            _pnlOnlineDot = new Panel
            {
                Width = 12,
                Height = 12,
                BackColor = Color.FromArgb(16, 185, 129), // Emerald-500 (Online by default)
                Margin = new Padding(0, 10, 6, 0)
            };

            _lblOnlineStatus = new Label
            {
                Text = "ONLINE",
                Font = new Font("Segoe UI Bold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(16, 185, 129),
                AutoSize = true,
                Margin = new Padding(0, 7, 0, 0)
            };

            pnlRightHeader.Controls.Add(_lblDepoBadge);
            pnlRightHeader.Controls.Add(_lblOperatorBadge);
            pnlRightHeader.Controls.Add(_btnSwitchOperator);
            pnlRightHeader.Controls.Add(_pnlOnlineDot);
            pnlRightHeader.Controls.Add(_lblOnlineStatus);

            _pnlHeader.Controls.Add(_lblAppTitle);
            _pnlHeader.Controls.Add(pnlRightHeader);
            Controls.Add(_pnlHeader);
        }

        private void BuildStatusStrip()
        {
            _statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                SizingGrip = false
            };

            _lblStatusScanner = new ToolStripStatusLabel
            {
                Text = "Scanner: Aktif (IMessageFilter)",
                ForeColor = Color.FromArgb(15, 23, 42),
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Padding = new Padding(8, 0, 8, 0)
            };

            _lblStatusLatency = new ToolStripStatusLabel
            {
                Text = "Buffer Latency: <= 50ms",
                ForeColor = Color.FromArgb(71, 85, 105),
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Padding = new Padding(8, 0, 8, 0)
            };

            _lblStatusLastSync = new ToolStripStatusLabel
            {
                Text = "Terakhir Sync: Belum ada",
                ForeColor = Color.FromArgb(71, 85, 105),
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Padding = new Padding(8, 0, 8, 0)
            };

            _lblStatusInfo = new ToolStripStatusLabel
            {
                Text = "Siap menerima transaksi.",
                ForeColor = Color.FromArgb(100, 116, 139),
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 8, 0)
            };

            _statusStrip.Items.AddRange(new ToolStripItem[]
            {
                _lblStatusScanner,
                _lblStatusLatency,
                _lblStatusLastSync,
                _lblStatusInfo
            });

            Controls.Add(_statusStrip);
        }

        private void BuildSidebar()
        {
            _pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(30, 41, 59), // Slate-800
                Padding = new Padding(12, 16, 12, 16)
            };

            var lblMenuSection = new Label
            {
                Text = "NAVIGASI UTAMA",
                Font = new Font("Segoe UI Bold", 8F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(148, 163, 184), // Slate-400
                Location = new Point(16, 16),
                AutoSize = true
            };

            _btnNavIntake = CreateSidebarButton("F1   Intake Retur", 42, () => NavigateTo(NavigationModule.Intake));
            _btnNavDaftar = CreateSidebarButton("F3   Daftar Retur", 98, () => NavigateTo(NavigationModule.DaftarRetur));
            _btnNavBarcode = CreateSidebarButton("F4   Barcode Registry", 154, () => NavigateTo(NavigationModule.BarcodeRegistry));
            _btnNavSettings = CreateSidebarButton("F5   Pengaturan", 210, () => NavigateTo(NavigationModule.Pengaturan));

            // Manual sync button container at bottom of sidebar
            var pnlSyncContainer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 90,
                BackColor = Color.Transparent
            };

            _btnManualSync = new Button
            {
                Text = "🔄 Sync Sekarang",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(16, 185, 129), // Emerald-500
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Top,
                Height = 44,
                Cursor = Cursors.Hand
            };
            _btnManualSync.FlatAppearance.BorderSize = 0;
            _btnManualSync.Click += async (s, e) => await TriggerManualSyncAsync();

            _lblSyncAnimation = new Label
            {
                Text = "Sinkronisasi otomatis: Tiap 5 menit",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(148, 163, 184),
                Dock = DockStyle.Bottom,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlSyncContainer.Controls.Add(_btnManualSync);
            pnlSyncContainer.Controls.Add(_lblSyncAnimation);

            _pnlSidebar.Controls.Add(lblMenuSection);
            _pnlSidebar.Controls.Add(_btnNavIntake);
            _pnlSidebar.Controls.Add(_btnNavDaftar);
            _pnlSidebar.Controls.Add(_btnNavBarcode);
            _pnlSidebar.Controls.Add(_btnNavSettings);
            _pnlSidebar.Controls.Add(pnlSyncContainer);

            Controls.Add(_pnlSidebar);
        }

        private Button CreateSidebarButton(string text, int top, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(12, top),
                Width = 196,
                Height = 46,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private void BuildContentHost()
        {
            _pnlContentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(241, 245, 249)
            };
            Controls.Add(_pnlContentHost);
        }

        private void SetupHardwareScannerHook()
        {
            // Register global scanner message filter hook
            Application.AddMessageFilter(_scannerListener);

            _scannerListener.BarcodeScanned += OnBarcodeScannedReceived;
        }

        private void SetupBackgroundSyncTimer()
        {
            // 5 minutes = 300,000 ms
            _bgSyncTimer = new Timer
            {
                Interval = 5 * 60 * 1000
            };
            _bgSyncTimer.Tick += async (s, e) => await ExecuteBackgroundSyncPassAsync(isBackground: true);
            _bgSyncTimer.Start();

            // Short animation timer for sync button spinner
            _syncAnimationTimer = new Timer
            {
                Interval = 300
            };
            _syncAnimationTimer.Tick += OnSyncAnimationTick;

            // Trigger non-blocking initial sync pass shortly after startup
            Task.Delay(1500).ContinueWith(_ =>
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    Invoke(new Action(async () => await ExecuteBackgroundSyncPassAsync(isBackground: true)));
                }
            });
        }

        private void OnBarcodeScannedReceived(object sender, BarcodeScannedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<BarcodeScannedEventArgs>(OnBarcodeScannedReceived), sender, e);
                return;
            }

            _lblStatusInfo.Text = $"Scan Terakhir: [{e.NormalizedBarcode}] pada {DateTime.Now:HH:mm:ss}";

            // Dispatch to active view if it consumes barcode scans
            if (_activeViewControl is IScannerConsumer consumer)
            {
                consumer.OnBarcodeScanned(e.NormalizedBarcode);
            }
        }

        private void OnSwitchOperatorClick(object sender, EventArgs e)
        {
            using (var dlg = new OperatorSwitcherDialog(_registryHelper))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _lblOperatorBadge.Text = $"👤 {dlg.SelectedOperatorName}";
                    _lblStatusInfo.Text = $"Operator shift aktif diubah: {dlg.SelectedOperatorName}";
                }
            }
        }

        public void NavigateTo(NavigationModule module)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<NavigationModule>(NavigateTo), module);
                return;
            }

            _currentModule = module;
            UpdateSidebarHighlight(module);
            LoadView(module);
        }

        private void UpdateSidebarHighlight(NavigationModule module)
        {
            var activeColor = Color.FromArgb(37, 99, 235);   // Blue-600
            var defaultColor = Color.FromArgb(30, 41, 59);  // Slate-800

            _btnNavIntake.BackColor = module == NavigationModule.Intake ? activeColor : defaultColor;
            _btnNavDaftar.BackColor = module == NavigationModule.DaftarRetur ? activeColor : defaultColor;
            _btnNavBarcode.BackColor = module == NavigationModule.BarcodeRegistry ? activeColor : defaultColor;
            _btnNavSettings.BackColor = module == NavigationModule.Pengaturan ? activeColor : defaultColor;
        }

        private void LoadView(NavigationModule module)
        {
            Control targetView = null;

            switch (module)
            {
                case NavigationModule.Intake:
                    targetView = _serviceProvider.GetService<ReturnOrderEntryView>() ?? new ReturnOrderEntryView();
                    break;
                case NavigationModule.DaftarRetur:
                    targetView = _serviceProvider.GetService<ReturnOrderListView>() ?? new ReturnOrderListView();
                    break;
                case NavigationModule.BarcodeRegistry:
                    targetView = _serviceProvider.GetService<BarcodeRegistryView>() ?? new BarcodeRegistryView();
                    break;
                case NavigationModule.Pengaturan:
                    targetView = _serviceProvider.GetService<SettingsView>() ?? new SettingsView();
                    break;
            }

            if (targetView == null) return;

            _pnlContentHost.SuspendLayout();
            _pnlContentHost.Controls.Clear();
            targetView.Dock = DockStyle.Fill;
            _pnlContentHost.Controls.Add(targetView);
            _pnlContentHost.ResumeLayout(true);

            _activeViewControl = targetView;
            targetView.Focus();
        }

        private async Task TriggerManualSyncAsync()
        {
            if (_isManualSyncRunning || _syncService.IsSyncInProgress) return;

            await ExecuteBackgroundSyncPassAsync(isBackground: false);
        }

        private async Task ExecuteBackgroundSyncPassAsync(bool isBackground)
        {
            if (_isManualSyncRunning || _syncService.IsSyncInProgress) return;

            _isManualSyncRunning = true;
            StartSyncAnimation();

            _lblStatusInfo.Text = isBackground
                ? "Menjalankan sinkronisasi background 5-menit..."
                : "Menjalankan sinkronisasi manual...";

            bool success = false;
            try
            {
                var result = await _syncService.RunFullSyncPassAsync();
                success = result != null && result.Success;
            }
            catch (Exception ex)
            {
                success = false;
                _lblStatusInfo.Text = $"Sinkronisasi terhambat: {ex.Message}";
            }
            finally
            {
                StopSyncAnimation();
                _isManualSyncRunning = false;

                UpdateSyncStatusDisplay(success);
            }
        }

        private void StartSyncAnimation()
        {
            _btnManualSync.Enabled = false;
            _animationStep = 0;
            _syncAnimationTimer.Start();
        }

        private void StopSyncAnimation()
        {
            _syncAnimationTimer.Stop();
            _btnManualSync.Text = "🔄 Sync Sekarang";
            _btnManualSync.Enabled = true;
            _lblSyncAnimation.Text = "Sinkronisasi otomatis: Tiap 5 menit";
        }

        private void OnSyncAnimationTick(object sender, EventArgs e)
        {
            string[] spinner = new[] { "◐ Menyinkronkan...", "◓ Menyinkronkan...", "◑ Menyinkronkan...", "◒ Menyinkronkan..." };
            _btnManualSync.Text = spinner[_animationStep % spinner.Length];
            _animationStep++;
        }

        private void UpdateSyncStatusDisplay(bool success)
        {
            string timeStr = DateTime.Now.ToString("HH:mm:ss");

            if (success)
            {
                _lblStatusLastSync.Text = $"Terakhir Sync: {timeStr} (Sukses)";
                _lblStatusLastSync.ForeColor = Color.FromArgb(16, 185, 129); // Emerald
                SetOnlineIndicator(true);
            }
            else
            {
                _lblStatusLastSync.Text = $"Terakhir Sync: {timeStr} (Offline/Pending)";
                _lblStatusLastSync.ForeColor = Color.FromArgb(239, 68, 68); // Red
                SetOnlineIndicator(false);
            }
        }

        public void SetOnlineIndicator(bool isOnline)
        {
            if (isOnline)
            {
                _pnlOnlineDot.BackColor = Color.FromArgb(16, 185, 129); // Emerald
                _lblOnlineStatus.Text = "ONLINE";
                _lblOnlineStatus.ForeColor = Color.FromArgb(16, 185, 129);
            }
            else
            {
                _pnlOnlineDot.BackColor = Color.FromArgb(239, 68, 68); // Red-500
                _lblOnlineStatus.Text = "OFFLINE";
                _lblOnlineStatus.ForeColor = Color.FromArgb(239, 68, 68);
            }
        }

        /// <summary>
        /// Test hook exposing currently active hosted view control.
        /// </summary>
        public Control ActiveViewControl => _activeViewControl;

        /// <summary>
        /// Test hook exposing currently active navigation module.
        /// </summary>
        public NavigationModule CurrentModule => _currentModule;

        /// <summary>
        /// Test hook exposing manual sync button.
        /// </summary>
        public Button ManualSyncButton => _btnManualSync;

        /// <summary>
        /// Test hook triggering background sync pass.
        /// </summary>
        public Task TriggerBackgroundSyncPassForTestingAsync(bool isBackground = true) => ExecuteBackgroundSyncPassAsync(isBackground);

        /// <summary>
        /// Test hook triggering manual sync.
        /// </summary>
        public Task TriggerManualSyncForTestingAsync() => TriggerManualSyncAsync();

        /// <summary>
        /// Intercepts global keyboard shortcuts F1, F3, F4, F5 for screen navigation.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F1:
                    NavigateTo(NavigationModule.Intake);
                    return true;
                case Keys.F3:
                    NavigateTo(NavigationModule.DaftarRetur);
                    return true;
                case Keys.F4:
                    NavigateTo(NavigationModule.BarcodeRegistry);
                    return true;
                case Keys.F5:
                    NavigateTo(NavigationModule.Pengaturan);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                _bgSyncTimer?.Stop();
                _bgSyncTimer?.Dispose();
                _syncAnimationTimer?.Stop();
                _syncAnimationTimer?.Dispose();

                Application.RemoveMessageFilter(_scannerListener);
            }
            catch
            {
                // Suppress on close
            }

            base.OnFormClosed(e);
        }
    }
}
