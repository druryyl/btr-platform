using System;
using System.Diagnostics;
using System.Linq;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.PurchaseContext.SupplierAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.Portal;
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
        private readonly DashboardSupplierRelationshipAggregator _relationshipAggregator;
        private readonly ISupplierDal _supplierDal;
        private readonly ISupplierMtdItemRollupDal _supplierMtdItemRollupDal;
        private readonly IDashboardPurchasingManagementSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;
        private readonly DashboardSnapshotOptions _options;
        private readonly EntityAnalyticsProducerOrchestrator _entityAnalyticsOrchestrator;

        public RefreshDashboardPurchasingManagementSnapshotWorker(
            IInvoiceViewDal invoiceViewDal,
            IDashboardInventorySnapshotDal inventorySnapshotDal,
            IDashboardInventoryRiskSnapshotDal inventoryRiskSnapshotDal,
            IDashboardPurchasingSnapshotDal purchasingSnapshotDal,
            DashboardPurchasingManagementAggregator aggregator,
            DashboardSupplierRelationshipAggregator relationshipAggregator,
            ISupplierDal supplierDal,
            ISupplierMtdItemRollupDal supplierMtdItemRollupDal,
            IDashboardPurchasingManagementSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider,
            DashboardSnapshotOptions options,
            EntityAnalyticsProducerOrchestrator entityAnalyticsOrchestrator)
        {
            _invoiceViewDal = invoiceViewDal;
            _inventorySnapshotDal = inventorySnapshotDal;
            _inventoryRiskSnapshotDal = inventoryRiskSnapshotDal;
            _purchasingSnapshotDal = purchasingSnapshotDal;
            _aggregator = aggregator;
            _relationshipAggregator = relationshipAggregator;
            _supplierDal = supplierDal;
            _supplierMtdItemRollupDal = supplierMtdItemRollupDal;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
            _options = options ?? new DashboardSnapshotOptions();
            _entityAnalyticsOrchestrator = entityAnalyticsOrchestrator;
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
                var today = _businessDateProvider.Today;
                var periode = CurrentMonthPeriode(today);
                var generatedAt = _tglJamDal.Now;
                const int loadSteps = 7;

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

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load suppliers", 5, loadSteps);
                var suppliers = _supplierDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.PurchaseContext.SupplierAgg.SupplierModel>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load item rollups", 6, loadSteps);
                var itemRollupRows = _supplierMtdItemRollupDal.ListMtdItemRollups(periode)?.ToList()
                    ?? new System.Collections.Generic.List<SupplierMtdItemRollupDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load catalog counts", 7, loadSteps);
                var catalogCounts = _supplierMtdItemRollupDal.ListSupplierCatalogCounts()?.ToList()
                    ?? new System.Collections.Generic.List<SupplierCatalogCountDto>();
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = invoiceRows.Count + suppliers.Count + itemRollupRows.Count
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
                    _options.PurchasingQualifiedBacklogDays,
                    suppliers,
                    itemRollupRows,
                    catalogCounts);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Aggregate");

                WorkerProgressScope.Current.StepStarted($"{Domain}:AggregateRelationships", "Aggregate supplier relationship rollups");
                var relationshipAggregate = _relationshipAggregator.Aggregate(itemRollupRows, today, generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:AggregateRelationships");

                WorkerProgressScope.Current.StepStarted($"{Domain}:Save", "Save snapshot");
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, refreshLogId);

                    WorkerProgressScope.Current.StepStarted($"{Domain}:EntityAnalytics", "Produce entity analytics L0+L1+L2+L3+L4+L5 snapshot");
                    _entityAnalyticsOrchestrator.ProduceForDomain(Domain, new EntityAnalyticsProduceContext
                    {
                        RefreshLogId = refreshLogId,
                        GeneratedAt = generatedAt,
                        BusinessDate = today,
                        DomainInput = new SupplierEntityAnalyticsProduceInput
                        {
                            ManagementAggregate = aggregate,
                            RelationshipAggregate = relationshipAggregate
                        }
                    });
                    WorkerProgressScope.Current.StepCompleted($"{Domain}:EntityAnalytics");

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
