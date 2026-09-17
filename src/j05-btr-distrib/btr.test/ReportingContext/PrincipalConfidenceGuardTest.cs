using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalConfidenceGuardTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 15, 8, 0, 0);

        [Fact]
        public void ApplyPacingElapsedDaysGuard_ElapsedDaysBelowMinimum_SuppressesValueLowConfidence()
        {
            var guard = CreateGuard(ElapsedDayThreshold: 6);

            var row = PacingRow("SUPA", "Alpha", PacingValue: 250.000000m);
            guard.ApplyPacingElapsedDaysGuard(row, elapsedDays: 3);

            row.PacingAchievementPercentage.Should().BeNull();
            row.ConfidenceStatus.Should().Be(KpiConfidenceStatus.LowConfidence);
        }

        [Fact]
        public void ApplyPacingElapsedDaysGuard_ElapsedDaysAtOrAboveMinimum_NormalKeepsValue()
        {
            var guard = CreateGuard(ElapsedDayThreshold: 6);

            var atBoundary = PacingRow("SUPA", "Alpha", PacingValue: 250.000000m);
            guard.ApplyPacingElapsedDaysGuard(atBoundary, elapsedDays: 6);
            atBoundary.ConfidenceStatus.Should().Be(KpiConfidenceStatus.Normal);
            atBoundary.PacingAchievementPercentage.Should().Be(250.000000m);

            var above = PacingRow("SUPB", "Beta", PacingValue: 160.000000m);
            guard.ApplyPacingElapsedDaysGuard(above, elapsedDays: 7);
            above.ConfidenceStatus.Should().Be(KpiConfidenceStatus.Normal);
            above.PacingAchievementPercentage.Should().Be(160.000000m);
        }

        [Fact]
        public void ApplyYoyMtdBaseGuard_PriorBaseBelowMinimum_SuppressesValueLowConfidence()
        {
            var guard = CreateGuard(BaseThreshold: 1000000m);

            var row = YoyRow("SUPA", "Alpha", Current: 600000m, Prior: 500000m, Growth: 20.000000m);
            guard.ApplyYoyMtdBaseGuard(row);

            row.YoyMtdGrowthPercentage.Should().BeNull();
            row.ConfidenceStatus.Should().Be(KpiConfidenceStatus.LowConfidence);
        }

        [Fact]
        public void ApplyYoyMtdBaseGuard_MissingPriorBase_LowConfidence()
        {
            var guard = CreateGuard(BaseThreshold: 1000000m);

            var row = YoyRow("SUPA", "Alpha", Current: 1000m, Prior: null, Growth: null);
            guard.ApplyYoyMtdBaseGuard(row);

            row.YoyMtdGrowthPercentage.Should().BeNull();
            row.ConfidenceStatus.Should().Be(KpiConfidenceStatus.LowConfidence);
        }

        [Fact]
        public void ApplyYoyMtdBaseGuard_PriorBaseAtOrAboveMinimum_NormalKeepsValue()
        {
            var guard = CreateGuard(BaseThreshold: 1000000m);

            var atBoundary = YoyRow("SUPA", "Alpha", Current: 1250000m, Prior: 1000000m, Growth: 25.000000m);
            guard.ApplyYoyMtdBaseGuard(atBoundary);
            atBoundary.ConfidenceStatus.Should().Be(KpiConfidenceStatus.Normal);
            atBoundary.YoyMtdGrowthPercentage.Should().Be(25.000000m);

            var above = YoyRow("SUPB", "Beta", Current: 1200000m, Prior: 2000000m, Growth: -40.000000m);
            guard.ApplyYoyMtdBaseGuard(above);
            above.ConfidenceStatus.Should().Be(KpiConfidenceStatus.Normal);
            above.YoyMtdGrowthPercentage.Should().Be(-40.000000m);
        }

        [Fact]
        public void Apply_NoMetadataThreshold_StatusNormalKeepsValue()
        {
            var guard = CreateGuard(ElapsedDayThreshold: null, BaseThreshold: null);

            var pacing = PacingRow("SUPA", "Alpha", PacingValue: 250.000000m);
            var yoy = YoyRow("SUPA", "Alpha", Current: 1000m, Prior: 800m, Growth: 25.000000m);

            guard.ApplyPacingElapsedDaysGuard(pacing, elapsedDays: 1);
            guard.ApplyYoyMtdBaseGuard(yoy);

            pacing.ConfidenceStatus.Should().Be(KpiConfidenceStatus.Normal);
            pacing.PacingAchievementPercentage.Should().Be(250.000000m);
            yoy.ConfidenceStatus.Should().Be(KpiConfidenceStatus.Normal);
            yoy.YoyMtdGrowthPercentage.Should().Be(25.000000m);
        }

        [Fact]
        public void ApplyPacingElapsedDaysGuards_AppliesToEveryRow()
        {
            var guard = CreateGuard(ElapsedDayThreshold: 6);

            var result = new PrincipalPacingAchievementResult
            {
                PacingAchievementKpiId = PrincipalKpiCatalog.PacingAchievementPercentageId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = GeneratedAt,
                Principals = new[]
                {
                    PacingRow("SUPA", "Alpha", PacingValue: 250.000000m),
                    PacingRow("SUPB", "Beta", PacingValue: 160.000000m)
                }
            };

            var applied = guard.ApplyPacingElapsedDaysGuards(result, elapsedDays: 3);

            applied.Should().BeSameAs(result);
            applied.Principals.Should().OnlyContain(row => row.ConfidenceStatus == KpiConfidenceStatus.LowConfidence);
            applied.Principals.Should().OnlyContain(row => row.PacingAchievementPercentage == null);
        }

        [Fact]
        public void ApplyYoyMtdBaseGuards_AppliesToEveryRow()
        {
            var guard = CreateGuard(BaseThreshold: 1000000m);

            var result = new PrincipalYoyMtdGrowthResult
            {
                YoyMtdGrowthKpiId = PrincipalKpiCatalog.YoyMtdGrowthId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = GeneratedAt,
                Principals = new[]
                {
                    YoyRow("SUPA", "Alpha", Current: 600000m, Prior: 500000m, Growth: 20.000000m),
                    YoyRow("SUPB", "Beta", Current: 1200000m, Prior: 2000000m, Growth: -40.000000m)
                }
            };

            var applied = guard.ApplyYoyMtdBaseGuards(result);

            applied.Should().BeSameAs(result);
            applied.Principals[0].ConfidenceStatus.Should().Be(KpiConfidenceStatus.LowConfidence);
            applied.Principals[0].YoyMtdGrowthPercentage.Should().BeNull();
            applied.Principals[1].ConfidenceStatus.Should().Be(KpiConfidenceStatus.Normal);
            applied.Principals[1].YoyMtdGrowthPercentage.Should().Be(-40.000000m);
        }

        [Fact]
        public void Apply_PerRowMethods_IgnoreNullRow()
        {
            var guard = CreateGuard(ElapsedDayThreshold: 6, BaseThreshold: 1000000m);

            guard.ApplyPacingElapsedDaysGuard(null, elapsedDays: 1);
            guard.ApplyYoyMtdBaseGuard(null);
        }

        [Fact]
        public void IsElapsedDaysBelowMinimum_ComparesAgainstConfiguredMinimum()
        {
            PrincipalConfidenceGuard.IsElapsedDaysBelowMinimum(5, 6).Should().BeTrue();
            PrincipalConfidenceGuard.IsElapsedDaysBelowMinimum(6, 6).Should().BeFalse();
            PrincipalConfidenceGuard.IsElapsedDaysBelowMinimum(7, 6).Should().BeFalse();
            PrincipalConfidenceGuard.IsElapsedDaysBelowMinimum(3, null).Should().BeFalse();
        }

        [Fact]
        public void IsBaseBelowMinimum_ComparesAgainstConfiguredMinimum()
        {
            PrincipalConfidenceGuard.IsBaseBelowMinimum(500000m, 1000000m).Should().BeTrue();
            PrincipalConfidenceGuard.IsBaseBelowMinimum(1000000m, 1000000m).Should().BeFalse();
            PrincipalConfidenceGuard.IsBaseBelowMinimum(2000000m, 1000000m).Should().BeFalse();
            PrincipalConfidenceGuard.IsBaseBelowMinimum(null, 1000000m).Should().BeTrue();
            PrincipalConfidenceGuard.IsBaseBelowMinimum(500000m, null).Should().BeFalse();
        }

        private static PrincipalConfidenceGuard CreateGuard(int? ElapsedDayThreshold = null, decimal? BaseThreshold = null)
        {
            var entityTypes = new EntityTypeRegistry();
            var registry = new EntityAnalyticsKpiRegistry(entityTypes);

            if (ElapsedDayThreshold.HasValue)
            {
                registry.RegisterMetadata(new EntityKpiMetadata
                {
                    KpiId = PrincipalKpiCatalog.PacingAchievementPercentageId,
                    Category = EntityKpiCategory.Financial,
                    DisplayName = "Pacing Achievement %",
                    Unit = "Percent",
                    MinimumElapsedDays = ElapsedDayThreshold.Value
                });
            }

            if (BaseThreshold.HasValue)
            {
                registry.RegisterMetadata(new EntityKpiMetadata
                {
                    KpiId = PrincipalKpiCatalog.YoyMtdGrowthId,
                    Category = EntityKpiCategory.Growth,
                    DisplayName = "YoY MTD Growth %",
                    Unit = "Percent",
                    MinimumBaseValue = BaseThreshold.Value
                });
            }

            return new PrincipalConfidenceGuard(registry);
        }

        private static PrincipalPacingAchievementRow PacingRow(
            string supplierId,
            string supplierName,
            decimal? PacingValue)
        {
            return new PrincipalPacingAchievementRow
            {
                PacingAchievementKpiId = PrincipalKpiCatalog.PacingAchievementPercentageId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SalesOutAmount = 1000m,
                TargetAmount = 800m,
                PacingAchievementPercentage = PacingValue
            };
        }

        private static PrincipalYoyMtdGrowthRow YoyRow(
            string supplierId,
            string supplierName,
            decimal? Current,
            decimal? Prior,
            decimal? Growth)
        {
            return new PrincipalYoyMtdGrowthRow
            {
                YoyMtdGrowthKpiId = PrincipalKpiCatalog.YoyMtdGrowthId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                CurrentYearMtdSalesOutAmount = Current,
                PriorYearMtdSalesOutAmount = Prior,
                YoyMtdGrowthPercentage = Growth
            };
        }
    }
}