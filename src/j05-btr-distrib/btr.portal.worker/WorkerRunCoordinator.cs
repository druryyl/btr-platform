using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.UseCases;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.UseCases;
using btr.infrastructure.Helpers;
using btr.portal.worker.Progress;
using WorkerStepIds = btr.application.ReportingContext.DashboardSnapshotAgg.Progress.WorkerProgressStepIds;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace btr.portal.worker
{
    internal sealed class WorkerRunCoordinator
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly Stopwatch _totalStopwatch = new Stopwatch();
        private readonly Dictionary<string, TimeSpan> _stepDurations = new Dictionary<string, TimeSpan>(StringComparer.Ordinal);
        private ConsoleWorkerProgressReporter _reporter;

        public int Run(string[] args)
        {
            LogManager.Setup().LoadConfigurationFromFile("NLog.config");
            _totalStopwatch.Start();

            var domain = ParseRequiredOption(args, "--domain", "All");
            var triggeredBy = ParseRequiredOption(args, "--triggered-by", "Scheduler");
            var failed = false;

            try
            {
                ValidateOptions(domain, triggeredBy);

                Logger.Info(
                    "Starting dashboard snapshot refresh. Domain={Domain}, TriggeredBy={TriggeredBy}",
                    domain,
                    triggeredBy);

                _reporter = new ConsoleWorkerProgressReporter();
                var plan = BuildExecutionPlan(domain);
                _reporter.BeginPlan(plan.StepIds, plan.DisplayNames);
                _reporter.RenderStartupHeader("BTR Dashboard Snapshot Worker", domain, triggeredBy);

                using (WorkerProgressScope.Push(_reporter))
                {
                    var configuration = ExecuteLoadConfiguration();
                    var serviceProvider = WorkerDependencyConfig.Configure(configuration);
                    ConnStringHelper.Initialize(serviceProvider.GetRequiredService<ConnectionStringFactory>());
                    ExecuteValidateDatabase(serviceProvider);

                    using (var scope = serviceProvider.CreateScope())
                    {
                        ExecuteRefresh(scope.ServiceProvider, domain, triggeredBy, args);
                    }

                    ExecuteGenerateSummary();
                }

                Logger.Info("Dashboard snapshot refresh completed successfully. Domain={Domain}", domain);
                RenderFinalSummary(failed);

                if (string.Equals(triggeredBy, "Manual", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Out.WriteLine("Press any key to exit...");
                    Console.ReadKey(intercept: true);
                }

                return 0;
            }
            catch (Exception ex)
            {
                failed = true;
                Logger.Error(ex, "Dashboard snapshot refresh failed.");
                TryMarkSummaryFailed();
                RenderFinalSummary(failed: true);

                if (string.Equals(triggeredBy, "Manual", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Out.WriteLine("Press any key to exit...");
                    Console.ReadKey(intercept: true);
                }

                return 1;
            }
            finally
            {
                _totalStopwatch.Stop();
                _reporter?.Dispose();
                LogManager.Shutdown();
            }
        }

        private IConfiguration ExecuteLoadConfiguration()
        {
            var stepId = WorkerStepIds.LoadConfiguration;
            var sw = Stopwatch.StartNew();
            _reporter.StepStarted(stepId, "Load Configuration");

            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true)
                    .Build();

                var machineConfigExists = System.IO.File.Exists(
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"appsettings.{Environment.MachineName}.json"));

                sw.Stop();
                _stepDurations[stepId] = sw.Elapsed;
                _reporter.StepCompleted(stepId, new WorkerProgressStepInfo
                {
                    Duration = sw.Elapsed,
                    Detail = machineConfigExists
                        ? $"Using machine override: appsettings.{Environment.MachineName}.json"
                        : "Using appsettings.json"
                });

                return configuration;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _reporter.StepFailed(stepId, ex.Message);
                throw;
            }
        }

        private void ExecuteValidateDatabase(IServiceProvider serviceProvider)
        {
            var stepId = WorkerStepIds.ValidateDatabase;
            var sw = Stopwatch.StartNew();
            _reporter.StepStarted(stepId, "Validate Database Connection");

            try
            {
                var factory = serviceProvider.GetRequiredService<ConnectionStringFactory>();
                var result = DatabaseConnectionValidator.Validate(factory);

                sw.Stop();
                _stepDurations[stepId] = sw.Elapsed;
                _reporter.StepCompleted(stepId, new WorkerProgressStepInfo
                {
                    Duration = sw.Elapsed,
                    Detail = $"Server: {result.Server} | Database: {result.Database}"
                });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _reporter.StepFailed(stepId, ex.Message);
                throw;
            }
        }

        private void ExecuteGenerateSummary()
        {
            var stepId = WorkerStepIds.GenerateSummary;
            var sw = Stopwatch.StartNew();
            _reporter.StepStarted(stepId, "Generate Summary");
            sw.Stop();
            _stepDurations[stepId] = sw.Elapsed;
            _reporter.StepCompleted(stepId, new WorkerProgressStepInfo { Duration = sw.Elapsed });
        }

        private void ExecuteRefresh(IServiceProvider serviceProvider, string domain, string triggeredBy, string[] args)
        {
            switch (domain.ToUpperInvariant())
            {
                case "ALL":
                    var allWorker = serviceProvider.GetRequiredService<IRefreshAllDashboardSnapshotsWorker>();
                    var allRequest = new RefreshAllDashboardSnapshotsRequest { TriggeredBy = triggeredBy };
                    allWorker.Execute(allRequest);
                    CaptureDomainDurations(allRequest.Result?.Domains);
                    break;

                case "PIUTANG":
                    RunDomain(serviceProvider, "Piutang", triggeredBy, sp =>
                    {
                        var worker = sp.GetRequiredService<IRefreshDashboardPiutangSnapshotWorker>();
                        var request = new RefreshDashboardPiutangSnapshotRequest { TriggeredBy = triggeredBy };
                        worker.Execute(request);
                        return request.Result?.DurationMs ?? 0;
                    });
                    break;

                case "INVENTORY":
                    RunDomain(serviceProvider, "Inventory", triggeredBy, sp =>
                    {
                        var worker = sp.GetRequiredService<IRefreshDashboardInventorySnapshotWorker>();
                        var request = new RefreshDashboardInventorySnapshotRequest { TriggeredBy = triggeredBy };
                        worker.Execute(request);
                        return request.Result?.DurationMs ?? 0;
                    });
                    break;

                case "INVENTORYRISK":
                    RunDomain(serviceProvider, "InventoryRisk", triggeredBy, sp =>
                    {
                        var worker = sp.GetRequiredService<IRefreshDashboardInventoryRiskSnapshotWorker>();
                        var request = new RefreshDashboardInventoryRiskSnapshotRequest { TriggeredBy = triggeredBy };
                        worker.Execute(request);
                        return request.Result?.DurationMs ?? 0;
                    });
                    break;

                case "SALES":
                    RunDomain(serviceProvider, "Sales", triggeredBy, sp =>
                    {
                        var worker = sp.GetRequiredService<IRefreshDashboardSalesSnapshotWorker>();
                        var request = new RefreshDashboardSalesSnapshotRequest { TriggeredBy = triggeredBy };
                        worker.Execute(request);
                        return request.Result?.DurationMs ?? 0;
                    });
                    break;

                case "PURCHASING":
                    RunDomain(serviceProvider, "Purchasing", triggeredBy, sp =>
                    {
                        var worker = sp.GetRequiredService<IRefreshDashboardPurchasingSnapshotWorker>();
                        var request = new RefreshDashboardPurchasingSnapshotRequest { TriggeredBy = triggeredBy };
                        worker.Execute(request);
                        return request.Result?.DurationMs ?? 0;
                    });
                    break;

                case "PURCHASINGMANAGEMENT":
                    RunDomain(serviceProvider, "PurchasingManagement", triggeredBy, sp =>
                    {
                        var worker = sp.GetRequiredService<IRefreshDashboardPurchasingManagementSnapshotWorker>();
                        var request = new RefreshDashboardPurchasingManagementSnapshotRequest { TriggeredBy = triggeredBy };
                        worker.Execute(request);
                        return request.Result?.DurationMs ?? 0;
                    });
                    break;

                case "CUSTOMER":
                    RunDomain(serviceProvider, "Customer", triggeredBy, sp =>
                    {
                        var worker = sp.GetRequiredService<IRefreshDashboardCustomerSnapshotWorker>();
                        var request = new RefreshDashboardCustomerSnapshotRequest { TriggeredBy = triggeredBy };
                        worker.Execute(request);
                        return request.Result?.DurationMs ?? 0;
                    });
                    break;

                case "SALESMAN":
                    RunDomain(serviceProvider, "Salesman", triggeredBy, sp =>
                    {
                        var worker = sp.GetRequiredService<IRefreshDashboardSalesmanSnapshotWorker>();
                        var request = new RefreshDashboardSalesmanSnapshotRequest { TriggeredBy = triggeredBy };
                        worker.Execute(request);
                        return request.Result?.DurationMs ?? 0;
                    });
                    break;

                case "COLLECTION":
                    RunDomain(serviceProvider, "Collection", triggeredBy, sp =>
                    {
                        var worker = sp.GetRequiredService<IRefreshDashboardCollectionSnapshotWorker>();
                        var request = new RefreshDashboardCollectionSnapshotRequest { TriggeredBy = triggeredBy };
                        worker.Execute(request);
                        return request.Result?.DurationMs ?? 0;
                    });
                    break;

                case "LOCATION":
                    RunDomain(serviceProvider, "Location", triggeredBy, sp =>
                    {
                        var worker = sp.GetRequiredService<IRefreshDashboardLocationSnapshotWorker>();
                        var request = new RefreshDashboardLocationSnapshotRequest { TriggeredBy = triggeredBy };
                        worker.Execute(request);
                        return request.Result?.DurationMs ?? 0;
                    });
                    break;

                case "ENTITYANALYTICSHISTORICALBACKFILL":
                    RunDomain(serviceProvider, "EntityAnalyticsHistoricalBackfill", triggeredBy, sp =>
                    {
                        var worker = sp.GetRequiredService<IEntityAnalyticsHistoricalBackfillWorker>();
                        var request = EntityAnalyticsBackfillCliParser.Parse(args, triggeredBy);

                        using (var cancellationSource = new CancellationTokenSource())
                        {
                            Console.CancelKeyPress += OnCancelKeyPress;
                            try
                            {
                                request.CancellationToken = cancellationSource.Token;
                                worker.Execute(request);
                            }
                            finally
                            {
                                Console.CancelKeyPress -= OnCancelKeyPress;
                            }
                        }

                        return request.Result?.DurationMs ?? 0;

                        void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
                        {
                            e.Cancel = true;
                            cancellationSource.Cancel();
                        }
                    });
                    break;

                default:
                    throw new ArgumentException($"Unsupported domain '{domain}'.");
            }
        }

        private void RunDomain(
            IServiceProvider serviceProvider,
            string domain,
            string triggeredBy,
            Func<IServiceProvider, int> execute)
        {
            var stepId = WorkerStepIds.DomainStep(domain);
            var sw = Stopwatch.StartNew();
            _reporter.StepStarted(stepId, $"Refresh {domain} Snapshot");

            try
            {
                var durationMs = execute(serviceProvider);
                sw.Stop();
                var duration = durationMs > 0
                    ? TimeSpan.FromMilliseconds(durationMs)
                    : sw.Elapsed;
                _stepDurations[stepId] = duration;
                _reporter.StepCompleted(stepId, new WorkerProgressStepInfo { Duration = duration });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _reporter.StepFailed(stepId, ex.Message);
                throw;
            }
        }

        private void CaptureDomainDurations(IList<RefreshDashboardDomainResult> domains)
        {
            if (domains == null)
                return;

            foreach (var domain in domains)
            {
                if (string.IsNullOrWhiteSpace(domain?.Domain))
                    continue;

                _stepDurations[WorkerStepIds.DomainStep(domain.Domain)] =
                    TimeSpan.FromMilliseconds(domain.DurationMs);
            }
        }

        private void TryMarkSummaryFailed()
        {
            if (_reporter == null)
                return;

            foreach (var item in _reporter.Tracker.Snapshot())
            {
                if (item.Status == ConsoleTaskStatus.InProgress)
                    _reporter.StepFailed(item.StepId, "Execution interrupted.");
            }
        }

        private void RenderFinalSummary(bool failed)
        {
            if (_reporter == null)
                return;

            _reporter.Tracker.RenderFinalSummary(
                _reporter.BuildSummaryRows(_stepDurations),
                _totalStopwatch.Elapsed,
                failed);
        }

        private static ExecutionPlan BuildExecutionPlan(string domain)
        {
            var stepIds = new List<string>
            {
                WorkerStepIds.LoadConfiguration,
                WorkerStepIds.ValidateDatabase
            };
            var displayNames = new List<string>
            {
                "Load Configuration",
                "Validate Database Connection"
            };

            if (string.Equals(domain, "All", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var domainName in WorkerDomainOrder.AllDomainOrder)
                {
                    stepIds.Add(WorkerStepIds.DomainStep(domainName));
                    displayNames.Add($"Refresh {domainName} Snapshot");
                }
            }
            else if (string.Equals(domain, "EntityAnalyticsHistoricalBackfill", StringComparison.OrdinalIgnoreCase))
            {
                stepIds.Add(WorkerStepIds.DomainStep("EntityAnalyticsHistoricalBackfill"));
                displayNames.Add("Entity Analytics Historical Backfill");
            }
            else
            {
                stepIds.Add(WorkerStepIds.DomainStep(domain));
                displayNames.Add($"Refresh {domain} Snapshot");
            }

            stepIds.Add(WorkerStepIds.GenerateSummary);
            displayNames.Add("Generate Summary");

            return new ExecutionPlan(stepIds, displayNames);
        }

        private static void ValidateOptions(string domain, string triggeredBy)
        {
            var validDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "All", "Sales", "Piutang", "Inventory", "InventoryRisk", "Purchasing",
                "PurchasingManagement", "Customer", "Salesman", "Collection", "Location",
                "EntityAnalyticsHistoricalBackfill"
            };

            var validTriggers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Scheduler", "Manual"
            };

            if (!validDomains.Contains(domain))
                throw new ArgumentException(
                    $"Invalid --domain '{domain}'. Expected All, Sales, Piutang, Inventory, InventoryRisk, Purchasing, PurchasingManagement, Customer, Salesman, Collection, Location, or EntityAnalyticsHistoricalBackfill.");

            if (!validTriggers.Contains(triggeredBy))
                throw new ArgumentException(
                    $"Invalid --triggered-by '{triggeredBy}'. Expected Scheduler or Manual.");
        }

        private static string ParseRequiredOption(string[] args, string name, string defaultValue)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    throw new ArgumentException($"Missing value for {name}.");

                return args[i + 1];
            }

            return defaultValue;
        }

        private sealed class ExecutionPlan
        {
            public ExecutionPlan(IReadOnlyList<string> stepIds, IReadOnlyList<string> displayNames)
            {
                StepIds = stepIds;
                DisplayNames = displayNames;
            }

            public IReadOnlyList<string> StepIds { get; }

            public IReadOnlyList<string> DisplayNames { get; }
        }
    }
}
