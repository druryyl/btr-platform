using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    /// <summary>Maps M18 salesman attention signal keys to platform L3 catalog entries.</summary>
    public static class SalesmanAttentionSignalCatalog
    {
        public static void Register(IAttentionSignalRegistry registry)
        {
            if (registry == null)
                return;

            Register(registry, DashboardSalesmanAggregator.SignalBelowTarget, "Performance", "Below Target");
            Register(registry, DashboardSalesmanAggregator.SignalMissingTargetSetup, "Performance", "Missing Target Setup");
            Register(registry, DashboardSalesmanAggregator.SignalHighOverdueExposure, "Finance", "High Overdue Exposure");
            Register(registry, DashboardSalesmanAggregator.SignalHighPiutangExposure, "Finance", "High Piutang Exposure");
            Register(registry, DashboardSalesmanAggregator.SignalCustomerConcentration, "Portfolio", "Customer Concentration");
            Register(registry, DashboardSalesmanAggregator.SignalDormantCustomerPortfolio, "Portfolio", "Dormant Customer Portfolio");
        }

        private static void Register(
            IAttentionSignalRegistry registry,
            string signalCode,
            string category,
            string title)
        {
            registry.Register(EntityTypeCode.Salesman, new AttentionSignalDefinition
            {
                SignalCode = signalCode,
                SignalCategory = category,
                SignalTitle = title
            });
        }
    }
}
