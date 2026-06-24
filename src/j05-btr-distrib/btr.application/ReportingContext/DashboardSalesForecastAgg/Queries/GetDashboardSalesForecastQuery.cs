using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.application.ReportingContext.DashboardSalesForecastAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using MediatR;

namespace btr.application.ReportingContext.DashboardSalesForecastAgg.Queries
{
    public class GetDashboardSalesForecastQuery : IRequest<DashboardSalesForecastResponse>
    {
    }

    public class DashboardSalesForecastResponse
    {
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

        public string RequiredDailySeverity { get; set; }

        public string ExecutiveSummary { get; set; }

        public DashboardSalesForecastVsTarget ForecastVsTarget { get; set; }

        public List<DashboardSalesDailyPaceItem> DailyPace { get; set; }
            = new List<DashboardSalesDailyPaceItem>();

        public List<DashboardSalesWeekTrendItem> WeeklyTrend { get; set; }
            = new List<DashboardSalesWeekTrendItem>();
    }

    public class DashboardSalesForecastVsTarget
    {
        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; }

        public decimal ForecastAmount { get; set; }
    }

    public class DashboardSalesDailyPaceItem
    {
        public DateTime PaceDate { get; set; }

        public int DayOfMonth { get; set; }

        public bool IsElapsed { get; set; }

        public decimal ActualAmount { get; set; }

        public decimal ProjectedDailyAmount { get; set; }
    }

    public class GetDashboardSalesForecastHandler
        : IRequestHandler<GetDashboardSalesForecastQuery, DashboardSalesForecastResponse>
    {
        private readonly IDashboardSalesForecastDal _dal;

        public GetDashboardSalesForecastHandler(IDashboardSalesForecastDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardSalesForecastResponse> Handle(
            GetDashboardSalesForecastQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }

    public static class SalesForecastExecutiveSummaryBuilder
    {
        public static string Build(DashboardSalesForecastResponse response)
        {
            if (response is null)
                return string.Empty;

            var confidencePrefix = response.ForecastConfidence == SalesForecastPolicy.ConfidenceLow
                ? $"Forecast is preliminary (day {response.DaysElapsed} of {response.DaysInMonth}). Projection will become more reliable as billing accumulates. "
                : string.Empty;

            if (response.TotalTarget <= 0)
            {
                return confidencePrefix +
                       $"At current pace, invoiced sales are projected to reach {FormatCurrency(response.ForecastSales)}. No monthly target is configured — achievement comparison unavailable.";
            }

            var forecastPercentText = SalesOmzetChartAchievementPolicy.FormatPercentDisplay(
                response.ForecastAchievementPercent);
            var targetText = FormatCurrency(response.TotalTarget);
            var forecastText = FormatCurrency(response.ForecastSales);
            var requiredText = FormatCurrency(response.RequiredDailySales.GetValueOrDefault());

            if (response.ForecastAchievementPercent.GetValueOrDefault() >= 100m)
            {
                return confidencePrefix +
                       $"At current pace, invoiced sales are projected to reach {forecastText} ({forecastPercentText}) against a target of {targetText}. The team is on track to meet or exceed the monthly target.";
            }

            if (response.ForecastAchievementPercent.GetValueOrDefault() >= 80m)
            {
                return confidencePrefix +
                       $"At current pace, invoiced sales are projected to reach {forecastText} ({forecastPercentText}) against a target of {targetText}. {requiredText} daily billing is required on remaining days to close the gap.";
            }

            if (response.ForecastRiskBand == ExecutiveSalesAchievementBandResolver.Unknown)
            {
                return confidencePrefix +
                       $"At current pace, invoiced sales are projected to reach {forecastText}. No monthly target is configured — achievement comparison unavailable.";
            }

            return confidencePrefix +
                   $"At current pace, invoiced sales are projected to reach {forecastText} ({forecastPercentText}) against a target of {targetText}. Immediate corrective action is needed — {requiredText} daily billing required to recover.";
        }

        private static string FormatCurrency(decimal value) =>
            value.ToString("N0", CultureInfo.InvariantCulture);
    }
}
