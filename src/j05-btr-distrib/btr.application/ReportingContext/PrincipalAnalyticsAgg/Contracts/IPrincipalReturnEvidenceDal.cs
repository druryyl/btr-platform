using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalReturnEvidenceDal
    {
        IReadOnlyList<ReturnItemEvidence> ListReturnItemEvidence(int year, int month);
    }

    public class ReturnItemEvidence
    {
        public string ReturJualId { get; set; }

        public string ReturJualItemId { get; set; }

        public string JenisRetur { get; set; }

        public string ItemSupplierId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal SubTotal { get; set; }

        public decimal DiscRp { get; set; }

        public decimal PpnRp { get; set; }

        public decimal LineTotal { get; set; }
    }
}
