using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Models
{
    public class PrincipalInventoryAggregateResult
    {
        public string InventoryValueKpiId { get; set; }

        public string InventoryDaysKpiId { get; set; }

        public DateTime BusinessDate { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<PrincipalInventoryRow> Principals { get; set; }
            = new List<PrincipalInventoryRow>();
    }

    public class PrincipalInventoryRow
    {
        public string InventoryValueKpiId { get; set; }

        public string InventoryDaysKpiId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal InventoryValue { get; set; }

        public decimal? InventoryDays { get; set; }

        public int ItemCount { get; set; }

        public int SortOrder { get; set; }
    }
}
