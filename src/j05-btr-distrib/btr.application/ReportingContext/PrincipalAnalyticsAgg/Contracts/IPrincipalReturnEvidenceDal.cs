using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalReturnEvidenceDal
    {
        IReadOnlyList<ReturnItemEvidence> ListReturnItemEvidence(int year, int month);

        IReadOnlyList<PrincipalReturnItemEvidenceLine> ListReturnItemEvidenceForPrincipal(
            int year,
            int month,
            string supplierId);
    }

    public class PrincipalReturnItemEvidenceLine
    {
        public string ReturJualId { get; set; }

        public string ReturJualCode { get; set; }

        public System.DateTime ReturJualDate { get; set; }

        public string ReturJualItemId { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string JenisRetur { get; set; }

        public string ItemSupplierId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal SubTotal { get; set; }

        public decimal DiscRp { get; set; }
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
