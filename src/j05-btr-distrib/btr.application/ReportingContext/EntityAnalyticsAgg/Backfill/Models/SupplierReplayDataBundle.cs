using System.Collections.Generic;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.domain.PurchaseContext.SupplierAgg;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public sealed class SupplierReplayDataBundle
    {
        public IList<InvoiceView> InvoiceRows { get; set; } = new List<InvoiceView>();

        public IList<SupplierModel> Suppliers { get; set; } = new List<SupplierModel>();

        public IList<SupplierMtdItemRollupDto> ItemRollupRows { get; set; } = new List<SupplierMtdItemRollupDto>();

        public IList<SupplierCatalogCountDto> CatalogCounts { get; set; } = new List<SupplierCatalogCountDto>();

        /// <summary>
        /// Month-scoped Principal Sales-Out evidence (Faktur Item grain).
        /// Loaded only for replay; empty in contexts that predate Principal replay support.
        /// </summary>
        public IList<PrincipalSalesOutFakturItemEvidence> SalesOutEvidence { get; set; }
            = new List<PrincipalSalesOutFakturItemEvidence>();

        /// <summary>
        /// Month-scoped Principal Return evidence (Return Item grain).
        /// </summary>
        public IList<ReturnItemEvidence> ReturnEvidence { get; set; }
            = new List<ReturnItemEvidence>();

        /// <summary>
        /// Month-scoped Salesman Principal Target evidence.
        /// </summary>
        public IList<SalesmanPrincipalTargetEvidence> TargetEvidence { get; set; }
            = new List<SalesmanPrincipalTargetEvidence>();

        /// <summary>
        /// Month-scoped Principal Purchase-In evidence (Purchase Detail grain).
        /// </summary>
        public IList<PurchaseDetailEvidence> PurchaseEvidence { get; set; }
            = new List<PurchaseDetailEvidence>();
    }
}
