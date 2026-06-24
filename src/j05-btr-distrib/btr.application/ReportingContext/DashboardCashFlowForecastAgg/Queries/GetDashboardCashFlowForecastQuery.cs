using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardCashFlowForecastAgg.Contracts;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using MediatR;

namespace btr.application.ReportingContext.DashboardCashFlowForecastAgg.Queries
{
    public class GetDashboardCashFlowForecastQuery : IRequest<DashboardCashFlowForecastResponse>
    {
    }

    public class DashboardCashFlowForecastResponse
    {
        public bool IsAvailable { get; set; }

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

        public string RequiredDailySeverity { get; set; }

        public string ExecutiveSummary { get; set; }

        public List<DashboardCashFlowDailyPaceItem> DailyPace { get; set; }
            = new List<DashboardCashFlowDailyPaceItem>();

        public List<DashboardCashFlowRecoveryTrendItem> RecoveryTrend { get; set; }
            = new List<DashboardCashFlowRecoveryTrendItem>();

        public List<DashboardCashFlowCollectionRiskItem> CollectionRisks { get; set; }
            = new List<DashboardCashFlowCollectionRiskItem>();
    }

    public class DashboardCashFlowDailyPaceItem
    {
        public DateTime PaceDate { get; set; }

        public int DayOfMonth { get; set; }

        public bool IsElapsed { get; set; }

        public decimal ActualCashAmount { get; set; }

        public decimal ActualCollectionAmount { get; set; }

        public decimal ProjectedDailyCashAmount { get; set; }
    }

    public class DashboardCashFlowRecoveryTrendItem
    {
        public DateTime TrendDate { get; set; }

        public int DayOfMonth { get; set; }

        public bool IsElapsed { get; set; }

        public decimal CumulativeCollections { get; set; }

        public decimal CumulativeBilling { get; set; }
    }

    public class DashboardCashFlowCollectionRiskItem
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

    public class GetDashboardCashFlowForecastHandler
        : IRequestHandler<GetDashboardCashFlowForecastQuery, DashboardCashFlowForecastResponse>
    {
        private readonly IDashboardCashFlowForecastDal _dal;

        public GetDashboardCashFlowForecastHandler(IDashboardCashFlowForecastDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardCashFlowForecastResponse> Handle(
            GetDashboardCashFlowForecastQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }

    public static class CashFlowForecastExecutiveSummaryBuilder
    {
        public static string Build(DashboardCashFlowForecastResponse response)
        {
            if (response is null)
                return string.Empty;

            if (response.MonthFakturOmzet <= 0)
            {
                return "No billing recorded this month — collection forecast unavailable.";
            }

            var confidencePrefix = response.ForecastConfidence == CashFlowForecastPolicy.ConfidenceLow
                ? "Early-month forecast — cash pace may change significantly. "
                : string.Empty;

            var expectedCashText = FormatCurrency(response.ExpectedCashCollection);
            var projectedTotalText = FormatCurrency(response.ProjectedMonthEndTotalCollections);
            var forecastPercentText = response.CollectionForecastPercent.HasValue
                ? $"{response.CollectionForecastPercent.Value:F1}%"
                : "—";

            if (response.CollectionForecastPercent.GetValueOrDefault() >= 100m)
            {
                return confidencePrefix +
                       $"At current pace, cash collection is projected to reach {expectedCashText} by month-end. Total collections are projected at {projectedTotalText} ({forecastPercentText} of billing). Cash collection is projected to keep pace with billing.";
            }

            if (response.ForecastRiskBand == ExecutiveSalesAchievementBandResolver.Warning)
            {
                return confidencePrefix +
                       $"At current pace, cash collection is projected to reach {expectedCashText} by month-end. Total collections are projected at {projectedTotalText} ({forecastPercentText} of billing). Projected collections trail billing — increased collection effort needed.";
            }

            if (response.ForecastRiskBand == ExecutiveSalesAchievementBandResolver.Critical)
            {
                return confidencePrefix +
                       $"At current pace, cash collection is projected to reach {expectedCashText} by month-end. Total collections are projected at {projectedTotalText} ({forecastPercentText} of billing). Liquidity risk — projected collections significantly below billing.";
            }

            return confidencePrefix +
                   $"At current pace, cash collection is projected to reach {expectedCashText} by month-end. Total collections are projected at {projectedTotalText} ({forecastPercentText} of billing).";
        }

        private static string FormatCurrency(decimal value) =>
            value.ToString("N0", CultureInfo.InvariantCulture);
    }
}
