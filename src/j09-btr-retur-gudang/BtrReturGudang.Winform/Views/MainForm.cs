using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Winform.Hardware;
using BtrReturGudang.Winform.Services;
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
        private readonly OperatorSessionValidator _sessionValidator;
        private bool _sessionValidationInProgress = false;

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
        private ToolTip _operatorToolTip;

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
            IRegistryHelper registryHelper,
            OperatorSessionValidator sessionValidator = null)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _scannerListener = scannerListener ?? throw new ArgumentNullException(nameof(scannerListener));
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
            _registryHelper = registryHelper ?? throw new ArgumentNullException(nameof(registryHelper));
            _sessionValidator = sessionValidator;

            InitializeComponent();
            SetupHardwareScannerHook();
            SetupBackgroundSyncTimer();

            // Validate the configured operator email on startup and warn when unresolved.
            Shown += async (s, e) => await ValidateOperatorSessionAsync();
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

            // Build and dock controls in explicit Z-order.
            // In Windows Forms, docked controls are evaluated in reverse child index order (from highest index to 0).
            // To ensure:
            // 1. _pnlHeader (DockStyle.Top) spans the FULL width across the top of the form (index 3).
            // 2. _statusStrip (DockStyle.Bottom) spans the FULL width across the bottom of the form (index 2).
            // 3. _pnlSidebar (DockStyle.Left) docks cleanly BETWEEN Header and StatusStrip (index 1).
            // 4. _pnlContentHost (DockStyle.Fill) fills the remaining rectangle to the right of the sidebar (index 0).
            BuildContentHost();
            BuildSidebar();
            BuildStatusStrip();
            BuildHeader();

            Controls.SetChildIndex(_pnlContentHost, 0);
            Controls.SetChildIndex(_pnlSidebar, 1);
            Controls.SetChildIndex(_statusStrip, 2);
            Controls.SetChildIndex(_pnlHeader, 3);

            // Initial view load
            LoadView(NavigationModule.Intake);
        }

        private void BuildHeader()
        {
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = UiTheme.Slate900,
                Padding = new Padding(20, 0, 20, 0)
            };

            _pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(UiTheme.Slate800, 1))
                {
                    e.Graphics.DrawLine(pen, 0, _pnlHeader.Height - 1, _pnlHeader.Width, _pnlHeader.Height - 1);
                }
            };

            // Brand Mark container
            var pnlBrand = new Panel
            {
                Dock = DockStyle.Left,
                Width = 420,
                Height = 64,
                BackColor = Color.Transparent
            };

            var lblLogoBadge = new Label
            {
                Text = "BTR",
                Font = new Font("Segoe UI Bold", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = UiTheme.Primary,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(44, 26),
                Location = new Point(0, 19)
            };

            _lblAppTitle = new Label
            {
                Text = "RETUR GUDANG",
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(52, 14)
            };

            var lblAppSubtitle = new Label
            {
                Text = "Desktop Return Order & Barcode Station",
                Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = UiTheme.Slate400,
                AutoSize = true,
                Location = new Point(54, 38)
            };

            pnlBrand.Controls.Add(lblLogoBadge);
            pnlBrand.Controls.Add(_lblAppTitle);
            pnlBrand.Controls.Add(lblAppSubtitle);

            // Flow panel for right-aligned items
            var pnlRightHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Height = 64,
                Padding = new Padding(0, 16, 0, 0)
            };

            // Depo badge
            string depoCode = !string.IsNullOrWhiteSpace(_registryHelper.WarehouseCode)
                ? _registryHelper.WarehouseCode
                : "GDG-PST";

            _lblDepoBadge = new Label
            {
                Text = $"🏢 DEPO: {depoCode}",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = UiTheme.Slate300,
                BackColor = UiTheme.Slate800,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(0, 0, 10, 0),
                AutoSize = true
            };

            // Operator badge
            string operatorName = !string.IsNullOrWhiteSpace(_registryHelper.LastOperatorName)
                ? _registryHelper.LastOperatorName
                : "Operator-1";

            _lblOperatorBadge = new Label
            {
                Text = $"👤 {operatorName}",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.White,
                BackColor = UiTheme.Slate800,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(0, 0, 6, 0),
                AutoSize = true
            };

            // Operator Google email (Cloud session actor) is exposed as the badge tooltip.
            string operatorEmail = _registryHelper.LastOperatorEmail ?? string.Empty;
            _operatorToolTip = new ToolTip();
            _operatorToolTip.SetToolTip(_lblOperatorBadge, FormatOperatorEmailTooltip(operatorEmail));

            // Switch operator button
            _btnSwitchOperator = new Button
            {
                Text = "Ganti",
                Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = UiTheme.Slate300,
                BackColor = UiTheme.Slate700,
                FlatStyle = FlatStyle.Flat,
                Width = 54,
                Height = 28,
                Margin = new Padding(0, 2, 16, 0),
                Cursor = Cursors.Hand
            };
            _btnSwitchOperator.FlatAppearance.BorderSize = 0;
            _btnSwitchOperator.Click += OnSwitchOperatorClick;

            // Online/offline indicator container pill
            var pnlOnlinePill = new Panel
            {
                BackColor = Color.FromArgb(20, 35, 30),
                Height = 28,
                AutoSize = true,
                Padding = new Padding(8, 6, 10, 6),
                Margin = new Padding(0, 2, 0, 0)
            };

            _pnlOnlineDot = new Panel
            {
                Width = 8,
                Height = 8,
                BackColor = UiTheme.Emerald,
                Location = new Point(10, 10)
            };

            _lblOnlineStatus = new Label
            {
                Text = "ONLINE",
                Font = new Font("Segoe UI Bold", 8.5F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = UiTheme.Emerald,
                AutoSize = true,
                Location = new Point(24, 6)
            };

            pnlOnlinePill.Controls.Add(_pnlOnlineDot);
            pnlOnlinePill.Controls.Add(_lblOnlineStatus);

            pnlRightHeader.Controls.Add(_lblDepoBadge);
            pnlRightHeader.Controls.Add(_lblOperatorBadge);
            pnlRightHeader.Controls.Add(_btnSwitchOperator);
            pnlRightHeader.Controls.Add(pnlOnlinePill);

            _pnlHeader.Controls.Add(pnlBrand);
            _pnlHeader.Controls.Add(pnlRightHeader);
            Controls.Add(_pnlHeader);
        }

        private void BuildStatusStrip()
        {
            _statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                BackColor = UiTheme.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                SizingGrip = false,
                Height = 28
            };

            _statusStrip.Paint += (s, e) =>
            {
                using (var pen = new Pen(UiTheme.Slate300, 1))
                {
                    e.Graphics.DrawLine(pen, 0, 0, _statusStrip.Width, 0);
                }
            };

            _lblStatusScanner = new ToolStripStatusLabel
            {
                Text = "⚡ Scanner: Aktif (IMessageFilter)",
                ForeColor = UiTheme.Slate900,
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Padding = new Padding(10, 2, 10, 2)
            };

            _lblStatusLatency = new ToolStripStatusLabel
            {
                Text = "Latency: <= 50ms",
                ForeColor = UiTheme.Slate600,
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Padding = new Padding(10, 2, 10, 2)
            };

            _lblStatusLastSync = new ToolStripStatusLabel
            {
                Text = "Terakhir Sync: Belum ada",
                ForeColor = UiTheme.Slate600,
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Padding = new Padding(10, 2, 10, 2)
            };

            _lblStatusInfo = new ToolStripStatusLabel
            {
                Text = "Siap menerima transaksi fisik retur.",
                ForeColor = UiTheme.Slate500,
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 2, 10, 2)
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
                Width = 230,
                BackColor = UiTheme.Slate850,
                Padding = new Padding(12, 18, 12, 16)
            };

            _pnlSidebar.Paint += (s, e) =>
            {
                using (var pen = new Pen(UiTheme.Slate800, 1))
                {
                    e.Graphics.DrawLine(pen, _pnlSidebar.Width - 1, 0, _pnlSidebar.Width - 1, _pnlSidebar.Height);
                }
            };

            var lblMenuSection = new Label
            {
                Text = "NAVIGASI UTAMA",
                Font = new Font("Segoe UI Bold", 7.5F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = UiTheme.Slate400,
                Location = new Point(16, 16),
                AutoSize = true
            };

            _btnNavIntake = CreateSidebarButton("Intake Retur", "F1", 42, () => NavigateTo(NavigationModule.Intake));
            _btnNavDaftar = CreateSidebarButton("Daftar Retur", "F3", 98, () => NavigateTo(NavigationModule.DaftarRetur));
            _btnNavBarcode = CreateSidebarButton("Barcode Registry", "F4", 154, () => NavigateTo(NavigationModule.BarcodeRegistry));
            _btnNavSettings = CreateSidebarButton("Pengaturan", "F5", 210, () => NavigateTo(NavigationModule.Pengaturan));

            // Manual sync button container at bottom of sidebar
            var pnlSyncContainer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 92,
                BackColor = Color.Transparent,
                Padding = new Padding(4, 8, 4, 0)
            };

            _btnManualSync = new Button
            {
                Text = "🔄 Sync Sekarang",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.White,
                BackColor = UiTheme.Emerald,
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
                ForeColor = UiTheme.Slate400,
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

        private Button CreateSidebarButton(string title, string hotkey, int top, Action onClick)
        {
            var btn = new Button
            {
                Text = $"  [{hotkey}]  {title}",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = UiTheme.Slate300,
                BackColor = UiTheme.Slate850,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(12, top),
                Width = 206,
                Height = 46,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => onClick();

            btn.MouseEnter += (s, e) =>
            {
                if (btn.BackColor != UiTheme.Primary)
                {
                    btn.BackColor = UiTheme.Slate700;
                    btn.ForeColor = Color.White;
                }
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn.BackColor != UiTheme.Primary)
                {
                    btn.BackColor = UiTheme.Slate850;
                    btn.ForeColor = UiTheme.Slate300;
                }
            };

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

        private async void OnSwitchOperatorClick(object sender, EventArgs e)
        {
            using (var dlg = new OperatorSwitcherDialog(_registryHelper))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _lblOperatorBadge.Text = $"👤 {dlg.SelectedOperatorName}";
                    _operatorToolTip?.SetToolTip(_lblOperatorBadge, FormatOperatorEmailTooltip(dlg.SelectedOperatorEmail));
                    _lblStatusInfo.Text = $"Operator shift aktif diubah: {dlg.SelectedOperatorName}";

                    // Operator change: validate the configured email and warn when unresolved.
                    await ValidateOperatorSessionAsync();
                }
            }
        }

        /// <summary>
        /// Validates the configured operator Google email through
        /// <c>POST api/session/resolve</c> and surfaces the outcome on the status strip.
        /// When the operator is unresolved a clear warning is shown instead of allowing
        /// uploads to fail later with an opaque HTTP 409.
        /// Traceable to: ARCHITECTURE §4 TD-012, §5.2 SCR-07, P2-S05.
        /// </summary>
        public async Task ValidateOperatorSessionAsync()
        {
            if (_sessionValidator == null || SuppressSessionWarnings || _sessionValidationInProgress)
                return;

            _sessionValidationInProgress = true;
            try
            {
                var result = await _sessionValidator.ValidateAsync();

                if (result != null && result.Success)
                {
                    _lblStatusInfo.Text = $"Operator terverifikasi: {result.UserName} (Role: {result.RoleId})";
                }
                else if (_sessionValidator.IsUnresolved)
                {
                    _lblStatusInfo.Text = $"Operator belum terverifikasi: {_sessionValidator.RefusalMessage}";
                    WarnUnresolvedOperator(_sessionValidator.RefusalMessage);
                }
                else if (result != null)
                {
                    _lblStatusInfo.Text = $"Verifikasi operator belum berhasil: {_sessionValidator.RefusalMessage}";
                }
            }
            catch (Exception ex)
            {
                _lblStatusInfo.Text = $"Verifikasi operator terhambat: {ex.Message}";
            }
            finally
            {
                _sessionValidationInProgress = false;
            }
        }

        /// <summary>
        /// Shows the unresolved-operator warning to the user. Suppressed in headless tests.
        /// </summary>
        private void WarnUnresolvedOperator(string message)
        {
            if (SuppressSessionWarnings) return;
            if (IsDisposed || !IsHandleCreated) return;

            MessageBox.Show(
                message,
                "Operator Belum Terverifikasi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Formats the operator Google email as the operator badge tooltip text.
        /// The badge label itself continues to show the operator display name.
        /// </summary>
        private static string FormatOperatorEmailTooltip(string operatorEmail)
        {
            return string.IsNullOrWhiteSpace(operatorEmail)
                ? "Google Email Operator belum dikonfigurasi."
                : $"Google Email Operator: {operatorEmail.Trim()}";
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
            var activeColor = UiTheme.Primary;       // Blue-600
            var defaultColor = UiTheme.Slate850;     // Slate-850

            SetNavButtonState(_btnNavIntake, module == NavigationModule.Intake, activeColor, defaultColor);
            SetNavButtonState(_btnNavDaftar, module == NavigationModule.DaftarRetur, activeColor, defaultColor);
            SetNavButtonState(_btnNavBarcode, module == NavigationModule.BarcodeRegistry, activeColor, defaultColor);
            SetNavButtonState(_btnNavSettings, module == NavigationModule.Pengaturan, activeColor, defaultColor);
        }

        private void SetNavButtonState(Button btn, bool isActive, Color activeColor, Color defaultColor)
        {
            btn.BackColor = isActive ? activeColor : defaultColor;
            btn.ForeColor = isActive ? Color.White : UiTheme.Slate300;
            btn.Font = isActive ? UiTheme.FontBodyBold : UiTheme.FontLabel;
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

            // Pre-empt sync/upload when the operator identity is unresolved so the
            // Cloud refusal message is surfaced instead of an opaque HTTP 409.
            if (_sessionValidator != null && _sessionValidator.IsUnresolved)
            {
                _lblStatusInfo.Text = $"Sinkronisasi dihentikan: {_sessionValidator.RefusalMessage}";
                WarnUnresolvedOperator(_sessionValidator.RefusalMessage);
                UpdateSyncStatusDisplay(false);
                return;
            }

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
        /// Test hook exposing the operator badge (tooltip carries the operator Google email).
        /// </summary>
        public Label OperatorBadge => _lblOperatorBadge;

        /// <summary>
        /// Test hook exposing currently active navigation module.
        /// </summary>
        public NavigationModule CurrentModule => _currentModule;

        /// <summary>
        /// Test hook exposing manual sync button.
        /// </summary>
        public Button ManualSyncButton => _btnManualSync;

        /// <summary>
        /// Test hook exposing the shared operator session validator.
        /// </summary>
        public OperatorSessionValidator SessionValidator => _sessionValidator;

        /// <summary>
        /// When true, unresolved-operator modal warnings are suppressed (headless tests).
        /// </summary>
        public bool SuppressSessionWarnings { get; set; }

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

                _operatorToolTip?.Dispose();

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
