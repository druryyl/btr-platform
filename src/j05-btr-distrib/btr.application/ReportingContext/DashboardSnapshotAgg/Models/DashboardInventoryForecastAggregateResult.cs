using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardInventoryForecastAggregateResult
    {
        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public int PlanningHorizonDays { get; set; }

        public decimal CurrentInventoryValue { get; set; }

        public decimal ProjectedInventoryValue { get; set; }

        public decimal BestCaseProjectedValue { get; set; }

        public decimal WorstCaseProjectedValue { get; set; }

        public decimal AverageDailyConsumptionUnits { get; set; }

        public decimal? WeightedAverageDaysOfSupply { get; set; }

        public decimal UnderstockValue { get; set; }

        public decimal OverstockValue { get; set; }

        public int StockOutRiskItemCount { get; set; }

        public decimal? InventoryCoveragePercent { get; set; }

        public decimal? InventoryTurnoverForecast { get; set; }

        public int InventoryHealthScore { get; set; }

        public string ForecastConfidence { get; set; }

        public decimal? AtRiskInventoryPercent { get; set; }

        public decimal ForecastConsumptionUnits { get; set; }

        public int HeatCellLowLow { get; set; }

        public int HeatCellLowMed { get; set; }

        public int HeatCellLowHigh { get; set; }

        public int HeatCellMedLow { get; set; }

        public int HeatCellMedMed { get; set; }

        public int HeatCellMedHigh { get; set; }

        public int HeatCellHighLow { get; set; }

        public int HeatCellHighMed { get; set; }

        public int HeatCellHighHigh { get; set; }

        public List<DashboardInventoryForecastDailyConsumptionRow> DailyConsumption { get; set; }
            = new List<DashboardInventoryForecastDailyConsumptionRow>();

        public List<DashboardInventoryForecastLevelRow> ProjectedLevel { get; set; }
            = new List<DashboardInventoryForecastLevelRow>();

        public List<DashboardInventoryForecastRiskRow> TopRisks { get; set; }
            = new List<DashboardInventoryForecastRiskRow>();

        public List<DashboardInventoryForecastRecommendationRow> PurchaseRecommendations { get; set; }
            = new List<DashboardInventoryForecastRecommendationRow>();

        public List<ForecastItemContext> ItemContexts { get; set; }
            = new List<ForecastItemContext>();
    }

    public class DashboardInventoryForecastDailyConsumptionRow
    {
        public DateTime ConsumptionDate { get; set; }

        public int DayIndex { get; set; }

        public decimal UnitsSold { get; set; }

        public decimal AdcReference { get; set; }
    }

    public class DashboardInventoryForecastLevelRow
    {
        public int HorizonDay { get; set; }

        public decimal ProjectedInventoryValue { get; set; }
    }

    public class DashboardInventoryForecastRiskRow
    {
        public int SortOrder { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string SupplierName { get; set; }

        public decimal? DaysOfSupply { get; set; }

        public DateTime? StockOutDate { get; set; }

        public decimal ValueAmount { get; set; }

        public string Urgency { get; set; }

        public string RuleExplanation { get; set; }

        public string ReportRoute { get; set; }

        public string EntityCode { get; set; }
    }

    public class DashboardInventoryForecastRecommendationRow
    {
        public int SortOrder { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string SupplierName { get; set; }

        public DateTime? ReorderDate { get; set; }

        public decimal RecommendedPurchaseQty { get; set; }

        public decimal AverageDailyConsumption { get; set; }

        public decimal CurrentQty { get; set; }

        public decimal? DaysOfSupply { get; set; }

        public string Urgency { get; set; }

        public string ReportRoute { get; set; }

        public string EntityCode { get; set; }
    }
}
