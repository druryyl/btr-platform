using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CustomerPortfolioExecutiveSummaryBuilder
    {
        public static string Build(
            DashboardCustomerPortfolioKpiSnapshot kpi,
            DashboardCustomerPortfolioPriorityRow topPriority,
            DateTime businessDate)
        {
            if (kpi is null)
                return string.Empty;

            var lines = new List<string>
            {
                $"Customer Portfolio Optimization (as of {businessDate:yyyy-MM-dd}):",
                string.Empty,
                $"Portfolio health: {kpi.PortfolioHealthyPercent:N1}% healthy ({kpi.PortfolioHealthScore:N1} score from M29).",
                $"Attention customers: {kpi.AttentionCustomerCount} of {kpi.TotalCustomerCount} total.",
                $"Strategic at risk: {kpi.StrategicAtRiskCount}; customers at elevated forward risk: {kpi.CustomersAtRiskCount}.",
                $"Working capital tied in attention customers: Rp {kpi.WorkingCapitalTiedAmount:N0}.",
                string.Empty,
                $"Lifecycle mix — never purchased: {kpi.NeverPurchasedCount}, dormant: {kpi.DormantCount}, declining: {kpi.DecliningCount}.",
                CustomerPortfolioOptimizationPolicy.ValueDisclaimerText
            };

            if (topPriority != null)
            {
                lines.Insert(4,
                    $"Top portfolio priority: {topPriority.CustomerName} — {topPriority.PrimaryActionLabel} ({topPriority.ActionOwner}).");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
