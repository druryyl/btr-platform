using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CustomerRiskSignalBuilder
    {
        public const string SignalLikelyLatePayer = "LikelyLatePayer";
        public const string SignalEscalatingOverdue = "EscalatingOverdue";
        public const string SignalNoRecentPayment = "NoRecentPayment";
        public const string SignalDueSoonSlowPayer = "DueSoonSlowPayer";
        public const string SignalProjectedPlafondBreach = "ProjectedPlafondBreach";
        public const string SignalApproachingPlafond = "ApproachingPlafond";
        public const string SignalBreachedWorsening = "BreachedWorsening";
        public const string SignalApproachingDormant = "ApproachingDormant";
        public const string SignalImminentDormant = "ImminentDormant";
        public const string SignalLegacyDebtForward = "LegacyDebtForward";
        public const string SignalModerateDecline = "ModerateDecline";
        public const string SignalSevereDecline = "SevereDecline";
        public const string SignalStoppedAfterHistory = "StoppedAfterHistory";
        public const string SignalDueExposureConcentration = "DueExposureConcentration";
        public const string SignalChronicTrajectory = "ChronicTrajectory";
        public const string SignalLowRecoveryCustomer = "LowRecoveryCustomer";
        public const string SignalHighCollectionRisk = "HighCollectionRisk";

        private const string ReportRoute = "/reports/piutang";

        public static List<CustomerRiskSignalRow> Build(
            CustomerRiskForecastContext context,
            CustomerRiskForecastOptions options,
            decimal companyDueWithinHorizon,
            DateTime businessDate,
            IReadOnlyCollection<string> worstAgingBuckets,
            ISet<string> lowRecoverySalesmanIds)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            options = options ?? CustomerRiskForecastOptions.FromDashboardOptions(null);
            var today = businessDate.Date;
            var horizonEnd = today.AddDays(options.HorizonDays);
            var rows = new List<CustomerRiskSignalRow>();
            var added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void TryAdd(string signalKey, string signalLabel, string severity, string ruleId, string explanation, decimal? amount = null)
            {
                var dedupeKey = $"{context.CustomerKey}|{signalKey}";
                if (!added.Add(dedupeKey))
                    return;

                rows.Add(new CustomerRiskSignalRow
                {
                    CustomerKey = context.CustomerKey,
                    SignalKey = signalKey,
                    SignalLabel = signalLabel,
                    Severity = severity,
                    RuleId = ruleId,
                    Explanation = explanation,
                    Amount = amount
                });
            }

            var hasOpenBalance = context.OpenBalance > 1;
            var hasDueWithinHorizon = context.DueWithinHorizon > 0;
            var avgLag = context.AvgPaymentLagDays;

            if (hasOpenBalance && hasDueWithinHorizon &&
                avgLag.HasValue && avgLag.Value >= CustomerRiskForecastPolicy.AvgPaymentLagLikelyLateDays)
            {
                TryAdd(
                    SignalLikelyLatePayer,
                    "Likely Late Payer",
                    CustomerRiskForecastPolicy.SeverityModerate,
                    "CRF-P01",
                    $"Average payment lag is {avgLag.Value:F0} days with balance due within {options.HorizonDays} days.",
                    context.DueWithinHorizon);
            }

            if (hasOpenBalance && avgLag.HasValue &&
                avgLag.Value >= CustomerRiskForecastPolicy.AvgPaymentLagEscalatingDays &&
                worstAgingBuckets != null &&
                worstAgingBuckets.Any(b =>
                    string.Equals(b, "Days1To30", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(b, "Days31To60", StringComparison.OrdinalIgnoreCase)))
            {
                TryAdd(
                    SignalEscalatingOverdue,
                    "Escalating Overdue Trajectory",
                    CustomerRiskForecastPolicy.SeverityStrong,
                    "CRF-P02",
                    $"Overdue balance with average payment lag {avgLag.Value:F0} days.",
                    context.OverdueBalance);
            }

            if (hasOpenBalance &&
                (!context.DaysSinceLastPayment.HasValue ||
                 context.DaysSinceLastPayment.Value >= options.NoPaymentRecencyDays))
            {
                TryAdd(
                    SignalNoRecentPayment,
                    "No Recent Payment",
                    CustomerRiskForecastPolicy.SeverityWeak,
                    "CRF-P03",
                    $"No payment recorded in the last {options.NoPaymentRecencyDays} days.",
                    context.OpenBalance);
            }

            if (hasOpenBalance &&
                context.MinDaysUntilDue >= 0 &&
                context.MinDaysUntilDue <= CustomerRiskForecastPolicy.DueSoonSlowPayerDays &&
                avgLag.HasValue &&
                avgLag.Value >= CustomerRiskForecastPolicy.AvgPaymentLagLikelyLateDays)
            {
                TryAdd(
                    SignalDueSoonSlowPayer,
                    "Due Soon — Slow Payer",
                    CustomerRiskForecastPolicy.SeverityModerate,
                    "CRF-P04",
                    $"Balance due in {context.MinDaysUntilDue} days with average lag {avgLag.Value:F0} days.",
                    context.DueWithinHorizon);
            }

            if (context.Plafond > 0 && context.ProjectedOpenBalance > context.Plafond)
            {
                TryAdd(
                    SignalProjectedPlafondBreach,
                    "Projected Plafond Breach",
                    CustomerRiskForecastPolicy.SeverityStrong,
                    "CRF-C01",
                    $"Projected open balance {FormatAmount(context.ProjectedOpenBalance)} exceeds plafond {FormatAmount(context.Plafond)}.",
                    context.ProjectedOpenBalance - context.Plafond);
            }
            else if (context.Plafond > 0 &&
                     context.ProjectedOpenBalance >= context.Plafond * CustomerRiskForecastPolicy.ApproachingPlafondRatio)
            {
                TryAdd(
                    SignalApproachingPlafond,
                    "Approaching Plafond",
                    CustomerRiskForecastPolicy.SeverityModerate,
                    "CRF-C02",
                    $"Projected open balance {FormatAmount(context.ProjectedOpenBalance)} is at least 90% of plafond.",
                    context.ProjectedOpenBalance);
            }

            if (context.IsCurrentlyPlafondBreach && context.MtdOmzet > 0)
            {
                TryAdd(
                    SignalBreachedWorsening,
                    "Plafond Breach — Worsening",
                    CustomerRiskForecastPolicy.SeverityModerate,
                    "CRF-C03",
                    "Current plafond breach with active billing this month.",
                    context.OpenBalance - context.Plafond);
            }

            var daysSinceLast = context.DaysSinceLastFaktur;
            if (!context.IsActiveThisMonth && daysSinceLast.HasValue)
            {
                if (daysSinceLast.Value >= 60 && daysSinceLast.Value <= 79)
                {
                    TryAdd(
                        SignalApproachingDormant,
                        "Approaching Dormant",
                        CustomerRiskForecastPolicy.SeverityModerate,
                        "CRF-I01",
                        $"{daysSinceLast.Value} days since last purchase — approaching dormant threshold.",
                        null);
                }

                if (daysSinceLast.Value >= 80 && daysSinceLast.Value <= 89)
                {
                    TryAdd(
                        SignalImminentDormant,
                        "Imminent Dormant",
                        CustomerRiskForecastPolicy.SeverityStrong,
                        "CRF-I02",
                        $"{daysSinceLast.Value} days since last purchase — imminent dormant status.",
                        null);
                }

                if (daysSinceLast.Value >= CustomerRiskForecastPolicy.ApproachingDormantDaysMin &&
                    daysSinceLast.Value < CustomerRiskForecastPolicy.DormantDaysThreshold &&
                    hasOpenBalance)
                {
                    TryAdd(
                        SignalLegacyDebtForward,
                        "Legacy Debt + Inactivity",
                        CustomerRiskForecastPolicy.SeverityModerate,
                        "CRF-I03",
                        "Approaching dormant with outstanding balance.",
                        context.OpenBalance);
                }
            }

            if (context.PriorMonthOmzet >= options.PriorMonthOmzetFloorIdr && context.DeclineRatio.HasValue)
            {
                if (context.DeclineRatio.Value < CustomerRiskForecastPolicy.SevereDeclineRatio)
                {
                    TryAdd(
                        SignalSevereDecline,
                        "Severe Purchase Decline",
                        CustomerRiskForecastPolicy.SeverityStrong,
                        "CRF-D02",
                        $"Projected month billing is {context.DeclineRatio.Value:P0} of prior month.",
                        context.MtdOmzet);
                }
                else if (context.DeclineRatio.Value < CustomerRiskForecastPolicy.ModerateDeclineRatio)
                {
                    TryAdd(
                        SignalModerateDecline,
                        "Moderate Purchase Decline",
                        CustomerRiskForecastPolicy.SeverityModerate,
                        "CRF-D01",
                        $"Projected month billing is {context.DeclineRatio.Value:P0} of prior month.",
                        context.MtdOmzet);
                }
            }

            if (context.PriorMonthOmzet >= options.PriorMonthOmzetFloorIdr &&
                context.MtdOmzet <= 0 &&
                daysSinceLast.HasValue &&
                daysSinceLast.Value < CustomerRiskForecastPolicy.DormantDaysThreshold)
            {
                TryAdd(
                    SignalStoppedAfterHistory,
                    "Stopped After History",
                    CustomerRiskForecastPolicy.SeverityWeak,
                    "CRF-D03",
                    "Prior month billing exists but no billing recorded this month.",
                    null);
            }

            if (companyDueWithinHorizon > 0 &&
                context.DueWithinHorizon / companyDueWithinHorizon * 100m >=
                CustomerRiskForecastPolicy.DueConcentrationThresholdPercent)
            {
                TryAdd(
                    SignalDueExposureConcentration,
                    "Due Exposure Concentration",
                    CustomerRiskForecastPolicy.SeverityWeak,
                    "CRF-L02",
                    $"Due-within-horizon balance is {context.DueWithinHorizon / companyDueWithinHorizon * 100m:F1}% of company total.",
                    context.DueWithinHorizon);
            }

            if (context.HasChronicOverdue &&
                (!context.DaysSinceLastPayment.HasValue ||
                 context.DaysSinceLastPayment.Value >= options.NoPaymentRecencyDays))
            {
                TryAdd(
                    SignalChronicTrajectory,
                    "Chronic Trajectory",
                    CustomerRiskForecastPolicy.SeverityStrong,
                    "CRF-L03",
                    "Chronic overdue exposure with no recent payment.",
                    context.OverdueBalance);
            }

            if (lowRecoverySalesmanIds != null &&
                lowRecoverySalesmanIds.Count > 0 &&
                !string.IsNullOrWhiteSpace(context.SalesPersonId) &&
                lowRecoverySalesmanIds.Contains(context.SalesPersonId) &&
                context.OverdueBalance > 0)
            {
                TryAdd(
                    SignalLowRecoveryCustomer,
                    "Low Recovery Customer",
                    CustomerRiskForecastPolicy.SeverityModerate,
                    "CRF-L04",
                    "Assigned salesman has low recovery vs billing and customer is overdue.",
                    context.OverdueBalance);
            }

            return rows;
        }

        public static string ResolveSignalLabel(string signalKey)
        {
            switch (signalKey)
            {
                case SignalLikelyLatePayer: return "Likely Late Payer";
                case SignalEscalatingOverdue: return "Escalating Overdue Trajectory";
                case SignalNoRecentPayment: return "No Recent Payment";
                case SignalDueSoonSlowPayer: return "Due Soon — Slow Payer";
                case SignalProjectedPlafondBreach: return "Projected Plafond Breach";
                case SignalApproachingPlafond: return "Approaching Plafond";
                case SignalBreachedWorsening: return "Plafond Breach — Worsening";
                case SignalApproachingDormant: return "Approaching Dormant";
                case SignalImminentDormant: return "Imminent Dormant";
                case SignalLegacyDebtForward: return "Legacy Debt + Inactivity";
                case SignalModerateDecline: return "Moderate Purchase Decline";
                case SignalSevereDecline: return "Severe Purchase Decline";
                case SignalStoppedAfterHistory: return "Stopped After History";
                case SignalDueExposureConcentration: return "Due Exposure Concentration";
                case SignalChronicTrajectory: return "Chronic Trajectory";
                case SignalLowRecoveryCustomer: return "Low Recovery Customer";
                case SignalHighCollectionRisk: return "High Collection Risk";
                default: return signalKey ?? string.Empty;
            }
        }

        public static bool IsPaymentDelaySignal(string signalKey) =>
            signalKey == SignalLikelyLatePayer ||
            signalKey == SignalEscalatingOverdue ||
            signalKey == SignalNoRecentPayment ||
            signalKey == SignalDueSoonSlowPayer;

        public static bool IsCreditLimitSignal(string signalKey) =>
            signalKey == SignalProjectedPlafondBreach ||
            signalKey == SignalApproachingPlafond ||
            signalKey == SignalBreachedWorsening;

        public static bool IsInactivitySignal(string signalKey) =>
            signalKey == SignalApproachingDormant ||
            signalKey == SignalImminentDormant ||
            signalKey == SignalLegacyDebtForward;

        public static bool IsPurchaseDeclineSignal(string signalKey) =>
            signalKey == SignalModerateDecline ||
            signalKey == SignalSevereDecline ||
            signalKey == SignalStoppedAfterHistory;

        public static bool IsCollectionRiskSignal(string signalKey) =>
            signalKey == SignalDueExposureConcentration ||
            signalKey == SignalChronicTrajectory ||
            signalKey == SignalLowRecoveryCustomer ||
            signalKey == SignalHighCollectionRisk;

        private static string FormatAmount(decimal amount) =>
            amount.ToString("N0", CultureInfo.GetCultureInfo("id-ID"));
    }
}
