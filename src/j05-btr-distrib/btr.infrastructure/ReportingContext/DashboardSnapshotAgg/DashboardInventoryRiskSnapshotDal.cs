using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardInventoryRiskSnapshotDal : IDashboardInventoryRiskSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardInventoryRiskSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardInventoryRiskAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, TotalInventoryValue, TotalItem,
       DeadStockItemCount, DeadStockValue, SlowMovingItemCount, SlowMovingValue,
       NeverSoldItemCount, NeverSoldValue, AtRiskInventoryValue, AtRiskInventoryPercent,
       RequiresAttention, LastRefreshLogId
FROM BTRPD_InventoryRiskKpi
WHERE SnapshotKey = @SnapshotKey";

            const string agingSql = @"
SELECT BucketKey, BucketLabel, InventoryValue, ItemCount, SortOrder
FROM BTRPD_InventoryRiskAging
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string attentionSql = @"
SELECT BrgId, BrgCode, BrgName, KategoriName, SupplierName, Qty, InventoryValue,
       DaysSinceLastFaktur, SignalKey, SignalLabel, SortOrder
FROM BTRPD_InventoryRiskAttention
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string topDeadSql = @"
SELECT Rank, BrgId, BrgCode, BrgName, KategoriName, SupplierName, Qty, InventoryValue,
       DaysSinceLastFaktur, PercentOfAtRisk
FROM BTRPD_InventoryRiskTopDead
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topSlowSql = @"
SELECT Rank, BrgId, BrgCode, BrgName, KategoriName, SupplierName, Qty, InventoryValue,
       DaysSinceLastFaktur, PercentOfAtRisk
FROM BTRPD_InventoryRiskTopSlow
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string breakdownSql = @"
SELECT DimensionType, Name, AtRiskValue, ItemCount, Rank, PercentOfAtRisk
FROM BTRPD_InventoryRiskBreakdown
WHERE SnapshotKey = @SnapshotKey
ORDER BY DimensionType, Rank";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                var aging = conn.Query<AgingRow>(agingSql, new { SnapshotKey }).ToList();
                var attention = conn.Query<AttentionRow>(attentionSql, new { SnapshotKey }).ToList();
                var topDead = conn.Query<TopRow>(topDeadSql, new { SnapshotKey }).ToList();
                var topSlow = conn.Query<TopRow>(topSlowSql, new { SnapshotKey }).ToList();
                var breakdown = conn.Query<BreakdownRow>(breakdownSql, new { SnapshotKey }).ToList();

                return new DashboardInventoryRiskAggregateResult
                {
                    GeneratedAt = kpi.GeneratedAt,
                    TotalInventoryValue = kpi.TotalInventoryValue,
                    TotalItem = kpi.TotalItem,
                    DeadStockItemCount = kpi.DeadStockItemCount,
                    DeadStockValue = kpi.DeadStockValue,
                    SlowMovingItemCount = kpi.SlowMovingItemCount,
                    SlowMovingValue = kpi.SlowMovingValue,
                    NeverSoldItemCount = kpi.NeverSoldItemCount,
                    NeverSoldValue = kpi.NeverSoldValue,
                    AtRiskInventoryValue = kpi.AtRiskInventoryValue,
                    AtRiskInventoryPercent = kpi.AtRiskInventoryPercent,
                    RequiresAttention = kpi.RequiresAttention,
                    AgingBuckets = aging.Select(MapAging).ToList(),
                    AttentionList = attention.Select(MapAttention).ToList(),
                    TopDead = topDead.Select(MapTop).ToList(),
                    TopSlow = topSlow.Select(MapTop).ToList(),
                    Breakdown = breakdown.Select(MapBreakdown).ToList()
                };
            }
        }

        public void ReplaceCurrent(DashboardInventoryRiskAggregateResult result, string refreshLogId)
        {
            ReplaceCurrent(result, null, refreshLogId);
        }

        public void ReplaceCurrent(
            DashboardInventoryRiskAggregateResult result,
            DashboardInventoryForecastAggregateResult forecast,
            string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        ReplaceCurrentCore(conn, transaction, result, refreshLogId);
                        if (forecast != null)
                            ReplaceForecastCore(conn, transaction, forecast, refreshLogId);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void ReplaceCurrentCore(
            SqlConnection conn,
            SqlTransaction transaction,
            DashboardInventoryRiskAggregateResult result,
            string refreshLogId)
        {
            conn.Execute(
                "DELETE FROM BTRPD_InventoryRiskAging WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_InventoryRiskAttention WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_InventoryRiskTopDead WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_InventoryRiskTopSlow WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_InventoryRiskBreakdown WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);

            const string mergeKpiSql = @"
MERGE BTRPD_InventoryRiskKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        TotalInventoryValue = @TotalInventoryValue,
        TotalItem = @TotalItem,
        DeadStockItemCount = @DeadStockItemCount,
        DeadStockValue = @DeadStockValue,
        SlowMovingItemCount = @SlowMovingItemCount,
        SlowMovingValue = @SlowMovingValue,
        NeverSoldItemCount = @NeverSoldItemCount,
        NeverSoldValue = @NeverSoldValue,
        AtRiskInventoryValue = @AtRiskInventoryValue,
        AtRiskInventoryPercent = @AtRiskInventoryPercent,
        RequiresAttention = @RequiresAttention,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, TotalInventoryValue, TotalItem,
        DeadStockItemCount, DeadStockValue, SlowMovingItemCount, SlowMovingValue,
        NeverSoldItemCount, NeverSoldValue, AtRiskInventoryValue, AtRiskInventoryPercent,
        RequiresAttention, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @TotalInventoryValue, @TotalItem,
        @DeadStockItemCount, @DeadStockValue, @SlowMovingItemCount, @SlowMovingValue,
        @NeverSoldItemCount, @NeverSoldValue, @AtRiskInventoryValue, @AtRiskInventoryPercent,
        @RequiresAttention, @LastRefreshLogId);";

            conn.Execute(mergeKpiSql, new
            {
                SnapshotKey,
                result.GeneratedAt,
                result.TotalInventoryValue,
                result.TotalItem,
                result.DeadStockItemCount,
                result.DeadStockValue,
                result.SlowMovingItemCount,
                result.SlowMovingValue,
                result.NeverSoldItemCount,
                result.NeverSoldValue,
                result.AtRiskInventoryValue,
                result.AtRiskInventoryPercent,
                result.RequiresAttention,
                LastRefreshLogId = refreshLogId ?? string.Empty
            }, transaction);

            const string insertAgingSql = @"
INSERT INTO BTRPD_InventoryRiskAging (
    InventoryRiskAgingId, SnapshotKey, BucketKey, BucketLabel, InventoryValue, ItemCount, SortOrder)
VALUES (
    @InventoryRiskAgingId, @SnapshotKey, @BucketKey, @BucketLabel, @InventoryValue, @ItemCount, @SortOrder)";

            foreach (var row in result.AgingBuckets ?? new List<DashboardInventoryRiskAgingRow>())
            {
                conn.Execute(insertAgingSql, new
                {
                    InventoryRiskAgingId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    BucketKey = row.BucketKey ?? string.Empty,
                    BucketLabel = row.BucketLabel ?? string.Empty,
                    row.InventoryValue,
                    row.ItemCount,
                    row.SortOrder
                }, transaction);
            }

            const string insertAttentionSql = @"
INSERT INTO BTRPD_InventoryRiskAttention (
    InventoryRiskAttentionId, SnapshotKey, BrgId, BrgCode, BrgName, KategoriName, SupplierName,
    Qty, InventoryValue, DaysSinceLastFaktur, SignalKey, SignalLabel, SortOrder)
VALUES (
    @InventoryRiskAttentionId, @SnapshotKey, @BrgId, @BrgCode, @BrgName, @KategoriName, @SupplierName,
    @Qty, @InventoryValue, @DaysSinceLastFaktur, @SignalKey, @SignalLabel, @SortOrder)";

            foreach (var row in result.AttentionList ?? new List<DashboardInventoryRiskAttentionRow>())
            {
                conn.Execute(insertAttentionSql, new
                {
                    InventoryRiskAttentionId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    BrgId = row.BrgId ?? string.Empty,
                    BrgCode = row.BrgCode ?? string.Empty,
                    BrgName = row.BrgName ?? string.Empty,
                    KategoriName = row.KategoriName ?? string.Empty,
                    SupplierName = row.SupplierName ?? string.Empty,
                    row.Qty,
                    row.InventoryValue,
                    row.DaysSinceLastFaktur,
                    SignalKey = row.SignalKey ?? string.Empty,
                    SignalLabel = row.SignalLabel ?? string.Empty,
                    row.SortOrder
                }, transaction);
            }

            const string insertTopDeadSql = @"
INSERT INTO BTRPD_InventoryRiskTopDead (
    InventoryRiskTopDeadId, SnapshotKey, Rank, BrgId, BrgCode, BrgName, KategoriName, SupplierName,
    Qty, InventoryValue, DaysSinceLastFaktur, PercentOfAtRisk)
VALUES (
    @InventoryRiskTopDeadId, @SnapshotKey, @Rank, @BrgId, @BrgCode, @BrgName, @KategoriName, @SupplierName,
    @Qty, @InventoryValue, @DaysSinceLastFaktur, @PercentOfAtRisk)";

            foreach (var row in result.TopDead ?? new List<DashboardInventoryRiskTopRow>())
            {
                conn.Execute(insertTopDeadSql, new
                {
                    InventoryRiskTopDeadId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    BrgId = row.BrgId ?? string.Empty,
                    BrgCode = row.BrgCode ?? string.Empty,
                    BrgName = row.BrgName ?? string.Empty,
                    KategoriName = row.KategoriName ?? string.Empty,
                    SupplierName = row.SupplierName ?? string.Empty,
                    row.Qty,
                    row.InventoryValue,
                    row.DaysSinceLastFaktur,
                    row.PercentOfAtRisk
                }, transaction);
            }

            const string insertTopSlowSql = @"
INSERT INTO BTRPD_InventoryRiskTopSlow (
    InventoryRiskTopSlowId, SnapshotKey, Rank, BrgId, BrgCode, BrgName, KategoriName, SupplierName,
    Qty, InventoryValue, DaysSinceLastFaktur, PercentOfAtRisk)
VALUES (
    @InventoryRiskTopSlowId, @SnapshotKey, @Rank, @BrgId, @BrgCode, @BrgName, @KategoriName, @SupplierName,
    @Qty, @InventoryValue, @DaysSinceLastFaktur, @PercentOfAtRisk)";

            foreach (var row in result.TopSlow ?? new List<DashboardInventoryRiskTopRow>())
            {
                conn.Execute(insertTopSlowSql, new
                {
                    InventoryRiskTopSlowId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    BrgId = row.BrgId ?? string.Empty,
                    BrgCode = row.BrgCode ?? string.Empty,
                    BrgName = row.BrgName ?? string.Empty,
                    KategoriName = row.KategoriName ?? string.Empty,
                    SupplierName = row.SupplierName ?? string.Empty,
                    row.Qty,
                    row.InventoryValue,
                    row.DaysSinceLastFaktur,
                    row.PercentOfAtRisk
                }, transaction);
            }

            const string insertBreakdownSql = @"
INSERT INTO BTRPD_InventoryRiskBreakdown (
    InventoryRiskBreakdownId, SnapshotKey, DimensionType, Name, AtRiskValue, ItemCount, Rank, PercentOfAtRisk)
VALUES (
    @InventoryRiskBreakdownId, @SnapshotKey, @DimensionType, @Name, @AtRiskValue, @ItemCount, @Rank, @PercentOfAtRisk)";

            foreach (var row in result.Breakdown ?? new List<DashboardInventoryRiskBreakdownRow>())
            {
                conn.Execute(insertBreakdownSql, new
                {
                    InventoryRiskBreakdownId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    DimensionType = row.DimensionType ?? string.Empty,
                    Name = row.Name ?? string.Empty,
                    row.AtRiskValue,
                    row.ItemCount,
                    row.Rank,
                    row.PercentOfAtRisk
                }, transaction);
            }
        }

        private void ReplaceForecastCore(
            SqlConnection conn,
            SqlTransaction transaction,
            DashboardInventoryForecastAggregateResult forecast,
            string refreshLogId)
        {
            conn.Execute(
                "DELETE FROM BTRPD_InventoryForecastDailyConsumption WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_InventoryForecastLevel WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_InventoryForecastRisk WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_InventoryForecastRecommendation WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);

            const string mergeForecastKpiSql = @"
MERGE BTRPD_InventoryForecastKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        BusinessDate = @BusinessDate,
        PlanningHorizonDays = @PlanningHorizonDays,
        CurrentInventoryValue = @CurrentInventoryValue,
        ProjectedInventoryValue = @ProjectedInventoryValue,
        BestCaseProjectedValue = @BestCaseProjectedValue,
        WorstCaseProjectedValue = @WorstCaseProjectedValue,
        AverageDailyConsumptionUnits = @AverageDailyConsumptionUnits,
        WeightedAverageDaysOfSupply = @WeightedAverageDaysOfSupply,
        UnderstockValue = @UnderstockValue,
        OverstockValue = @OverstockValue,
        StockOutRiskItemCount = @StockOutRiskItemCount,
        InventoryCoveragePercent = @InventoryCoveragePercent,
        InventoryTurnoverForecast = @InventoryTurnoverForecast,
        InventoryHealthScore = @InventoryHealthScore,
        ForecastConfidence = @ForecastConfidence,
        AtRiskInventoryPercent = @AtRiskInventoryPercent,
        ForecastConsumptionUnits = @ForecastConsumptionUnits,
        HeatCellLowLow = @HeatCellLowLow,
        HeatCellLowMed = @HeatCellLowMed,
        HeatCellLowHigh = @HeatCellLowHigh,
        HeatCellMedLow = @HeatCellMedLow,
        HeatCellMedMed = @HeatCellMedMed,
        HeatCellMedHigh = @HeatCellMedHigh,
        HeatCellHighLow = @HeatCellHighLow,
        HeatCellHighMed = @HeatCellHighMed,
        HeatCellHighHigh = @HeatCellHighHigh,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, BusinessDate, PlanningHorizonDays,
        CurrentInventoryValue, ProjectedInventoryValue, BestCaseProjectedValue, WorstCaseProjectedValue,
        AverageDailyConsumptionUnits, WeightedAverageDaysOfSupply, UnderstockValue, OverstockValue,
        StockOutRiskItemCount, InventoryCoveragePercent, InventoryTurnoverForecast, InventoryHealthScore,
        ForecastConfidence, AtRiskInventoryPercent, ForecastConsumptionUnits,
        HeatCellLowLow, HeatCellLowMed, HeatCellLowHigh, HeatCellMedLow, HeatCellMedMed, HeatCellMedHigh,
        HeatCellHighLow, HeatCellHighMed, HeatCellHighHigh, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @BusinessDate, @PlanningHorizonDays,
        @CurrentInventoryValue, @ProjectedInventoryValue, @BestCaseProjectedValue, @WorstCaseProjectedValue,
        @AverageDailyConsumptionUnits, @WeightedAverageDaysOfSupply, @UnderstockValue, @OverstockValue,
        @StockOutRiskItemCount, @InventoryCoveragePercent, @InventoryTurnoverForecast, @InventoryHealthScore,
        @ForecastConfidence, @AtRiskInventoryPercent, @ForecastConsumptionUnits,
        @HeatCellLowLow, @HeatCellLowMed, @HeatCellLowHigh, @HeatCellMedLow, @HeatCellMedMed, @HeatCellMedHigh,
        @HeatCellHighLow, @HeatCellHighMed, @HeatCellHighHigh, @LastRefreshLogId);";

            conn.Execute(mergeForecastKpiSql, new
            {
                SnapshotKey,
                forecast.GeneratedAt,
                forecast.BusinessDate,
                forecast.PlanningHorizonDays,
                forecast.CurrentInventoryValue,
                forecast.ProjectedInventoryValue,
                forecast.BestCaseProjectedValue,
                forecast.WorstCaseProjectedValue,
                forecast.AverageDailyConsumptionUnits,
                forecast.WeightedAverageDaysOfSupply,
                forecast.UnderstockValue,
                forecast.OverstockValue,
                forecast.StockOutRiskItemCount,
                forecast.InventoryCoveragePercent,
                forecast.InventoryTurnoverForecast,
                forecast.InventoryHealthScore,
                ForecastConfidence = forecast.ForecastConfidence ?? string.Empty,
                forecast.AtRiskInventoryPercent,
                forecast.ForecastConsumptionUnits,
                forecast.HeatCellLowLow,
                forecast.HeatCellLowMed,
                forecast.HeatCellLowHigh,
                forecast.HeatCellMedLow,
                forecast.HeatCellMedMed,
                forecast.HeatCellMedHigh,
                forecast.HeatCellHighLow,
                forecast.HeatCellHighMed,
                forecast.HeatCellHighHigh,
                LastRefreshLogId = refreshLogId ?? string.Empty
            }, transaction);

            const string insertDailySql = @"
INSERT INTO BTRPD_InventoryForecastDailyConsumption (
    InventoryForecastDailyConsumptionId, SnapshotKey, ConsumptionDate, DayIndex, UnitsSold, AdcReference)
VALUES (
    @InventoryForecastDailyConsumptionId, @SnapshotKey, @ConsumptionDate, @DayIndex, @UnitsSold, @AdcReference)";

            foreach (var row in forecast.DailyConsumption ?? new List<DashboardInventoryForecastDailyConsumptionRow>())
            {
                conn.Execute(insertDailySql, new
                {
                    InventoryForecastDailyConsumptionId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.ConsumptionDate,
                    row.DayIndex,
                    row.UnitsSold,
                    row.AdcReference
                }, transaction);
            }

            const string insertLevelSql = @"
INSERT INTO BTRPD_InventoryForecastLevel (
    InventoryForecastLevelId, SnapshotKey, HorizonDay, ProjectedInventoryValue)
VALUES (
    @InventoryForecastLevelId, @SnapshotKey, @HorizonDay, @ProjectedInventoryValue)";

            foreach (var row in forecast.ProjectedLevel ?? new List<DashboardInventoryForecastLevelRow>())
            {
                conn.Execute(insertLevelSql, new
                {
                    InventoryForecastLevelId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.HorizonDay,
                    row.ProjectedInventoryValue
                }, transaction);
            }

            const string insertRiskSql = @"
INSERT INTO BTRPD_InventoryForecastRisk (
    InventoryForecastRiskId, SnapshotKey, SortOrder, SignalKey, SignalLabel,
    BrgId, BrgCode, BrgName, SupplierName, DaysOfSupply, StockOutDate,
    ValueAmount, Urgency, RuleExplanation, ReportRoute, EntityCode)
VALUES (
    @InventoryForecastRiskId, @SnapshotKey, @SortOrder, @SignalKey, @SignalLabel,
    @BrgId, @BrgCode, @BrgName, @SupplierName, @DaysOfSupply, @StockOutDate,
    @ValueAmount, @Urgency, @RuleExplanation, @ReportRoute, @EntityCode)";

            foreach (var row in forecast.TopRisks ?? new List<DashboardInventoryForecastRiskRow>())
            {
                conn.Execute(insertRiskSql, new
                {
                    InventoryForecastRiskId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.SortOrder,
                    SignalKey = row.SignalKey ?? string.Empty,
                    SignalLabel = row.SignalLabel ?? string.Empty,
                    BrgId = row.BrgId ?? string.Empty,
                    BrgCode = row.BrgCode ?? string.Empty,
                    BrgName = row.BrgName ?? string.Empty,
                    SupplierName = row.SupplierName ?? string.Empty,
                    row.DaysOfSupply,
                    row.StockOutDate,
                    row.ValueAmount,
                    Urgency = row.Urgency ?? string.Empty,
                    RuleExplanation = row.RuleExplanation ?? string.Empty,
                    ReportRoute = row.ReportRoute ?? string.Empty,
                    EntityCode = row.EntityCode ?? string.Empty
                }, transaction);
            }

            const string insertRecommendationSql = @"
INSERT INTO BTRPD_InventoryForecastRecommendation (
    InventoryForecastRecommendationId, SnapshotKey, SortOrder, BrgId, BrgCode, BrgName, SupplierName,
    ReorderDate, RecommendedPurchaseQty, AverageDailyConsumption, CurrentQty, DaysOfSupply,
    Urgency, ReportRoute, EntityCode)
VALUES (
    @InventoryForecastRecommendationId, @SnapshotKey, @SortOrder, @BrgId, @BrgCode, @BrgName, @SupplierName,
    @ReorderDate, @RecommendedPurchaseQty, @AverageDailyConsumption, @CurrentQty, @DaysOfSupply,
    @Urgency, @ReportRoute, @EntityCode)";

            foreach (var row in forecast.PurchaseRecommendations ?? new List<DashboardInventoryForecastRecommendationRow>())
            {
                conn.Execute(insertRecommendationSql, new
                {
                    InventoryForecastRecommendationId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.SortOrder,
                    BrgId = row.BrgId ?? string.Empty,
                    BrgCode = row.BrgCode ?? string.Empty,
                    BrgName = row.BrgName ?? string.Empty,
                    SupplierName = row.SupplierName ?? string.Empty,
                    row.ReorderDate,
                    row.RecommendedPurchaseQty,
                    row.AverageDailyConsumption,
                    row.CurrentQty,
                    row.DaysOfSupply,
                    Urgency = row.Urgency ?? string.Empty,
                    ReportRoute = row.ReportRoute ?? string.Empty,
                    EntityCode = row.EntityCode ?? string.Empty
                }, transaction);
            }
        }

        private static DashboardInventoryRiskAgingRow MapAging(AgingRow row) =>
            new DashboardInventoryRiskAgingRow
            {
                BucketKey = row.BucketKey,
                BucketLabel = row.BucketLabel,
                InventoryValue = row.InventoryValue,
                ItemCount = row.ItemCount,
                SortOrder = row.SortOrder
            };

        private static DashboardInventoryRiskAttentionRow MapAttention(AttentionRow row) =>
            new DashboardInventoryRiskAttentionRow
            {
                BrgId = row.BrgId,
                BrgCode = row.BrgCode,
                BrgName = row.BrgName,
                KategoriName = row.KategoriName,
                SupplierName = row.SupplierName,
                Qty = row.Qty,
                InventoryValue = row.InventoryValue,
                DaysSinceLastFaktur = row.DaysSinceLastFaktur,
                SignalKey = row.SignalKey,
                SignalLabel = row.SignalLabel,
                SortOrder = row.SortOrder
            };

        private static DashboardInventoryRiskTopRow MapTop(TopRow row) =>
            new DashboardInventoryRiskTopRow
            {
                Rank = row.Rank,
                BrgId = row.BrgId,
                BrgCode = row.BrgCode,
                BrgName = row.BrgName,
                KategoriName = row.KategoriName,
                SupplierName = row.SupplierName,
                Qty = row.Qty,
                InventoryValue = row.InventoryValue,
                DaysSinceLastFaktur = row.DaysSinceLastFaktur,
                PercentOfAtRisk = row.PercentOfAtRisk
            };

        private static DashboardInventoryRiskBreakdownRow MapBreakdown(BreakdownRow row) =>
            new DashboardInventoryRiskBreakdownRow
            {
                DimensionType = row.DimensionType,
                Name = row.Name,
                AtRiskValue = row.AtRiskValue,
                ItemCount = row.ItemCount,
                Rank = row.Rank,
                PercentOfAtRisk = row.PercentOfAtRisk
            };

        private sealed class KpiRow
        {
            public DateTime GeneratedAt { get; set; }
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
        }

        private sealed class AgingRow
        {
            public string BucketKey { get; set; }
            public string BucketLabel { get; set; }
            public decimal InventoryValue { get; set; }
            public int ItemCount { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class AttentionRow
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

        private sealed class TopRow
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

        private sealed class BreakdownRow
        {
            public string DimensionType { get; set; }
            public string Name { get; set; }
            public decimal AtRiskValue { get; set; }
            public int ItemCount { get; set; }
            public int Rank { get; set; }
            public decimal? PercentOfAtRisk { get; set; }
        }
    }
}
