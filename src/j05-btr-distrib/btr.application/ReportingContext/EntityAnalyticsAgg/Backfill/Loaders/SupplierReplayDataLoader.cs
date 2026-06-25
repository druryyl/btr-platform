using System;
using System.Linq;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.PurchaseContext.SupplierAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Loaders
{
    public class SupplierReplayDataLoader : IEntityAnalyticsReplayDataLoader
    {
        private readonly IInvoiceViewDal _invoiceViewDal;
        private readonly ISupplierDal _supplierDal;
        private readonly ISupplierMtdItemRollupDal _supplierMtdItemRollupDal;

        public SupplierReplayDataLoader(
            IInvoiceViewDal invoiceViewDal,
            ISupplierDal supplierDal,
            ISupplierMtdItemRollupDal supplierMtdItemRollupDal)
        {
            _invoiceViewDal = invoiceViewDal;
            _supplierDal = supplierDal;
            _supplierMtdItemRollupDal = supplierMtdItemRollupDal;
        }

        public string EntityType => EntityTypeCode.Supplier;

        public object Load(EntityAnalyticsReplayContext replayContext)
        {
            if (replayContext is null)
                throw new ArgumentNullException(nameof(replayContext));

            var periodEnd = replayContext.PeriodEnd.Date;
            var periode = new Periode(replayContext.PeriodStart, periodEnd);
            var stepPrefix = $"Backfill:{EntityType}:{replayContext.PeriodYear:D4}-{replayContext.PeriodMonth:D2}";

            WorkerProgressScope.Current?.StepStarted($"{stepPrefix}:Load", "Load historical supplier source data");

            var bundle = new SupplierReplayDataBundle
            {
                InvoiceRows = _invoiceViewDal.ListData(periode)?.ToList() ?? new System.Collections.Generic.List<InvoiceView>(),
                Suppliers = _supplierDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.PurchaseContext.SupplierAgg.SupplierModel>(),
                ItemRollupRows = _supplierMtdItemRollupDal.ListMtdItemRollups(periode)?.ToList()
                    ?? new System.Collections.Generic.List<SupplierMtdItemRollupDto>(),
                CatalogCounts = _supplierMtdItemRollupDal.ListSupplierCatalogCounts()?.ToList()
                    ?? new System.Collections.Generic.List<SupplierCatalogCountDto>()
            };

            WorkerProgressScope.Current?.StepCompleted($"{stepPrefix}:Load", new WorkerProgressStepInfo
            {
                RecordCount = bundle.InvoiceRows.Count + bundle.Suppliers.Count
            });

            return bundle;
        }
    }
}
