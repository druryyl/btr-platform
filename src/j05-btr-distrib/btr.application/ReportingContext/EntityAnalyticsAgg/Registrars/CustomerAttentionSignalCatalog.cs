using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    /// <summary>Maps M17 customer attention signal keys to platform L3 catalog entries.</summary>
    public static class CustomerAttentionSignalCatalog
    {
        public static void Register(IAttentionSignalRegistry registry)
        {
            if (registry == null)
                return;

            Register(registry, DashboardCustomerAggregator.SignalOverdue, "Finance", "Overdue");
            Register(registry, DashboardCustomerAggregator.SignalDormant, "Activity", "Dormant");
            Register(registry, DashboardCustomerAggregator.SignalPlafondBreach, "Credit", "Plafond Breach");
            Register(registry, DashboardCustomerAggregator.SignalSuspendedWithSales, "Compliance", "Suspended + Sales");
        }

        private static void Register(
            IAttentionSignalRegistry registry,
            string signalCode,
            string category,
            string title)
        {
            registry.Register(EntityTypeCode.Customer, new AttentionSignalDefinition
            {
                SignalCode = signalCode,
                SignalCategory = category,
                SignalTitle = title
            });
        }
    }
}
