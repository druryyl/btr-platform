using System;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SupplierPurchasingRelationshipCatalogTest
    {
        [Fact]
        public void RegistersTopPurchasedItemsFromPurchaseInvoiceDetail()
        {
            var relationships = RegisterSupplierRelationships();

            relationships.TryGet(
                EntityTypeCode.Supplier,
                SupplierRelationshipCatalog.TopPurchasedItems,
                out var definition).Should().BeTrue();

            definition.DisplayName.Should().Be("Top Purchased Items");
            definition.TargetEntityType.Should().Be(EntityTypeCode.Item);
            definition.MetricKpiId.Should().Be(PrincipalKpiCatalog.PurchaseInId);
            definition.MetricKpiId.Should().Be(SupplierRelationshipCatalog.PurchaseInvoiceDetailMetricKpiId);
            definition.MetricKpiId.Should().NotBe(PrincipalKpiCatalog.SalesOutId);
            definition.PeriodSemantics.Should().Be("MTD");
            definition.TopN.Should().Be(10);
        }

        [Fact]
        public void RegistersPurchaseHistoryFromMonthlyPurchaseHistory()
        {
            var relationships = RegisterSupplierRelationships();

            relationships.TryGet(
                EntityTypeCode.Supplier,
                SupplierRelationshipCatalog.PurchaseHistory,
                out var definition).Should().BeTrue();

            definition.DisplayName.Should().Be("Purchase History");
            definition.MetricKpiId.Should().Be(SupplierRelationshipCatalog.PurchaseHistoryMetricKpiId);
            definition.MetricKpiId.Should().Be("PU-KPI-001");
            definition.MetricKpiId.Should().NotBe(PrincipalKpiCatalog.SalesOutId);
            definition.PeriodSemantics.Should().Be("Monthly");
            definition.TopN.Should().Be(12);
        }

        [Fact]
        public void PurchasingDrivers_AreResolvedAsPartOfTheSupplierPack()
        {
            var relationships = RegisterSupplierRelationships();
            var pack = relationships.ResolvePackForEntityType(EntityTypeCode.Supplier);

            pack.Select(d => d.RelationshipCode).Should().Contain(
                SupplierRelationshipCatalog.TopPurchasedItems);
            pack.Select(d => d.RelationshipCode).Should().Contain(
                SupplierRelationshipCatalog.PurchaseHistory);

            pack.Where(d => SupplierRelationshipCatalog.IsSalesOmzetRelationship(d.RelationshipCode))
                .Should().HaveCount(3);
            pack.Where(d => SupplierRelationshipCatalog.IsPurchasingRelationship(d.RelationshipCode))
                .Should().HaveCount(2);
        }

        [Fact]
        public void RegistersNoPoBasedPurchasingUserOrProcurementRelationships()
        {
            var relationships = RegisterSupplierRelationships();
            var codes = relationships.ResolvePackForEntityType(EntityTypeCode.Supplier)
                .Select(d => d.RelationshipCode)
                .ToList();

            codes.Should().BeEquivalentTo(new[]
            {
                SupplierRelationshipCatalog.TopCustomersByOmzet,
                SupplierRelationshipCatalog.TopSalesmenByOmzet,
                SupplierRelationshipCatalog.TopProductsByOmzet,
                SupplierRelationshipCatalog.TopPurchasedItems,
                SupplierRelationshipCatalog.PurchaseHistory
            });

            codes.Should().NotContain(code =>
                code.IndexOf("PO", StringComparison.OrdinalIgnoreCase) >= 0
                || code.IndexOf("PurchaseOrder", StringComparison.OrdinalIgnoreCase) >= 0
                || code.IndexOf("Procurement", StringComparison.OrdinalIgnoreCase) >= 0
                || code.IndexOf("Buyer", StringComparison.OrdinalIgnoreCase) >= 0
                || code.IndexOf("PurchasingUser", StringComparison.OrdinalIgnoreCase) >= 0
                || code.IndexOf("User", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public void PurchasingDrivers_AreScopedToThePurchasingLens()
        {
            var purchasing = EntityInvestigationLensRegistry.TryGetLens(
                EntityTypeCode.Supplier, EntityInvestigationLensIds.Purchasing);
            var salesOut = EntityInvestigationLensRegistry.TryGetLens(
                EntityTypeCode.Supplier, EntityInvestigationLensIds.SalesOut);

            purchasing.RelationshipDrivers.Should().BeEquivalentTo(new[]
            {
                SupplierRelationshipCatalog.TopPurchasedItems,
                SupplierRelationshipCatalog.PurchaseHistory
            });

            salesOut.RelationshipDrivers.Should().NotContain(SupplierRelationshipCatalog.TopPurchasedItems);
            salesOut.RelationshipDrivers.Should().NotContain(SupplierRelationshipCatalog.PurchaseHistory);
        }

        [Fact]
        public void RelationshipClassifications_AreDisjoint()
        {
            SupplierRelationshipCatalog.IsPurchasingRelationship(
                SupplierRelationshipCatalog.TopPurchasedItems).Should().BeTrue();
            SupplierRelationshipCatalog.IsPurchasingRelationship(
                SupplierRelationshipCatalog.PurchaseHistory).Should().BeTrue();
            SupplierRelationshipCatalog.IsPurchasingRelationship(
                SupplierRelationshipCatalog.TopCustomersByOmzet).Should().BeFalse();
            SupplierRelationshipCatalog.IsSalesOmzetRelationship(
                SupplierRelationshipCatalog.TopPurchasedItems).Should().BeFalse();
            SupplierRelationshipCatalog.IsSalesOmzetRelationship(
                SupplierRelationshipCatalog.PurchaseHistory).Should().BeFalse();
        }

        private static EntityRelationshipDefinitionRegistry RegisterSupplierRelationships()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Principal",
                KpiPackId = SupplierEntityAnalyticsRegistrar.KpiPackId,
                RelationshipPackId = SupplierRelationshipCatalog.PackId
            });

            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            SupplierRelationshipCatalog.Register(relationships);
            return relationships;
        }
    }
}
