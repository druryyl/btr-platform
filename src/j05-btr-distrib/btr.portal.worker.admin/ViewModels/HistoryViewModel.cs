using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using btr.portal.worker.admin.Models;
using btr.portal.worker.admin.Services;

namespace btr.portal.worker.admin.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        private readonly PortalApiService _apiService;
        private bool _isLoading;
        private string _errorMessage;
        private string _selectedFilterDomain = "All";
        private string _selectedFilterStatus = "All";
        private string _selectedErrorDetail;

        public HistoryViewModel(PortalApiService apiService)
        {
            _apiService = apiService;
            RefreshLogs = new ObservableCollection<RefreshLogEntry>();
            FilterDomains = new System.Collections.Generic.List<string>
            {
                "All", "Piutang", "Inventory", "InventoryRisk", "Sales",
                "Purchasing", "PurchasingManagement", "Customer", "Salesman",
                "Collection", "FieldActivity", "Location"
            };
            FilterStatuses = new System.Collections.Generic.List<string>
            {
                "All", "Success", "Failed", "InProgress"
            };

            LoadLogsCommand = new RelayCommand(async () => await LoadLogsAsync());
            CopyErrorCommand = new RelayCommand(() =>
            {
                if (!string.IsNullOrWhiteSpace(_selectedErrorDetail))
                {
                    Clipboard.SetText(_selectedErrorDetail);
                }
            });
        }

        public ObservableCollection<RefreshLogEntry> RefreshLogs { get; }
        public System.Collections.Generic.List<string> FilterDomains { get; }
        public System.Collections.Generic.List<string> FilterStatuses { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string SelectedFilterDomain
        {
            get => _selectedFilterDomain;
            set => SetProperty(ref _selectedFilterDomain, value);
        }

        public string SelectedFilterStatus
        {
            get => _selectedFilterStatus;
            set => SetProperty(ref _selectedFilterStatus, value);
        }

        public string SelectedErrorDetail
        {
            get => _selectedErrorDetail;
            set => SetProperty(ref _selectedErrorDetail, value);
        }

        public ICommand LoadLogsCommand { get; }
        public ICommand CopyErrorCommand { get; }

        public async Task LoadLogsAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var data = await _apiService.GetDashboardSnapshotHealthAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    RefreshLogs.Clear();
                    if (data?.Domains != null)
                    {
                        foreach (var domain in data.Domains)
                        {
                            if (domain.LastRefresh != null)
                            {
                                var entry = new RefreshLogEntry
                                {
                                    Domain = domain.Domain,
                                    Status = domain.LastRefresh.Status,
                                    StartedAt = domain.LastRefresh.StartedAt,
                                    CompletedAt = domain.LastRefresh.CompletedAt,
                                    DurationMs = domain.LastRefresh.DurationMs,
                                    TriggeredBy = domain.LastRefresh.TriggeredBy,
                                    ErrorMessage = domain.LastRefresh.ErrorMessage
                                };

                                if (MatchesFilter(entry))
                                {
                                    RefreshLogs.Add(entry);
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load refresh logs: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool MatchesFilter(RefreshLogEntry entry)
        {
            if (!string.Equals(SelectedFilterDomain, "All", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entry.Domain, SelectedFilterDomain, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.Equals(SelectedFilterStatus, "All", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entry.Status, SelectedFilterStatus, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }
    }
}
