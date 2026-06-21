using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CollectionOptimizationActionBuilder
    {
        private const string ReportRoutePiutang = "/reports/piutang";
        private const string DrillDownRiskForecast = "/dashboard/customer-risk-forecast";

        public static string BuildSelectionReasonText(
            CustomerRiskForecastContext forecast,
            CollectionOptimizationContext opt,
            string primarySignalLabel)
        {
            if (forecast is null)
                return string.Empty;

            var parts = new List<string>();
            if (forecast.OverdueBalance > 0)
                parts.Add($"Overdue Rp {forecast.OverdueBalance:N0}");

            if (!string.IsNullOrWhiteSpace(forecast.Category))
                parts.Add($"{CustomerRiskForecastPolicy.ResolveCategoryLabel(forecast.Category)} forecast category");

            if (!string.IsNullOrWhiteSpace(primarySignalLabel))
                parts.Add(primarySignalLabel);

            if (forecast.MinDaysUntilDue >= 0 && forecast.MinDaysUntilDue <= 14 && forecast.OverdueBalance <= 0)
                parts.Add($"balance due in {forecast.MinDaysUntilDue} days");

            if (opt?.HasChronicOverdue == true)
                parts.Add("chronic overdue exposure");

            return parts.Count > 0
                ? string.Join(" with ", parts.Take(1)) + (parts.Count > 1 ? " and " + string.Join(", ", parts.Skip(1)) : string.Empty) + "."
                : "Customer selected for collection optimization review.";
        }

        public static string BuildPriorityReasonText(
            string actionCategory,
            decimal collectionImpactAmount,
            int minDaysUntilDue)
        {
            var components = new List<string>();

            if (CollectionOptimizationPolicy.IsImmediateOrPriority(actionCategory))
                components.Add("immediate collection category");
            else if (string.Equals(actionCategory, CollectionOptimizationPolicy.ActionProactiveReminder, StringComparison.OrdinalIgnoreCase))
                components.Add("proactive reminder category");
            else
                components.Add(CollectionOptimizationPolicy.ResolveActionCategoryLabel(actionCategory).ToLowerInvariant());

            if (collectionImpactAmount > 0)
                components.Add($"Rp {collectionImpactAmount:N0} impact");

            if (minDaysUntilDue >= 0 && minDaysUntilDue <= 7)
                components.Add($"balance due in {minDaysUntilDue} days");

            if (components.Count == 0)
                return "Ranked by collection priority score.";

            return "Ranked high due to " + string.Join(" and ", components.Take(2)) + ".";
        }

        public static string BuildActionReasonText(string actionCategory, CustomerRiskForecastContext forecast, string catRuleId)
        {
            var label = CollectionOptimizationPolicy.ResolveActionCategoryLabel(actionCategory);
            var categoryLabel = CustomerRiskForecastPolicy.ResolveCategoryLabel(forecast?.Category);

            switch (actionCategory)
            {
                case CollectionOptimizationPolicy.ActionImmediateCollection:
                    return $"{label} assigned ({catRuleId}): overdue balance with elevated forecast risk ({categoryLabel}).";
                case CollectionOptimizationPolicy.ActionEscalateManagement:
                    return $"{label} assigned ({catRuleId}): critical forecast risk requires management review.";
                case CollectionOptimizationPolicy.ActionPriorityFollowUp:
                    return $"{label} assigned ({catRuleId}): overdue with {categoryLabel} forecast category.";
                case CollectionOptimizationPolicy.ActionProactiveReminder:
                    return $"{label} assigned ({catRuleId}): current balance due soon with elevated forecast risk.";
                case CollectionOptimizationPolicy.ActionCreditReview:
                    return $"{label} assigned ({catRuleId}): credit limit review before additional sales.";
                case CollectionOptimizationPolicy.ActionSalesRecoveryVisit:
                    return $"{label} assigned ({catRuleId}): purchase decline — sales visit recommended before collection pressure.";
                case CollectionOptimizationPolicy.ActionLegacyDebtReview:
                    return $"{label} assigned ({catRuleId}): legacy debt requires specialized handling.";
                case CollectionOptimizationPolicy.ActionRelationshipMonitor:
                    return $"{label} assigned ({catRuleId}): strategic customer on watch — light touch monitoring.";
                case CollectionOptimizationPolicy.ActionDeferCollection:
                    return $"{label} assigned ({catRuleId}): healthy payer with no near-term due urgency.";
                default:
                    return $"No collection action required today ({catRuleId}).";
            }
        }

        public static string BuildTriggeredRuleIds(
            CustomerRiskForecastContext forecast,
            string catRuleId,
            string m29RuleId)
        {
            var ids = new List<string>();
            if (!string.IsNullOrWhiteSpace(m29RuleId))
                ids.Add(m29RuleId);

            var primary = forecast?.Signals?
                .OrderByDescending(s => SeverityRank(s.Severity))
                .FirstOrDefault();
            if (primary != null && !string.IsNullOrWhiteSpace(primary.RuleId) && !ids.Contains(primary.RuleId))
                ids.Add(primary.RuleId);

            if (!string.IsNullOrWhiteSpace(catRuleId))
                ids.Add(catRuleId);

            ids.Add("COL-OPT-REC-02");
            return string.Join(",", ids);
        }

        public static string BuildQueueReasonText(string actionCategory, CustomerRiskForecastContext forecast)
        {
            return BuildActionReasonText(actionCategory, forecast, CollectionOptimizationPolicy.ResolveCatRuleId(actionCategory));
        }

        public static DashboardCollectionOptimizationPriorityRow ToPriorityRow(
            CustomerRiskForecastContext forecast,
            CollectionOptimizationContext opt,
            int sortOrder)
        {
            var recommendedLabel = CollectionOptimizationPolicy.ResolveRecommendedActionLabel(opt.RecommendedActionKey);
            return new DashboardCollectionOptimizationPriorityRow
            {
                SortOrder = sortOrder,
                CollectionPriorityScore = opt.CollectionPriorityScore,
                CustomerCode = forecast.CustomerCode,
                CustomerName = forecast.CustomerName,
                WilayahName = forecast.WilayahName,
                SalesPersonName = forecast.SalesPersonName,
                Klasifikasi = opt.Klasifikasi ?? string.Empty,
                ActionCategoryKey = opt.ActionCategoryKey,
                ActionCategoryLabel = CollectionOptimizationPolicy.ResolveActionCategoryLabel(opt.ActionCategoryKey),
                RecommendedActionKey = opt.RecommendedActionKey,
                RecommendedActionLabel = recommendedLabel,
                ActionOwner = opt.ActionOwner,
                OpenBalance = forecast.OpenBalance,
                OverdueBalance = forecast.OverdueBalance,
                DueWithin7Days = opt.DueWithin7Days,
                CollectionImpactAmount = opt.CollectionImpactAmount,
                M29Category = CustomerRiskForecastPolicy.ResolveCategoryLabel(forecast.Category),
                M29RecommendationKey = opt.M29RecommendationKey,
                M29PrimarySignalKey = opt.M29PrimarySignalKey,
                MinDaysUntilDue = forecast.MinDaysUntilDue >= 0 ? forecast.MinDaysUntilDue : (int?)null,
                CreditUtilizationPercent = opt.CreditUtilizationPercent,
                SelectionReasonText = opt.SelectionReasonText,
                PriorityReasonText = opt.PriorityReasonText,
                ActionReasonText = opt.ActionReasonText,
                TriggeredRuleIds = opt.TriggeredRuleIds,
                ReportRoute = ReportRoutePiutang,
                DrillDownRoute = DrillDownRiskForecast
            };
        }

        public static DashboardCollectionOptimizationQueueRow ToQueueRow(
            string queueKey,
            CustomerRiskForecastContext forecast,
            CollectionOptimizationContext opt,
            int sortOrder)
        {
            return new DashboardCollectionOptimizationQueueRow
            {
                QueueKey = queueKey,
                SortOrder = sortOrder,
                CollectionPriorityScore = opt.CollectionPriorityScore,
                CustomerCode = forecast.CustomerCode,
                CustomerName = forecast.CustomerName,
                WilayahName = forecast.WilayahName,
                SalesPersonName = forecast.SalesPersonName,
                ActionCategoryKey = opt.ActionCategoryKey,
                ActionCategoryLabel = CollectionOptimizationPolicy.ResolveActionCategoryLabel(opt.ActionCategoryKey),
                RecommendedActionKey = opt.RecommendedActionKey,
                RecommendedActionLabel = CollectionOptimizationPolicy.ResolveRecommendedActionLabel(opt.RecommendedActionKey),
                ActionOwner = opt.ActionOwner,
                OverdueBalance = forecast.OverdueBalance,
                DueWithin7Days = opt.DueWithin7Days,
                CollectionImpactAmount = opt.CollectionImpactAmount,
                M29Category = CustomerRiskForecastPolicy.ResolveCategoryLabel(forecast.Category),
                QueueReasonText = opt.ActionReasonText,
                ReportRoute = ReportRoutePiutang,
                DrillDownRoute = DrillDownRiskForecast
            };
        }

        public static DashboardCollectionOptimizationImpactRow ToImpactRow(
            CustomerRiskForecastContext forecast,
            CollectionOptimizationContext opt,
            int sortOrder)
        {
            return new DashboardCollectionOptimizationImpactRow
            {
                SortOrder = sortOrder,
                CustomerCode = forecast.CustomerCode,
                CustomerName = forecast.CustomerName,
                WilayahName = forecast.WilayahName,
                SalesPersonName = forecast.SalesPersonName,
                ActionCategoryKey = opt.ActionCategoryKey,
                ActionCategoryLabel = CollectionOptimizationPolicy.ResolveActionCategoryLabel(opt.ActionCategoryKey),
                CollectionImpactAmount = opt.CollectionImpactAmount,
                OverdueBalance = forecast.OverdueBalance,
                DueWithin7Days = opt.DueWithin7Days,
                ReportRoute = ReportRoutePiutang,
                DrillDownRoute = DrillDownRiskForecast
            };
        }

        private static int SeverityRank(string severity)
        {
            if (severity == CustomerRiskForecastPolicy.SeverityStrong) return 3;
            if (severity == CustomerRiskForecastPolicy.SeverityModerate) return 2;
            if (severity == CustomerRiskForecastPolicy.SeverityWeak) return 1;
            return 0;
        }
    }
}
