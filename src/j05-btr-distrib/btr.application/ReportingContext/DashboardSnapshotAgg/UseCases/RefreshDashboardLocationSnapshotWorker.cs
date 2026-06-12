using System;
using System.Diagnostics;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.InventoryContext.WarehouseAgg;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.application.Portal;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public interface IRefreshDashboardLocationSnapshotWorker
        : INunaServiceVoid<RefreshDashboardLocationSnapshotRequest>
    {
    }

    public class RefreshDashboardLocationSnapshotWorker : IRefreshDashboardLocationSnapshotWorker
    {
        private const string Domain = "Location";
        private const int MaxErrorMessageLength = 500;

        private readonly IStokBalanceViewDal _stokBalanceViewDal;
        private readonly IBrgLastFakturDal _brgLastFakturDal;
        private readonly IFakturViewDal _fakturViewDal;
        private readonly IInvoiceViewDal _invoiceViewDal;
        private readonly IWarehouseDal _warehouseDal;
        private readonly IDashboardInventorySnapshotDal _inventorySnapshotDal;
        private readonly IDashboardInventoryRiskSnapshotDal _inventoryRiskSnapshotDal;
        private readonly IDashboardSalesSnapshotDal _salesSnapshotDal;
        private readonly IDashboardPurchasingSnapshotDal _purchasingSnapshotDal;
        private readonly DashboardLocationAggregator _aggregator;
        private readonly IDashboardLocationSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;

        public RefreshDashboardLocationSnapshotWorker(
            IStokBalanceViewDal stokBalanceViewDal,
            IBrgLastFakturDal brgLastFakturDal,
            IFakturViewDal fakturViewDal,
            IInvoiceViewDal invoiceViewDal,
            IWarehouseDal warehouseDal,
            IDashboardInventorySnapshotDal inventorySnapshotDal,
            IDashboardInventoryRiskSnapshotDal inventoryRiskSnapshotDal,
            IDashboardSalesSnapshotDal salesSnapshotDal,
            IDashboardPurchasingSnapshotDal purchasingSnapshotDal,
            DashboardLocationAggregator aggregator,
            IDashboardLocationSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider)
        {
            _stokBalanceViewDal = stokBalanceViewDal;
            _brgLastFakturDal = brgLastFakturDal;
            _fakturViewDal = fakturViewDal;
            _invoiceViewDal = invoiceViewDal;
            _warehouseDal = warehouseDal;
            _inventorySnapshotDal = inventorySnapshotDal;
            _inventoryRiskSnapshotDal = inventoryRiskSnapshotDal;
            _salesSnapshotDal = salesSnapshotDal;
            _purchasingSnapshotDal = purchasingSnapshotDal;
            _aggregator = aggregator;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
        }

        public void Execute(RefreshDashboardLocationSnapshotRequest request)
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
                const int loadSteps = 9;

                WorkerProgressScope.Current.StepStarted($"{Domain}:Load", "Load source data");
                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load stock balances", 1, loadSteps);
                var stokRows = _stokBalanceViewDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<StokBalanceView>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load last faktur by item", 2, loadSteps);
                var lastFakturRows = _brgLastFakturDal.ListLastFakturByBrg()?.ToList()
                    ?? new System.Collections.Generic.List<BrgLastFakturDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load faktur", 3, loadSteps);
                var fakturRows = _fakturViewDal.ListData(periode)?.ToList()
                    ?? new System.Collections.Generic.List<FakturView>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load invoices", 4, loadSteps);
                var invoiceRows = _invoiceViewDal.ListData(periode)?.ToList()
                    ?? new System.Collections.Generic.List<InvoiceView>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load warehouses", 5, loadSteps);
                var warehouses = _warehouseDal.ListAllForPortal()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.InventoryContext.WarehouseAgg.WarehouseModel>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load inventory snapshot", 6, loadSteps);
                var inventorySnapshot = _inventorySnapshotDal.GetCurrent();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load inventory risk snapshot", 7, loadSteps);
                var inventoryRiskSnapshot = _inventoryRiskSnapshotDal.GetCurrent();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load sales snapshot", 8, loadSteps);
                var salesSnapshot = _salesSnapshotDal.GetCurrent();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load purchasing snapshot", 9, loadSteps);
                var purchasingSnapshot = _purchasingSnapshotDal.GetCurrent();
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = stokRows.Count + lastFakturRows.Count + fakturRows.Count +
                                  invoiceRows.Count + warehouses.Count
                });

                WorkerProgressScope.Current.StepStarted($"{Domain}:Aggregate", "Aggregate metrics");
                var aggregate = _aggregator.Aggregate(
                    stokRows,
                    lastFakturRows,
                    fakturRows,
                    invoiceRows,
                    warehouses,
                    inventorySnapshot,
                    inventoryRiskSnapshot,
                    salesSnapshot,
                    purchasingSnapshot,
                    periode,
                    today,
                    generatedAt);
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

                request.Result = new RefreshDashboardLocationSnapshotResult
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
