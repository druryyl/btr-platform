using System;
using System.Diagnostics;
using System.Linq;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public interface IRefreshDashboardPurchasingManagementSnapshotWorker
        : INunaServiceVoid<RefreshDashboardPurchasingManagementSnapshotRequest>
    {
    }

    public class RefreshDashboardPurchasingManagementSnapshotWorker
        : IRefreshDashboardPurchasingManagementSnapshotWorker
    {
        private const string Domain = "PurchasingManagement";
        private const int MaxErrorMessageLength = 500;

        private readonly IInvoiceViewDal _invoiceViewDal;
        private readonly IDashboardInventorySnapshotDal _inventorySnapshotDal;
        private readonly IDashboardInventoryRiskSnapshotDal _inventoryRiskSnapshotDal;
        private readonly IDashboardPurchasingSnapshotDal _purchasingSnapshotDal;
        private readonly DashboardPurchasingManagementAggregator _aggregator;
        private readonly IDashboardPurchasingManagementSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly DashboardSnapshotOptions _options;

        public RefreshDashboardPurchasingManagementSnapshotWorker(
            IInvoiceViewDal invoiceViewDal,
            IDashboardInventorySnapshotDal inventorySnapshotDal,
            IDashboardInventoryRiskSnapshotDal inventoryRiskSnapshotDal,
            IDashboardPurchasingSnapshotDal purchasingSnapshotDal,
            DashboardPurchasingManagementAggregator aggregator,
            IDashboardPurchasingManagementSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            DashboardSnapshotOptions options)
        {
            _invoiceViewDal = invoiceViewDal;
            _inventorySnapshotDal = inventorySnapshotDal;
            _inventoryRiskSnapshotDal = inventoryRiskSnapshotDal;
            _purchasingSnapshotDal = purchasingSnapshotDal;
            _aggregator = aggregator;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _options = options ?? new DashboardSnapshotOptions();
        }

        public void Execute(RefreshDashboardPurchasingManagementSnapshotRequest request)
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
                var today = _tglJamDal.Now.Date;
                var periode = CurrentMonthPeriode(today);
                var generatedAt = _tglJamDal.Now;
                const int loadSteps = 4;

                WorkerProgressScope.Current.StepStarted($"{Domain}:Load", "Load source data");
                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load invoices", 1, loadSteps);
                var invoiceRows = _invoiceViewDal.ListData(periode)?.ToList()
                    ?? new System.Collections.Generic.List<InvoiceView>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load inventory snapshot", 2, loadSteps);
                var inventorySnapshot = _inventorySnapshotDal.GetCurrent();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load inventory risk snapshot", 3, loadSteps);
                var inventoryRiskSnapshot = _inventoryRiskSnapshotDal.GetCurrent();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load purchasing snapshot", 4, loadSteps);
                var purchasingSnapshot = _purchasingSnapshotDal.GetCurrent();
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = invoiceRows.Count
                });

                if (inventorySnapshot == null)
                {
                    Trace.TraceWarning(
                        "PurchasingManagement refresh: inventory snapshot unavailable; cross-domain inventory signals omitted.");
                }

                if (inventoryRiskSnapshot == null)
                {
                    Trace.TraceWarning(
                        "PurchasingManagement refresh: inventory risk snapshot unavailable; cross-domain at-risk signals omitted.");
                }

                WorkerProgressScope.Current.StepStarted($"{Domain}:Aggregate", "Aggregate metrics");
                var aggregate = _aggregator.Aggregate(
                    invoiceRows,
                    purchasingSnapshot,
                    inventorySnapshot,
                    inventoryRiskSnapshot,
                    periode,
                    today,
                    generatedAt,
                    _options.PurchasingQualifiedBacklogDays);
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

                request.Result = new RefreshDashboardPurchasingManagementSnapshotResult
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

        private static Periode CurrentMonthPeriode(DateTime today)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            return new Periode(monthStart, monthEnd);
        }
    }
}
