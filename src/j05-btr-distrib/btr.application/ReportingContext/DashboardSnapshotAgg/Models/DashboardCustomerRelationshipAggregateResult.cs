using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardCustomerRelationshipItemRow
    {
        public int Rank { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public decimal MetricValue { get; set; }
    }

    public class DashboardCustomerRelationshipPrincipalRow
    {
        public int Rank { get; set; }

        public string SupplierId { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }

        public decimal MetricValue { get; set; }
    }

    public class DashboardCustomerRelationshipCustomerRollup
    {
        public string CustomerCode { get; set; }

        public List<DashboardCustomerRelationshipItemRow> TopItems { get; set; }
            = new List<DashboardCustomerRelationshipItemRow>();

        public List<DashboardCustomerRelationshipPrincipalRow> TopPrincipals { get; set; }
            = new List<DashboardCustomerRelationshipPrincipalRow>();
    }

    public class DashboardCustomerRelationshipAggregateResult
    {
        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public Dictionary<string, DashboardCustomerRelationshipCustomerRollup> ByCustomerCode { get; set; }
            = new Dictionary<string, DashboardCustomerRelationshipCustomerRollup>(StringComparer.OrdinalIgnoreCase);
    }
}
