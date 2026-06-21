using System;
using System.Globalization;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CustomerRiskExecutiveSummaryBuilder
    {
        public static string Build(
            DashboardCustomerRiskForecastKpiSnapshot kpi,
            DashboardCustomerRiskForecastCustomerRow topCustomer,
            DateTime businessDate,
            int horizonDays,
            int daysElapsed,
            int severeDeclineCount)
        {
            if (kpi is null)
                return string.Empty;

            var elevatedCount = kpi.HighRiskCustomerCount + kpi.CriticalCustomerCount;
            var confidence = kpi.ForecastConfidence ?? string.Empty;
            var earlyMonthNote = confidence == CashFlowForecastPolicy.ConfidenceLow
                ? "Early-month forecast — billing and decline signals may shift as month progresses."
                : string.Empty;

            var topCustomerName = topCustomer?.CustomerName ?? "—";
            var topRecommendation = topCustomer?.RecommendationLabel ?? "—";
            var topCategory = topCustomer?.CategoryLabel ?? "—";

            var summary =
                $"Customer Risk Forecast (as of {businessDate:dd MMM yyyy}, {horizonDays}-day horizon):\n\n" +
                $"• {elevatedCount} customers are forecast at elevated collection risk\n" +
                $"• {kpi.PaymentDelaySignalCount} customers show likely payment delay patterns\n" +
                $"• {kpi.PurchaseDeclineSignalCount} customers show declining purchase activity ({severeDeclineCount} severe)\n" +
                $"• {kpi.InactivitySignalCount} customers are approaching dormant status\n" +
                $"• {kpi.CreditLimitSignalCount} customers may exceed credit limit at current billing pace\n" +
                $"• Elevated-risk receivables: approximately Rp {FormatAmount(kpi.ElevatedRiskReceivable)}\n\n" +
                $"Highest priority: {topCustomerName} — {topRecommendation} ({topCategory})\n" +
                $"Forecast confidence: {confidence} (based on {daysElapsed} days elapsed in month)";

            if (!string.IsNullOrEmpty(earlyMonthNote))
                summary += $"\n{earlyMonthNote}";

            return summary;
        }

        private static string FormatAmount(decimal value) =>
            value.ToString("N0", CultureInfo.InvariantCulture);
    }
}
