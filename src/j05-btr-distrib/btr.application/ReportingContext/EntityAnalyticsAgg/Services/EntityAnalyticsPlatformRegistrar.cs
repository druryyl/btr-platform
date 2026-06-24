using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// <summary>
    /// Registers platform entity type metadata. Does not register KPI definitions or pack contents.
    /// Entity-specific KPI packs are added by entity registrars in later milestones.
    /// </summary>
    public class EntityAnalyticsPlatformRegistrar : IEntityAnalyticsRegistrar
    {
        public void Register(IEntityTypeRegistry entityTypes, IKpiRegistry kpiRegistry, IDimensionLabelRegistry dimensionLabels)
        {
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = "customer-default",
                RelationshipPackId = "customer-relationships",
                PeerGroupRuleId = "customer-wilayah",
                WorkerDomainHook = "Customer",
                ProfileRouteTemplate = "/analytics/customers/{code}"
            });

            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Salesman,
                DisplayName = "Salesman",
                KpiPackId = "salesman-default",
                RelationshipPackId = "salesman-relationships",
                PeerGroupRuleId = "salesman-all-active",
                WorkerDomainHook = "Salesman",
                ProfileRouteTemplate = "/analytics/salesmen/{code}"
            });

            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Item,
                DisplayName = "Item",
                KpiPackId = "item-default",
                RelationshipPackId = "item-relationships",
                PeerGroupRuleId = "item-category",
                WorkerDomainHook = "InventoryRisk",
                ProfileRouteTemplate = "/analytics/items/{code}"
            });

            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Supplier",
                KpiPackId = "supplier-default",
                RelationshipPackId = "supplier-relationships",
                PeerGroupRuleId = "supplier-all-active",
                WorkerDomainHook = "PurchasingManagement",
                ProfileRouteTemplate = "/analytics/suppliers/{code}"
            });
        }
    }
}
