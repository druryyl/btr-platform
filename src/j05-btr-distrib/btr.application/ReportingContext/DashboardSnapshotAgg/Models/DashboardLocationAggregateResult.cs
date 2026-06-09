using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardLocationAggregateResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public decimal? Top1WarehouseInventoryPercent { get; set; }

        public decimal? Top3WarehouseInventoryPercent { get; set; }

        public decimal? Top1WarehouseAtRiskPercent { get; set; }

        public decimal? Top1WarehouseSalesPercent { get; set; }

        public decimal? Top1WilayahSalesPercent { get; set; }

        public int InactiveWarehouseWithStockCount { get; set; }

        public int WarehouseNoSalesWithInventoryCount { get; set; }

        public decimal TotalInventoryValue { get; set; }

        public decimal TotalAtRiskValue { get; set; }

        public decimal TotalOmzet { get; set; }

        public decimal TotalPurchase { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<DashboardLocationTopWarehouseInventoryRow> TopWarehouseInventory { get; set; }
            = new List<DashboardLocationTopWarehouseInventoryRow>();

        public List<DashboardLocationTopWarehouseAtRiskRow> TopWarehouseAtRisk { get; set; }
            = new List<DashboardLocationTopWarehouseAtRiskRow>();

        public List<DashboardLocationTopWarehouseSalesRow> TopWarehouseSales { get; set; }
            = new List<DashboardLocationTopWarehouseSalesRow>();

        public List<DashboardLocationTopWarehousePurchasingRow> TopWarehousePurchasing { get; set; }
            = new List<DashboardLocationTopWarehousePurchasingRow>();

        public List<DashboardLocationTopWilayahSalesRow> TopWilayahSales { get; set; }
            = new List<DashboardLocationTopWilayahSalesRow>();

        public List<DashboardLocationAttentionRow> AttentionList { get; set; }
            = new List<DashboardLocationAttentionRow>();
    }

    public class DashboardLocationTopWarehouseInventoryRow
    {
        public int Rank { get; set; }

        public string WarehouseId { get; set; }

        public string WarehouseName { get; set; }

        public decimal InventoryValue { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public string ReportRoute { get; set; }
    }

    public class DashboardLocationTopWarehouseAtRiskRow
    {
        public int Rank { get; set; }

        public string WarehouseId { get; set; }

        public string WarehouseName { get; set; }

        public decimal AtRiskValue { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public string ReportRoute { get; set; }
    }

    public class DashboardLocationTopWarehouseSalesRow
    {
        public int Rank { get; set; }

        public string WarehouseId { get; set; }

        public string WarehouseName { get; set; }

        public decimal MtdOmzet { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public string ReportRoute { get; set; }
    }

    public class DashboardLocationTopWarehousePurchasingRow
    {
        public int Rank { get; set; }

        public string WarehouseId { get; set; }

        public string WarehouseName { get; set; }

        public decimal MtdPurchaseAmount { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public string ReportRoute { get; set; }
    }

    public class DashboardLocationTopWilayahSalesRow
    {
        public int Rank { get; set; }

        public string WilayahId { get; set; }

        public string WilayahName { get; set; }

        public decimal MtdOmzet { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public string DashboardRoute { get; set; }
    }

    public class DashboardLocationAttentionRow
    {
        public string EntityType { get; set; }

        public string EntityCode { get; set; }

        public string EntityName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public decimal? ValueAmount { get; set; }

        public string ValueText { get; set; }

        public string ReportRoute { get; set; }

        public int SortOrder { get; set; }
    }
}
