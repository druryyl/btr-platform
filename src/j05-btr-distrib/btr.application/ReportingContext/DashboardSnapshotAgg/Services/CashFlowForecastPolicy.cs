using System;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public sealed class CashFlowForecastCalculation
    {
        public int DaysInMonth { get; set; }

        public int DaysElapsed { get; set; }

        public int DaysRemaining { get; set; }

        public decimal DailyCashCollectionAverage { get; set; }

        public decimal DailyCollectionAverage { get; set; }

        public decimal ExpectedCashCollection { get; set; }

        public decimal ProjectedMonthEndTotalCollections { get; set; }

        public decimal? CollectionForecastPercent { get; set; }

        public decimal? RecoveryVsBillingPercent { get; set; }

        public decimal? RecoveryVsBillingForecastPercent { get; set; }

        public decimal RemainingCollectionTarget { get; set; }

        public decimal? RequiredDailyCollection { get; set; }

        public decimal CollectionGap { get; set; }

        public decimal ForecastVarianceCash { get; set; }

        public decimal? ExpectedCollectionRatePercent { get; set; }

        public string ForecastConfidence { get; set; }

        public string ForecastRiskBand { get; set; }
    }

    public static class CashFlowForecastPolicy
    {
        public const string ConfidenceLow = "Low";
        public const string ConfidenceMedium = "Medium";
        public const string ConfidenceHigh = "High";

        public const string RequiredDailySeverityNormal = "Normal";
        public const string RequiredDailySeverityWarning = "Warning";
        public const string RequiredDailySeverityCritical = "Critical";

        public static CashFlowForecastCalculation Compute(
            decimal cashCollectedMtd,
            decimal monthCollections,
            decimal monthFakturOmzet,
            DateTime businessDate,
            DateTime monthStart,
            DateTime monthEnd,
            decimal outstandingDueRemaining = 0m)
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

            var dailyCashAverage = Math.Round(cashCollectedMtd / daysElapsed, 2, MidpointRounding.AwayFromZero);
            var dailyCollectionAverage = Math.Round(monthCollections / daysElapsed, 2, MidpointRounding.AwayFromZero);

            var expectedCash = daysRemaining == 0
                ? cashCollectedMtd
                : Math.Round(dailyCashAverage * daysInMonth, 2, MidpointRounding.AwayFromZero);

            var projectedTotalCollections = daysRemaining == 0
                ? monthCollections
                : Math.Round(dailyCollectionAverage * daysInMonth, 2, MidpointRounding.AwayFromZero);

            decimal? collectionForecastPercent = null;
            decimal? recoveryVsBillingPercent = null;
            decimal? recoveryVsBillingForecastPercent = null;
            decimal? requiredDaily = null;
            var remainingTarget = 0m;

            if (monthFakturOmzet > 0)
            {
                recoveryVsBillingPercent = Math.Round(
                    monthCollections / monthFakturOmzet * 100m,
                    1,
                    MidpointRounding.AwayFromZero);

                collectionForecastPercent = Math.Round(
                    projectedTotalCollections / monthFakturOmzet * 100m,
                    1,
                    MidpointRounding.AwayFromZero);

                recoveryVsBillingForecastPercent = collectionForecastPercent;

                if (monthFakturOmzet > monthCollections)
                {
                    remainingTarget = Math.Round(
                        monthFakturOmzet - monthCollections,
                        2,
                        MidpointRounding.AwayFromZero);

                    if (daysRemaining > 0)
                    {
                        requiredDaily = Math.Round(
                            remainingTarget / daysRemaining,
                            2,
                            MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        requiredDaily = 0m;
                    }
                }
                else
                {
                    requiredDaily = 0m;
                }
            }

            var collectionGap = Math.Round(
                monthFakturOmzet - projectedTotalCollections,
                2,
                MidpointRounding.AwayFromZero);

            var forecastVarianceCash = Math.Round(
                expectedCash - cashCollectedMtd,
                2,
                MidpointRounding.AwayFromZero);

            decimal? expectedCollectionRate = null;
            var pipelineDenominator = cashCollectedMtd + outstandingDueRemaining;
            if (pipelineDenominator > 0)
            {
                expectedCollectionRate = Math.Round(
                    cashCollectedMtd / pipelineDenominator * 100m,
                    1,
                    MidpointRounding.AwayFromZero);
            }

            return new CashFlowForecastCalculation
            {
                DaysInMonth = daysInMonth,
                DaysElapsed = daysElapsed,
                DaysRemaining = daysRemaining,
                DailyCashCollectionAverage = dailyCashAverage,
                DailyCollectionAverage = dailyCollectionAverage,
                ExpectedCashCollection = expectedCash,
                ProjectedMonthEndTotalCollections = projectedTotalCollections,
                CollectionForecastPercent = collectionForecastPercent,
                RecoveryVsBillingPercent = recoveryVsBillingPercent,
                RecoveryVsBillingForecastPercent = recoveryVsBillingForecastPercent,
                RemainingCollectionTarget = remainingTarget,
                RequiredDailyCollection = requiredDaily,
                CollectionGap = collectionGap,
                ForecastVarianceCash = forecastVarianceCash,
                ExpectedCollectionRatePercent = expectedCollectionRate,
                ForecastConfidence = ResolveConfidence(daysElapsed, daysInMonth),
                ForecastRiskBand = ResolveRecoveryForecastBand(collectionForecastPercent)
            };
        }

        public static decimal ComputeBestCaseCash(decimal mtdDailyCash, decimal recent7DailyCash, int daysInMonth)
        {
            var pace = Math.Max(mtdDailyCash, recent7DailyCash);
            return Math.Round(pace * daysInMonth, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal ComputeWorstCaseCash(decimal mtdDailyCash, decimal recent7DailyCash, int daysInMonth)
        {
            var pace = Math.Min(mtdDailyCash, recent7DailyCash);
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

        public static string ResolveRecoveryForecastBand(decimal? collectionForecastPercent) =>
            ExecutiveSalesAchievementBandResolver.Resolve(collectionForecastPercent);

        public static string ResolveRequiredDailySeverity(decimal requiredDaily, decimal dailyCollectionAverage)
        {
            if (dailyCollectionAverage <= 0)
                return requiredDaily > 0
                    ? RequiredDailySeverityCritical
                    : RequiredDailySeverityNormal;

            var ratio = requiredDaily / dailyCollectionAverage;
            if (ratio > 2m)
                return RequiredDailySeverityCritical;

            if (ratio > 1.5m)
                return RequiredDailySeverityWarning;

            return RequiredDailySeverityNormal;
        }
    }
}
