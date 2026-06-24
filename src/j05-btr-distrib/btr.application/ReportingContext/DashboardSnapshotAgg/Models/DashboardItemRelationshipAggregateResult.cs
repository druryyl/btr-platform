using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardItemRelationshipAggregateResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime BusinessDate { get; set; }

        public DateTime GeneratedAt { get; set; }

        public Dictionary<string, DashboardItemRelationshipItemRollup> ByBrgId { get; set; }
            = new Dictionary<string, DashboardItemRelationshipItemRollup>(StringComparer.OrdinalIgnoreCase);
    }

    public class DashboardItemRelationshipItemRollup
    {
        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string SupplierId { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }

        public List<DashboardItemRelationshipCustomerRow> TopCustomers { get; set; }
            = new List<DashboardItemRelationshipCustomerRow>();

        public List<DashboardItemRelationshipSalesmanRow> TopSalesmen { get; set; }
            = new List<DashboardItemRelationshipSalesmanRow>();
    }

    public class DashboardItemRelationshipCustomerRow
    {
        public int Rank { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal MetricValue { get; set; }
    }

    public class DashboardItemRelationshipSalesmanRow
    {
        public int Rank { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public decimal MetricValue { get; set; }
    }
}
