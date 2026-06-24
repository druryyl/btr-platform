using System;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.SalesContext.SalesOmzetAgg.Policies;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public sealed class SalesForecastCalculation
    {
        public int DaysInMonth { get; set; }

        public int DaysElapsed { get; set; }

        public int DaysRemaining { get; set; }

        public decimal DailyAverageSales { get; set; }

        public decimal ForecastSales { get; set; }

        public decimal? CurrentAchievementPercent { get; set; }

        public decimal? ForecastAchievementPercent { get; set; }

        public decimal? RequiredDailySales { get; set; }

        public decimal TargetGap { get; set; }

        public decimal ForecastVariance { get; set; }

        public string ForecastConfidence { get; set; }

        public string ForecastRiskBand { get; set; }
    }

    public static class SalesForecastPolicy
    {
        public const string ConfidenceLow = "Low";
        public const string ConfidenceMedium = "Medium";
        public const string ConfidenceHigh = "High";

        public const string RequiredDailySeverityNormal = "Normal";
        public const string RequiredDailySeverityWarning = "Warning";
        public const string RequiredDailySeverityCritical = "Critical";

        public static SalesForecastCalculation Compute(
            decimal currentSales,
            decimal? target,
            DateTime businessDate,
            DateTime monthStart,
            DateTime monthEnd)
        {
            var ms = monthStart.Date;
            var me = monthEnd.Date;
            var b = businessDate.Date;

            if (b < ms)
                b = ms;
            if (b > me)
                b = me;

            var daysInMonth = (me - ms).Days + 1;
            var daysElapsed = Math.Max(1, (b - ms).Days + 1);
            var daysRemaining = Math.Max(0, (me - b).Days);

            var dailyAverage = Math.Round(currentSales / daysElapsed, 2, MidpointRounding.AwayFromZero);
            var forecastSales = daysRemaining == 0
                ? currentSales
                : Math.Round(dailyAverage * daysInMonth, 2, MidpointRounding.AwayFromZero);

            decimal? requiredDaily = null;
            if (target.HasValue && target.Value > 0)
            {
                if (target.Value > currentSales && daysRemaining > 0)
                {
                    requiredDaily = Math.Round(
                        (target.Value - currentSales) / daysRemaining,
                        2,
                        MidpointRounding.AwayFromZero);
                }
                else
                {
                    requiredDaily = 0m;
                }
            }

            var targetGap = target.HasValue
                ? Math.Round(target.Value - forecastSales, 2, MidpointRounding.AwayFromZero)
                : 0m;

            var forecastVariance = Math.Round(forecastSales - currentSales, 2, MidpointRounding.AwayFromZero);

            var currentAchievement = SalesOmzetChartAchievementPolicy.ComputePercent(
                currentSales,
                target);
            var forecastAchievement = SalesOmzetChartAchievementPolicy.ComputePercent(
                forecastSales,
                target);

            return new SalesForecastCalculation
            {
                DaysInMonth = daysInMonth,
                DaysElapsed = daysElapsed,
                DaysRemaining = daysRemaining,
                DailyAverageSales = dailyAverage,
                ForecastSales = forecastSales,
                CurrentAchievementPercent = currentAchievement,
                ForecastAchievementPercent = forecastAchievement,
                RequiredDailySales = requiredDaily,
                TargetGap = targetGap,
                ForecastVariance = forecastVariance,
                ForecastConfidence = ResolveConfidence(daysElapsed, daysInMonth),
                ForecastRiskBand = ExecutiveSalesAchievementBandResolver.Resolve(forecastAchievement)
            };
        }

        public static decimal ComputeBestCase(decimal mtdDailyAverage, decimal recent7DailyAverage, int daysInMonth)
        {
            var pace = Math.Max(mtdDailyAverage, recent7DailyAverage);
            return Math.Round(pace * daysInMonth, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal ComputeWorstCase(decimal mtdDailyAverage, decimal recent7DailyAverage, int daysInMonth)
        {
            var pace = Math.Min(mtdDailyAverage, recent7DailyAverage);
            return Math.Round(pace * daysInMonth, 2, MidpointRounding.AwayFromZero);
        }

        public static string ResolveConfidence(int daysElapsed, int daysInMonth)
        {
            if (daysElapsed <= 5)
                return ConfidenceLow;

            if (daysElapsed <= 20)
                return ConfidenceMedium;

            return ConfidenceHigh;
        }

        public static string ResolveRequiredDailySeverity(decimal requiredDaily, decimal dailyAverage)
        {
            if (dailyAverage <= 0)
                return requiredDaily > 0
                    ? RequiredDailySeverityCritical
                    : RequiredDailySeverityNormal;

            var ratio = requiredDaily / dailyAverage;
            if (ratio > 2m)
                return RequiredDailySeverityCritical;

            if (ratio > 1.5m)
                return RequiredDailySeverityWarning;

            return RequiredDailySeverityNormal;
        }
    }
}
