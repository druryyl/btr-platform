using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.Infrastructure.Helpers;
using BtrReturGudang.Wpf.Mvvm;
using BtrReturGudang.Wpf.Services;

namespace BtrReturGudang.Wpf.ViewModels
{
    /// <summary>
    /// ViewModel for Workstation Settings &amp; Hardware Configuration (SCR-07).
    /// Manages Depo/Warehouse code assignment, PC machine identification, shift operator switcher,
    /// local SQL Server connection tester, cloud API relay ping test, and audio feedback test/toggle.
    /// Persists workstation parameters via IRegistryHelper.
    /// Traceable to: ARCHITECTURE §5.2 SCR-07, TD-009, TD-011, TD-012, TD-013, P4-S10.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IRegistryHelper _registryHelper;
        private readonly IAudioHelper _audioHelper;
        private readonly IBtradeApiClient _apiClient;
        private readonly OperatorSessionValidator _sessionValidator;

        private string _warehouseCode = "GDG-PST";
        private string _serverTargetId = RegistryHelper.DefaultServerTargetId;
        private string _pcName = Environment.MachineName;
        private string _apiBaseUrl = RegistryHelper.DefaultApiBaseUrl;
        private string _operatorName = "Operator-1";
        private string _operatorEmail = string.Empty;
        private string _connectionString = string.Empty;
        private bool _enableSoundFeedback = true;

        private string _operatorSessionStatus = "Status operator: Belum diverifikasi.";
        private string _operatorStatusColor = "#64748B"; // Slate-500
        private string _dbStatusMessage = string.Empty;
        private string _dbStatusColor = "#64748B";
        private string _apiStatusMessage = string.Empty;
        private string _apiStatusColor = "#64748B";
        private string _statusMessage = string.Empty;

        private bool _isTestingDb;
        private bool _isTestingApi;
        private bool _isVerifyingOperator;

        /// <summary>
        /// Delegate for invoking the OperatorSwitcherDialog in UI or headless unit tests.
        /// Returns (name, email) if saved, or null if cancelled.
        /// </summary>
        public Func<Tuple<string, string>> SwitchOperatorDialogFunc { get; set; }

        public SettingsViewModel()
            : this(null, null, null, null)
        {
        }

        public SettingsViewModel(
            IRegistryHelper registryHelper = null,
            IAudioHelper audioHelper = null,
            IBtradeApiClient apiClient = null,
            OperatorSessionValidator sessionValidator = null)
        {
            _registryHelper = registryHelper;
            _audioHelper = audioHelper;
            _apiClient = apiClient;
            _sessionValidator = sessionValidator;

            LoadConfiguration();

            SwitchOperatorCommand = new RelayCommand(async () => await SwitchOperatorAsync());
            VerifyOperatorCommand = new RelayCommand(async () => await VerifyOperatorAsync(), () => !IsVerifyingOperator);
            TestDbCommand = new RelayCommand(async () => await TestDatabaseConnectionAsync(), () => !IsTestingDb);
            TestApiCommand = new RelayCommand(async () => await TestApiConnectionAsync(), () => !IsTestingApi);
            TestSuccessBeepCommand = new RelayCommand(() => _audioHelper?.PlaySuccess());
            TestWarningBeepCommand = new RelayCommand(() => _audioHelper?.PlayWarning());
            SaveAllCommand = new RelayCommand(SaveAllConfiguration);
            ResetDbCommand = new RelayCommand(() =>
            {
                ConnectionString = ConnStringHelper.DefaultConnectionString;
                DbStatusMessage = "Connection string direset ke default.";
                DbStatusColor = "#64748B";
            });
        }

        #region Properties

        public string WarehouseCode
        {
            get => _warehouseCode;
            set => SetProperty(ref _warehouseCode, value);
        }

        public string ServerTargetId
        {
            get => _serverTargetId;
            set => SetProperty(ref _serverTargetId, value);
        }

        public string PcName
        {
            get => _pcName;
            set => SetProperty(ref _pcName, value);
        }

        public string ApiBaseUrl
        {
            get => _apiBaseUrl;
            set => SetProperty(ref _apiBaseUrl, value);
        }

        public string OperatorName
        {
            get => _operatorName;
            set => SetProperty(ref _operatorName, value);
        }

        public string OperatorEmail
        {
            get => _operatorEmail;
            set => SetProperty(ref _operatorEmail, value);
        }

        public string ConnectionString
        {
            get => _connectionString;
            set => SetProperty(ref _connectionString, value);
        }

        public bool EnableSoundFeedback
        {
            get => _enableSoundFeedback;
            set
            {
                if (SetProperty(ref _enableSoundFeedback, value))
                {
                    if (_registryHelper != null) _registryHelper.EnableSoundFeedback = value;
                    if (_audioHelper != null) _audioHelper.EnableSoundFeedback = value;
                }
            }
        }

        public string OperatorSessionStatus
        {
            get => _operatorSessionStatus;
            private set => SetProperty(ref _operatorSessionStatus, value);
        }

        public string OperatorStatusColor
        {
            get => _operatorStatusColor;
            private set => SetProperty(ref _operatorStatusColor, value);
        }

        public string DbStatusMessage
        {
            get => _dbStatusMessage;
            private set => SetProperty(ref _dbStatusMessage, value);
        }

        public string DbStatusColor
        {
            get => _dbStatusColor;
            private set => SetProperty(ref _dbStatusColor, value);
        }

        public string ApiStatusMessage
        {
            get => _apiStatusMessage;
            private set => SetProperty(ref _apiStatusMessage, value);
        }

        public string ApiStatusColor
        {
            get => _apiStatusColor;
            private set => SetProperty(ref _apiStatusColor, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool IsTestingDb
        {
            get => _isTestingDb;
            private set
            {
                if (SetProperty(ref _isTestingDb, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool IsTestingApi
        {
            get => _isTestingApi;
            private set
            {
                if (SetProperty(ref _isTestingApi, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool IsVerifyingOperator
        {
            get => _isVerifyingOperator;
            private set
            {
                if (SetProperty(ref _isVerifyingOperator, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand SwitchOperatorCommand { get; }
        public ICommand VerifyOperatorCommand { get; }
        public ICommand TestDbCommand { get; }
        public ICommand ResetDbCommand { get; }
        public ICommand TestApiCommand { get; }
        public ICommand TestSuccessBeepCommand { get; }
        public ICommand TestWarningBeepCommand { get; }
        public ICommand SaveAllCommand { get; }

        #endregion

        #region Methods

        public void LoadConfiguration()
        {
            if (_registryHelper != null)
            {
                WarehouseCode = _registryHelper.WarehouseCode ?? "GDG-PST";
                ServerTargetId = !string.IsNullOrWhiteSpace(_registryHelper.ServerTargetId)
                    ? _registryHelper.ServerTargetId
                    : RegistryHelper.DefaultServerTargetId;
                ApiBaseUrl = RegistryHelper.NormalizeApiUrl(_registryHelper.ApiBaseUrl ?? RegistryHelper.DefaultApiBaseUrl);
                OperatorName = !string.IsNullOrWhiteSpace(_registryHelper.LastOperatorName)
                    ? _registryHelper.LastOperatorName
                    : "Operator-1";
                OperatorEmail = _registryHelper.LastOperatorEmail ?? string.Empty;
                EnableSoundFeedback = _registryHelper.EnableSoundFeedback;
            }
            else
            {
                WarehouseCode = "GDG-PST";
                ServerTargetId = RegistryHelper.DefaultServerTargetId;
                ApiBaseUrl = RegistryHelper.NormalizeApiUrl(RegistryHelper.DefaultApiBaseUrl);
                OperatorName = "Operator-1";
                OperatorEmail = string.Empty;
                EnableSoundFeedback = true;
            }

            ConnectionString = ConnStringHelper.GetConnectionString();
            OperatorSessionStatus = "Status operator: Belum diverifikasi.";
            OperatorStatusColor = "#64748B";
        }

        public async Task SwitchOperatorAsync()
        {
            if (SwitchOperatorDialogFunc != null)
            {
                var result = SwitchOperatorDialogFunc();
                if (result != null)
                {
                    OperatorName = result.Item1;
                    OperatorEmail = result.Item2;
                    await VerifyOperatorAsync();
                }
            }
        }

        public async Task VerifyOperatorAsync()
        {
            if (_sessionValidator == null && _apiClient == null)
            {
                OperatorSessionStatus = "Status operator: Verifikasi tidak tersedia (Cloud API client belum terhubung).";
                OperatorStatusColor = "#64748B";
                return;
            }

            string email = OperatorEmail;
            if (string.IsNullOrWhiteSpace(email))
            {
                email = _registryHelper?.LastOperatorEmail ?? string.Empty;
            }

            IsVerifyingOperator = true;
            OperatorSessionStatus = "Status operator: Memverifikasi...";
            OperatorStatusColor = "#64748B";

            try
            {
                SessionResolveResultDto result;
                if (_sessionValidator != null)
                {
                    result = await _sessionValidator.ValidateAsync();
                }
                else
                {
                    result = string.IsNullOrWhiteSpace(email)
                        ? null
                        : await _apiClient.ResolveSessionAsync(email);
                }

                if (result != null && result.Success)
                {
                    _audioHelper?.PlaySuccess();
                    OperatorStatusColor = "#047857"; // Emerald-700
                    OperatorSessionStatus = $"✅ Operator terverifikasi: {result.UserName} (Role: {result.RoleId})";
                }
                else if (result == null)
                {
                    OperatorStatusColor = "#64748B";
                    OperatorSessionStatus = "Status operator: Verifikasi tidak tersedia (Cloud API client belum terhubung).";
                }
                else
                {
                    _audioHelper?.PlayWarning();
                    OperatorStatusColor = "#B91C1C"; // Crimson-700
                    OperatorSessionStatus = OperatorSessionValidator.ExtractRefusalMessage(result);
                }
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                OperatorStatusColor = "#B91C1C";
                OperatorSessionStatus = ex.Message;
            }
            finally
            {
                IsVerifyingOperator = false;
            }
        }

        public async Task TestDatabaseConnectionAsync()
        {
            string connStr = ConnectionString?.Trim();
            if (string.IsNullOrWhiteSpace(connStr))
            {
                DbStatusMessage = "❌ Connection string tidak boleh kosong.";
                DbStatusColor = "#B91C1C";
                return;
            }

            IsTestingDb = true;
            DbStatusMessage = "Menguji koneksi database...";
            DbStatusColor = "#64748B";

            try
            {
                bool success = await Task.Run(() =>
                {
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT 1";
                            cmd.ExecuteScalar();
                        }
                    }
                    return true;
                });

                if (success)
                {
                    DbStatusMessage = "✅ Koneksi database lokal BERHASIL! (btrgd terhubung)";
                    DbStatusColor = "#047857";
                    _audioHelper?.PlaySuccess();
                }
            }
            catch (Exception ex)
            {
                DbStatusMessage = $"❌ Gagal: {ex.Message}";
                DbStatusColor = "#B91C1C";
                _audioHelper?.PlayWarning();
            }
            finally
            {
                IsTestingDb = false;
            }
        }

        public async Task TestApiConnectionAsync()
        {
            string url = RegistryHelper.NormalizeApiUrl(ApiBaseUrl);
            if (string.IsNullOrWhiteSpace(url))
            {
                ApiStatusMessage = "❌ Base URL relay tidak boleh kosong.";
                ApiStatusColor = "#B91C1C";
                return;
            }

            ApiBaseUrl = url;
            IsTestingApi = true;
            ApiStatusMessage = "Menguji route HTTP API...";
            ApiStatusColor = "#64748B";

            try
            {
                bool connected = await Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        http.Timeout = TimeSpan.FromSeconds(5);
                        var response = await http.GetAsync(url).ConfigureAwait(false);
                        return response.IsSuccessStatusCode || ((int)response.StatusCode < 500);
                    }
                });

                if (connected)
                {
                    ApiStatusMessage = "✅ Cloud API relay terhubung!";
                    ApiStatusColor = "#047857";
                    _audioHelper?.PlaySuccess();
                }
                else
                {
                    ApiStatusMessage = "⚠️ Respons relay non-OK dari server.";
                    ApiStatusColor = "#B45309";
                    _audioHelper?.PlayWarning();
                }
            }
            catch (Exception ex)
            {
                ApiStatusMessage = $"❌ Gagal koneksi relay: {ex.Message}";
                ApiStatusColor = "#B91C1C";
                _audioHelper?.PlayWarning();
            }
            finally
            {
                IsTestingApi = false;
            }
        }

        public void SaveAllConfiguration()
        {
            try
            {
                string wh = WarehouseCode?.Trim() ?? string.Empty;
                string serverId = ServerTargetId?.Trim() ?? string.Empty;
                string apiUrl = RegistryHelper.NormalizeApiUrl(ApiBaseUrl);
                string connStr = ConnectionString?.Trim() ?? string.Empty;
                bool audio = EnableSoundFeedback;

                if (_registryHelper != null)
                {
                    _registryHelper.WarehouseCode = wh;
                    _registryHelper.ServerTargetId = serverId;
                    _registryHelper.ApiBaseUrl = apiUrl;
                    _registryHelper.EnableSoundFeedback = audio;
                    _registryHelper.LastOperatorName = OperatorName?.Trim() ?? string.Empty;
                    _registryHelper.LastOperatorEmail = OperatorEmail?.Trim() ?? string.Empty;

                    ApiBaseUrl = _registryHelper.ApiBaseUrl;
                }
                else
                {
                    ApiBaseUrl = apiUrl;
                }

                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    ConnStringHelper.SetConnectionString(connStr);
                }

                _audioHelper?.PlaySuccess();
                StatusMessage = "Semua konfigurasi workstation berhasil disimpan.";
            }
            catch (Exception ex)
            {
                _audioHelper?.PlayWarning();
                StatusMessage = $"Gagal menyimpan konfigurasi: {ex.Message}";
            }
        }

        #endregion
    }
}
