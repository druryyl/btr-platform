using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardInventoryForecastAgg.Contracts;
using btr.application.ReportingContext.DashboardInventoryForecastAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardInventoryForecastAgg
{
    public class DashboardInventoryForecastDal : IDashboardInventoryForecastDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardInventoryForecastDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardInventoryForecastResponse GetSummary()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, BusinessDate, PlanningHorizonDays,
       CurrentInventoryValue, ProjectedInventoryValue, BestCaseProjectedValue, WorstCaseProjectedValue,
       AverageDailyConsumptionUnits, WeightedAverageDaysOfSupply, UnderstockValue, OverstockValue,
       StockOutRiskItemCount, InventoryCoveragePercent, InventoryTurnoverForecast, InventoryHealthScore,
       ForecastConfidence, AtRiskInventoryPercent, ForecastConsumptionUnits,
       HeatCellLowLow, HeatCellLowMed, HeatCellLowHigh, HeatCellMedLow, HeatCellMedMed, HeatCellMedHigh,
       HeatCellHighLow, HeatCellHighMed, HeatCellHighHigh
FROM BTRPD_InventoryForecastKpi
WHERE SnapshotKey = @SnapshotKey";

            const string dailySql = @"
SELECT ConsumptionDate, DayIndex, UnitsSold, AdcReference
FROM BTRPD_InventoryForecastDailyConsumption
WHERE SnapshotKey = @SnapshotKey
ORDER BY ConsumptionDate";

            const string levelSql = @"
SELECT HorizonDay, ProjectedInventoryValue
FROM BTRPD_InventoryForecastLevel
WHERE SnapshotKey = @SnapshotKey
ORDER BY HorizonDay";

            const string riskSql = @"
SELECT SortOrder, SignalKey, SignalLabel, BrgId, BrgCode, BrgName, SupplierName,
       DaysOfSupply, StockOutDate, ValueAmount, Urgency, RuleExplanation, ReportRoute, EntityCode
FROM BTRPD_InventoryForecastRisk
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string recommendationSql = @"
SELECT SortOrder, BrgId, BrgCode, BrgName, SupplierName, ReorderDate,
       RecommendedPurchaseQty, AverageDailyConsumption, CurrentQty, DaysOfSupply,
       Urgency, ReportRoute, EntityCode
FROM BTRPD_InventoryForecastRecommendation
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<ForecastKpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                {
                    return new DashboardInventoryForecastResponse
                    {
                        IsAvailable = false,
                        ExecutiveSummary = "Inventory forecast data not yet available."
                    };
                }

                var dailyRows = conn.Query<DailyRow>(dailySql, new { SnapshotKey }).ToList();
                var levelRows = conn.Query<LevelRow>(levelSql, new { SnapshotKey }).ToList();
                var riskRows = conn.Query<RiskRow>(riskSql, new { SnapshotKey }).ToList();
                var recommendationRows = conn.Query<RecommendationRow>(recommendationSql, new { SnapshotKey }).ToList();

                var response = new DashboardInventoryForecastResponse
                {
                    IsAvailable = true,
                    GeneratedAt = kpi.GeneratedAt,
                    BusinessDate = kpi.BusinessDate,
                    PlanningHorizonDays = kpi.PlanningHorizonDays,
                    CurrentInventoryValue = kpi.CurrentInventoryValue,
                    ProjectedInventoryValue = kpi.ProjectedInventoryValue,
                    BestCaseProjectedValue = kpi.BestCaseProjectedValue,
                    WorstCaseProjectedValue = kpi.WorstCaseProjectedValue,
                    AverageDailyConsumptionUnits = kpi.AverageDailyConsumptionUnits,
                    WeightedAverageDaysOfSupply = kpi.WeightedAverageDaysOfSupply,
                    UnderstockValue = kpi.UnderstockValue,
                    OverstockValue = kpi.OverstockValue,
                    StockOutRiskItemCount = kpi.StockOutRiskItemCount,
                    InventoryCoveragePercent = kpi.InventoryCoveragePercent,
                    InventoryTurnoverForecast = kpi.InventoryTurnoverForecast,
                    InventoryHealthScore = kpi.InventoryHealthScore,
                    ForecastConfidence = kpi.ForecastConfidence ?? string.Empty,
                    AtRiskInventoryPercent = kpi.AtRiskInventoryPercent,
                    ForecastConsumptionUnits = kpi.ForecastConsumptionUnits,
                    DailyConsumption = dailyRows.Select(r => new DashboardInventoryForecastDailyConsumptionItem
                    {
                        ConsumptionDate = r.ConsumptionDate,
                        DayIndex = r.DayIndex,
                        UnitsSold = r.UnitsSold,
                        AdcReference = r.AdcReference
                    }).ToList(),
                    ProjectedLevel = levelRows.Select(r => new DashboardInventoryForecastLevelItem
                    {
                        HorizonDay = r.HorizonDay,
                        ProjectedInventoryValue = r.ProjectedInventoryValue
                    }).ToList(),
                    HeatSummary = BuildHeatSummary(kpi),
                    TopRisks = riskRows.Select(r => new DashboardInventoryForecastRiskItem
                    {
                        SortOrder = r.SortOrder,
                        SignalKey = r.SignalKey,
                        SignalLabel = r.SignalLabel,
                        BrgId = r.BrgId,
                        BrgCode = r.BrgCode,
                        BrgName = r.BrgName,
                        SupplierName = r.SupplierName,
                        DaysOfSupply = r.DaysOfSupply,
                        StockOutDate = r.StockOutDate,
                        ValueAmount = r.ValueAmount,
                        Urgency = r.Urgency,
                        DosSeverity = InventoryForecastPolicy.ResolveDosSeverity(r.DaysOfSupply),
                        RuleExplanation = r.RuleExplanation,
                        ReportRoute = r.ReportRoute,
                        EntityCode = r.EntityCode
                    }).ToList(),
                    PurchaseRecommendations = recommendationRows.Select(r => new DashboardInventoryForecastRecommendationItem
                    {
                        SortOrder = r.SortOrder,
                        BrgId = r.BrgId,
                        BrgCode = r.BrgCode,
                        BrgName = r.BrgName,
                        SupplierName = r.SupplierName,
                        ReorderDate = r.ReorderDate,
                        RecommendedPurchaseQty = r.RecommendedPurchaseQty,
                        AverageDailyConsumption = r.AverageDailyConsumption,
                        CurrentQty = r.CurrentQty,
                        DaysOfSupply = r.DaysOfSupply,
                        Urgency = r.Urgency,
                        ReportRoute = r.ReportRoute,
                        EntityCode = r.EntityCode
                    }).ToList()
                };

                response.ExecutiveSummary = InventoryForecastExecutiveSummaryBuilder.Build(response);
                response.Traceability = new DashboardInventoryForecastTraceability();
                return response;
            }
        }

        private static List<DashboardInventoryForecastHeatCellItem> BuildHeatSummary(ForecastKpiRow kpi)
        {
            var dosBands = new[] { "Low DOS (≤14d)", "Med DOS (15–60d)", "High DOS (>60d)" };
            var valueBands = new[] { "Low Value", "Med Value", "High Value" };
            var counts = new[]
            {
                kpi.HeatCellLowLow, kpi.HeatCellLowMed, kpi.HeatCellLowHigh,
                kpi.HeatCellMedLow, kpi.HeatCellMedMed, kpi.HeatCellMedHigh,
                kpi.HeatCellHighLow, kpi.HeatCellHighMed, kpi.HeatCellHighHigh
            };

            var cells = new List<DashboardInventoryForecastHeatCellItem>();
            for (var i = 0; i < counts.Length; i++)
            {
                cells.Add(new DashboardInventoryForecastHeatCellItem
                {
                    DosBand = dosBands[i / 3],
                    ValueBand = valueBands[i % 3],
                    ItemCount = counts[i]
                });
            }

            return cells;
        }

        private sealed class ForecastKpiRow
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
        }

        private sealed class DailyRow
        {
            public DateTime ConsumptionDate { get; set; }
            public int DayIndex { get; set; }
            public decimal UnitsSold { get; set; }
            public decimal AdcReference { get; set; }
        }

        private sealed class LevelRow
        {
            public int HorizonDay { get; set; }
            public decimal ProjectedInventoryValue { get; set; }
        }

        private sealed class RiskRow
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

        private sealed class RecommendationRow
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
}
