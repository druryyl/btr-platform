using System;
using System.Diagnostics;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public interface IRefreshDashboardInventoryRiskSnapshotWorker
        : INunaServiceVoid<RefreshDashboardInventoryRiskSnapshotRequest>
    {
    }

    public class RefreshDashboardInventoryRiskSnapshotWorker : IRefreshDashboardInventoryRiskSnapshotWorker
    {
        private const string Domain = "InventoryRisk";
        private const int MaxErrorMessageLength = 500;

        private readonly IStokBalanceViewDal _stokBalanceViewDal;
        private readonly IBrgLastFakturDal _brgLastFakturDal;
        private readonly DashboardInventoryRiskAggregator _aggregator;
        private readonly IDashboardInventoryRiskSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;

        public RefreshDashboardInventoryRiskSnapshotWorker(
            IStokBalanceViewDal stokBalanceViewDal,
            IBrgLastFakturDal brgLastFakturDal,
            DashboardInventoryRiskAggregator aggregator,
            IDashboardInventoryRiskSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal)
        {
            _stokBalanceViewDal = stokBalanceViewDal;
            _brgLastFakturDal = brgLastFakturDal;
            _aggregator = aggregator;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
        }

        public void Execute(RefreshDashboardInventoryRiskSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;

            WorkerProgressScope.Current.StepStarted($"{Domain}:Initialize", "Initialize refresh log");
            _refreshLogDal.InsertRunning(new DashboardSnapshotRefreshLogModel
            {
                RefreshLogId = refreshLogId,
                Domain = Domain,
                StartedAt = startedAt,
                Status = "Running",
                TriggeredBy = request.TriggeredBy ?? "Scheduler"
            });
            WorkerProgressScope.Current.StepCompleted($"{Domain}:Initialize");

            try
            {
                var generatedAt = _tglJamDal.Now;
                var today = generatedAt.Date;
                const int loadSteps = 2;

                WorkerProgressScope.Current.StepStarted($"{Domain}:Load", "Load source data");
                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load stock balances", 1, loadSteps);
                var rows = _stokBalanceViewDal.ListData();
                var rowCount = rows == null ? 0 : System.Linq.Enumerable.Count(rows);

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load last faktur by item", 2, loadSteps);
                var lastFakturRows = _brgLastFakturDal.ListLastFakturByBrg()?.ToList()
                    ?? new System.Collections.Generic.List<BrgLastFakturDto>();
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = rowCount + lastFakturRows.Count
                });

                WorkerProgressScope.Current.StepStarted($"{Domain}:Aggregate", "Aggregate metrics");
                var aggregate = _aggregator.Aggregate(rows, lastFakturRows, today, generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Aggregate");

                WorkerProgressScope.Current.StepStarted($"{Domain}:Save", "Save snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, refreshLogId);
                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshDashboardInventoryRiskSnapshotResult
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
                WorkerProgressScope.Current.StepFailed($"{Domain}:Execute", message);
                throw;
            }
        }
    }
}
