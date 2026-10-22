using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Wpf.Mvvm;
using BtrReturGudang.Wpf.Services;

namespace BtrReturGudang.Wpf.ViewModels
{
    /// <summary>
    /// ViewModel for the application shell (MainWindow SCR-01).
    /// Orchestrates top header metadata, docked sidebar navigation, active content view model,
    /// 5-minute background sync timer, global hotkeys, and bottom status bar.
    /// Traceable to: ARCHITECTURE §4 TD-008, TD-011, TD-012, TD-013, §5.2 SCR-01, P2-S04.
    /// </summary>
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISyncService _syncService;
        private readonly IRegistryHelper _registryHelper;
        private readonly WpfScannerWedgeListener _scannerListener;
        private readonly OperatorSessionValidator _sessionValidator;

        // Navigation state
        private NavigationModule _currentModule = NavigationModule.Intake;
        private ViewModelBase _currentViewModel;

        // Header properties
        private string _depoCode;
        private string _operatorName;
        private string _operatorEmail;
        private bool _isOnline = true;
        private string _onlineStatusText = "ONLINE";

        // Status bar properties
        private string _scannerStatusText = "⚡ Scanner: Aktif (WPF Wedge Hook)";
        private string _latencyStatusText = "Latency: <= 50ms";
        private string _lastSyncStatusText = "Terakhir Sync: Belum ada";
        private bool? _lastSyncSuccess = null;
        private string _statusInfoText = "Siap menerima transaksi fisik retur.";

        // Sync & background timers
        private Timer _bgSyncTimer;
        private Timer _syncAnimationTimer;
        private int _animationStep = 0;
        private bool _isSyncRunning = false;
        private string _manualSyncButtonText = "🔄 Sync Sekarang";
        private string _syncAnimationText = "Sinkronisasi otomatis: Tiap 5 menit";
        private bool _sessionValidationInProgress = false;

        private static readonly string[] SpinnerFrames = new[]
        {
            "◐ Menyinkronkan...",
            "◓ Menyinkronkan...",
            "◑ Menyinkronkan...",
            "◒ Menyinkronkan..."
        };

        public MainViewModel(
            IServiceProvider serviceProvider,
            ISyncService syncService,
            IRegistryHelper registryHelper,
            WpfScannerWedgeListener scannerListener = null,
            OperatorSessionValidator sessionValidator = null)
        {
            _serviceProvider = serviceProvider;
            _syncService = syncService;
            _registryHelper = registryHelper;
            _scannerListener = scannerListener;
            _sessionValidator = sessionValidator;

            // Initialize Header info from Registry
            _depoCode = !string.IsNullOrWhiteSpace(_registryHelper?.WarehouseCode)
                ? _registryHelper.WarehouseCode
                : "GDG-PST";

            _operatorName = !string.IsNullOrWhiteSpace(_registryHelper?.LastOperatorName)
                ? _registryHelper.LastOperatorName
                : "Operator-1";

            _operatorEmail = _registryHelper?.LastOperatorEmail ?? string.Empty;

            // Commands
            NavigateToIntakeCommand = new RelayCommand(() => NavigateTo(NavigationModule.Intake));
            NavigateToDaftarReturCommand = new RelayCommand(() => NavigateTo(NavigationModule.DaftarRetur));
            NavigateToBarcodeRegistryCommand = new RelayCommand(() => NavigateTo(NavigationModule.BarcodeRegistry));
            NavigateToSettingsCommand = new RelayCommand(() => NavigateTo(NavigationModule.Pengaturan));
            NavigateCommand = new RelayCommand<NavigationModule>(NavigateTo);

            SaveDraftCommand = new RelayCommand(ExecuteSaveDraft);
            SaveAndUploadCommand = new RelayCommand(ExecuteSaveAndUpload);
            CancelOrClearCommand = new RelayCommand(ExecuteCancelOrClear);

            ManualSyncCommand = new AsyncRelayCommand(
                () => ExecuteBackgroundSyncPassAsync(isBackground: false),
                () => !IsSyncRunning);

            SwitchOperatorCommand = new RelayCommand(() => OnSwitchOperatorRequested?.Invoke());

            // Setup hardware scanner listener hook
            if (_scannerListener != null)
            {
                _scannerListener.BarcodeScannedDetailed += OnBarcodeScannedReceived;
                UpdateScannerStatus();
            }

            // Setup background sync timer (5 minutes = 300,000 ms)
            SetupBackgroundSyncTimer();

            // Load initial view
            NavigateTo(NavigationModule.Intake);
        }

        #region Navigation Properties & Commands

        public NavigationModule CurrentModule
        {
            get => _currentModule;
            set
            {
                if (SetProperty(ref _currentModule, value))
                {
                    OnPropertyChanged(nameof(IsIntakeActive));
                    OnPropertyChanged(nameof(IsDaftarReturActive));
                    OnPropertyChanged(nameof(IsBarcodeRegistryActive));
                    OnPropertyChanged(nameof(IsPengaturanActive));
                }
            }
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public bool IsIntakeActive => CurrentModule == NavigationModule.Intake;
        public bool IsDaftarReturActive => CurrentModule == NavigationModule.DaftarRetur;
        public bool IsBarcodeRegistryActive => CurrentModule == NavigationModule.BarcodeRegistry;
        public bool IsPengaturanActive => CurrentModule == NavigationModule.Pengaturan;

        public ICommand NavigateToIntakeCommand { get; }
        public ICommand NavigateToDaftarReturCommand { get; }
        public ICommand NavigateToBarcodeRegistryCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateCommand { get; }

        public void NavigateTo(NavigationModule module)
        {
            CurrentModule = module;

            ViewModelBase targetViewModel = null;

            switch (module)
            {
                case NavigationModule.Intake:
                    targetViewModel = ResolveViewModel<ReturnOrderEntryViewModel>();
                    break;
                case NavigationModule.DaftarRetur:
                    targetViewModel = ResolveViewModel<ReturnOrderListViewModel>();
                    break;
                case NavigationModule.BarcodeRegistry:
                    targetViewModel = ResolveViewModel<BarcodeRegistryViewModel>();
                    break;
                case NavigationModule.Pengaturan:
                    targetViewModel = ResolveViewModel<SettingsViewModel>();
                    break;
            }

            CurrentViewModel = targetViewModel;

            // Route scanner listener to the newly active view model if it consumes scans
            if (_scannerListener != null)
            {
                _scannerListener.ActiveConsumer = CurrentViewModel as IScannerConsumer;
            }
        }

        private ViewModelBase ResolveViewModel<T>() where T : ViewModelBase, new()
        {
            if (_serviceProvider != null)
            {
                var resolved = _serviceProvider.GetService(typeof(T)) as ViewModelBase;
                if (resolved != null) return resolved;
            }

            return new T();
        }

        #endregion

        #region Header Properties & Commands

        public string DepoCode
        {
            get => _depoCode;
            set
            {
                if (SetProperty(ref _depoCode, value))
                {
                    OnPropertyChanged(nameof(DepoBadgeText));
                }
            }
        }

        public string DepoBadgeText => $"🏢 DEPO: {_depoCode}";

        public string OperatorName
        {
            get => _operatorName;
            set
            {
                if (SetProperty(ref _operatorName, value))
                {
                    OnPropertyChanged(nameof(OperatorBadgeText));
                }
            }
        }

        public string OperatorBadgeText => $"👤 {_operatorName}";

        public string OperatorEmail
        {
            get => _operatorEmail;
            set
            {
                if (SetProperty(ref _operatorEmail, value))
                {
                    OnPropertyChanged(nameof(OperatorEmailTooltip));
                }
            }
        }

        public string OperatorEmailTooltip =>
            string.IsNullOrWhiteSpace(_operatorEmail)
                ? "Google Email Operator belum dikonfigurasi."
                : $"Google Email Operator: {_operatorEmail.Trim()}";

        public bool IsOnline
        {
            get => _isOnline;
            set
            {
                if (SetProperty(ref _isOnline, value))
                {
                    OnlineStatusText = value ? "ONLINE" : "OFFLINE";
                }
            }
        }

        public string OnlineStatusText
        {
            get => _onlineStatusText;
            private set => SetProperty(ref _onlineStatusText, value);
        }

        public ICommand SwitchOperatorCommand { get; }
        public Action OnSwitchOperatorRequested { get; set; }

        public async Task UpdateOperatorAsync(string name, string email)
        {
            if (_registryHelper != null)
            {
                _registryHelper.LastOperatorName = name;
                _registryHelper.LastOperatorEmail = email;
            }

            OperatorName = name;
            OperatorEmail = email;
            StatusInfoText = $"Operator shift aktif diubah: {name}";

            await ValidateOperatorSessionAsync();
        }

        #endregion

        #region Status Bar Properties

        public string ScannerStatusText
        {
            get => _scannerStatusText;
            set => SetProperty(ref _scannerStatusText, value);
        }

        public string LatencyStatusText
        {
            get => _latencyStatusText;
            set => SetProperty(ref _latencyStatusText, value);
        }

        public string LastSyncStatusText
        {
            get => _lastSyncStatusText;
            set => SetProperty(ref _lastSyncStatusText, value);
        }

        public bool? LastSyncSuccess
        {
            get => _lastSyncSuccess;
            set => SetProperty(ref _lastSyncSuccess, value);
        }

        public string StatusInfoText
        {
            get => _statusInfoText;
            set => SetProperty(ref _statusInfoText, value);
        }

        private void UpdateScannerStatus()
        {
            if (_scannerListener != null)
            {
                ScannerStatusText = _scannerListener.IsEnabled
                    ? "⚡ Scanner: Aktif (WPF Wedge Hook)"
                    : "⚡ Scanner: Nonaktif";
            }
        }

        #endregion

        #region Global Hotkey Commands (F9, F10, Esc)

        public ICommand SaveDraftCommand { get; }
        public ICommand SaveAndUploadCommand { get; }
        public ICommand CancelOrClearCommand { get; }

        private void ExecuteSaveDraft()
        {
            if (CurrentViewModel is IOrderActionHandler handler)
            {
                handler.HandleSaveDraft();
            }
            else
            {
                StatusInfoText = "[F9] Simpan Draft Lokal dieksekusi.";
            }
        }

        private void ExecuteSaveAndUpload()
        {
            if (CurrentViewModel is IOrderActionHandler handler)
            {
                handler.HandleSaveAndUpload();
            }
            else
            {
                StatusInfoText = "[F10] Simpan & Upload dieksekusi.";
            }
        }

        private void ExecuteCancelOrClear()
        {
            if (CurrentViewModel is IOrderActionHandler handler)
            {
                handler.HandleCancelOrClear();
            }
            else
            {
                StatusInfoText = "[Esc] Form dibersihkan.";
            }
        }

        #endregion

        #region Synchronization & Timers

        public bool IsSyncRunning
        {
            get => _isSyncRunning;
            private set
            {
                if (SetProperty(ref _isSyncRunning, value))
                {
                    OnPropertyChanged(nameof(CanManualSync));
                    (ManualSyncCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool CanManualSync => !IsSyncRunning;

        public string ManualSyncButtonText
        {
            get => _manualSyncButtonText;
            private set => SetProperty(ref _manualSyncButtonText, value);
        }

        public string SyncAnimationText
        {
            get => _syncAnimationText;
            private set => SetProperty(ref _syncAnimationText, value);
        }

        public ICommand ManualSyncCommand { get; }

        public void SetupBackgroundSyncTimer(int intervalMs = 300000)
        {
            _bgSyncTimer?.Stop();
            _bgSyncTimer?.Dispose();

            // Periodic 5-minute background sync timer runs without blocking the UI thread
            _bgSyncTimer = new Timer(intervalMs)
            {
                AutoReset = true,
                Enabled = true
            };
            _bgSyncTimer.Elapsed += async (s, e) => await ExecuteBackgroundSyncPassAsync(isBackground: true);
            _bgSyncTimer.Start();

            // Short spinner timer for sync button text
            _syncAnimationTimer?.Stop();
            _syncAnimationTimer?.Dispose();
            _syncAnimationTimer = new Timer(300)
            {
                AutoReset = true
            };
            _syncAnimationTimer.Elapsed += (s, e) =>
            {
                _animationStep++;
                ManualSyncButtonText = SpinnerFrames[_animationStep % SpinnerFrames.Length];
            };
        }

        public async Task ExecuteBackgroundSyncPassAsync(bool isBackground)
        {
            if (IsSyncRunning || (_syncService != null && _syncService.IsSyncInProgress))
                return;

            // Pre-empt sync/upload when the operator identity is unresolved
            if (_sessionValidator != null && _sessionValidator.IsUnresolved)
            {
                StatusInfoText = $"Sinkronisasi dihentikan: {_sessionValidator.RefusalMessage}";
                WarnUnresolvedOperator(_sessionValidator.RefusalMessage);
                UpdateSyncStatusDisplay(false);
                return;
            }

            IsSyncRunning = true;
            StartSyncAnimation();

            StatusInfoText = isBackground
                ? "Menjalankan sinkronisasi background 5-menit..."
                : "Menjalankan sinkronisasi manual...";

            bool success = false;
            try
            {
                if (_syncService != null)
                {
                    var result = await _syncService.RunFullSyncPassAsync();
                    success = result != null && result.Success;
                }
                else
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
                success = false;
                StatusInfoText = $"Sinkronisasi terhambat: {ex.Message}";
            }
            finally
            {
                StopSyncAnimation();
                IsSyncRunning = false;
                UpdateSyncStatusDisplay(success);
            }
        }

        private void StartSyncAnimation()
        {
            _animationStep = 0;
            ManualSyncButtonText = SpinnerFrames[0];
            _syncAnimationTimer?.Start();
        }

        private void StopSyncAnimation()
        {
            _syncAnimationTimer?.Stop();
            ManualSyncButtonText = "🔄 Sync Sekarang";
            SyncAnimationText = "Sinkronisasi otomatis: Tiap 5 menit";
        }

        private void UpdateSyncStatusDisplay(bool success)
        {
            string timeStr = DateTime.Now.ToString("HH:mm:ss");

            if (success)
            {
                LastSyncStatusText = $"Terakhir Sync: {timeStr} (Sukses)";
                LastSyncSuccess = true;
                IsOnline = true;
            }
            else
            {
                LastSyncStatusText = $"Terakhir Sync: {timeStr} (Offline/Pending)";
                LastSyncSuccess = false;
                IsOnline = false;
            }
        }

        #endregion

        #region Operator Session Validation Gate

        public bool SuppressSessionWarnings { get; set; } = false;
        public Action<string> OnWarnUnresolvedOperator { get; set; }

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
                    StatusInfoText = $"Operator terverifikasi: {result.UserName} (Role: {result.RoleId})";
                }
                else if (_sessionValidator.IsUnresolved)
                {
                    StatusInfoText = $"Operator belum terverifikasi: {_sessionValidator.RefusalMessage}";
                    WarnUnresolvedOperator(_sessionValidator.RefusalMessage);
                }
                else if (result != null)
                {
                    StatusInfoText = $"Verifikasi operator belum berhasil: {_sessionValidator.RefusalMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusInfoText = $"Verifikasi operator terhambat: {ex.Message}";
            }
            finally
            {
                _sessionValidationInProgress = false;
            }
        }

        private void WarnUnresolvedOperator(string message)
        {
            if (SuppressSessionWarnings) return;
            OnWarnUnresolvedOperator?.Invoke(message);
        }

        #endregion

        #region Scanner Event Routing

        private void OnBarcodeScannedReceived(object sender, BarcodeScannedEventArgs e)
        {
            StatusInfoText = $"Scan Terakhir: [{e.NormalizedBarcode}] pada {DateTime.Now:HH:mm:ss}";

            if (CurrentViewModel is IScannerConsumer consumer)
            {
                consumer.OnBarcodeScanned(e.NormalizedBarcode);
            }
        }

        #endregion

        #region Test Hooks

        public Task TriggerBackgroundSyncPassForTestingAsync(bool isBackground = true) =>
            ExecuteBackgroundSyncPassAsync(isBackground);

        public Task TriggerManualSyncForTestingAsync() =>
            ExecuteBackgroundSyncPassAsync(isBackground: false);

        public Task ValidateOperatorSessionForTestingAsync() =>
            ValidateOperatorSessionAsync();

        public OperatorSessionValidator SessionValidator => _sessionValidator;
        public WpfScannerWedgeListener ScannerListener => _scannerListener;
        public ISyncService SyncService => _syncService;
        public IRegistryHelper RegistryHelper => _registryHelper;

        #endregion

        public void Dispose()
        {
            try
            {
                _bgSyncTimer?.Stop();
                _bgSyncTimer?.Dispose();

                _syncAnimationTimer?.Stop();
                _syncAnimationTimer?.Dispose();

                if (_scannerListener != null)
                {
                    _scannerListener.BarcodeScannedDetailed -= OnBarcodeScannedReceived;
                }
            }
            catch
            {
                // Suppress on dispose
            }
        }
    }
}
