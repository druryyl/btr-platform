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

        [Fact]
        public void PrincipalSalesOutMap_UsesSalesOutEncoding()
        {
            var preset = EntityMapPresetRegistry.TryGetPreset(EntityTypeCode.Supplier, "principal-sales-out-map");

            preset.Should().NotBeNull();
            preset.AxisXKpiId.Should().Be("PRN-TGT-003");
            preset.AxisYKpiId.Should().Be("PRN-GRW-002");
            preset.BubbleKpiId.Should().Be("PRN-SALES-001");
            preset.BubbleColorKpiId.Should().Be("PRN-RET-004");
            preset.FilterDimensionKpiId.Should().BeNull();
        }

        [Fact]
        public void SupplierDefaultPreset_IsPrincipalSalesOutMap()
        {
            var preset = EntityMapPresetRegistry.ResolveDefaultPreset(EntityTypeCode.Supplier);

            preset.Should().NotBeNull();
            preset.PresetId.Should().Be("principal-sales-out-map");
        }

        [Fact]
        public void SupplierPresets_RetainPurchasingMapsAlongsideSalesOutDefault()
        {
            var presets = EntityMapPresetRegistry.GetPresetsForEntityType(EntityTypeCode.Supplier);

            presets.Should().Contain(p => p.PresetId == "principal-sales-out-map");
            presets.Should().Contain(p => p.PresetId == "purchase-exposure-map");
            presets.Should().Contain(p => p.PresetId == "purchasing-discipline-map");
            presets.Should().ContainSingle(p => p.IsDefault)
                .Which.PresetId.Should().Be("principal-sales-out-map");
        }
    }
}
