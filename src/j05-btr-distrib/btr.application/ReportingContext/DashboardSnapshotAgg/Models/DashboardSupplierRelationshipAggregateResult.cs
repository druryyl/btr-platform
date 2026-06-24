using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardSupplierRelationshipAggregateResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime BusinessDate { get; set; }

        public DateTime GeneratedAt { get; set; }

        public Dictionary<string, DashboardSupplierRelationshipSupplierRollup> BySupplierId { get; set; }
            = new Dictionary<string, DashboardSupplierRelationshipSupplierRollup>(StringComparer.OrdinalIgnoreCase);
    }

    public class DashboardSupplierRelationshipSupplierRollup
    {
        public string SupplierId { get; set; }

        public string SupplierCode { get; set; }

        public List<DashboardSupplierRelationshipCustomerRow> TopCustomers { get; set; }
            = new List<DashboardSupplierRelationshipCustomerRow>();

        public List<DashboardSupplierRelationshipSalesmanRow> TopSalesmen { get; set; }
            = new List<DashboardSupplierRelationshipSalesmanRow>();

        public List<DashboardSupplierRelationshipItemRow> TopItems { get; set; }
            = new List<DashboardSupplierRelationshipItemRow>();
    }

    public class DashboardSupplierRelationshipCustomerRow
    {
        public int Rank { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal MetricValue { get; set; }
    }

    public class DashboardSupplierRelationshipSalesmanRow
    {
        public int Rank { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public decimal MetricValue { get; set; }
    }

    public class DashboardSupplierRelationshipItemRow
    {
        public int Rank { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public decimal MetricValue { get; set; }
    }
}
