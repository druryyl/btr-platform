using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntitySignatureDimensionIntegrityTest
    {
        [Theory]
        [InlineData(EntityTypeCode.Customer, CustomerEntityAnalyticsRegistrar.KpiPackId, typeof(CustomerEntityAnalyticsRegistrar))]
        [InlineData(EntityTypeCode.Salesman, SalesmanEntityAnalyticsRegistrar.KpiPackId, typeof(SalesmanEntityAnalyticsRegistrar))]
        [InlineData(EntityTypeCode.Supplier, SupplierEntityAnalyticsRegistrar.KpiPackId, typeof(SupplierEntityAnalyticsRegistrar))]
        [InlineData(EntityTypeCode.Item, ItemEntityAnalyticsRegistrar.KpiPackId, typeof(ItemEntityAnalyticsRegistrar))]
        public void Registrar_ExposesSixUniversalSignatureDimensionsInOrder(
            string entityType,
            string kpiPackId,
            System.Type registrarType)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = entityType,
                DisplayName = entityType,
                KpiPackId = kpiPackId
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var registrar = (IEntityAnalyticsRegistrar)System.Activator.CreateInstance(registrarType);
            registrar.Register(entityTypes, registry, new EntityAnalyticsDimensionLabelRegistry());

            var radarAxes = registry.ResolvePackMetadata(kpiPackId)
                .Where(m => m.RadarEligible
                    && !string.Equals(m.Direction, "Neutral", System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => EntityAnalyticsSignatureDimensions.GetOrderIndex(m.SignatureDimensionKey))
                .ThenBy(m => m.RadarAxisOrder)
                .ToList();

            radarAxes.Should().HaveCount(6);
            radarAxes.Select(a => a.SignatureDimensionKey).Should().Equal(EntityAnalyticsSignatureDimensions.OrderedKeys);
            radarAxes.Select(a => EntityAnalyticsSignatureDimensions.GetDisplayLabel(a.SignatureDimensionKey)).Should().Equal(EntityAnalyticsSignatureDimensions.OrderedLabels);
        }
    }
}
