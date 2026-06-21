using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class InventoryOptimizationExecutiveSummaryBuilder
    {
        public static string Build(
            DateTime businessDate,
            DashboardInventoryOptimizationAggregateResult result)
        {
            if (result is null)
                return string.Empty;

            var criticalPurchases = result.ReorderList
                .Count(r => string.Equals(r.Category, InventoryOptimizationPolicy.CategoryCritical, StringComparison.OrdinalIgnoreCase));

            var requiredBudget = result.RequiredPurchaseBudgetIdr;
            var budgetCapLine = result.BudgetCapIdr.HasValue
                ? $" (budget cap Rp {result.BudgetCapIdr.Value:N0})"
                : string.Empty;

            var lines = new List<string>
            {
                $"Today's Recommendations (as of {businessDate:yyyy-MM-dd}):",
                string.Empty,
                $"• Purchase {criticalPurchases} critical products (est. Rp {requiredBudget:N0})",
                $"• Delay purchasing for {result.DelayCount} products",
                $"• Transfer inventory for {result.TransferCount} warehouse pairs",
                $"• Post {result.PostFirstCount} pending purchases before new orders",
                $"• Defer {result.DeferCount} lower-priority purchases{budgetCapLine}",
                $"• Review {result.ClearanceCount} dead stock items (Rp {result.RecoverableCapitalIdr:N0} recoverable)",
                string.Empty,
                $"Highest priority: {result.TopActionSummary ?? "No critical actions at this time."}"
            };

            return string.Join(Environment.NewLine, lines);
        }
    }
}
