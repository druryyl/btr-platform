using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardSalesmanRelationshipItemRow
    {
        public int Rank { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public decimal MetricValue { get; set; }
    }

    public class DashboardSalesmanRelationshipCustomerRow
    {
        public int Rank { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal MetricValue { get; set; }
    }

    public class DashboardSalesmanRelationshipSalesmanRollup
    {
        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public List<DashboardSalesmanRelationshipCustomerRow> TopCustomers { get; set; }
            = new List<DashboardSalesmanRelationshipCustomerRow>();

        public List<DashboardSalesmanRelationshipItemRow> TopItems { get; set; }
            = new List<DashboardSalesmanRelationshipItemRow>();
    }

    public class DashboardSalesmanRelationshipAggregateResult
    {
        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public Dictionary<string, DashboardSalesmanRelationshipSalesmanRollup> BySalesPersonId { get; set; }
            = new Dictionary<string, DashboardSalesmanRelationshipSalesmanRollup>(StringComparer.OrdinalIgnoreCase);
    }
}
