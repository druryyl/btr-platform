using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using btr.portal.worker.admin.Models;
using btr.portal.worker.admin.Services;

namespace btr.portal.worker.admin.ViewModels
{
    public class RefreshViewModel : ViewModelBase
    {
        private readonly PortalApiService _apiService;
        private readonly WorkerProcessService _workerProcessService;

        private string _selectedDomain = "All";
        private bool _isRefreshing;
        private string _statusMessage;
        private bool _useLocalWorker;
        private string _workerExePath;
        private string _consoleOutput;
        private bool _isWorkerRunning;

        private string _backfillEntityType = "All";
        private string _backfillLayers = "L1,L2,L5";
        private bool _backfillResume = true;
        private bool _backfillRestart;
        private bool _backfillForce;
        private bool _backfillDryRun;
        private bool _backfillContinueOnError;
        private int _backfillBatchSize = 500;
        private string _backfillConfirmToken;
        private bool _backfillSkipMutex;
        private int? _backfillFromYear;
        private int? _backfillFromMonth;
        private int? _backfillToYear;
        private int? _backfillToMonth;

        public RefreshViewModel(PortalApiService apiService, WorkerProcessService workerProcessService)
        {
            _apiService = apiService;
            _workerProcessService = workerProcessService;
            _workerProcessService.OutputReceived += OnWorkerOutputReceived;
            _workerProcessService.Exited += OnWorkerExited;

            AvailableDomains = new List<string>
            {
                "All", "Piutang", "Inventory", "InventoryRisk", "Sales",
                "Purchasing", "PurchasingManagement", "Customer", "Salesman",
                "Collection", "FieldActivity", "Location",
                "PrincipalSalesOut", "PrincipalReturn", "PrincipalInventory",
                "PrincipalTarget", "PrincipalAchievement", "PrincipalSalesmanContribution",
                "PrincipalActiveCustomer", "PrincipalCustomerCoverage",
                "CustomerPrincipalRelationship", "PrincipalSalesOutHistory",
                "PrincipalReturnHistory", "PrincipalReturnPercentage",
                "PrincipalPurchaseIn", "EntityAnalyticsHistoricalBackfill"
            };

            EntityTypes = new List<string> { "All", "Customer", "Salesman", "Supplier", "Item" };

            RefreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !IsRefreshing);
            CancelWorkerCommand = new RelayCommand(() => _workerProcessService.CancelWorker(), () => IsWorkerRunning);
            ClearOutputCommand = new RelayCommand(() => ConsoleOutput = string.Empty);
        }

        public List<string> AvailableDomains { get; }
        public List<string> EntityTypes { get; }

        public string SelectedDomain
        {
            get => _selectedDomain;
            set => SetProperty(ref _selectedDomain, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (SetProperty(ref _isRefreshing, value))
                {
                    ((RelayCommand)RefreshCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool UseLocalWorker
        {
            get => _useLocalWorker;
            set => SetProperty(ref _useLocalWorker, value);
        }

        public string WorkerExePath
        {
            get => _workerExePath;
            set => SetProperty(ref _workerExePath, value);
        }

        public string ConsoleOutput
        {
            get => _consoleOutput;
            set => SetProperty(ref _consoleOutput, value);
        }

        public bool IsWorkerRunning
        {
            get => _isWorkerRunning;
            set
            {
                if (SetProperty(ref _isWorkerRunning, value))
                {
                    ((RelayCommand)CancelWorkerCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string BackfillEntityType
        {
            get => _backfillEntityType;
            set => SetProperty(ref _backfillEntityType, value);
        }

        public string BackfillLayers
        {
            get => _backfillLayers;
            set => SetProperty(ref _backfillLayers, value);
        }

        public bool BackfillResume
        {
            get => _backfillResume;
            set => SetProperty(ref _backfillResume, value);
        }

        public bool BackfillRestart
        {
            get => _backfillRestart;
            set => SetProperty(ref _backfillRestart, value);
        }

        public bool BackfillForce
        {
            get => _backfillForce;
            set => SetProperty(ref _backfillForce, value);
        }

        public bool BackfillDryRun
        {
            get => _backfillDryRun;
            set => SetProperty(ref _backfillDryRun, value);
        }

        public bool BackfillContinueOnError
        {
            get => _backfillContinueOnError;
            set => SetProperty(ref _backfillContinueOnError, value);
        }

        public int BackfillBatchSize
        {
            get => _backfillBatchSize;
            set => SetProperty(ref _backfillBatchSize, value);
        }

        public string BackfillConfirmToken
        {
            get => _backfillConfirmToken;
            set => SetProperty(ref _backfillConfirmToken, value);
        }

        public bool BackfillSkipMutex
        {
            get => _backfillSkipMutex;
            set => SetProperty(ref _backfillSkipMutex, value);
        }

        public int? BackfillFromYear
        {
            get => _backfillFromYear;
            set => SetProperty(ref _backfillFromYear, value);
        }

        public int? BackfillFromMonth
        {
            get => _backfillFromMonth;
            set => SetProperty(ref _backfillFromMonth, value);
        }

        public int? BackfillToYear
        {
            get => _backfillToYear;
            set => SetProperty(ref _backfillToYear, value);
        }

        public int? BackfillToMonth
        {
            get => _backfillToMonth;
            set => SetProperty(ref _backfillToMonth, value);
        }

        public bool IsBackfillDomain =>
            string.Equals(SelectedDomain, "EntityAnalyticsHistoricalBackfill", StringComparison.OrdinalIgnoreCase);

        public ICommand RefreshCommand { get; }
        public ICommand CancelWorkerCommand { get; }
        public ICommand ClearOutputCommand { get; }

        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            StatusMessage = string.Empty;

            try
            {
                if (UseLocalWorker)
                {
                    await RefreshViaLocalWorker();
                }
                else
                {
                    await RefreshViaApi();
                }
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task RefreshViaApi()
        {
            try
            {
                StatusMessage = $"Triggering refresh for '{SelectedDomain}' via API...";
                var result = await _apiService.TriggerRefreshAsync(SelectedDomain);
                StatusMessage = $"Refresh triggered successfully. Overall status: {result?.Status ?? "unknown"}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"API refresh failed: {ex.Message}";
            }
        }

        private Task RefreshViaLocalWorker()
        {
            if (string.IsNullOrWhiteSpace(WorkerExePath))
            {
                StatusMessage = "Worker executable path is not configured.";
                return Task.CompletedTask;
            }

            var args = BuildWorkerArguments();
            StatusMessage = $"Starting worker: {System.IO.Path.GetFileName(WorkerExePath)} {args}";
            ConsoleOutput = string.Empty;

            try
            {
                _workerProcessService.StartWorker(WorkerExePath, args);
                IsWorkerRunning = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to start worker: {ex.Message}";
            }

            return Task.CompletedTask;
        }

        private string BuildWorkerArguments()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"--domain {SelectedDomain}");
            sb.Append(" --triggered-by Manual");

            if (IsBackfillDomain)
            {
                sb.Append($" --entity-type {BackfillEntityType}");
                sb.Append($" --layers {BackfillLayers}");

                if (!BackfillResume) sb.Append(" --resume false");
                if (BackfillRestart) sb.Append(" --restart");
                if (BackfillForce) sb.Append(" --force");
                if (BackfillDryRun) sb.Append(" --dry-run");
                if (BackfillContinueOnError) sb.Append(" --continue-on-error");
                if (BackfillBatchSize != 500) sb.Append($" --batch-size {BackfillBatchSize}");
                if (!string.IsNullOrWhiteSpace(BackfillConfirmToken)) sb.Append($" --confirm {BackfillConfirmToken}");
                if (BackfillSkipMutex) sb.Append(" --skip-live-mutex-check");
                if (BackfillFromYear.HasValue && BackfillFromMonth.HasValue)
                    sb.Append($" --from-period {BackfillFromYear.Value}-{BackfillFromMonth.Value:D2}");
                if (BackfillToYear.HasValue && BackfillToMonth.HasValue)
                    sb.Append($" --to-period {BackfillToYear.Value}-{BackfillToMonth.Value:D2}");
            }

            return sb.ToString();
        }

        private void OnWorkerOutputReceived(object sender, string output)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ConsoleOutput += output + Environment.NewLine;
            });
        }

        private void OnWorkerExited(object sender, int exitCode)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsWorkerRunning = false;
                IsRefreshing = false;
                StatusMessage = $"Worker exited with code {exitCode}. {(exitCode == 0 ? "Success." : "Check output for errors.")}";
            });
        }

        public void Dispose()
        {
            _workerProcessService.OutputReceived -= OnWorkerOutputReceived;
            _workerProcessService.Exited -= OnWorkerExited;
            _workerProcessService.Dispose();
        }
    }
}
