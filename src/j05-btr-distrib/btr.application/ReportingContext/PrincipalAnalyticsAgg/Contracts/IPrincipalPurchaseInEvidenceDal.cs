using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalPurchaseInEvidenceDal
    {
        IReadOnlyList<PurchaseDetailEvidence> ListPurchaseDetail(int year, int month);
    }

    public class PurchaseDetailEvidence
    {
        public string InvoiceId { get; set; }

        public string InvoiceItemId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal PurchaseDetailTotal { get; set; }
    }
}
