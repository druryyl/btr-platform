using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using btr.portal.worker.admin.Models;
using btr.portal.worker.admin.Services;

namespace btr.portal.worker.admin.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        private readonly PortalApiService _apiService;

        private string _apiBaseUrl;
        private string _workerExePath;
        private int _healthPollIntervalSeconds = 30;
        private int _apiTimeoutSeconds = 30;
        private string _connectionStatus = "Not tested";
        private bool _isTestingConnection;
        private Models.ConfigurationSettings _configSettings;

        public ConfigurationViewModel(PortalApiService apiService)
        {
            _apiService = apiService;

            _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:8080";
            _workerExePath = ConfigurationManager.AppSettings["WorkerExePath"] ?? "";
            _healthPollIntervalSeconds = int.TryParse(ConfigurationManager.AppSettings["HealthPollIntervalSeconds"], out var poll) ? poll : 30;
            _apiTimeoutSeconds = int.TryParse(ConfigurationManager.AppSettings["ApiTimeoutSeconds"], out var timeout) ? timeout : 30;

            TestConnectionCommand = new RelayCommand(async () => await TestConnectionAsync(), () => !IsTestingConnection);
            SaveSettingsCommand = new RelayCommand(SaveSettings);
            LoadSettingsCommand = new RelayCommand(LoadSettings);
        }

        public string ApiBaseUrl
        {
            get => _apiBaseUrl;
            set => SetProperty(ref _apiBaseUrl, value);
        }

        public string WorkerExePath
        {
            get => _workerExePath;
            set => SetProperty(ref _workerExePath, value);
        }

        public int HealthPollIntervalSeconds
        {
            get => _healthPollIntervalSeconds;
            set => SetProperty(ref _healthPollIntervalSeconds, value);
        }

        public int ApiTimeoutSeconds
        {
            get => _apiTimeoutSeconds;
            set => SetProperty(ref _apiTimeoutSeconds, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        public bool IsTestingConnection
        {
            get => _isTestingConnection;
            set
            {
                if (SetProperty(ref _isTestingConnection, value))
                {
                    ((RelayCommand)TestConnectionCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public Models.ConfigurationSettings ConfigSettings
        {
            get => _configSettings;
            set => SetProperty(ref _configSettings, value);
        }

        public ICommand TestConnectionCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand LoadSettingsCommand { get; }

        private async Task TestConnectionAsync()
        {
            IsTestingConnection = true;
            ConnectionStatus = "Testing...";

            try
            {
                _apiService.UpdateBaseUrl(ApiBaseUrl);
                var data = await _apiService.GetDashboardSnapshotHealthAsync();

                if (data != null)
                {
                    ConnectionStatus = $"Connected. Overall status: {data.Status}";
                    ConfigSettings = new Models.ConfigurationSettings
                    {
                        PiutangIntervalMinutes = data.Domains != null && data.Domains.Count > 0
                            ? data.Domains[0].IntervalMinutes : 0
                    };

                    if (data.Domains != null)
                    {
                        foreach (var domain in data.Domains)
                        {
                            switch (domain.Domain)
                            {
                                case "Piutang": ConfigSettings.PiutangIntervalMinutes = domain.IntervalMinutes; break;
                                case "Inventory": ConfigSettings.InventoryIntervalMinutes = domain.IntervalMinutes; break;
                                case "InventoryRisk": ConfigSettings.InventoryRiskIntervalMinutes = domain.IntervalMinutes; break;
                                case "Sales": ConfigSettings.SalesIntervalMinutes = domain.IntervalMinutes; break;
                                case "Purchasing": ConfigSettings.PurchasingIntervalMinutes = domain.IntervalMinutes; break;
                                case "PurchasingManagement": ConfigSettings.PurchasingManagementIntervalMinutes = domain.IntervalMinutes; break;
                                case "Customer": ConfigSettings.CustomerIntervalMinutes = domain.IntervalMinutes; break;
                                case "Salesman": ConfigSettings.SalesmanIntervalMinutes = domain.IntervalMinutes; break;
                                case "Collection": ConfigSettings.CollectionIntervalMinutes = domain.IntervalMinutes; break;
                                case "FieldActivity": ConfigSettings.FieldActivityIntervalMinutes = domain.IntervalMinutes; break;
                                case "Location": ConfigSettings.LocationIntervalMinutes = domain.IntervalMinutes; break;
                            }
                        }
                    }

                    OnPropertyChanged(nameof(ConfigSettings));
                }
                else
                {
                    ConnectionStatus = "Connected but received empty response.";
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                    message += $" | Inner: {ex.InnerException.Message}";
                ConnectionStatus = $"Connection failed: {message}";
            }
            finally
            {
                IsTestingConnection = false;
            }
        }

        private void SaveSettings()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["ApiBaseUrl"].Value = ApiBaseUrl;
                config.AppSettings.Settings["WorkerExePath"].Value = WorkerExePath;
                config.AppSettings.Settings["HealthPollIntervalSeconds"].Value = HealthPollIntervalSeconds.ToString();
                config.AppSettings.Settings["ApiTimeoutSeconds"].Value = ApiTimeoutSeconds.ToString();
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSettings()
        {
            ApiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:8080";
            WorkerExePath = ConfigurationManager.AppSettings["WorkerExePath"] ?? "";
            HealthPollIntervalSeconds = int.TryParse(ConfigurationManager.AppSettings["HealthPollIntervalSeconds"], out var poll) ? poll : 30;
            ApiTimeoutSeconds = int.TryParse(ConfigurationManager.AppSettings["ApiTimeoutSeconds"], out var timeout) ? timeout : 30;
        }
    }
}
