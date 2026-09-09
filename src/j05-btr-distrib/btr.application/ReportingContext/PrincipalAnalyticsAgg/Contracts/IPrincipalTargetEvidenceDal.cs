using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalTargetEvidenceDal
    {
        IReadOnlyList<SalesmanPrincipalTargetEvidence> ListSalesmanPrincipalTargets(int year, int month);
    }

    public class SalesmanPrincipalTargetEvidence
    {
        public string SalesPersonId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public int TargetYear { get; set; }

        public int TargetMonth { get; set; }

        public decimal TargetAmount { get; set; }
    }
}
