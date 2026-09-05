using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityMapPresetRegistryTest
    {
        [Fact]
        public void ReplenishmentRiskMap_UsesRecommendedPurchaseValueOnXAxis()
        {
            var preset = EntityMapPresetRegistry.TryGetPreset(EntityTypeCode.Item, "replenishment-risk-map");

            preset.Should().NotBeNull();
            preset.AxisXKpiId.Should().Be("IN-KPI-028");
            preset.AxisYKpiId.Should().Be("IN-KPI-020");
            preset.TooltipSupplementaryKpiId.Should().Be("IN-KPI-021");
            preset.FilterDimensionKpiId.Should().Be(EntityAnalyticsMetaKpiIds.SupplierName);
        }

        [Fact]
        public void InventoryHealthMap_FiltersByPrincipal()
        {
            var preset = EntityMapPresetRegistry.TryGetPreset(EntityTypeCode.Item, "inventory-health-map");

            preset.Should().NotBeNull();
            preset.FilterDimensionKpiId.Should().Be(EntityAnalyticsMetaKpiIds.SupplierName);
        }
    }
}
