using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Registrars
{
    public static class ItemAttentionSignalCatalog
    {
        public static void Register(IAttentionSignalRegistry registry)
        {
            if (registry == null)
                return;

            Register(registry, DashboardInventoryRiskAggregator.SignalNeverSold, "Activity", "Never Sold");
            Register(registry, DashboardInventoryRiskAggregator.SignalSlowMoving, "Risk", "Slow Moving");
            Register(registry, DashboardInventoryRiskAggregator.SignalDeadStock, "Risk", "Dead Stock");
            Register(registry, InventoryForecastRiskBuilder.SignalOverstockRisk, "Risk", "Overstock Risk");
            Register(registry, InventoryForecastRiskBuilder.SignalStockOutRisk, "Risk", "Stock-Out Risk");
            Register(registry, InventoryForecastRiskBuilder.SignalCriticalStockOut, "Risk", "Critical Stock-Out");
        }

        private static void Register(
            IAttentionSignalRegistry registry,
            string signalCode,
            string category,
            string title)
        {
            registry.Register(EntityTypeCode.Item, new AttentionSignalDefinition
            {
                SignalCode = signalCode,
                SignalCategory = category,
                SignalTitle = title
            });
        }
    }
}
