using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CustomerRiskForecastPolicy
    {
        public const int DefaultHorizonDays = 30;
        public const int ApproachingDormantDaysMin = 60;
        public const int DormantDaysThreshold = 90;
        public const int PaymentLagLookbackDays = 90;
        public const int MinSettledFaktursForLag = 2;
        public const decimal ModerateDeclineRatio = 0.70m;
        public const decimal SevereDeclineRatio = 0.50m;
        public const decimal ApproachingPlafondRatio = 0.90m;
        public const int AvgPaymentLagLikelyLateDays = 7;
        public const int AvgPaymentLagEscalatingDays = 14;
        public const int NoPaymentRecencyDays = 30;
        public const int DueSoonSlowPayerDays = 14;
        public const int DueUrgentDays = 7;
        public const decimal DueConcentrationThresholdPercent = 15m;

        public const string CategoryHealthy = "Healthy";
        public const string CategoryWatch = "Watch";
        public const string CategoryAttention = "Attention";
        public const string CategoryHighRisk = "HighRisk";
        public const string CategoryCritical = "Critical";

        public const string SeverityStrong = "Strong";
        public const string SeverityModerate = "Moderate";
        public const string SeverityWeak = "Weak";

        private const int CategoryWeightCritical = 1000;
        private const int CategoryWeightHighRisk = 800;
        private const int CategoryWeightAttention = 600;
        private const int CategoryWeightWatch = 400;

        public static string ResolveCategory(IReadOnlyList<CustomerRiskSignalContext> signals)
        {
            var list = signals ?? Array.Empty<CustomerRiskSignalContext>();
            var strong = CountSeverity(list, SeverityStrong);
            var moderate = CountSeverity(list, SeverityModerate);
            var weak = CountSeverity(list, SeverityWeak);

            var hasChronic = HasSignal(list, CustomerRiskSignalBuilder.SignalChronicTrajectory);
            var hasProjectedPlafondBreach = HasSignal(list, CustomerRiskSignalBuilder.SignalProjectedPlafondBreach);
            var hasSevereDecline = HasSignal(list, CustomerRiskSignalBuilder.SignalSevereDecline);
            var hasImminentDormant = HasSignal(list, CustomerRiskSignalBuilder.SignalImminentDormant);
            var hasLegacyForward = HasSignal(list, CustomerRiskSignalBuilder.SignalLegacyDebtForward);

            if (strong >= 3 ||
                (hasChronic && hasProjectedPlafondBreach && (hasSevereDecline || hasImminentDormant)) ||
                (hasLegacyForward && hasChronic))
            {
                return CategoryCritical;
            }

            if (strong >= 2 ||
                (strong >= 1 && moderate >= 2) ||
                (hasChronic && list.Count > 0))
            {
                return CategoryHighRisk;
            }

            if (strong == 1 || moderate >= 2)
                return CategoryAttention;

            if (moderate == 1 || weak >= 2)
                return CategoryWatch;

            return CategoryHealthy;
        }

        public static int ComputeRiskPriorityScore(
            string category,
            decimal openBalance,
            int strongCount,
            int moderateCount,
            int weakCount,
            int minDaysUntilDue)
        {
            var categoryWeight = ResolveCategoryWeight(category);
            var exposureComponent = Math.Min(300, (int)Math.Floor(openBalance / 1_000_000m) * 5);
            var signalComponent = strongCount * 50 + moderateCount * 25 + weakCount * 10;
            var dueUrgency = 0;
            if (minDaysUntilDue >= 0 && minDaysUntilDue <= DueUrgentDays)
                dueUrgency = 200;
            else if (minDaysUntilDue > DueUrgentDays && minDaysUntilDue <= DueSoonSlowPayerDays)
                dueUrgency = 100;

            return categoryWeight + exposureComponent + signalComponent + dueUrgency;
        }

        public static decimal ComputeProjectedOpenBalance(
            decimal currentOpen,
            decimal mtdOmzet,
            int daysElapsed,
            int horizonDays)
        {
            if (daysElapsed <= 0 || mtdOmzet <= 0)
                return currentOpen;

            var projectedBilling = Math.Round(mtdOmzet / daysElapsed * horizonDays, 2, MidpointRounding.AwayFromZero);
            return Math.Round(currentOpen + projectedBilling, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal ComputeProjectedMonthOmzet(decimal mtdOmzet, int daysElapsed, int daysInMonth)
        {
            if (daysElapsed <= 0)
                return 0m;

            return Math.Round(mtdOmzet / daysElapsed * daysInMonth, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal ComputePortfolioHealthScore(
            decimal elevatedRiskReceivable,
            decimal totalPiutang,
            int highRiskCount,
            int activeCustomerCount)
        {
            if (totalPiutang <= 0 && activeCustomerCount <= 0)
                return 100m;

            var receivableComponent = totalPiutang > 0
                ? elevatedRiskReceivable / totalPiutang * 50m
                : 0m;
            var customerComponent = activeCustomerCount > 0
                ? (decimal)highRiskCount / activeCustomerCount * 50m
                : 0m;

            var penalty = receivableComponent + customerComponent;
            if (penalty > 100m)
                penalty = 100m;

            return Math.Round(100m - penalty, 1, MidpointRounding.AwayFromZero);
        }

        public static string ResolveForecastConfidence(int daysElapsed)
        {
            return CashFlowForecastPolicy.ResolveConfidence(daysElapsed, 31);
        }

        public static string ResolveCategoryLabel(string category)
        {
            switch (category)
            {
                case CategoryHealthy:
                    return "Healthy";
                case CategoryWatch:
                    return "Watch";
                case CategoryAttention:
                    return "Attention";
                case CategoryHighRisk:
                    return "High Risk";
                case CategoryCritical:
                    return "Critical";
                default:
                    return category ?? string.Empty;
            }
        }

        private static int ResolveCategoryWeight(string category)
        {
            switch (category)
            {
                case CategoryCritical:
                    return CategoryWeightCritical;
                case CategoryHighRisk:
                    return CategoryWeightHighRisk;
                case CategoryAttention:
                    return CategoryWeightAttention;
                case CategoryWatch:
                    return CategoryWeightWatch;
                default:
                    return 0;
            }
        }

        private static int CountSeverity(IReadOnlyList<CustomerRiskSignalContext> signals, string severity) =>
            signals.Count(s => string.Equals(s.Severity, severity, StringComparison.OrdinalIgnoreCase));

        private static bool HasSignal(IReadOnlyList<CustomerRiskSignalContext> signals, string signalKey) =>
            signals.Any(s => string.Equals(s.SignalKey, signalKey, StringComparison.OrdinalIgnoreCase));
    }
}
