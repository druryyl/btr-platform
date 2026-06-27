using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Models;

namespace btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Services
{
    public static class FieldActivityStatusPolicy
    {
        public static FieldActivitySalesmanStatus Resolve(FieldActivityKpiInputResult kpis, bool hasEmail)
        {
            if (!hasEmail)
                return FieldActivitySalesmanStatus.NoFieldData;

            if (kpis == null)
                return FieldActivitySalesmanStatus.NoFieldData;

            if (kpis.PlannedVisits == 0 && kpis.ActualVisits == 0)
                return FieldActivitySalesmanStatus.NoPlan;

            if (kpis.PlannedVisits >= 1 && kpis.ActualVisits == 0)
                return FieldActivitySalesmanStatus.Critical;

            var execution = kpis.VisitExecutionPercent ?? 0;
            var effective = kpis.EffectiveCallRate ?? 0;

            if (execution < 50)
                return FieldActivitySalesmanStatus.Critical;

            if (execution >= 80 && effective >= 50)
                return FieldActivitySalesmanStatus.OnTrack;

            if ((execution >= 50 && execution < 80)
                || (effective >= 30 && effective < 50)
                || kpis.UnplannedVisits >= 3)
                return FieldActivitySalesmanStatus.NeedsAttention;

            return FieldActivitySalesmanStatus.OnTrack;
        }

        public static string ToStatusCode(FieldActivitySalesmanStatus status)
        {
            switch (status)
            {
                case FieldActivitySalesmanStatus.OnTrack:
                    return "OnTrack";
                case FieldActivitySalesmanStatus.NeedsAttention:
                    return "NeedsAttention";
                case FieldActivitySalesmanStatus.Critical:
                    return "Critical";
                case FieldActivitySalesmanStatus.NoPlan:
                    return "NoPlan";
                default:
                    return "NoFieldData";
            }
        }
    }
}
