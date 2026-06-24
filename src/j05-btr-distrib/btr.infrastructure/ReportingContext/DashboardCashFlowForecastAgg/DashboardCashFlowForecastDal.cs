using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardCashFlowForecastAgg.Contracts;
using btr.application.ReportingContext.DashboardCashFlowForecastAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardCashFlowForecastAgg
{
    public class DashboardCashFlowForecastDal : IDashboardCashFlowForecastDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardCashFlowForecastDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardCashFlowForecastResponse GetSummary()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, BusinessDate,
       DaysInMonth, DaysElapsed, DaysRemaining, CashCollectedMtd, MonthCollections,
       MonthFakturOmzet, DailyCashCollectionAverage, DailyCollectionAverage,
       ExpectedCashCollection, ProjectedMonthEndTotalCollections, CollectionForecastPercent,
       RecoveryVsBillingPercent, RecoveryVsBillingForecastPercent, RemainingCollectionTarget,
       RequiredDailyCollection, OutstandingDueRemaining, OverdueOutstanding, CollectionGap,
       ForecastVarianceCash, ExpectedCollectionRatePercent, BestCaseCash, WorstCaseCash,
       ForecastConfidence, ForecastRiskBand
FROM BTRPD_CashFlowForecastKpi
WHERE SnapshotKey = @SnapshotKey";

            const string dailyPaceSql = @"
SELECT PaceDate, DayOfMonth, IsElapsed, ActualCashAmount, ActualCollectionAmount, ProjectedDailyCashAmount
FROM BTRPD_CashFlowDailyPace
WHERE SnapshotKey = @SnapshotKey
ORDER BY PaceDate";

            const string recoveryTrendSql = @"
SELECT TrendDate, DayOfMonth, IsElapsed, CumulativeCollections, CumulativeBilling
FROM BTRPD_CashFlowRecoveryTrend
WHERE SnapshotKey = @SnapshotKey
ORDER BY TrendDate";

            const string collectionRiskSql = @"
SELECT SortOrder, RiskKey, RiskLabel, EntityType, EntityId, EntityName,
       Amount, DueOrAgingText, RuleExplanation, ReportRoute
FROM BTRPD_CashFlowCollectionRisk
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<ForecastKpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                {
                    return new DashboardCashFlowForecastResponse
                    {
                        IsAvailable = false,
                        ExecutiveSummary = "Cash flow forecast data not yet available."
                    };
                }

                var dailyRows = conn.Query<DailyPaceRow>(dailyPaceSql, new { SnapshotKey }).ToList();
                var trendRows = conn.Query<RecoveryTrendRow>(recoveryTrendSql, new { SnapshotKey }).ToList();
                var riskRows = conn.Query<CollectionRiskRow>(collectionRiskSql, new { SnapshotKey }).ToList();

                var response = new DashboardCashFlowForecastResponse
                {
                    IsAvailable = true,
                    GeneratedAt = kpi.GeneratedAt,
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    BusinessDate = kpi.BusinessDate,
                    DaysInMonth = kpi.DaysInMonth,
                    DaysElapsed = kpi.DaysElapsed,
                    DaysRemaining = kpi.DaysRemaining,
                    CashCollectedMtd = kpi.CashCollectedMtd,
                    MonthCollections = kpi.MonthCollections,
                    MonthFakturOmzet = kpi.MonthFakturOmzet,
                    DailyCashCollectionAverage = kpi.DailyCashCollectionAverage,
                    DailyCollectionAverage = kpi.DailyCollectionAverage,
                    ExpectedCashCollection = kpi.ExpectedCashCollection,
                    ProjectedMonthEndTotalCollections = kpi.ProjectedMonthEndTotalCollections,
                    CollectionForecastPercent = kpi.CollectionForecastPercent,
                    RecoveryVsBillingPercent = kpi.RecoveryVsBillingPercent,
                    RecoveryVsBillingForecastPercent = kpi.RecoveryVsBillingForecastPercent,
                    RemainingCollectionTarget = kpi.RemainingCollectionTarget,
                    RequiredDailyCollection = kpi.RequiredDailyCollection,
                    OutstandingDueRemaining = kpi.OutstandingDueRemaining,
                    OverdueOutstanding = kpi.OverdueOutstanding,
                    CollectionGap = kpi.CollectionGap,
                    ForecastVarianceCash = kpi.ForecastVarianceCash,
                    ExpectedCollectionRatePercent = kpi.ExpectedCollectionRatePercent,
                    BestCaseCash = kpi.BestCaseCash,
                    WorstCaseCash = kpi.WorstCaseCash,
                    ForecastConfidence = kpi.ForecastConfidence ?? string.Empty,
                    ForecastRiskBand = kpi.ForecastRiskBand ?? string.Empty,
                    RequiredDailySeverity = CashFlowForecastPolicy.ResolveRequiredDailySeverity(
                        kpi.RequiredDailyCollection.GetValueOrDefault(),
                        kpi.DailyCollectionAverage),
                    DailyPace = dailyRows.Select(r => new DashboardCashFlowDailyPaceItem
                    {
                        PaceDate = r.PaceDate,
                        DayOfMonth = r.DayOfMonth,
                        IsElapsed = r.IsElapsed,
                        ActualCashAmount = r.ActualCashAmount,
                        ActualCollectionAmount = r.ActualCollectionAmount,
                        ProjectedDailyCashAmount = r.ProjectedDailyCashAmount
                    }).ToList(),
                    RecoveryTrend = trendRows.Select(r => new DashboardCashFlowRecoveryTrendItem
                    {
                        TrendDate = r.TrendDate,
                        DayOfMonth = r.DayOfMonth,
                        IsElapsed = r.IsElapsed,
                        CumulativeCollections = r.CumulativeCollections,
                        CumulativeBilling = r.CumulativeBilling
                    }).ToList(),
                    CollectionRisks = riskRows.Select(r => new DashboardCashFlowCollectionRiskItem
                    {
                        SortOrder = r.SortOrder,
                        RiskKey = r.RiskKey,
                        RiskLabel = r.RiskLabel,
                        EntityType = r.EntityType,
                        EntityId = r.EntityId,
                        EntityName = r.EntityName,
                        Amount = r.Amount,
                        DueOrAgingText = r.DueOrAgingText,
                        RuleExplanation = r.RuleExplanation,
                        ReportRoute = r.ReportRoute
                    }).ToList()
                };

                response.ExecutiveSummary = CashFlowForecastExecutiveSummaryBuilder.Build(response);
                return response;
            }
        }

        private sealed class ForecastKpiRow
        {
            public DateTime GeneratedAt { get; set; }
            public int PeriodYear { get; set; }
            public int PeriodMonth { get; set; }
            public DateTime BusinessDate { get; set; }
            public int DaysInMonth { get; set; }
            public int DaysElapsed { get; set; }
            public int DaysRemaining { get; set; }
            public decimal CashCollectedMtd { get; set; }
            public decimal MonthCollections { get; set; }
            public decimal MonthFakturOmzet { get; set; }
            public decimal DailyCashCollectionAverage { get; set; }
            public decimal DailyCollectionAverage { get; set; }
            public decimal ExpectedCashCollection { get; set; }
            public decimal ProjectedMonthEndTotalCollections { get; set; }
            public decimal? CollectionForecastPercent { get; set; }
            public decimal? RecoveryVsBillingPercent { get; set; }
            public decimal? RecoveryVsBillingForecastPercent { get; set; }
            public decimal RemainingCollectionTarget { get; set; }
            public decimal? RequiredDailyCollection { get; set; }
            public decimal OutstandingDueRemaining { get; set; }
            public decimal OverdueOutstanding { get; set; }
            public decimal CollectionGap { get; set; }
            public decimal ForecastVarianceCash { get; set; }
            public decimal? ExpectedCollectionRatePercent { get; set; }
            public decimal BestCaseCash { get; set; }
            public decimal WorstCaseCash { get; set; }
            public string ForecastConfidence { get; set; }
            public string ForecastRiskBand { get; set; }
        }

        private sealed class DailyPaceRow
        {
            public DateTime PaceDate { get; set; }
            public int DayOfMonth { get; set; }
            public bool IsElapsed { get; set; }
            public decimal ActualCashAmount { get; set; }
            public decimal ActualCollectionAmount { get; set; }
            public decimal ProjectedDailyCashAmount { get; set; }
        }

        private sealed class RecoveryTrendRow
        {
            public DateTime TrendDate { get; set; }
            public int DayOfMonth { get; set; }
            public bool IsElapsed { get; set; }
            public decimal CumulativeCollections { get; set; }
            public decimal CumulativeBilling { get; set; }
        }

        private sealed class CollectionRiskRow
        {
            public int SortOrder { get; set; }
            public string RiskKey { get; set; }
            public string RiskLabel { get; set; }
            public string EntityType { get; set; }
            public string EntityId { get; set; }
            public string EntityName { get; set; }
            public decimal Amount { get; set; }
            public string DueOrAgingText { get; set; }
            public string RuleExplanation { get; set; }
            public string ReportRoute { get; set; }
        }
    }
}
