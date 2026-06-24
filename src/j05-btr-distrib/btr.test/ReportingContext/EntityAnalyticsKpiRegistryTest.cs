using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityAnalyticsKpiRegistryTest
    {
        [Fact]
        public void GetPackKpiIds_UnknownPack_ReturnsEmpty()
        {
            var registry = CreateRegistry();

            var ids = registry.GetPackKpiIds("unknown-pack");

            ids.Should().BeEmpty();
        }

        [Fact]
        public void CustomerDefaultPack_ContainsCatalogBackedKpiIds()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = CustomerEntityAnalyticsRegistrar.KpiPackId
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new CustomerEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            registry.GetPackKpiIdsForEntityType(EntityTypeCode.Customer)
                .Should().Equal("CU-KPI-009", "CU-KPI-010", "FI-KPI-013");
        }

        [Fact]
        public void GetPackKpiIdsForEntityType_UnknownEntityType_ReturnsEmpty()
        {
            var registry = CreateRegistry();

            registry.GetPackKpiIdsForEntityType("Warehouse").Should().BeEmpty();
        }

        [Fact]
        public void GetPackKpiIdsForEntityType_ResolvesPackThroughEntityRegistration()
        {
            var registry = CreateRegistry();
            registry.RegisterPack("customer-default", new[] { "KPI-1", "KPI-2" });

            registry.GetPackKpiIdsForEntityType(EntityTypeCode.Customer)
                .Should().Equal("KPI-1", "KPI-2");
        }

        [Fact]
        public void TryGetMetadata_UnknownKpi_ReturnsFalse()
        {
            var registry = CreateRegistry();

            var found = registry.TryGetMetadata("CU-KPI-009", out var metadata);

            found.Should().BeFalse();
            metadata.Should().BeNull();
        }

        [Fact]
        public void RegisterMetadata_ValidCategory_CanBeRetrieved()
        {
            var registry = CreateRegistry();
            registry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "TEST-KPI-001",
                Category = EntityKpiCategory.Financial,
                DisplayName = "Test KPI",
                Unit = "IDR",
                Direction = "HigherIsBetter",
                PeriodSemantics = "MTD"
            });

            var found = registry.TryGetMetadata("TEST-KPI-001", out var metadata);

            found.Should().BeTrue();
            metadata.Category.Should().Be(EntityKpiCategory.Financial);
            metadata.DisplayName.Should().Be("Test KPI");
        }

        [Fact]
        public void RegisterPack_ReturnsRegisteredIds()
        {
            var registry = CreateRegistry();
            registry.RegisterPack("test-pack", new[] { "KPI-1", "KPI-2" });

            registry.GetPackKpiIds("test-pack").Should().Equal("KPI-1", "KPI-2");
        }

        [Fact]
        public void ResolvePackMetadata_ReturnsRegisteredDefinitionsOnly()
        {
            var registry = CreateRegistry();
            registry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "KPI-1",
                Category = EntityKpiCategory.Financial,
                DisplayName = "One"
            });
            registry.RegisterPack("test-pack", new[] { "KPI-1", "KPI-MISSING" });

            registry.ResolvePackMetadata("test-pack")
                .Should().ContainSingle(m => m.KpiId == "KPI-1");
        }

        [Fact]
        public void ValidatePack_MissingMetadata_ReturnsMissingIds()
        {
            var registry = CreateRegistry();
            registry.RegisterPack("incomplete-pack", new[] { "KPI-1", "KPI-MISSING" });
            registry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "KPI-1",
                Category = EntityKpiCategory.Financial,
                DisplayName = "One"
            });

            registry.ValidatePack("incomplete-pack").Should().Equal("KPI-MISSING");
        }

        [Fact]
        public void CustomerDefaultPack_HasCompleteMetadata()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = CustomerEntityAnalyticsRegistrar.KpiPackId
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new CustomerEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            registry.ValidatePack(CustomerEntityAnalyticsRegistrar.KpiPackId).Should().BeEmpty();
            registry.TryGetMetadata("CU-KPI-009", out var omzet).Should().BeTrue();
            omzet.ValueType.Should().Be("Numeric");
            omzet.TimeGrain.Should().Be("Month");
            omzet.EvidenceFilterDimension.Should().Be("customerCode");
            omzet.ApplicableEntityTypes.Should().Contain(EntityTypeCode.Customer);
        }

        [Fact]
        public void ValidateCategory_InvalidValue_Throws()
        {
            Action act = () => EntityAnalyticsKpiRegistry.ValidateCategory((EntityKpiCategory)999);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        private static EntityAnalyticsKpiRegistry CreateRegistry()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = "customer-default"
            });
            return new EntityAnalyticsKpiRegistry(entityTypes);
        }
    }
}
