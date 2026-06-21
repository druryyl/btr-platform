using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.application.ReportingContext.Shared;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;
using System;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardSalesSnapshotDal : IDashboardSalesSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardSalesSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardSalesAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth,
       TotalOmzet, TotalFaktur, TotalCustomer, TotalTarget, TotalAchievement,
       AchievementPercent, CompletedOmzet, PipelineOmzet, LastRefreshLogId
FROM BTRPD_SalesKpi
WHERE SnapshotKey = @SnapshotKey";

            const string weekTrendSql = @"
SELECT WeekStart, WeekEnd, WeekLabel, RecognizedAmount
FROM BTRPD_SalesWeekTrend
WHERE SnapshotKey = @SnapshotKey
ORDER BY WeekStart";

            const string topSalesmanSql = @"
SELECT Rank, SalesPersonName, CompletedOmzet
FROM BTRPD_SalesTopSalesman
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                var weekRows = conn.Query<WeekTrendRow>(weekTrendSql, new { SnapshotKey }).ToList();
                var topRows = conn.Query<TopSalesmanRow>(topSalesmanSql, new { SnapshotKey }).ToList();

                return new DashboardSalesAggregateResult
                {
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    TotalOmzet = kpi.TotalOmzet,
                    CompletedOmzet = kpi.CompletedOmzet,
                    PipelineOmzet = kpi.PipelineOmzet,
                    TotalFaktur = kpi.TotalFaktur,
                    TotalCustomer = kpi.TotalCustomer,
                    GeneratedAt = kpi.GeneratedAt,
                    TotalTarget = kpi.TotalTarget,
                    TotalAchievement = kpi.TotalAchievement,
                    AchievementPercent = kpi.AchievementPercent,
                    WeekTrend = weekRows.Select(r => new DashboardSalesWeekTrendRow
                    {
                        WeekStart = r.WeekStart,
                        WeekEnd = r.WeekEnd,
                        WeekLabel = r.WeekLabel,
                        RecognizedAmount = r.RecognizedAmount
                    }).ToList(),
                    TopSalesman = topRows.Select(r => new DashboardSalesTopSalesmanRow
                    {
                        Rank = r.Rank,
                        SalesPersonName = r.SalesPersonName,
                        CompletedOmzet = r.CompletedOmzet
                    }).ToList()
                };
            }
        }

        public void ReplaceCurrent(
            DashboardSalesAggregateResult result,
            DashboardSalesForecastAggregateResult forecast,
            string refreshLogId)
        {
            if (result is null)
                throw new System.ArgumentNullException(nameof(result));
            if (forecast is null)
                throw new System.ArgumentNullException(nameof(forecast));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();

                conn.Execute(
                    "DELETE FROM BTRPD_SalesWeekTrend WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                conn.Execute(
                    "DELETE FROM BTRPD_SalesTopSalesman WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                const string mergeKpiSql = @"
MERGE BTRPD_SalesKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        TotalOmzet = @TotalOmzet,
        TotalFaktur = @TotalFaktur,
        TotalCustomer = @TotalCustomer,
        TotalTarget = @TotalTarget,
        TotalAchievement = @TotalAchievement,
        AchievementPercent = @AchievementPercent,
        CompletedOmzet = @CompletedOmzet,
        PipelineOmzet = @PipelineOmzet,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth,
        TotalOmzet, TotalFaktur, TotalCustomer, TotalTarget, TotalAchievement,
        AchievementPercent, CompletedOmzet, PipelineOmzet, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @PeriodYear, @PeriodMonth,
        @TotalOmzet, @TotalFaktur, @TotalCustomer, @TotalTarget, @TotalAchievement,
        @AchievementPercent, @CompletedOmzet, @PipelineOmzet, @LastRefreshLogId);";

                conn.Execute(mergeKpiSql, new
                {
                    SnapshotKey,
                    result.GeneratedAt,
                    result.PeriodYear,
                    result.PeriodMonth,
                    result.TotalOmzet,
                    result.TotalFaktur,
                    result.TotalCustomer,
                    result.TotalTarget,
                    result.TotalAchievement,
                    result.AchievementPercent,
                    result.CompletedOmzet,
                    result.PipelineOmzet,
                    LastRefreshLogId = refreshLogId ?? string.Empty
                });

                const string insertWeekTrendSql = @"
INSERT INTO BTRPD_SalesWeekTrend (
    SalesWeekTrendId, SnapshotKey, WeekStart, WeekEnd, WeekLabel, RecognizedAmount)
VALUES (
    @SalesWeekTrendId, @SnapshotKey, @WeekStart, @WeekEnd, @WeekLabel, @RecognizedAmount)";

                foreach (var row in result.WeekTrend ?? new List<DashboardSalesWeekTrendRow>())
                {
                    conn.Execute(insertWeekTrendSql, new
                    {
                        SalesWeekTrendId = Ulid.NewUlid().ToString(),
                        SnapshotKey,
                        row.WeekStart,
                        row.WeekEnd,
                        WeekLabel = row.WeekLabel ?? string.Empty,
                        row.RecognizedAmount
                    });
                }

                const string insertTopSalesmanSql = @"
INSERT INTO BTRPD_SalesTopSalesman (
    SalesTopSalesmanId, SnapshotKey, Rank, SalesPersonName, CompletedOmzet)
VALUES (
    @SalesTopSalesmanId, @SnapshotKey, @Rank, @SalesPersonName, @CompletedOmzet)";

                foreach (var row in result.TopSalesman ?? new List<DashboardSalesTopSalesmanRow>())
                {
                    conn.Execute(insertTopSalesmanSql, new
                    {
                        SalesTopSalesmanId = Ulid.NewUlid().ToString(),
                        SnapshotKey,
                        row.Rank,
                        SalesPersonName = row.SalesPersonName ?? string.Empty,
                        row.CompletedOmzet
                    });
                }

                conn.Execute(
                    "DELETE FROM BTRPD_SalesDailyPace WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                const string mergeForecastKpiSql = @"
MERGE BTRPD_SalesForecastKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        BusinessDate = @BusinessDate,
        DaysInMonth = @DaysInMonth,
        DaysElapsed = @DaysElapsed,
        DaysRemaining = @DaysRemaining,
        CurrentSales = @CurrentSales,
        TotalTarget = @TotalTarget,
        CurrentAchievementPercent = @CurrentAchievementPercent,
        DailyAverageSales = @DailyAverageSales,
        ForecastSales = @ForecastSales,
        ForecastAchievementPercent = @ForecastAchievementPercent,
        RequiredDailySales = @RequiredDailySales,
        TargetGap = @TargetGap,
        ForecastVariance = @ForecastVariance,
        BestCaseSales = @BestCaseSales,
        WorstCaseSales = @WorstCaseSales,
        ForecastConfidence = @ForecastConfidence,
        ForecastRiskBand = @ForecastRiskBand,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, BusinessDate,
        DaysInMonth, DaysElapsed, DaysRemaining, CurrentSales, TotalTarget,
        CurrentAchievementPercent, DailyAverageSales, ForecastSales,
        ForecastAchievementPercent, RequiredDailySales, TargetGap, ForecastVariance,
        BestCaseSales, WorstCaseSales, ForecastConfidence, ForecastRiskBand, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @PeriodYear, @PeriodMonth, @BusinessDate,
        @DaysInMonth, @DaysElapsed, @DaysRemaining, @CurrentSales, @TotalTarget,
        @CurrentAchievementPercent, @DailyAverageSales, @ForecastSales,
        @ForecastAchievementPercent, @RequiredDailySales, @TargetGap, @ForecastVariance,
        @BestCaseSales, @WorstCaseSales, @ForecastConfidence, @ForecastRiskBand, @LastRefreshLogId);";

                conn.Execute(mergeForecastKpiSql, new
                {
                    SnapshotKey,
                    forecast.GeneratedAt,
                    forecast.PeriodYear,
                    forecast.PeriodMonth,
                    forecast.BusinessDate,
                    forecast.DaysInMonth,
                    forecast.DaysElapsed,
                    forecast.DaysRemaining,
                    forecast.CurrentSales,
                    forecast.TotalTarget,
                    forecast.CurrentAchievementPercent,
                    forecast.DailyAverageSales,
                    forecast.ForecastSales,
                    forecast.ForecastAchievementPercent,
                    forecast.RequiredDailySales,
                    forecast.TargetGap,
                    forecast.ForecastVariance,
                    forecast.BestCaseSales,
                    forecast.WorstCaseSales,
                    ForecastConfidence = forecast.ForecastConfidence ?? string.Empty,
                    ForecastRiskBand = forecast.ForecastRiskBand ?? string.Empty,
                    LastRefreshLogId = refreshLogId ?? string.Empty
                });

                const string insertDailyPaceSql = @"
INSERT INTO BTRPD_SalesDailyPace (
    SalesDailyPaceId, SnapshotKey, PaceDate, DayOfMonth, IsElapsed,
    ActualAmount, ProjectedDailyAmount)
VALUES (
    @SalesDailyPaceId, @SnapshotKey, @PaceDate, @DayOfMonth, @IsElapsed,
    @ActualAmount, @ProjectedDailyAmount)";

                foreach (var row in forecast.DailyPace ?? new List<DashboardSalesDailyPaceRow>())
                {
                    conn.Execute(insertDailyPaceSql, new
                    {
                        SalesDailyPaceId = Ulid.NewUlid().ToString(),
                        SnapshotKey,
                        row.PaceDate,
                        row.DayOfMonth,
                        IsElapsed = row.IsElapsed ? 1 : 0,
                        row.ActualAmount,
                        row.ProjectedDailyAmount
                    });
                }
            }
        }

        public static DashboardSalesResponse MapToResponse(DashboardSalesAggregateResult snapshot)
        {
            return new DashboardSalesResponse
            {
                TotalOmzet = snapshot.TotalOmzet,
                CompletedOmzet = snapshot.CompletedOmzet,
                PipelineOmzet = snapshot.PipelineOmzet,
                TotalFaktur = snapshot.TotalFaktur,
                TotalCustomer = snapshot.TotalCustomer,
                GeneratedAt = snapshot.GeneratedAt,
                WeeklyTrend = (snapshot.WeekTrend ?? new List<DashboardSalesWeekTrendRow>())
                    .Select(w => new DashboardSalesWeekTrendItem
                    {
                        WeekStart = w.WeekStart,
                        WeekEnd = w.WeekEnd,
                        WeekLabel = w.WeekLabel,
                        RecognizedAmount = w.RecognizedAmount
                    })
                    .ToList(),
                TotalTarget = snapshot.TotalTarget,
                TotalAchievement = snapshot.TotalAchievement,
                AchievementPercent = snapshot.AchievementPercent,
                TargetVsAchievement = new DashboardSalesTargetVsAchievement
                {
                    TargetAmount = snapshot.TotalTarget,
                    AchievementAmount = snapshot.TotalAchievement
                },
                TopSalesmanRanking = (snapshot.TopSalesman ?? new List<DashboardSalesTopSalesmanRow>())
                    .OrderBy(r => r.Rank)
                    .Select(r => new DashboardSalesRankingItem
                    {
                        Rank = r.Rank,
                        SalesPersonId = r.SalesPersonId,
                        SalesPersonName = r.SalesPersonName,
                        CompletedOmzet = r.CompletedOmzet,
                        Investigation = InvestigationMetadataBuilder.Build(
                            InvestigationRegistry.SignalLegacyTopSalesman,
                            InvestigationMetadataBuilder.EntityTypeSalesman,
                            r.SalesPersonId,
                            r.SalesPersonName)
                    })
                    .ToList()
            };
        }

        private sealed class KpiRow
        {
            public string SnapshotKey { get; set; }
            public System.DateTime GeneratedAt { get; set; }
            public int PeriodYear { get; set; }
            public int PeriodMonth { get; set; }
            public decimal TotalOmzet { get; set; }
            public int TotalFaktur { get; set; }
            public int TotalCustomer { get; set; }
            public decimal TotalTarget { get; set; }
            public decimal TotalAchievement { get; set; }
            public decimal? AchievementPercent { get; set; }
            public decimal CompletedOmzet { get; set; }
            public decimal PipelineOmzet { get; set; }
            public string LastRefreshLogId { get; set; }
        }

        private sealed class WeekTrendRow
        {
            public System.DateTime WeekStart { get; set; }
            public System.DateTime WeekEnd { get; set; }
            public string WeekLabel { get; set; }
            public decimal RecognizedAmount { get; set; }
        }

        private sealed class TopSalesmanRow
        {
            public int Rank { get; set; }
            public string SalesPersonName { get; set; }
            public decimal CompletedOmzet { get; set; }
        }
    }
}
