using System;
using System.Diagnostics;
using System.Linq;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
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
        private readonly INunaCounterBL _counter;
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
            INunaCounterBL counter,
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
            _counter = counter;
            _options = options ?? new DashboardSnapshotOptions();
        }

        public void Execute(RefreshDashboardPurchasingManagementSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = _counter.Generate("PDR", IDFormatEnum.PFnnnn);
            var startedAt = _tglJamDal.Now;

            _refreshLogDal.InsertRunning(new DashboardSnapshotRefreshLogModel
            {
                RefreshLogId = refreshLogId,
                Domain = Domain,
                StartedAt = startedAt,
                Status = "Running",
                TriggeredBy = request.TriggeredBy ?? "Scheduler"
            });

            try
            {
                var today = _tglJamDal.Now.Date;
                var periode = CurrentMonthPeriode(today);
                var generatedAt = _tglJamDal.Now;

                var invoiceRows = _invoiceViewDal.ListData(periode)?.ToList()
                    ?? new System.Collections.Generic.List<InvoiceView>();
                var inventorySnapshot = _inventorySnapshotDal.GetCurrent();
                var inventoryRiskSnapshot = _inventoryRiskSnapshotDal.GetCurrent();
                var purchasingSnapshot = _purchasingSnapshotDal.GetCurrent();

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

                var aggregate = _aggregator.Aggregate(
                    invoiceRows,
                    purchasingSnapshot,
                    inventorySnapshot,
                    inventoryRiskSnapshot,
                    periode,
                    today,
                    generatedAt,
                    _options.PurchasingQualifiedBacklogDays);

                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, refreshLogId);
                    trans.Complete();
                }

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
