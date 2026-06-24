using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    public static class SupplierAttentionSignalCatalog
    {
        public static void Register(IAttentionSignalRegistry registry)
        {
            if (registry == null)
                return;

            Register(registry, DashboardPurchasingManagementAggregator.SignalQualifiedBacklog, "Quality", "Qualified Backlog");
            Register(registry, DashboardPurchasingManagementAggregator.SignalPrincipalSpendConcentration, "Contribution", "Spend Concentration");
            Register(registry, DashboardPurchasingManagementAggregator.SignalPrincipalInventoryConcentration, "Portfolio", "Inventory Concentration");
            Register(registry, DashboardPurchasingManagementAggregator.SignalPrincipalAtRiskExposure, "Risk", "At-Risk Exposure");
            Register(registry, DashboardPurchasingManagementAggregator.SignalCompoundDependency, "Risk", "Compound Dependency");
            Register(registry, DashboardPurchasingManagementAggregator.SignalPrincipalInventoryNoPurchase, "Activity", "Inventory, No Purchase");
            Register(registry, DashboardPurchasingManagementAggregator.SignalUnknownPrincipal, "Quality", "Unknown Principal");
        }

        private static void Register(
            IAttentionSignalRegistry registry,
            string signalCode,
            string category,
            string title)
        {
            registry.Register(EntityTypeCode.Supplier, new AttentionSignalDefinition
            {
                SignalCode = signalCode,
                SignalCategory = category,
                SignalTitle = title
            });
        }
    }
}
