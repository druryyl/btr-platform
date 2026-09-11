using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SupplierAttentionSignalCatalogTest
    {
        private static EntityAttentionSignalRegistry RegisterSupplierCatalog()
        {
            var registry = new EntityAttentionSignalRegistry();
            SupplierAttentionSignalCatalog.Register(registry);
            return registry;
        }

        [Fact]
        public void RegistersTheFiveSalesOutAttentionCategories()
        {
            var registry = RegisterSupplierCatalog();

            ResolveTitle(registry, SupplierAttentionSignalCatalog.SignalSalesOutDecline)
                .Should().Be("Sales-Out Decline");
            ResolveTitle(registry, SupplierAttentionSignalCatalog.SignalGrowthDeterioration)
                .Should().Be("Growth Deterioration");
            ResolveTitle(registry, SupplierAttentionSignalCatalog.SignalTargetMiss)
                .Should().Be("Target Miss");
            ResolveTitle(registry, SupplierAttentionSignalCatalog.SignalReturnRisk)
                .Should().Be("Return Risk");
            ResolveTitle(registry, SupplierAttentionSignalCatalog.SignalCoverageDeterioration)
                .Should().Be("Coverage Deterioration");
        }

        [Fact]
        public void RetainsExistingPurchaseAndInventorySignals()
        {
            var registry = RegisterSupplierCatalog();

            ResolveTitle(registry, DashboardPurchasingManagementAggregator.SignalQualifiedBacklog)
                .Should().Be("Qualified Backlog");
            ResolveTitle(registry, DashboardPurchasingManagementAggregator.SignalPrincipalSpendConcentration)
                .Should().Be("Spend Concentration");
            ResolveTitle(registry, DashboardPurchasingManagementAggregator.SignalPrincipalInventoryConcentration)
                .Should().Be("Inventory Concentration");
            ResolveTitle(registry, DashboardPurchasingManagementAggregator.SignalPrincipalAtRiskExposure)
                .Should().Be("At-Risk Exposure");
            ResolveTitle(registry, DashboardPurchasingManagementAggregator.SignalCompoundDependency)
                .Should().Be("Compound Dependency");
            ResolveTitle(registry, DashboardPurchasingManagementAggregator.SignalPrincipalInventoryNoPurchase)
                .Should().Be("Inventory, No Purchase");
            ResolveTitle(registry, DashboardPurchasingManagementAggregator.SignalUnknownPrincipal)
                .Should().Be("Unknown Principal");
        }

        [Fact]
        public void SalesOutSignals_AreRegisteredUnderTheSalesOutLens()
        {
            var salesOutCategories = EntityInvestigationLensRegistry.TryGetLens(
                EntityTypeCode.Supplier, EntityInvestigationLensIds.SalesOut).AttentionCategories;

            salesOutCategories.Should().Contain("Sales-Out Decline");
            salesOutCategories.Should().Contain("Growth Deterioration");
            salesOutCategories.Should().Contain("Target Miss");
            salesOutCategories.Should().Contain("Return Risk");
            salesOutCategories.Should().Contain("Coverage Deterioration");
        }

        [Fact]
        public void PurchaseAndInventorySignals_AreRegisteredUnderThePurchasingLens()
        {
            var purchasingCategories = EntityInvestigationLensRegistry.TryGetLens(
                EntityTypeCode.Supplier, EntityInvestigationLensIds.Purchasing).AttentionCategories;

            purchasingCategories.Should().Contain("Qualified Backlog");
            purchasingCategories.Should().Contain("Spend Concentration");
            purchasingCategories.Should().Contain("Inventory Concentration");
            purchasingCategories.Should().Contain("At-Risk Exposure");
            purchasingCategories.Should().Contain("Compound Dependency");
            purchasingCategories.Should().Contain("Inventory, No Purchase");
            purchasingCategories.Should().Contain("Unknown Principal");
        }

        [Fact]
        public void SalesOutCategories_AreNeverDeclaredUnderThePurchasingLens()
        {
            var purchasingCategories = EntityInvestigationLensRegistry.TryGetLens(
                EntityTypeCode.Supplier, EntityInvestigationLensIds.Purchasing).AttentionCategories;

            purchasingCategories.Should().NotContain("Sales-Out Decline");
            purchasingCategories.Should().NotContain("Growth Deterioration");
            purchasingCategories.Should().NotContain("Target Miss");
            purchasingCategories.Should().NotContain("Return Risk");
            purchasingCategories.Should().NotContain("Coverage Deterioration");
        }

        [Fact]
        public void AllRegisteredSignalCodes_AreDistinct()
        {
            var registry = RegisterSupplierCatalog();
            var catalogCodes = new[]
            {
                SupplierAttentionSignalCatalog.SignalSalesOutDecline,
                SupplierAttentionSignalCatalog.SignalGrowthDeterioration,
                SupplierAttentionSignalCatalog.SignalTargetMiss,
                SupplierAttentionSignalCatalog.SignalReturnRisk,
                SupplierAttentionSignalCatalog.SignalCoverageDeterioration,
                DashboardPurchasingManagementAggregator.SignalQualifiedBacklog,
                DashboardPurchasingManagementAggregator.SignalPrincipalSpendConcentration,
                DashboardPurchasingManagementAggregator.SignalPrincipalInventoryConcentration,
                DashboardPurchasingManagementAggregator.SignalPrincipalAtRiskExposure,
                DashboardPurchasingManagementAggregator.SignalCompoundDependency,
                DashboardPurchasingManagementAggregator.SignalPrincipalInventoryNoPurchase,
                DashboardPurchasingManagementAggregator.SignalUnknownPrincipal
            };

            catalogCodes.Should().OnlyHaveUniqueItems();
            foreach (var expected in catalogCodes)
                registry.TryResolve(EntityTypeCode.Supplier, expected, out _).Should().BeTrue();
        }

        private static string ResolveTitle(EntityAttentionSignalRegistry registry, string signalCode)
        {
            registry.TryResolve(EntityTypeCode.Supplier, signalCode, out var definition).Should().BeTrue();
            return definition.SignalTitle;
        }
    }
}