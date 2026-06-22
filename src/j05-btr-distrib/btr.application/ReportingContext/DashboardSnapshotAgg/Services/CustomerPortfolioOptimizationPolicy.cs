using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CustomerPortfolioOptimizationPolicy
    {
        public const int NewCustomerDaysThreshold = 90;
        public const int PurchaseFrequencyLookbackMonths = 6;

        public const int StrategicOmzetRankMax = 10;
        public const int StrategicPiutangRankMax = 10;
        public const int HighValueOmzetRankMax = 20;
        public const int HighValuePiutangRankMax = 20;
        public const decimal StrategicOpenBalanceFloorIdr = 10_000_000m;
        public const decimal HighValueMtdOmzetFloorIdr = 5_000_000m;
        public const int HighFrequencyFakturCountMin = 4;

        public const string LifecycleNeverPurchased = "NeverPurchased";
        public const string LifecycleDormant = "Dormant";
        public const string LifecycleNew = "New";
        public const string LifecycleDeclining = "Declining";
        public const string LifecycleGrowing = "Growing";
        public const string LifecycleMature = "Mature";

        public const string TierStrategic = "Strategic";
        public const string TierHighValue = "HighValue";
        public const string TierMediumValue = "MediumValue";
        public const string TierLowValue = "LowValue";

        public const string ActionGrow = "Grow";
        public const string ActionRetain = "Retain";
        public const string ActionProtect = "Protect";
        public const string ActionCollect = "Collect";
        public const string ActionReviewCredit = "ReviewCredit";
        public const string ActionRecover = "Recover";
        public const string ActionMonitor = "Monitor";
        public const string ActionExitReview = "ExitReview";

        public const string OwnerSales = "Sales";
        public const string OwnerFinance = "Finance";
        public const string OwnerManagement = "Management";
        public const string OwnerManagementSales = "Management + Sales";

        public const string ValueDisclaimerText =
            "Customer Value = Omzet Proxy, NOT profitability.";

        public const string CustomerPortfolioDashboardRoute = "/dashboard/customer-portfolio";
        public const string CustomerReportRoute = "/reports/customers";
        public const string CustomerAnalyticsRoute = "/dashboard/customers";
        public const string CustomerRiskForecastRoute = "/dashboard/customer-risk-forecast";
        public const string CollectionOptimizationRoute = "/dashboard/collection-optimization";

        public static readonly IReadOnlyDictionary<string, string> LifecycleLabels =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [LifecycleNeverPurchased] = "Never Purchased",
                [LifecycleDormant] = "Dormant",
                [LifecycleNew] = "New",
                [LifecycleDeclining] = "Declining",
                [LifecycleGrowing] = "Growing",
                [LifecycleMature] = "Mature"
            };

        public static readonly IReadOnlyDictionary<string, string> TierLabels =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [TierStrategic] = "Strategic",
                [TierHighValue] = "High Value",
                [TierMediumValue] = "Medium Value",
                [TierLowValue] = "Low Value"
            };

        public static readonly IReadOnlyDictionary<string, string> ActionLabels =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [ActionGrow] = "Grow",
                [ActionRetain] = "Retain",
                [ActionProtect] = "Protect",
                [ActionCollect] = "Collect",
                [ActionReviewCredit] = "Review Credit",
                [ActionRecover] = "Recover",
                [ActionMonitor] = "Monitor",
                [ActionExitReview] = "Exit Review"
            };

        public static bool QualifiesForAttention(
            PortfolioCustomerContext ctx,
            HashSet<string> m17AttentionKeys)
        {
            if (ctx is null)
                return false;

            if (m17AttentionKeys != null && m17AttentionKeys.Contains(ctx.CustomerKey))
                return true;

            if (CompareCategory(ctx.M29Category, CustomerRiskForecastPolicy.CategoryHealthy) > 0)
                return true;

            if (!string.Equals(ctx.PrimaryActionKey, ActionMonitor, StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(ctx.LifecycleStage, LifecycleDeclining, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ctx.LifecycleStage, LifecycleDormant, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ctx.LifecycleStage, LifecycleNeverPurchased, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(ctx.PortfolioTier, TierStrategic, StringComparison.OrdinalIgnoreCase) &&
                CompareCategory(ctx.M29Category, CustomerRiskForecastPolicy.CategoryWatch) >= 0)
            {
                return true;
            }

            return false;
        }

        public static int ComputePortfolioPriorityScore(PortfolioCustomerContext ctx)
        {
            if (ctx is null)
                return 0;

            var actionWeight = ResolveActionWeight(ctx.PrimaryActionKey);
            var tierWeight = ResolveTierWeight(ctx.PortfolioTier);
            var categoryWeight = ResolveCategoryWeight(ctx.M29Category);
            var impact = Math.Min(200, (int)Math.Floor(Math.Min(ctx.OpenBalance, ctx.MtdOmzet) / 1_000_000m));

            return actionWeight + tierWeight + categoryWeight + impact;
        }

        public static string ResolveActionOwner(string actionKey)
        {
            switch (actionKey)
            {
                case ActionGrow:
                case ActionRetain:
                case ActionRecover:
                    return OwnerSales;
                case ActionCollect:
                case ActionReviewCredit:
                    return OwnerFinance;
                case ActionProtect:
                    return OwnerManagementSales;
                case ActionExitReview:
                case ActionMonitor:
                    return OwnerManagement;
                default:
                    return OwnerManagement;
            }
        }

        public static string ResolveLifecycleLabel(string lifecycleStage) =>
            LifecycleLabels.TryGetValue(lifecycleStage ?? string.Empty, out var label)
                ? label
                : lifecycleStage ?? string.Empty;

        public static string ResolveTierLabel(string tier) =>
            TierLabels.TryGetValue(tier ?? string.Empty, out var label)
                ? label
                : tier ?? string.Empty;

        public static string ResolveActionLabel(string actionKey) =>
            ActionLabels.TryGetValue(actionKey ?? string.Empty, out var label)
                ? label
                : actionKey ?? string.Empty;

        public static string BuildM30LinkRoute(string customerKey) =>
            $"{CollectionOptimizationRoute}?customerKey={Uri.EscapeDataString(customerKey ?? string.Empty)}";

        public static string BuildCustomerReportRoute(string customerCode) =>
            string.IsNullOrWhiteSpace(customerCode)
                ? CustomerReportRoute
                : $"{CustomerReportRoute}?customerCode={Uri.EscapeDataString(customerCode.Trim())}";

        public static int CompareCategory(string category, string baseline)
        {
            return ResolveCategoryOrder(category).CompareTo(ResolveCategoryOrder(baseline));
        }

        public static bool IsAtOrAboveCategory(string category, string baseline) =>
            CompareCategory(category, baseline) >= 0;

        private static int ResolveCategoryOrder(string category)
        {
            switch (category)
            {
                case CustomerRiskForecastPolicy.CategoryCritical:
                    return 5;
                case CustomerRiskForecastPolicy.CategoryHighRisk:
                    return 4;
                case CustomerRiskForecastPolicy.CategoryAttention:
                    return 3;
                case CustomerRiskForecastPolicy.CategoryWatch:
                    return 2;
                case CustomerRiskForecastPolicy.CategoryHealthy:
                    return 1;
                default:
                    return 0;
            }
        }

        private static int ResolveActionWeight(string actionKey)
        {
            switch (actionKey)
            {
                case ActionCollect: return 900;
                case ActionReviewCredit: return 850;
                case ActionExitReview: return 800;
                case ActionProtect: return 700;
                case ActionRetain: return 600;
                case ActionRecover: return 500;
                case ActionGrow: return 400;
                case ActionMonitor: return 100;
                default: return 0;
            }
        }

        private static int ResolveTierWeight(string tier)
        {
            switch (tier)
            {
                case TierStrategic: return 300;
                case TierHighValue: return 200;
                case TierMediumValue: return 100;
                default: return 0;
            }
        }

        private static int ResolveCategoryWeight(string category)
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
    }
}
