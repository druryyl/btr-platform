using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.infrastructure.ReportingContext.DashboardSnapshotAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SupplierPrincipalOmzetRelationshipMetadataTest
    {
        [Fact]
        public void SupplierOmzetRelationships_ReferencePrincipalSalesOutNotPurchaseOrSalesmanKpis()
        {
            var relationships = RegisterSupplierRelationships();

            relationships.TryGet(
                EntityTypeCode.Supplier,
                SupplierRelationshipCatalog.TopCustomersByOmzet,
                out var customers).Should().BeTrue();
            customers.MetricKpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            customers.MetricKpiId.Should().NotBe("PU-KPI-001");
            customers.MetricKpiId.Should().NotBe("PRN-PUR-001");

            relationships.TryGet(
                EntityTypeCode.Supplier,
                SupplierRelationshipCatalog.TopSalesmenByOmzet,
                out var salesmen).Should().BeTrue();
            salesmen.MetricKpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            salesmen.MetricKpiId.Should().NotBe("SF-KPI-008");

            relationships.TryGet(
                EntityTypeCode.Supplier,
                SupplierRelationshipCatalog.TopProductsByOmzet,
                out var products).Should().BeTrue();
            products.MetricKpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            products.MetricKpiId.Should().NotBe("SF-KPI-008");

            relationships.ResolvePackForEntityType(EntityTypeCode.Supplier)
                .Where(definition => SupplierRelationshipCatalog.IsSalesOmzetRelationship(definition.RelationshipCode))
                .Should().OnlyContain(definition =>
                    definition.MetricKpiId == PrincipalKpiCatalog.SalesOutId
                    && !definition.MetricKpiId.StartsWith("PR-KPI-")
                    && !definition.MetricKpiId.StartsWith("CP-KPI-")
                    && definition.MetricKpiId != "PRN-CUS-001"
                    && definition.MetricKpiId != "PRN-CUS-002");
        }

        [Fact]
        public void PurchasingAndSalesmanKpiDefinitions_RemainUnchanged()
        {
            var supplierRegistry = CreateSupplierRegistry();
            supplierRegistry.TryGetMetadata("PU-KPI-001", out var purchase).Should().BeTrue();
            purchase.DisplayName.Should().Be("MTD Purchase");
            purchase.EvidenceRoute.Should().Be("/reports/purchasing");
            purchase.SourceDomain.Should().Be("PurchasingManagement");

            var salesmanRegistry = CreateSalesmanRegistry();
            salesmanRegistry.TryGetMetadata("SF-KPI-008", out var salesmanOmzet).Should().BeTrue();
            salesmanOmzet.DisplayName.Should().Be("MTD Omzet");
            salesmanOmzet.EvidenceRoute.Should().Be("/reports/sales");
            salesmanOmzet.SourceDomain.Should().Be("Salesman");

            var salesmanRelationships = new EntityRelationshipDefinitionRegistry(CreateSalesmanEntityTypes());
            SalesmanRelationshipCatalog.Register(salesmanRelationships);
            salesmanRelationships.TryGet(
                EntityTypeCode.Salesman,
                SalesmanRelationshipCatalog.TopCustomersByOmzet,
                out var salesmanCustomers).Should().BeTrue();
            salesmanCustomers.MetricKpiId.Should().Be("SF-KPI-008");
        }

        [Fact]
        public void CustomerTopPrincipalMetadata_ReferencesPrincipalSalesOutNotPurchaseOrSalesmanKpis()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                RelationshipPackId = CustomerRelationshipCatalog.PackId
            });
            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            CustomerRelationshipCatalog.Register(relationships);

            relationships.TryGet(
                EntityTypeCode.Customer,
                CustomerRelationshipCatalog.TopPrincipalsByOmzet,
                out var principals).Should().BeTrue();
            principals.MetricKpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            principals.MetricKpiId.Should().NotBe("CU-KPI-009");
            principals.MetricKpiId.Should().NotBe("PU-KPI-001");
            principals.MetricKpiId.Should().NotBe("SF-KPI-008");
            principals.MetricKpiId.Should().NotBe("PRN-CUS-001");
            principals.MetricKpiId.Should().NotBe("PRN-CUS-002");
        }

        [Fact]
        public void OmzetRelationshipEvidence_UsesPrincipalSalesOutFakturItemEvidence()
        {
            var resolver = new SupplierEntityAnalyticsEvidenceResolver();
            var identity = new EntityIdentity
            {
                EntityType = EntityTypeCode.Supplier,
                EntityId = "S001",
                EntityCode = "SUPA"
            };

            var evidence = resolver.BuildEvidence("SUPA", identity);
            var omzetLinks = evidence.Links
                .Where(link => SupplierRelationshipCatalog.IsSalesOmzetRelationship(link.RelationshipCode))
                .ToList();

            omzetLinks.Should().HaveCount(3);
            omzetLinks.Should().OnlyContain(link =>
                link.MetricKpiId == PrincipalKpiCatalog.SalesOutId
                && link.FilterDimension == SupplierEntityAnalyticsRegistrar.PrincipalSalesOutEvidenceFilterDimension
                && link.ReportRoute.StartsWith(SupplierEntityAnalyticsRegistrar.PrincipalSalesOutEvidenceRoute)
                && link.ReportRoute.Contains("supplierId=S001")
                && !link.ReportRoute.Contains("/reports/purchasing")
                && !link.ReportRoute.Contains("/reports/sales")
                && link.MetricKpiId != "PRN-CUS-001"
                && link.MetricKpiId != "PRN-CUS-002"
                && !link.MetricKpiId.StartsWith("PR-KPI-")
                && !link.MetricKpiId.StartsWith("CP-KPI-"));

            resolver.ResolveOmzetRelationshipEvidence("AssignedSalesman", "SUPA", identity).Should().BeNull();
        }

        [Fact]
        public void RelationshipRollupSql_UsesPrincipalSalesOutFormulaAndKeepsPurchaseLineTotal()
        {
            var sql = SupplierMtdItemRollupDal.ListMtdItemRollupsSql;
            sql.Should().Contain("ISNULL(aa.SubTotal, 0) - ISNULL(aa.DiscRp, 0) AS LineSalesOut");
            sql.Should().Contain("aa.Total AS LineTotal");
            sql.Should().Contain("bb.VoidDate = '3000-01-01'");
            sql.Should().NotContain("PRN-CUS-001");
            sql.Should().NotContain("PRN-CUS-002");
        }

        private static EntityRelationshipDefinitionRegistry RegisterSupplierRelationships()
        {
            var relationships = new EntityRelationshipDefinitionRegistry(CreateSupplierEntityTypes());
            SupplierRelationshipCatalog.Register(relationships);
            return relationships;
        }

        private static EntityAnalyticsKpiRegistry CreateSupplierRegistry()
        {
            var entityTypes = CreateSupplierEntityTypes();
            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            new SupplierEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());
            return registry;
        }

        private static EntityAnalyticsKpiRegistry CreateSalesmanRegistry()
        {
            var entityTypes = CreateSalesmanEntityTypes();
            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            new SalesmanEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());
            return registry;
        }

        private static EntityTypeRegistry CreateSupplierEntityTypes()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Supplier",
                KpiPackId = SupplierEntityAnalyticsRegistrar.KpiPackId,
                RelationshipPackId = SupplierRelationshipCatalog.PackId
            });
            return entityTypes;
        }

        private static EntityTypeRegistry CreateSalesmanEntityTypes()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Salesman,
                DisplayName = "Salesman",
                KpiPackId = SalesmanEntityAnalyticsRegistrar.KpiPackId,
                RelationshipPackId = SalesmanRelationshipCatalog.PackId
            });
            return entityTypes;
        }
    }
}
