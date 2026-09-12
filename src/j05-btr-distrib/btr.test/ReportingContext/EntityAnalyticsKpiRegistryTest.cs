using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
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

        [Fact]
        public void SupplierDefaultPack_IncludesTimeAwareKpis()
        {
            var registry = CreateSupplierRegistry();

            var ids = registry.GetPackKpiIds(SupplierEntityAnalyticsRegistrar.KpiPackId);
            ids.Should().Contain(PrincipalKpiCatalog.PacingAchievementPercentageId);
            ids.Should().Contain(PrincipalKpiCatalog.YoyMtdGrowthId);
            registry.ValidatePack(SupplierEntityAnalyticsRegistrar.KpiPackId).Should().BeEmpty();
        }

        [Fact]
        public void PacingAchievementMetadata_IsPercentWithElapsedDayGuardAndXAxis()
        {
            var registry = CreateSupplierRegistry();

            registry.TryGetMetadata(PrincipalKpiCatalog.PacingAchievementPercentageId, out var pacing).Should().BeTrue();
            pacing.DisplayName.Should().Be("Pacing Achievement %");
            pacing.Unit.Should().Be("Percent");
            pacing.Direction.Should().Be("HigherIsBetter");
            pacing.Category.Should().Be(EntityKpiCategory.Financial);
            pacing.DisplayPrecision.Should().BeGreaterThan(0);
            pacing.Description.Should().Contain("Actual Sales MTD");
            pacing.DefaultAxisRole.Should().Be("X");
            pacing.MinimumElapsedDays.Should().HaveValue();
        }

        [Fact]
        public void YoyMtdGrowthMetadata_IsPercentWithBaseGuardAndYAxis()
        {
            var registry = CreateSupplierRegistry();

            registry.TryGetMetadata(PrincipalKpiCatalog.YoyMtdGrowthId, out var yoy).Should().BeTrue();
            yoy.DisplayName.Should().Be("YoY MTD Growth %");
            yoy.Unit.Should().Be("Percent");
            yoy.Direction.Should().Be("HigherIsBetter");
            yoy.Category.Should().Be(EntityKpiCategory.Growth);
            yoy.DisplayPrecision.Should().BeGreaterThan(0);
            yoy.NullableBehavior.Should().Be("ShowEmpty");
            yoy.Description.Should().Contain("prior year MTD");
            yoy.DefaultAxisRole.Should().Be("Y");
            yoy.MinimumBaseValue.Should().HaveValue();
        }

        [Fact]
        public void ExistingAchievementAndYoyMetadata_RemainUnchangedByTimeAwareKpis()
        {
            var registry = CreateSupplierRegistry();

            registry.TryGetMetadata(PrincipalKpiCatalog.AchievementPercentageId, out var achievement).Should().BeTrue();
            achievement.DisplayName.Should().Be("Achievement Percentage");
            achievement.Unit.Should().Be("Ratio");
            achievement.DefaultAxisRole.Should().BeNull();
            achievement.MinimumElapsedDays.Should().BeNull();
            achievement.MinimumBaseValue.Should().BeNull();

            registry.TryGetMetadata(PrincipalKpiCatalog.YoyGrowthId, out var yoy).Should().BeTrue();
            yoy.DisplayName.Should().Be("Year-over-Year Growth Percentage");
            yoy.Unit.Should().Be("Percent");
            yoy.DefaultAxisRole.Should().BeNull();
            yoy.MinimumElapsedDays.Should().BeNull();
            yoy.MinimumBaseValue.Should().BeNull();
        }

        private static EntityAnalyticsKpiRegistry CreateSupplierRegistry()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Principal",
                KpiPackId = SupplierEntityAnalyticsRegistrar.KpiPackId
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new SupplierEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);
            return registry;
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
