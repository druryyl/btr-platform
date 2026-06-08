using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardInventoryRiskAggregateResult
    {
        public decimal TotalInventoryValue { get; set; }

        public int TotalItem { get; set; }

        public int DeadStockItemCount { get; set; }

        public decimal DeadStockValue { get; set; }

        public int SlowMovingItemCount { get; set; }

        public decimal SlowMovingValue { get; set; }

        public int NeverSoldItemCount { get; set; }

        public decimal NeverSoldValue { get; set; }

        public decimal AtRiskInventoryValue { get; set; }

        public decimal? AtRiskInventoryPercent { get; set; }

        public bool RequiresAttention { get; set; }

        public DateTime GeneratedAt { get; set; }

        public IList<DashboardInventoryRiskAgingRow> AgingBuckets { get; set; }
            = new List<DashboardInventoryRiskAgingRow>();

        public IList<DashboardInventoryRiskAttentionRow> AttentionList { get; set; }
            = new List<DashboardInventoryRiskAttentionRow>();

        public IList<DashboardInventoryRiskTopRow> TopDead { get; set; }
            = new List<DashboardInventoryRiskTopRow>();

        public IList<DashboardInventoryRiskTopRow> TopSlow { get; set; }
            = new List<DashboardInventoryRiskTopRow>();

        public IList<DashboardInventoryRiskBreakdownRow> Breakdown { get; set; }
            = new List<DashboardInventoryRiskBreakdownRow>();
    }

    public class DashboardInventoryRiskAgingRow
    {
        public string BucketKey { get; set; }

        public string BucketLabel { get; set; }

        public decimal InventoryValue { get; set; }

        public int ItemCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardInventoryRiskAttentionRow
    {
        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string KategoriName { get; set; }

        public string SupplierName { get; set; }

        public int Qty { get; set; }

        public decimal InventoryValue { get; set; }

        public int? DaysSinceLastFaktur { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardInventoryRiskTopRow
    {
        public int Rank { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string KategoriName { get; set; }

        public string SupplierName { get; set; }

        public int Qty { get; set; }

        public decimal InventoryValue { get; set; }

        public int DaysSinceLastFaktur { get; set; }

        public decimal? PercentOfAtRisk { get; set; }
    }

    public class DashboardInventoryRiskBreakdownRow
    {
        public string DimensionType { get; set; }

        public string Name { get; set; }

        public decimal AtRiskValue { get; set; }

        public int ItemCount { get; set; }

        public int Rank { get; set; }

        public decimal? PercentOfAtRisk { get; set; }
    }
}
