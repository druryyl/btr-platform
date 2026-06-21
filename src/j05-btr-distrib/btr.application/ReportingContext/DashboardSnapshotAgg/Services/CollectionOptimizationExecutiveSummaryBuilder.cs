using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CollectionOptimizationExecutiveSummaryBuilder
    {
        public static string Build(
            DashboardCollectionOptimizationKpiSnapshot kpi,
            DashboardCollectionOptimizationPriorityRow topPriority,
            DateTime businessDate,
            int daysElapsed,
            bool collectionContextUnavailable)
        {
            if (kpi is null)
                return string.Empty;

            var followUpCount = kpi.ImmediateCollectionCount + kpi.PriorityFollowUpCount;
            var recoveryLabel = ResolveRecoveryLabel(kpi.RecoveryVsBillingPercent, collectionContextUnavailable);
            var confidence = kpi.PlanningConfidence ?? CollectionOptimizationPolicy.ResolvePlanningConfidence(daysElapsed);

            var lines = new List<string>
            {
                $"Collection Optimization (as of {businessDate:yyyy-MM-dd}):",
                string.Empty,
                "Today's Collection Priorities:",
                $"• Contact {followUpCount} customers for collection follow-up ({kpi.ImmediateCollectionCount} immediate).",
                $"• Send proactive reminders to {kpi.ProactiveReminderCount} customers before due date.",
                $"• Review credit for {kpi.CreditReviewCount} customers before additional sales.",
                $"• Schedule sales recovery visits for {kpi.SalesRecoveryCount} declining accounts.",
                $"• Escalate {kpi.EscalateManagementCount} accounts to management review.",
                $"• Immediate and priority collection focus represents approximately Rp {kpi.ImmediateImpactTotal:N0} in overdue and due-this-week exposure.",
                string.Empty
            };

            if (topPriority != null)
            {
                lines.Add($"Top priority: {topPriority.CustomerName} — {topPriority.ActionCategoryLabel} (Rp {topPriority.CollectionImpactAmount:N0})");
            }

            lines.Add($"Recovery context: {FormatPercent(kpi.RecoveryVsBillingPercent)}% recovery vs billing MTD ({recoveryLabel})");
            lines.Add($"Planning confidence: {confidence} (based on {daysElapsed} days elapsed in month)");

            if (daysElapsed <= 5)
                lines.Add("Early-month plan — customer actions may shift as billing and payments progress.");

            if (collectionContextUnavailable)
                lines.Add("Collection context unavailable — recovery metrics may be stale until Collection refresh completes.");

            return string.Join(Environment.NewLine, lines);
        }

        private static string ResolveRecoveryLabel(decimal? recoveryPercent, bool unavailable)
        {
            if (unavailable)
                return "collection snapshot unavailable";
            if (!recoveryPercent.HasValue)
                return "no billing data";
            if (recoveryPercent.Value >= 80)
                return "strong recovery pace";
            if (recoveryPercent.Value >= 50)
                return "moderate recovery pace";
            return "recovery below billing pace";
        }

        private static string FormatPercent(decimal? value) =>
            value.HasValue ? value.Value.ToString("N1") : "0";
    }
}
