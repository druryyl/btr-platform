using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CollectionOptimizationPolicy
    {
        public const decimal SalesRecoveryOverdueFloorIdr = 500_000m;
        public const decimal StrategicPriorMonthOmzetFloorIdr = 5_000_000m;
        public const int TopStrategicOmzetRank = 10;
        public const int SecondaryStrategicOmzetRank = 20;
        public const decimal ImpactConcentrationThresholdPercent = 10m;

        public const string ActionImmediateCollection = "ImmediateCollection";
        public const string ActionEscalateManagement = "EscalateManagement";
        public const string ActionPriorityFollowUp = "PriorityFollowUp";
        public const string ActionProactiveReminder = "ProactiveReminder";
        public const string ActionCreditReview = "CreditReview";
        public const string ActionSalesRecoveryVisit = "SalesRecoveryVisit";
        public const string ActionLegacyDebtReview = "LegacyDebtReview";
        public const string ActionRelationshipMonitor = "RelationshipMonitor";
        public const string ActionDeferCollection = "DeferCollection";
        public const string ActionNoActionToday = "NoActionToday";

        public const string OwnerCollection = "Collection";
        public const string OwnerSales = "Sales";
        public const string OwnerFinance = "Finance";
        public const string OwnerManagement = "Management";

        private const int WeightImmediateCollection = 1000;
        private const int WeightEscalateManagement = 950;
        private const int WeightPriorityFollowUp = 800;
        private const int WeightProactiveReminder = 600;
        private const int WeightCreditReview = 550;
        private const int WeightSalesRecoveryVisit = 500;
        private const int WeightLegacyDebtReview = 450;
        private const int WeightRelationshipMonitor = 300;
        private const int WeightDeferCollection = 100;

        public static readonly IReadOnlyDictionary<string, string> ActionCategoryLabels =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [ActionImmediateCollection] = "Immediate Collection",
                [ActionEscalateManagement] = "Escalate to Management",
                [ActionPriorityFollowUp] = "Priority Follow-up",
                [ActionProactiveReminder] = "Send Reminder",
                [ActionCreditReview] = "Credit Review",
                [ActionSalesRecoveryVisit] = "Schedule Sales Visit",
                [ActionLegacyDebtReview] = "Legacy Debt Review",
                [ActionRelationshipMonitor] = "Continue Monitoring",
                [ActionDeferCollection] = "Safe to Wait",
                [ActionNoActionToday] = "No Action Today"
            };

        public static readonly IReadOnlyDictionary<string, string> RecommendedActionLabels =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["CallCustomer"] = "Call Customer",
                [ActionEscalateManagement] = "Escalate to Management",
                ["SendReminder"] = "Send Reminder",
                ["ReviewCredit"] = "Review Credit",
                ["ScheduleVisit"] = "Schedule Visit",
                [ActionLegacyDebtReview] = "Legacy Debt Review",
                ["IncreaseMonitoring"] = "Increase Monitoring",
                ["DelayCollection"] = "Delay Collection",
                ["NoAction"] = "No Action Today"
            };

        public static string ResolveActionCategory(
            CustomerRiskForecastContext forecast,
            CollectionOptimizationEnrichment enrichment,
            string m29RecommendationKey,
            decimal salesRecoveryOverdueFloorIdr)
        {
            if (forecast is null)
                return ActionNoActionToday;

            enrichment = enrichment ?? new CollectionOptimizationEnrichment();
            var category = forecast.Category ?? CustomerRiskForecastPolicy.CategoryHealthy;
            var overdue = forecast.OverdueBalance;
            var minDays = forecast.MinDaysUntilDue;
            var hasChronic = forecast.HasChronicOverdue || enrichment.HasChronicOverdue;

            if (overdue > 0 &&
                (category == CustomerRiskForecastPolicy.CategoryHighRisk ||
                 category == CustomerRiskForecastPolicy.CategoryCritical ||
                 hasChronic))
            {
                return ActionImmediateCollection;
            }

            if (category == CustomerRiskForecastPolicy.CategoryCritical ||
                string.Equals(m29RecommendationKey, CustomerRiskRecommendationBuilder.ManagementReview, StringComparison.OrdinalIgnoreCase))
            {
                return ActionEscalateManagement;
            }

            if (overdue > 0 &&
                (category == CustomerRiskForecastPolicy.CategoryAttention ||
                 category == CustomerRiskForecastPolicy.CategoryHighRisk))
            {
                return ActionPriorityFollowUp;
            }

            if (overdue <= 0 &&
                minDays >= 1 && minDays <= 14 &&
                (category == CustomerRiskForecastPolicy.CategoryWatch ||
                 category == CustomerRiskForecastPolicy.CategoryAttention ||
                 category == CustomerRiskForecastPolicy.CategoryHighRisk ||
                 category == CustomerRiskForecastPolicy.CategoryCritical))
            {
                return ActionProactiveReminder;
            }

            if (string.Equals(m29RecommendationKey, CustomerRiskRecommendationBuilder.ReviewCredit, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m29RecommendationKey, CustomerRiskRecommendationBuilder.SuspendCreditReview, StringComparison.OrdinalIgnoreCase))
            {
                return ActionCreditReview;
            }

            if ((string.Equals(m29RecommendationKey, CustomerRiskRecommendationBuilder.SalesRecovery, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(m29RecommendationKey, CustomerRiskRecommendationBuilder.ScheduleVisit, StringComparison.OrdinalIgnoreCase)) &&
                overdue < salesRecoveryOverdueFloorIdr)
            {
                return ActionSalesRecoveryVisit;
            }

            if (string.Equals(m29RecommendationKey, CustomerRiskRecommendationBuilder.LegacyDebtReview, StringComparison.OrdinalIgnoreCase))
            {
                return ActionLegacyDebtReview;
            }

            if (category == CustomerRiskForecastPolicy.CategoryWatch && enrichment.IsTop10MtdOmzet)
            {
                return ActionRelationshipMonitor;
            }

            if (category == CustomerRiskForecastPolicy.CategoryHealthy &&
                overdue <= 0 &&
                (minDays > 14 || minDays < 0 || forecast.OpenBalance <= 0))
            {
                if (enrichment.IsTop10MtdOmzet &&
                    category == CustomerRiskForecastPolicy.CategoryWatch)
                {
                    return ActionRelationshipMonitor;
                }

                return ActionDeferCollection;
            }

            if (enrichment.IsTop10MtdOmzet &&
                category == CustomerRiskForecastPolicy.CategoryWatch &&
                forecast.Signals != null &&
                forecast.Signals.Count > 0)
            {
                return ActionRelationshipMonitor;
            }

            return ActionNoActionToday;
        }

        public static string ResolveCatRuleId(string actionCategory)
        {
            switch (actionCategory)
            {
                case ActionImmediateCollection: return "COL-OPT-CAT-01";
                case ActionEscalateManagement: return "COL-OPT-CAT-02";
                case ActionPriorityFollowUp: return "COL-OPT-CAT-03";
                case ActionProactiveReminder: return "COL-OPT-CAT-04";
                case ActionCreditReview: return "COL-OPT-CAT-05";
                case ActionSalesRecoveryVisit: return "COL-OPT-CAT-06";
                case ActionLegacyDebtReview: return "COL-OPT-CAT-07";
                case ActionRelationshipMonitor: return "COL-OPT-CAT-08";
                case ActionDeferCollection: return "COL-OPT-CAT-09";
                default: return "COL-OPT-CAT-10";
            }
        }

        public static string ResolveRecommendedActionKey(string actionCategory)
        {
            switch (actionCategory)
            {
                case ActionImmediateCollection:
                case ActionPriorityFollowUp:
                    return "CallCustomer";
                case ActionEscalateManagement:
                    return ActionEscalateManagement;
                case ActionProactiveReminder:
                    return "SendReminder";
                case ActionCreditReview:
                    return "ReviewCredit";
                case ActionSalesRecoveryVisit:
                    return "ScheduleVisit";
                case ActionLegacyDebtReview:
                    return ActionLegacyDebtReview;
                case ActionRelationshipMonitor:
                    return "IncreaseMonitoring";
                case ActionDeferCollection:
                    return "DelayCollection";
                default:
                    return "NoAction";
            }
        }

        public static int ComputeCollectionPriorityScore(
            string actionCategory,
            CustomerRiskForecastContext forecast,
            CollectionOptimizationEnrichment enrichment,
            decimal collectionImpactAmount)
        {
            var score = GetActionCategoryWeight(actionCategory);
            score += GetRiskCategoryWeight(forecast?.Category);
            score += Math.Min(350, (int)Math.Floor(collectionImpactAmount / 1_000_000m) * 8);
            score += GetDueUrgencyComponent(forecast?.MinDaysUntilDue ?? -1);

            enrichment = enrichment ?? new CollectionOptimizationEnrichment();
            if (enrichment.IsTop10MtdOmzet)
                score += 120;
            else if (enrichment.IsTop20MtdOmzet)
                score += 60;

            if (enrichment.HasChronicOverdue)
                score += 80;
            else if (enrichment.HasPlafondBreachOverdueSignal)
                score += 60;
            else if (enrichment.HasLegacyDebtSignal)
                score += 40;

            return score;
        }

        public static decimal ComputeCollectionImpactAmount(decimal overdueBalance, decimal dueWithin7Days) =>
            overdueBalance + dueWithin7Days;

        public static string ResolveActionOwner(string actionCategory)
        {
            switch (actionCategory)
            {
                case ActionSalesRecoveryVisit:
                case ActionRelationshipMonitor:
                    return OwnerSales;
                case ActionCreditReview:
                    return OwnerFinance;
                case ActionEscalateManagement:
                    return OwnerManagement;
                case ActionDeferCollection:
                case ActionNoActionToday:
                    return string.Empty;
                default:
                    return OwnerCollection;
            }
        }

        public static string ResolvePlanningConfidence(int daysElapsed)
        {
            return CashFlowForecastPolicy.ResolveConfidence(daysElapsed, 31);
        }

        public static string ResolveActionCategoryLabel(string actionCategory)
        {
            return ActionCategoryLabels.TryGetValue(actionCategory ?? string.Empty, out var label)
                ? label
                : actionCategory ?? string.Empty;
        }

        public static string ResolveRecommendedActionLabel(string recommendedActionKey)
        {
            return RecommendedActionLabels.TryGetValue(recommendedActionKey ?? string.Empty, out var label)
                ? label
                : recommendedActionKey ?? string.Empty;
        }

        public static bool IsActionable(string actionCategory) =>
            !string.Equals(actionCategory, ActionDeferCollection, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(actionCategory, ActionNoActionToday, StringComparison.OrdinalIgnoreCase);

        public static bool IsImmediateOrPriority(string actionCategory) =>
            string.Equals(actionCategory, ActionImmediateCollection, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(actionCategory, ActionPriorityFollowUp, StringComparison.OrdinalIgnoreCase);

        private static int GetActionCategoryWeight(string actionCategory)
        {
            switch (actionCategory)
            {
                case ActionImmediateCollection: return WeightImmediateCollection;
                case ActionEscalateManagement: return WeightEscalateManagement;
                case ActionPriorityFollowUp: return WeightPriorityFollowUp;
                case ActionProactiveReminder: return WeightProactiveReminder;
                case ActionCreditReview: return WeightCreditReview;
                case ActionSalesRecoveryVisit: return WeightSalesRecoveryVisit;
                case ActionLegacyDebtReview: return WeightLegacyDebtReview;
                case ActionRelationshipMonitor: return WeightRelationshipMonitor;
                case ActionDeferCollection: return WeightDeferCollection;
                default: return 0;
            }
        }

        private static int GetRiskCategoryWeight(string category)
        {
            switch (category)
            {
                case CustomerRiskForecastPolicy.CategoryCritical: return 400;
                case CustomerRiskForecastPolicy.CategoryHighRisk: return 320;
                case CustomerRiskForecastPolicy.CategoryAttention: return 240;
                case CustomerRiskForecastPolicy.CategoryWatch: return 160;
                default: return 0;
            }
        }

        private static int GetDueUrgencyComponent(int minDaysUntilDue)
        {
            if (minDaysUntilDue >= 0 && minDaysUntilDue <= 3)
                return 200;
            if (minDaysUntilDue >= 0 && minDaysUntilDue <= 7)
                return 150;
            if (minDaysUntilDue >= 0 && minDaysUntilDue <= 14)
                return 80;
            return 0;
        }
    }

    public sealed class CollectionOptimizationEnrichment
    {
        public decimal DueWithin7Days { get; set; }

        public decimal DueWithin14Days { get; set; }

        public bool HasChronicOverdue { get; set; }

        public bool HasLegacyDebtSignal { get; set; }

        public bool HasPlafondBreachOverdueSignal { get; set; }

        public bool SalesmanLowRecovery { get; set; }

        public bool IsTop10MtdOmzet { get; set; }

        public bool IsTop20MtdOmzet { get; set; }

        public string Klasifikasi { get; set; }
    }
}
