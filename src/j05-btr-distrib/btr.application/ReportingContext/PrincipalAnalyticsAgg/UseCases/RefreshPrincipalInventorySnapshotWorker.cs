using System;
using System.Diagnostics;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.application.Portal;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public interface IRefreshPrincipalInventorySnapshotWorker
        : INunaServiceVoid<RefreshPrincipalInventorySnapshotRequest>
    {
    }

    public class RefreshPrincipalInventorySnapshotWorker : IRefreshPrincipalInventorySnapshotWorker
    {
        private const int MaxErrorMessageLength = 500;

        private readonly IStokBalanceViewDal _stokBalanceViewDal;
        private readonly IBrgLastFakturDal _brgLastFakturDal;
        private readonly IBrgConsumptionDal _brgConsumptionDal;
        private readonly PrincipalInventoryAggregator _aggregator;
        private readonly IPrincipalInventorySnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;
        private readonly DashboardSnapshotOptions _options;

        public RefreshPrincipalInventorySnapshotWorker(
            IStokBalanceViewDal stokBalanceViewDal,
            IBrgLastFakturDal brgLastFakturDal,
            IBrgConsumptionDal brgConsumptionDal,
            PrincipalInventoryAggregator aggregator,
            IPrincipalInventorySnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider,
            DashboardSnapshotOptions options)
        {
            _stokBalanceViewDal = stokBalanceViewDal;
            _brgLastFakturDal = brgLastFakturDal;
            _brgConsumptionDal = brgConsumptionDal;
            _aggregator = aggregator;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
            _options = options ?? new DashboardSnapshotOptions();
        }

        public void Execute(RefreshPrincipalInventorySnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;
            var domain = PrincipalInventorySnapshot.Domain;

            WorkerProgressScope.Current.StepStarted($"{domain}:Initialize", "Initialize refresh log");
            _refreshLogDal.InsertRunning(new DashboardSnapshotRefreshLogModel
            {
                RefreshLogId = refreshLogId,
                Domain = domain,
                StartedAt = startedAt,
                Status = "Running",
                TriggeredBy = request.TriggeredBy ?? "Scheduler"
            });
            WorkerProgressScope.Current.StepCompleted($"{domain}:Initialize");

            try
            {
                var today = _businessDateProvider.Today;
                var generatedAt = _tglJamDal.Now;
                var window30Start = today.Date.AddDays(-(InventoryForecastPolicy.AdcWindow30Days - 1));
                var window90Start = today.Date.AddDays(-(InventoryForecastPolicy.AdcWindow90Days - 1));

                WorkerProgressScope.Current.StepStarted($"{domain}:Load", "Load Inventory Snapshot evidence");
                var stockRows = _stokBalanceViewDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<StokBalanceView>();
                var lastFakturRows = _brgLastFakturDal.ListLastFakturByBrg()?.ToList()
                    ?? new System.Collections.Generic.List<BrgLastFakturDto>();
                var consumptionRows = _brgConsumptionDal.ListConsumptionByBrg(window30Start, window90Start, today)?.ToList()
                    ?? new System.Collections.Generic.List<BrgConsumptionDto>();
                WorkerProgressScope.Current.StepCompleted($"{domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = stockRows.Count
                });

                WorkerProgressScope.Current.StepStarted($"{domain}:Aggregate", "Map Principal inventory KPIs");
                var aggregate = _aggregator.Aggregate(
                    stockRows,
                    lastFakturRows,
                    consumptionRows,
                    today,
                    generatedAt,
                    _options.InventoryForecastPlanningHorizonDays,
                    _options.InventoryForecastDefaultLeadTimeDays,
                    _options.InventoryForecastCoverageDays);
                WorkerProgressScope.Current.StepCompleted($"{domain}:Aggregate");

                WorkerProgressScope.Current.StepStarted($"{domain}:Save", "Save Principal inventory snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshPrincipalInventorySnapshotResult
                {
                    RefreshLogId = refreshLogId,
                    DurationMs = (int)sw.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                var message = ex.Message ?? ex.GetType().Name;
                if (message.Length > MaxErrorMessageLength)
                    message = message.Substring(0, MaxErrorMessageLength);

                _refreshLogDal.MarkFailed(refreshLogId, (int)sw.ElapsedMilliseconds, message);
                WorkerProgressScope.Current.StepFailed($"{domain}:Execute", message);
                throw;
            }
        }
    }
}
