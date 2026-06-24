using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CustomerPortfolioActionBuilder
    {
        private static readonly HashSet<string> CreditReviewRecommendations =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                CustomerRiskRecommendationBuilder.ReviewCredit,
                CustomerRiskRecommendationBuilder.SuspendCreditReview
            };

        public sealed class ActionResolution
        {
            public string PrimaryActionKey { get; set; }

            public string ActionOwner { get; set; }

            public string ActionReasonText { get; set; }

            public string TriggeredRuleIds { get; set; }

            public string M30LinkRoute { get; set; }
        }

        public static ActionResolution ResolvePrimaryAction(
            string lifecycleStage,
            string portfolioTier,
            string m29Category,
            CustomerRiskForecastContext forecast,
            CollectionOptimizationContext optimization,
            bool hasM17PlafondBreach,
            string m29RecommendationKey)
        {
            if (optimization != null &&
                CollectionOptimizationPolicy.IsActionable(optimization.ActionCategoryKey))
            {
                return new ActionResolution
                {
                    PrimaryActionKey = CustomerPortfolioOptimizationPolicy.ActionCollect,
                    ActionOwner = CustomerPortfolioOptimizationPolicy.ResolveActionOwner(
                        CustomerPortfolioOptimizationPolicy.ActionCollect),
                    ActionReasonText =
                        $"Collect — M30 action {CollectionOptimizationPolicy.ResolveActionCategoryLabel(optimization.ActionCategoryKey)} for {forecast?.CustomerName ?? "customer"}.",
                    TriggeredRuleIds = optimization.TriggeredRuleIds ?? "CPO-41",
                    M30LinkRoute = CustomerPortfolioOptimizationPolicy.BuildM30LinkRoute(forecast?.CustomerKey)
                };
            }

            if (optimization != null &&
                string.Equals(optimization.ActionCategoryKey, CollectionOptimizationPolicy.ActionCreditReview, StringComparison.OrdinalIgnoreCase))
            {
                return BuildReviewCredit("M30 credit review queue", "CPO-44-M30");
            }

            if (CreditReviewRecommendations.Contains(m29RecommendationKey ?? string.Empty))
            {
                return BuildReviewCredit("M29 credit recommendation", "CPO-44-M29");
            }

            if (hasM17PlafondBreach)
            {
                return BuildReviewCredit("M17 plafond breach", DashboardCustomerAggregator.SignalPlafondBreach);
            }

            if (string.Equals(portfolioTier, CustomerPortfolioOptimizationPolicy.TierLowValue, StringComparison.OrdinalIgnoreCase) &&
                CustomerPortfolioOptimizationPolicy.IsAtOrAboveCategory(m29Category, CustomerRiskForecastPolicy.CategoryHighRisk))
            {
                return new ActionResolution
                {
                    PrimaryActionKey = CustomerPortfolioOptimizationPolicy.ActionExitReview,
                    ActionOwner = CustomerPortfolioOptimizationPolicy.ResolveActionOwner(
                        CustomerPortfolioOptimizationPolicy.ActionExitReview),
                    ActionReasonText =
                        $"Exit Review — low portfolio tier with {CustomerRiskForecastPolicy.ResolveCategoryLabel(m29Category)} forward risk.",
                    TriggeredRuleIds = "CPO-44-ExitReview"
                };
            }

            if (string.Equals(portfolioTier, CustomerPortfolioOptimizationPolicy.TierStrategic, StringComparison.OrdinalIgnoreCase) &&
                CustomerPortfolioOptimizationPolicy.IsAtOrAboveCategory(m29Category, CustomerRiskForecastPolicy.CategoryWatch))
            {
                return new ActionResolution
                {
                    PrimaryActionKey = CustomerPortfolioOptimizationPolicy.ActionProtect,
                    ActionOwner = CustomerPortfolioOptimizationPolicy.ResolveActionOwner(
                        CustomerPortfolioOptimizationPolicy.ActionProtect),
                    ActionReasonText =
                        $"Protect — strategic customer with elevated {CustomerRiskForecastPolicy.ResolveCategoryLabel(m29Category)} risk.",
                    TriggeredRuleIds = "CPO-44-Protect"
                };
            }

            if ((string.Equals(portfolioTier, CustomerPortfolioOptimizationPolicy.TierStrategic, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(portfolioTier, CustomerPortfolioOptimizationPolicy.TierHighValue, StringComparison.OrdinalIgnoreCase)) &&
                string.Equals(lifecycleStage, CustomerPortfolioOptimizationPolicy.LifecycleDeclining, StringComparison.OrdinalIgnoreCase))
            {
                return new ActionResolution
                {
                    PrimaryActionKey = CustomerPortfolioOptimizationPolicy.ActionRetain,
                    ActionOwner = CustomerPortfolioOptimizationPolicy.ResolveActionOwner(
                        CustomerPortfolioOptimizationPolicy.ActionRetain),
                    ActionReasonText =
                        $"Retain — {CustomerPortfolioOptimizationPolicy.ResolveTierLabel(portfolioTier)} customer in declining lifecycle.",
                    TriggeredRuleIds = "CPO-44-Retain"
                };
            }

            if (string.Equals(lifecycleStage, CustomerPortfolioOptimizationPolicy.LifecycleDormant, StringComparison.OrdinalIgnoreCase))
            {
                return new ActionResolution
                {
                    PrimaryActionKey = CustomerPortfolioOptimizationPolicy.ActionRecover,
                    ActionOwner = CustomerPortfolioOptimizationPolicy.ResolveActionOwner(
                        CustomerPortfolioOptimizationPolicy.ActionRecover),
                    ActionReasonText = "Recover — dormant customer with prior purchase history.",
                    TriggeredRuleIds = DashboardCustomerAggregator.SignalDormant
                };
            }

            if ((string.Equals(lifecycleStage, CustomerPortfolioOptimizationPolicy.LifecycleNew, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(lifecycleStage, CustomerPortfolioOptimizationPolicy.LifecycleGrowing, StringComparison.OrdinalIgnoreCase)) &&
                CustomerPortfolioOptimizationPolicy.CompareCategory(m29Category, CustomerRiskForecastPolicy.CategoryWatch) <= 0)
            {
                return new ActionResolution
                {
                    PrimaryActionKey = CustomerPortfolioOptimizationPolicy.ActionGrow,
                    ActionOwner = CustomerPortfolioOptimizationPolicy.ResolveActionOwner(
                        CustomerPortfolioOptimizationPolicy.ActionGrow),
                    ActionReasonText =
                        $"Grow — {CustomerPortfolioOptimizationPolicy.ResolveLifecycleLabel(lifecycleStage)} customer with manageable forward risk.",
                    TriggeredRuleIds = "CPO-44-Grow"
                };
            }

            return new ActionResolution
            {
                PrimaryActionKey = CustomerPortfolioOptimizationPolicy.ActionMonitor,
                ActionOwner = CustomerPortfolioOptimizationPolicy.ResolveActionOwner(
                    CustomerPortfolioOptimizationPolicy.ActionMonitor),
                ActionReasonText =
                    $"Monitor — {CustomerPortfolioOptimizationPolicy.ResolveLifecycleLabel(lifecycleStage)} / {CustomerPortfolioOptimizationPolicy.ResolveTierLabel(portfolioTier)} portfolio profile.",
                TriggeredRuleIds = "CPO-44-Monitor"
            };
        }

        public static string BuildReasonText(
            string lifecycleStage,
            string portfolioTier,
            string m29Category,
            string primaryActionKey)
        {
            return $"{CustomerPortfolioOptimizationPolicy.ResolveActionLabel(primaryActionKey)} — " +
                   $"{CustomerPortfolioOptimizationPolicy.ResolveLifecycleLabel(lifecycleStage)}, " +
                   $"{CustomerPortfolioOptimizationPolicy.ResolveTierLabel(portfolioTier)}, " +
                   $"M29 {CustomerRiskForecastPolicy.ResolveCategoryLabel(m29Category)}.";
        }

        private static ActionResolution BuildReviewCredit(string source, string ruleId) =>
            new ActionResolution
            {
                PrimaryActionKey = CustomerPortfolioOptimizationPolicy.ActionReviewCredit,
                ActionOwner = CustomerPortfolioOptimizationPolicy.ResolveActionOwner(
                    CustomerPortfolioOptimizationPolicy.ActionReviewCredit),
                ActionReasonText = $"Review Credit — {source}.",
                TriggeredRuleIds = ruleId
            };
    }
}
