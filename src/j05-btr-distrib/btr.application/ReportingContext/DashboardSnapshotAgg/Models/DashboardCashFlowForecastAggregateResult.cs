using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardCashFlowForecastAggregateResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime GeneratedAt { get; set; }

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

        public List<DashboardCashFlowDailyPaceRow> DailyPace { get; set; }
            = new List<DashboardCashFlowDailyPaceRow>();

        public List<DashboardCashFlowRecoveryTrendRow> RecoveryTrend { get; set; }
            = new List<DashboardCashFlowRecoveryTrendRow>();

        public List<DashboardCashFlowCollectionRiskRow> CollectionRisks { get; set; }
            = new List<DashboardCashFlowCollectionRiskRow>();
    }

    public class DashboardCashFlowDailyPaceRow
    {
        public DateTime PaceDate { get; set; }

        public int DayOfMonth { get; set; }

        public bool IsElapsed { get; set; }

        public decimal ActualCashAmount { get; set; }

        public decimal ActualCollectionAmount { get; set; }

        public decimal ProjectedDailyCashAmount { get; set; }
    }

    public class DashboardCashFlowRecoveryTrendRow
    {
        public DateTime TrendDate { get; set; }

        public int DayOfMonth { get; set; }

        public bool IsElapsed { get; set; }

        public decimal CumulativeCollections { get; set; }

        public decimal CumulativeBilling { get; set; }
    }

    public class DashboardCashFlowCollectionRiskRow
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
