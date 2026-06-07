using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardInventoryAggregateResult
    {
        public decimal TotalInventoryValue { get; set; }

        public int TotalItem { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<DashboardInventoryBreakdownRow> Breakdown { get; set; }
            = new List<DashboardInventoryBreakdownRow>();
    }

    public class DashboardInventoryBreakdownRow
    {
        public string DimensionType { get; set; }

        public string Name { get; set; }

        public decimal InventoryValue { get; set; }

        public bool IsTop10 { get; set; }

        public int? Top10Rank { get; set; }
    }
}
