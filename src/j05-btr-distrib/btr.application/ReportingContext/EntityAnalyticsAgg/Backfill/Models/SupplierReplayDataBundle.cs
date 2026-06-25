using System.Collections.Generic;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.domain.PurchaseContext.SupplierAgg;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public sealed class SupplierReplayDataBundle
    {
        public IList<InvoiceView> InvoiceRows { get; set; } = new List<InvoiceView>();

        public IList<SupplierModel> Suppliers { get; set; } = new List<SupplierModel>();

        public IList<SupplierMtdItemRollupDto> ItemRollupRows { get; set; } = new List<SupplierMtdItemRollupDto>();

        public IList<SupplierCatalogCountDto> CatalogCounts { get; set; } = new List<SupplierCatalogCountDto>();
    }
}
