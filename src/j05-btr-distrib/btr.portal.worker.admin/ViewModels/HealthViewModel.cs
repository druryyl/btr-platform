using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using btr.portal.worker.admin.Models;
using btr.portal.worker.admin.Services;

namespace btr.portal.worker.admin.ViewModels
{
    public class HealthViewModel : ViewModelBase
    {
        private readonly PortalApiService _apiService;
        private HealthPollingService _pollingService;

        private DashboardSnapshotHealthData _healthData;
        private bool _isLoading;
        private string _errorMessage;
        private bool _autoRefreshEnabled;
        private DateTime _lastChecked;

        public HealthViewModel(PortalApiService apiService)
        {
            _apiService = apiService;
            Domains = new ObservableCollection<DashboardSnapshotDomainHealth>();
            BindingOperations.EnableCollectionSynchronization(Domains, new object());

            RefreshCommand = new RelayCommand(async () => await LoadHealthAsync());
            ToggleAutoRefreshCommand = new RelayCommand(ToggleAutoRefresh);
        }

        public ObservableCollection<DashboardSnapshotDomainHealth> Domains { get; }

        public DashboardSnapshotHealthData HealthData
        {
            get => _healthData;
            set => SetProperty(ref _healthData, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    ((RelayCommand)RefreshCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool AutoRefreshEnabled
        {
            get => _autoRefreshEnabled;
            set
            {
                if (SetProperty(ref _autoRefreshEnabled, value))
                {
                    if (value)
                        StartAutoRefresh();
                    else
                        StopAutoRefresh();
                }
            }
        }

        public DateTime LastChecked
        {
            get => _lastChecked;
            set => SetProperty(ref _lastChecked, value);
        }

        public string OverallStatus => HealthData?.Status?.ToUpperInvariant() ?? "UNKNOWN";

        public Brush OverallStatusBrush
        {
            get
            {
                switch (HealthData?.Status?.ToLowerInvariant())
                {
                    case "ok": return new SolidColorBrush(Colors.Green);
                    case "degraded": return new SolidColorBrush(Colors.Orange);
                    case "refreshing": return new SolidColorBrush(Colors.DodgerBlue);
                    default: return new SolidColorBrush(Colors.Red);
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ToggleAutoRefreshCommand { get; }

        public async Task LoadHealthAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var data = await _apiService.GetDashboardSnapshotHealthAsync();
                HealthData = data;
                LastChecked = DateTime.Now;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Domains.Clear();
                    if (data?.Domains != null)
                    {
                        foreach (var domain in data.Domains)
                        {
                            Domains.Add(domain);
                        }
                    }
                });

                OnPropertyChanged(nameof(OverallStatus));
                OnPropertyChanged(nameof(OverallStatusBrush));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load health data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ToggleAutoRefresh()
        {
            AutoRefreshEnabled = !AutoRefreshEnabled;
        }

        private void StartAutoRefresh()
        {
            _pollingService?.Dispose();
            _pollingService = new HealthPollingService(
                async () => await LoadHealthAsync(),
                TimeSpan.FromSeconds(30));
            _pollingService.PollFailed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"Auto-refresh failed: {e}";
                });
            };
            _pollingService.Start();
        }

        private void StopAutoRefresh()
        {
            _pollingService?.Stop();
            _pollingService?.Dispose();
            _pollingService = null;
        }

        public void Dispose()
        {
            _pollingService?.Dispose();
        }
    }
}
