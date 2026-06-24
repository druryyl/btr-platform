using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// <summary>
    /// Runs all <see cref="IEntityAnalyticsRegistrar"/> implementations at application startup.
    /// </summary>
    public class EntityAnalyticsRegistryBootstrap
    {
        public EntityAnalyticsRegistryBootstrap(
            IEntityTypeRegistry entityTypes,
            IKpiRegistry kpiRegistry,
            IDimensionLabelRegistry dimensionLabels,
            IAttentionSignalRegistry attentionSignals,
            IRelationshipDefinitionRegistry relationshipDefinitions,
            IEnumerable<IEntityAnalyticsRegistrar> registrars)
        {
            foreach (var registrar in registrars ?? Enumerable.Empty<IEntityAnalyticsRegistrar>())
            {
                registrar.Register(entityTypes, kpiRegistry, dimensionLabels);
            }

            CustomerAttentionSignalCatalog.Register(attentionSignals);
            CustomerRelationshipCatalog.Register(relationshipDefinitions);
            SalesmanAttentionSignalCatalog.Register(attentionSignals);
            SalesmanRelationshipCatalog.Register(relationshipDefinitions);
            SupplierAttentionSignalCatalog.Register(attentionSignals);
            SupplierRelationshipCatalog.Register(relationshipDefinitions);
            ItemAttentionSignalCatalog.Register(attentionSignals);
            ItemRelationshipCatalog.Register(relationshipDefinitions);
        }
    }
}
