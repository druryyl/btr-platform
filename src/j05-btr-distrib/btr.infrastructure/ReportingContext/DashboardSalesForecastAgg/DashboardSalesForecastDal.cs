using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.application.ReportingContext.DashboardSalesForecastAgg.Contracts;
using btr.application.ReportingContext.DashboardSalesForecastAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSalesForecastAgg
{
    public class DashboardSalesForecastDal : IDashboardSalesForecastDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardSalesForecastDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardSalesForecastResponse GetSummary()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, BusinessDate,
       DaysInMonth, DaysElapsed, DaysRemaining, CurrentSales, TotalTarget,
       CurrentAchievementPercent, DailyAverageSales, ForecastSales,
       ForecastAchievementPercent, RequiredDailySales, TargetGap, ForecastVariance,
       BestCaseSales, WorstCaseSales, ForecastConfidence, ForecastRiskBand
FROM BTRPD_SalesForecastKpi
WHERE SnapshotKey = @SnapshotKey";

            const string dailyPaceSql = @"
SELECT PaceDate, DayOfMonth, IsElapsed, ActualAmount, ProjectedDailyAmount
FROM BTRPD_SalesDailyPace
WHERE SnapshotKey = @SnapshotKey
ORDER BY PaceDate";

            const string weekTrendSql = @"
SELECT WeekStart, WeekEnd, WeekLabel, RecognizedAmount
FROM BTRPD_SalesWeekTrend
WHERE SnapshotKey = @SnapshotKey
ORDER BY WeekStart";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<ForecastKpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    throw new DashboardSnapshotUnavailableException("Sales forecast data not yet available");

                var dailyRows = conn.Query<DailyPaceRow>(dailyPaceSql, new { SnapshotKey }).ToList();
                var weekRows = conn.Query<WeekTrendRow>(weekTrendSql, new { SnapshotKey }).ToList();

                var response = new DashboardSalesForecastResponse
                {
                    GeneratedAt = kpi.GeneratedAt,
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    BusinessDate = kpi.BusinessDate,
                    DaysInMonth = kpi.DaysInMonth,
                    DaysElapsed = kpi.DaysElapsed,
                    DaysRemaining = kpi.DaysRemaining,
                    CurrentSales = kpi.CurrentSales,
                    TotalTarget = kpi.TotalTarget,
                    CurrentAchievementPercent = kpi.CurrentAchievementPercent,
                    DailyAverageSales = kpi.DailyAverageSales,
                    ForecastSales = kpi.ForecastSales,
                    ForecastAchievementPercent = kpi.ForecastAchievementPercent,
                    RequiredDailySales = kpi.RequiredDailySales,
                    TargetGap = kpi.TargetGap,
                    ForecastVariance = kpi.ForecastVariance,
                    BestCaseSales = kpi.BestCaseSales,
                    WorstCaseSales = kpi.WorstCaseSales,
                    ForecastConfidence = kpi.ForecastConfidence ?? string.Empty,
                    ForecastRiskBand = kpi.ForecastRiskBand ?? string.Empty,
                    RequiredDailySeverity = SalesForecastPolicy.ResolveRequiredDailySeverity(
                        kpi.RequiredDailySales.GetValueOrDefault(),
                        kpi.DailyAverageSales),
                    ForecastVsTarget = new DashboardSalesForecastVsTarget
                    {
                        TargetAmount = kpi.TotalTarget,
                        CurrentAmount = kpi.CurrentSales,
                        ForecastAmount = kpi.ForecastSales
                    },
                    DailyPace = dailyRows.Select(r => new DashboardSalesDailyPaceItem
                    {
                        PaceDate = r.PaceDate,
                        DayOfMonth = r.DayOfMonth,
                        IsElapsed = r.IsElapsed,
                        ActualAmount = r.ActualAmount,
                        ProjectedDailyAmount = r.ProjectedDailyAmount
                    }).ToList(),
                    WeeklyTrend = weekRows.Select(r => new DashboardSalesWeekTrendItem
                    {
                        WeekStart = r.WeekStart,
                        WeekEnd = r.WeekEnd,
                        WeekLabel = r.WeekLabel,
                        RecognizedAmount = r.RecognizedAmount
                    }).ToList()
                };

                response.ExecutiveSummary = SalesForecastExecutiveSummaryBuilder.Build(response);
                return response;
            }
        }

        private sealed class ForecastKpiRow
        {
            public string SnapshotKey { get; set; }
            public DateTime GeneratedAt { get; set; }
            public int PeriodYear { get; set; }
            public int PeriodMonth { get; set; }
            public DateTime BusinessDate { get; set; }
            public int DaysInMonth { get; set; }
            public int DaysElapsed { get; set; }
            public int DaysRemaining { get; set; }
            public decimal CurrentSales { get; set; }
            public decimal TotalTarget { get; set; }
            public decimal? CurrentAchievementPercent { get; set; }
            public decimal DailyAverageSales { get; set; }
            public decimal ForecastSales { get; set; }
            public decimal? ForecastAchievementPercent { get; set; }
            public decimal? RequiredDailySales { get; set; }
            public decimal TargetGap { get; set; }
            public decimal ForecastVariance { get; set; }
            public decimal BestCaseSales { get; set; }
            public decimal WorstCaseSales { get; set; }
            public string ForecastConfidence { get; set; }
            public string ForecastRiskBand { get; set; }
        }

        private sealed class DailyPaceRow
        {
            public DateTime PaceDate { get; set; }
            public int DayOfMonth { get; set; }
            public bool IsElapsed { get; set; }
            public decimal ActualAmount { get; set; }
            public decimal ProjectedDailyAmount { get; set; }
        }

        private sealed class WeekTrendRow
        {
            public DateTime WeekStart { get; set; }
            public DateTime WeekEnd { get; set; }
            public string WeekLabel { get; set; }
            public decimal RecognizedAmount { get; set; }
        }
    }
}
