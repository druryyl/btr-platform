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
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Loaders
{
    public class SupplierReplayDataLoader : IEntityAnalyticsReplayDataLoader
    {
        private readonly IInvoiceViewDal _invoiceViewDal;
        private readonly ISupplierDal _supplierDal;
        private readonly ISupplierMtdItemRollupDal _supplierMtdItemRollupDal;
        private readonly IPrincipalSalesOutEvidenceDal _salesOutEvidenceDal;
        private readonly IPrincipalReturnEvidenceDal _returnEvidenceDal;
        private readonly IPrincipalTargetEvidenceDal _targetEvidenceDal;
        private readonly IPrincipalPurchaseInEvidenceDal _purchaseInEvidenceDal;

        public SupplierReplayDataLoader(
            IInvoiceViewDal invoiceViewDal,
            ISupplierDal supplierDal,
            ISupplierMtdItemRollupDal supplierMtdItemRollupDal,
            IPrincipalSalesOutEvidenceDal salesOutEvidenceDal = null,
            IPrincipalReturnEvidenceDal returnEvidenceDal = null,
            IPrincipalTargetEvidenceDal targetEvidenceDal = null,
            IPrincipalPurchaseInEvidenceDal purchaseInEvidenceDal = null)
        {
            _invoiceViewDal = invoiceViewDal;
            _supplierDal = supplierDal;
            _supplierMtdItemRollupDal = supplierMtdItemRollupDal;
            _salesOutEvidenceDal = salesOutEvidenceDal;
            _returnEvidenceDal = returnEvidenceDal;
            _targetEvidenceDal = targetEvidenceDal;
            _purchaseInEvidenceDal = purchaseInEvidenceDal;
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

            LoadPrincipalEvidence(bundle, periode, replayContext.PeriodYear, replayContext.PeriodMonth, stepPrefix);

            WorkerProgressScope.Current?.StepCompleted($"{stepPrefix}:Load", new WorkerProgressStepInfo
            {
                RecordCount = bundle.InvoiceRows.Count + bundle.Suppliers.Count
            });

            return bundle;
        }

        private void LoadPrincipalEvidence(
            SupplierReplayDataBundle bundle,
            Periode periode,
            int year,
            int month,
            string stepPrefix)
        {
            if (bundle is null)
                return;

            if (_salesOutEvidenceDal is null
                && _returnEvidenceDal is null
                && _targetEvidenceDal is null
                && _purchaseInEvidenceDal is null)
            {
                return;
            }

            WorkerProgressScope.Current?.StepStarted(
                $"{stepPrefix}:LoadPrincipal",
                "Load historical Principal KPI evidence");

            var principalCount = 0;
            if (_salesOutEvidenceDal != null)
            {
                bundle.SalesOutEvidence = _salesOutEvidenceDal.ListFakturItemEvidence(periode)?.ToList()
                    ?? new System.Collections.Generic.List<PrincipalSalesOutFakturItemEvidence>();
                principalCount += bundle.SalesOutEvidence.Count;
            }

            if (_returnEvidenceDal != null)
            {
                bundle.ReturnEvidence = _returnEvidenceDal.ListReturnItemEvidence(year, month)?.ToList()
                    ?? new System.Collections.Generic.List<ReturnItemEvidence>();
                principalCount += bundle.ReturnEvidence.Count;
            }

            if (_targetEvidenceDal != null)
            {
                bundle.TargetEvidence = _targetEvidenceDal.ListSalesmanPrincipalTargets(year, month)?.ToList()
                    ?? new System.Collections.Generic.List<SalesmanPrincipalTargetEvidence>();
                principalCount += bundle.TargetEvidence.Count;
            }

            if (_purchaseInEvidenceDal != null)
            {
                bundle.PurchaseEvidence = _purchaseInEvidenceDal.ListPurchaseDetail(year, month)?.ToList()
                    ?? new System.Collections.Generic.List<PurchaseDetailEvidence>();
                principalCount += bundle.PurchaseEvidence.Count;
            }

            WorkerProgressScope.Current?.StepCompleted(
                $"{stepPrefix}:LoadPrincipal",
                new WorkerProgressStepInfo { RecordCount = principalCount });
        }
    }
}
