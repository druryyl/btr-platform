using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityComparisonEngineTest
    {
        private readonly ComparisonTestRepository _repository = new ComparisonTestRepository();
        private readonly EntityTypeRegistry _entityTypes = new EntityTypeRegistry();
        private readonly EntityAnalyticsKpiRegistry _registry;
        private readonly EntityComparisonEngine _engine;

        public EntityComparisonEngineTest()
        {
            _entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = CustomerEntityAnalyticsRegistrar.KpiPackId,
                RelationshipPackId = CustomerRelationshipCatalog.PackId,
                PeerGroupRuleId = PeerGroupResolver.CustomerWilayah,
                ProfileRouteTemplate = "/analytics/customers/{code}"
            });
            _registry = new EntityAnalyticsKpiRegistry(_entityTypes);
            new CustomerEntityAnalyticsRegistrar().Register(
                _entityTypes,
                _registry,
                new EntityAnalyticsDimensionLabelRegistry());

            var options = Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions());
            var relationships = new EntityRelationshipDefinitionRegistry(_entityTypes);
            CustomerRelationshipCatalog.Register(relationships);

            _engine = new EntityComparisonEngine(
                _repository,
                _registry,
                _entityTypes,
                new EntityKpiEnvelopeFormatter(),
                new EntityTrendEngine(_repository, _registry, _entityTypes, options),
                new EntityRankingEngine(_repository, _registry, _entityTypes, options),
                new EntityAttentionEngine(_repository),
                new EntityRelationshipEngine(_repository, relationships, _entityTypes),
                new EntityRadarEngine(_repository, _registry, _entityTypes));
        }

        [Fact]
        public void BuildCrossPeriodSection_WithL0AndL1_ComputesMomAndYoy()
        {
            SeedCustomer("C001", "Alpha", 1200m, 2026, 6, 1000m, 900m);

            var section = _engine.BuildCrossPeriodSection(EntityTypeCode.Customer, "C001");

            section.IsAvailable.Should().BeTrue();
            var omzet = section.Metrics.First(m => m.KpiId == "CU-KPI-009");
            omzet.CurrentValue.Should().Be(1200m);
            omzet.PriorMonthValue.Should().Be(1000m);
            omzet.MomDelta.Should().Be(200m);
            omzet.MomGrowthPercent.Should().Be(20m);
            omzet.PriorYearValue.Should().Be(900m);
            omzet.YoyDelta.Should().Be(300m);
        }

        [Fact]
        public void BuildMultiEntityComparison_TwoEntities_ReturnsKpiRows()
        {
            SeedCustomer("C001", "Alpha", 1200m, 2026, 6, 1000m, 900m);
            SeedCustomer("C002", "Beta", 800m, 2026, 6, 700m, 600m);

            var result = _engine.BuildMultiEntityComparison(new ComparisonContext
            {
                Mode = ComparisonMode.MultiEntity,
                EntityType = EntityTypeCode.Customer,
                EntityIds = new[] { "C001", "C002" }
            });

            result.Entities.Should().HaveCount(2);
            result.KpiComparison.IsAvailable.Should().BeTrue();
            result.KpiComparison.Rows.Should().NotBeEmpty();
            result.KpiComparison.Rows[0].Values.Should().HaveCount(2);
        }

        [Fact]
        public void BuildMultiEntityComparison_GeneratedAtMismatch_AddsWarning()
        {
            SeedCustomer("C001", "Alpha", 1200m, 2026, 6, 1000m, 900m, new DateTime(2026, 6, 24, 8, 0, 0));
            SeedCustomer("C002", "Beta", 800m, 2026, 6, 700m, 600m, new DateTime(2026, 6, 24, 9, 0, 0));

            var result = _engine.BuildMultiEntityComparison(new ComparisonContext
            {
                Mode = ComparisonMode.MultiEntity,
                EntityType = EntityTypeCode.Customer,
                EntityIds = new[] { "C001", "C002" }
            });

            result.Warnings.Should().Contain(EntityComparisonWarnings.GeneratedAtMismatch);
        }

        [Fact]
        public void BuildMultiEntityComparison_InvalidCount_Throws()
        {
            Action act = () => _engine.BuildMultiEntityComparison(new ComparisonContext
            {
                Mode = ComparisonMode.MultiEntity,
                EntityType = EntityTypeCode.Customer,
                EntityIds = new[] { "C001" }
            });

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void BuildMultiEntityComparison_WithL5Radar_ReturnsOverlayWithoutRecalculation()
        {
            SeedCustomer("C001", "Alpha", 1200m, 2026, 6, 1000m, 900m);
            SeedCustomer("C002", "Beta", 800m, 2026, 6, 700m, 600m);
            _repository.RadarRows.Add(new EntityAnalyticsRadarScoreRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = "C001",
                EntityCode = "C001",
                PeriodYear = 2026,
                PeriodMonth = 6,
                AxisKpiId = "CU-KPI-009",
                Score = 80m,
                PeerGroupRuleId = PeerGroupResolver.CustomerWilayah,
                PeerGroupSize = 6,
                NormalizationMethod = RadarNormalizationMethod.PeerPercentile
            });
            _repository.RadarRows.Add(new EntityAnalyticsRadarScoreRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = "C002",
                EntityCode = "C002",
                PeriodYear = 2026,
                PeriodMonth = 6,
                AxisKpiId = "CU-KPI-009",
                Score = 55m,
                PeerGroupRuleId = PeerGroupResolver.CustomerWilayah,
                PeerGroupSize = 6,
                NormalizationMethod = RadarNormalizationMethod.PeerPercentile
            });

            var result = _engine.BuildMultiEntityComparison(new ComparisonContext
            {
                Mode = ComparisonMode.Radar,
                EntityType = EntityTypeCode.Customer,
                EntityIds = new[] { "C001", "C002" },
                PeriodYear = 2026,
                PeriodMonth = 6
            });

            result.RadarComparison.IsAvailable.Should().BeTrue();
            result.RadarComparison.Overlays.Should().HaveCount(2);
            result.RadarComparison.Overlays[0].Scores[0].Should().Be(80m);
            result.RadarComparison.Overlays[1].Scores[0].Should().Be(55m);
        }

        private void SeedCustomer(
            string entityId,
            string displayName,
            decimal currentOmzet,
            int year,
            int month,
            decimal priorMonthOmzet,
            decimal priorYearOmzet,
            DateTime? generatedAt = null)
        {
            var at = generatedAt ?? new DateTime(2026, 6, 24);
            var priorMonth = EntityComparisonCalculator.ShiftMonth(year, month, -1);

            _repository.CurrentRows.AddRange(new[]
            {
                MetricRow(entityId, displayName, "CU-KPI-009", currentOmzet, at),
                MetricRow(entityId, displayName, "CU-KPI-010", 100m, at),
                MetricRow(entityId, displayName, "FI-KPI-013", 10m, at),
                MetaRow(entityId, displayName, at)
            });

            _repository.MonthlyRows.Add(MonthlyRow(entityId, "CU-KPI-009", year, month, currentOmzet, false));
            _repository.MonthlyRows.Add(MonthlyRow(entityId, "CU-KPI-009", priorMonth.Year, priorMonth.Month, priorMonthOmzet, true));
            _repository.MonthlyRows.Add(MonthlyRow(entityId, "CU-KPI-009", year - 1, month, priorYearOmzet, true));
        }

        private static EntityAnalyticsCurrentRow MetricRow(
            string entityId,
            string displayName,
            string kpiId,
            decimal value,
            DateTime generatedAt)
        {
            return new EntityAnalyticsCurrentRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = entityId,
                EntityCode = entityId,
                KpiId = kpiId,
                NumericValue = value,
                GeneratedAt = generatedAt
            };
        }

        private static EntityAnalyticsCurrentRow MetaRow(string entityId, string displayName, DateTime generatedAt)
        {
            return new EntityAnalyticsCurrentRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = entityId,
                EntityCode = entityId,
                KpiId = EntityAnalyticsMetaKpiIds.DisplayName,
                TextValue = displayName,
                GeneratedAt = generatedAt
            };
        }

        private static EntityAnalyticsMonthlyRow MonthlyRow(
            string entityId,
            string kpiId,
            int year,
            int month,
            decimal value,
            bool isClosed)
        {
            return new EntityAnalyticsMonthlyRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = entityId,
                EntityCode = entityId,
                KpiId = kpiId,
                PeriodYear = year,
                PeriodMonth = month,
                NumericValue = value,
                PeriodSemantics = "MTD",
                IsClosed = isClosed
            };
        }

        private sealed class ComparisonTestRepository : EntityAnalyticsRepositoryStubBase
        {
            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
            {
                return CurrentRows
                    .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                        && !EntityAnalyticsMetaKpiIds.IsMetaOrDimension(r.KpiId))
                    .ToList();
            }

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId)
            {
                var displayName = CurrentRows
                    .FirstOrDefault(r => string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(r.KpiId, EntityAnalyticsMetaKpiIds.DisplayName, StringComparison.OrdinalIgnoreCase))
                    ?.TextValue ?? entityId;

                return new EntityIdentity
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityCode = entityId,
                    DisplayName = displayName,
                    IsActive = true
                };
            }

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId)
            {
                return CurrentRows
                    .Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase))
                    .Select(r => r.GeneratedAt)
                    .Cast<DateTime?>()
                    .FirstOrDefault();
            }

            public override bool HasAnyCurrentMetrics(string entityType)
                => CurrentRows.Any(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase));
        }
    }
}
