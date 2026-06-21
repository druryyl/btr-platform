using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CustomerRiskRecommendationBuilder
    {
        public const string ManagementReview = "ManagementReview";
        public const string SuspendCreditReview = "SuspendCreditReview";
        public const string EarlyCollection = "EarlyCollection";
        public const string ReviewCredit = "ReviewCredit";
        public const string LegacyDebtReview = "LegacyDebtReview";
        public const string CallCustomer = "CallCustomer";
        public const string ScheduleVisit = "ScheduleVisit";
        public const string SalesRecovery = "SalesRecovery";
        public const string IncreaseMonitoring = "IncreaseMonitoring";
        public const string NoAction = "NoAction";

        private const string ReportRoutePiutang = "/reports/piutang";
        private const string ReportRouteSales = "/reports/sales";
        private const string DrillDownRoute = "/dashboard/customers";

        public sealed class RecommendationResult
        {
            public string RecommendationKey { get; set; }

            public string RecommendationLabel { get; set; }

            public string ReasonText { get; set; }

            public string RuleId { get; set; }

            public string ReportRoute { get; set; }

            public string DrillDownRoute { get; set; }
        }

        public static RecommendationResult Build(CustomerRiskForecastContext context)
        {
            if (context is null)
                return DefaultNoAction();

            var signals = context.Signals ?? new List<CustomerRiskSignalContext>();
            var category = context.Category ?? CustomerRiskForecastPolicy.CategoryHealthy;
            var hasSignal = new HashSet<string>(signals.Select(s => s.SignalKey));

            if (category == CustomerRiskForecastPolicy.CategoryCritical)
            {
                return Result(
                    ManagementReview,
                    "Management Review",
                    "Critical forecast risk — management review required.",
                    "CRF-REC-01");
            }

            if (hasSignal.Contains(CustomerRiskSignalBuilder.SignalProjectedPlafondBreach) &&
                (context.OverdueBalance > 0 || context.IsSuspended))
            {
                return Result(
                    SuspendCreditReview,
                    "Suspend Credit Review",
                    "Projected plafond breach with overdue exposure or suspended account with sales.",
                    "CRF-REC-02");
            }

            if (context.MinDaysUntilDue >= 0 &&
                context.MinDaysUntilDue <= CustomerRiskForecastPolicy.DueUrgentDays &&
                context.OpenBalance > 0)
            {
                return Result(
                    EarlyCollection,
                    "Early Collection Planning",
                    $"Balance due within {context.MinDaysUntilDue} days.",
                    "CRF-REC-03");
            }

            if (hasSignal.Contains(CustomerRiskSignalBuilder.SignalProjectedPlafondBreach) ||
                hasSignal.Contains(CustomerRiskSignalBuilder.SignalApproachingPlafond))
            {
                return Result(
                    ReviewCredit,
                    "Review Credit",
                    "Credit limit forecast requires review.",
                    "CRF-REC-04");
            }

            if (hasSignal.Contains(CustomerRiskSignalBuilder.SignalLegacyDebtForward))
            {
                return Result(
                    LegacyDebtReview,
                    "Legacy Debt Review",
                    "Approaching dormant customer with outstanding balance.",
                    "CRF-REC-05");
            }

            if (hasSignal.Contains(CustomerRiskSignalBuilder.SignalLikelyLatePayer) ||
                hasSignal.Contains(CustomerRiskSignalBuilder.SignalDueSoonSlowPayer))
            {
                return Result(
                    CallCustomer,
                    "Call Customer",
                    "Payment delay forecast — proactive collection call recommended.",
                    "CRF-REC-06");
            }

            if (hasSignal.Contains(CustomerRiskSignalBuilder.SignalApproachingDormant) ||
                hasSignal.Contains(CustomerRiskSignalBuilder.SignalImminentDormant))
            {
                return Result(
                    ScheduleVisit,
                    "Schedule Visit",
                    "Inactivity forecast — schedule sales visit.",
                    "CRF-REC-07");
            }

            if (hasSignal.Contains(CustomerRiskSignalBuilder.SignalSevereDecline) ||
                hasSignal.Contains(CustomerRiskSignalBuilder.SignalStoppedAfterHistory))
            {
                return Result(
                    SalesRecovery,
                    "Sales Recovery Campaign",
                    "Purchase decline forecast — sales recovery recommended.",
                    "CRF-REC-08",
                    ReportRouteSales);
            }

            if (category == CustomerRiskForecastPolicy.CategoryWatch &&
                signals.Count(s => s.Severity == CustomerRiskForecastPolicy.SeverityModerate) >= 2)
            {
                return Result(
                    IncreaseMonitoring,
                    "Increase Monitoring",
                    "Watch category with multiple moderate forecast signals.",
                    "CRF-REC-09");
            }

            return DefaultNoAction();
        }

        public static string ResolveLabel(string recommendationKey)
        {
            switch (recommendationKey)
            {
                case ManagementReview: return "Management Review";
                case SuspendCreditReview: return "Suspend Credit Review";
                case EarlyCollection: return "Early Collection Planning";
                case ReviewCredit: return "Review Credit";
                case LegacyDebtReview: return "Legacy Debt Review";
                case CallCustomer: return "Call Customer";
                case ScheduleVisit: return "Schedule Visit";
                case SalesRecovery: return "Sales Recovery Campaign";
                case IncreaseMonitoring: return "Increase Monitoring";
                case NoAction: return "Continue Monitoring";
                default: return recommendationKey ?? string.Empty;
            }
        }

        private static RecommendationResult DefaultNoAction() =>
            Result(NoAction, "Continue Monitoring", "No immediate action required.", "CRF-REC-10");

        private static RecommendationResult Result(
            string key,
            string label,
            string reason,
            string ruleId,
            string reportRoute = ReportRoutePiutang)
        {
            return new RecommendationResult
            {
                RecommendationKey = key,
                RecommendationLabel = label,
                ReasonText = reason,
                RuleId = ruleId,
                ReportRoute = reportRoute,
                DrillDownRoute = DrillDownRoute
            };
        }
    }
}
