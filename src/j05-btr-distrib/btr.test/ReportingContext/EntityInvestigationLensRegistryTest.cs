using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityInvestigationLensRegistryTest
    {
        [Fact]
        public void Supplier_DefinesExactlyTwoLenses_WithSalesOutDefault()
        {
            var lenses = EntityInvestigationLensRegistry.GetLensesForEntityType(EntityTypeCode.Supplier);

            lenses.Should().HaveCount(2);
            lenses.Select(l => l.LensId).Should().BeEquivalentTo(new[]
            {
                EntityInvestigationLensIds.SalesOut,
                EntityInvestigationLensIds.Purchasing
            });
            lenses.Count(l => l.IsDefault).Should().Be(1);
            lenses.Single(l => l.IsDefault).LensId.Should().Be(EntityInvestigationLensIds.SalesOut);
            EntityInvestigationLensRegistry.ResolveDefaultLens(EntityTypeCode.Supplier).LensId
                .Should().Be(EntityInvestigationLensIds.SalesOut);
        }

        [Fact]
        public void SalesOutLens_OwnsApprovedKpiSet_AndDefaultPreset()
        {
            var lens = EntityInvestigationLensRegistry.TryGetLens(
                EntityTypeCode.Supplier, EntityInvestigationLensIds.SalesOut);

            lens.Should().NotBeNull();
            lens.DefaultPresetId.Should().Be("principal-sales-out-map");
            lens.KpiIds.Should().BeEquivalentTo(new[]
            {
                PrincipalKpiCatalog.SalesOutId,
                PrincipalKpiCatalog.MomGrowthId,
                PrincipalKpiCatalog.YoyGrowthId,
                PrincipalKpiCatalog.GoodReturnAmountId,
                PrincipalKpiCatalog.BrokenReturnAmountId,
                PrincipalKpiCatalog.TotalReturnAmountId,
                PrincipalKpiCatalog.ReturnPercentageId,
                PrincipalKpiCatalog.AchievementAmountId,
                PrincipalKpiCatalog.AchievementPercentageId,
                PrincipalKpiCatalog.ActiveCustomerCountId,
                PrincipalKpiCatalog.CustomerCoverageId
            });
        }

        [Fact]
        public void PurchasingLens_OwnsApprovedKpiSet_DerivedRatio_AndDefaultPreset()
        {
            var lens = EntityInvestigationLensRegistry.TryGetLens(
                EntityTypeCode.Supplier, EntityInvestigationLensIds.Purchasing);

            lens.Should().NotBeNull();
            lens.DefaultPresetId.Should().Be("purchase-exposure-map");
            lens.KpiIds.Should().BeEquivalentTo(new[]
            {
                PrincipalKpiCatalog.PurchaseInId,
                "PU-KPI-001",
                PrincipalKpiCatalog.InventoryValueId,
                PrincipalKpiCatalog.InventoryDaysId
            });
            lens.DerivedMetricIds.Should().ContainSingle()
                .Which.Should().Be(EntityInvestigationDerivedMetricIds.PurchaseToSalesOutRatio);
        }

        [Fact]
        public void LensConfiguration_IntroducesNoNewKpiId()
        {
            var derived = EntityInvestigationDerivedMetricIds.PurchaseToSalesOutRatio;

            derived.Should().NotStartWith("PRN-");
            PrincipalKpiCatalog.Entries.Select(e => e.KpiId).Should().NotContain(derived);
        }

        [Fact]
        public void PurchaseInAndInventory_AreNeverPartOfSalesOutLens()
        {
            var salesOut = EntityInvestigationLensRegistry.TryGetLens(
                EntityTypeCode.Supplier, EntityInvestigationLensIds.SalesOut);

            salesOut.KpiIds.Should().NotContain(PrincipalKpiCatalog.PurchaseInId);
            salesOut.KpiIds.Should().NotContain(PrincipalKpiCatalog.InventoryValueId);
            salesOut.KpiIds.Should().NotContain(PrincipalKpiCatalog.InventoryDaysId);
        }

        [Fact]
        public void UnknownEntityType_HasNoLensConfiguration()
        {
            EntityInvestigationLensRegistry.GetLensesForEntityType(EntityTypeCode.Customer)
                .Should().BeEmpty();
            EntityInvestigationLensRegistry.ResolveDefaultLens(EntityTypeCode.Customer)
                .Should().BeNull();
        }
    }
}
