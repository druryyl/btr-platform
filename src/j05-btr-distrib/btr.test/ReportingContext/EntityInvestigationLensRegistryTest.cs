using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;
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

    public class GetInvestigationLensesHandlerTest
    {
        private static IEntityTypeRegistry SupplierRegistry()
        {
            var registry = new EntityTypeRegistry();
            registry.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Principal",
                KpiPackId = "supplier-default"
            });
            return registry;
        }

        [Fact]
        public async Task Supplier_Lenses_AreExposedWithSalesOutDefault()
        {
            var handler = new GetInvestigationLensesHandler(SupplierRegistry());

            var result = await handler.Handle(
                new GetInvestigationLensesQuery { EntityType = EntityTypeCode.Supplier },
                CancellationToken.None);

            result.EntityType.Should().Be(EntityTypeCode.Supplier);
            result.DefaultLensId.Should().Be(EntityInvestigationLensIds.SalesOut);
            result.Lenses.Should().HaveCount(2);
            result.Lenses.Single(l => l.IsDefault).LensId
                .Should().Be(EntityInvestigationLensIds.SalesOut);
            result.Lenses.Single(l => l.LensId == EntityInvestigationLensIds.SalesOut)
                .DefaultPresetId.Should().Be("principal-sales-out-map");
            result.Lenses.Single(l => l.LensId == EntityInvestigationLensIds.Purchasing)
                .DefaultPresetId.Should().Be("purchase-exposure-map");
        }

        [Fact]
        public async Task UnknownEntityType_Throws()
        {
            var handler = new GetInvestigationLensesHandler(SupplierRegistry());

            await Assert.ThrowsAsync<System.ArgumentException>(() => handler.Handle(
                new GetInvestigationLensesQuery { EntityType = EntityTypeCode.Customer },
                CancellationToken.None));
        }
    }
}
