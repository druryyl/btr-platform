using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityRadarEngineTest
    {
        private readonly RadarTestRepository _repository = new RadarTestRepository();
        private readonly EntityTypeRegistry _entityTypes = new EntityTypeRegistry();
        private readonly EntityAnalyticsKpiRegistry _registry;
        private readonly EntityRadarEngine _engine;

        public EntityRadarEngineTest()
        {
            _entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = CustomerEntityAnalyticsRegistrar.KpiPackId,
                PeerGroupRuleId = PeerGroupResolver.CustomerWilayah
            });
            _registry = new EntityAnalyticsKpiRegistry(_entityTypes);
            new CustomerEntityAnalyticsRegistrar().Register(
                _entityTypes,
                _registry,
                new EntityAnalyticsDimensionLabelRegistry());
            _engine = new EntityRadarEngine(_repository, _registry, _entityTypes);
        }

        [Fact]
        public void ComputeAndPersistScores_WithSufficientPeerGroup_WritesL5Rows()
        {
            SeedPeerGroup("Jakarta", 5, 1000m);

            _engine.ComputeAndPersistScores(
                EntityTypeCode.Customer,
                2026,
                6,
                "refresh-1",
                new DateTime(2026, 6, 24));

            _repository.RadarRows.Should().NotBeEmpty();
            _repository.RadarRows.Should().OnlyContain(r => r.PeerGroupSize >= EntityAnalyticsConstants.MinRadarPeerGroupSize);
        }

        [Fact]
        public void BuildRadarSection_SmallPeerGroup_ReturnsUnavailable()
        {
            SeedPeerGroup("Solo", 2, 500m);

            var section = _engine.BuildRadarSection(EntityTypeCode.Customer, "C001");

            section.IsAvailable.Should().BeFalse();
            section.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.PeerGroupTooSmall);
            section.UnavailableExplanation.Should().Contain("minimum 5");
        }

        [Fact]
        public void BuildRadarSection_WithL5Rows_ReturnsAxes()
        {
            _repository.RadarRows.Add(new EntityAnalyticsRadarScoreRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = "C001",
                PeriodYear = 2026,
                PeriodMonth = 6,
                AxisKpiId = "CU-KPI-009",
                Score = 88m,
                PeerGroupRuleId = PeerGroupResolver.CustomerWilayah,
                PeerGroupSize = 6,
                NormalizationMethod = RadarNormalizationMethod.PeerPercentile
            });

            var section = _engine.BuildRadarSection(EntityTypeCode.Customer, "C001");

            section.IsAvailable.Should().BeTrue();
            section.Axes.Should().ContainSingle(a => a.KpiId == "CU-KPI-009" && a.Score == 88m);
            section.Axes.First().DisplayName.Should().Be("Revenue");
        }

        [Fact]
        public void ComputeAndPersistScores_WithReplay_UsesL1KpiPopulationWithoutL0Omzet()
        {
            SeedReplayPeerGroupFromMonthlyOnly("Jakarta", 5, 1000m);
            var replay = EntityAnalyticsReplayContextFactory.Create(
                new YearMonthPeriod(2024, 3),
                EntityTypeCode.Customer,
                "job-replay",
                new EntityAnalyticsBackfillRequest());

            _engine.ComputeAndPersistScores(
                EntityTypeCode.Customer,
                2024,
                3,
                "refresh-replay",
                new DateTime(2024, 3, 31),
                replay);

            _repository.RadarRows.Should().Contain(r => r.AxisKpiId == "CU-KPI-009");
            _repository.CurrentRows.Should().NotContain(r => r.KpiId == "CU-KPI-009");
        }

        [Fact]
        public void ComputeAndPersistScores_SalesmanAllActivePeerGroup_UsesUndimensionedPopulationQuery()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Salesman,
                DisplayName = "Salesman",
                KpiPackId = SalesmanEntityAnalyticsRegistrar.KpiPackId,
                PeerGroupRuleId = PeerGroupResolver.SalesmanAllActive
            });
            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            new SalesmanEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());

            var repository = new PopulationTrackingRepository();
            var engine = new EntityRadarEngine(repository, registry, entityTypes);
            SeedSalesmanPeerGroup(repository, 5);

            engine.ComputeAndPersistScores(
                EntityTypeCode.Salesman,
                2026,
                6,
                "refresh-1",
                new DateTime(2026, 6, 24));

            repository.LastDimensionKpiId.Should().BeNull(
                "salesman-all-active has no dimension KPI; GetActivePopulation must use the undimensioned SQL branch");
            repository.RadarRows.Should().NotBeEmpty();
        }

        private void SeedPeerGroup(string wilayah, int count, decimal omzet)
        {
            for (var i = 1; i <= count; i++)
            {
                var entityId = $"C00{i}";
                _repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
                {
                    EntityType = EntityTypeCode.Customer,
                    EntityId = entityId,
                    EntityCode = entityId,
                    KpiId = EntityAnalyticsMetaKpiIds.IsActive,
                    NumericValue = 1m,
                    GeneratedAt = new DateTime(2026, 6, 24)
                });
                _repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
                {
                    EntityType = EntityTypeCode.Customer,
                    EntityId = entityId,
                    EntityCode = entityId,
                    KpiId = EntityAnalyticsMetaKpiIds.Wilayah,
                    TextValue = wilayah,
                    GeneratedAt = new DateTime(2026, 6, 24)
                });
                _repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
                {
                    EntityType = EntityTypeCode.Customer,
                    EntityId = entityId,
                    EntityCode = entityId,
                    KpiId = "CU-KPI-009",
                    NumericValue = omzet + i,
                    GeneratedAt = new DateTime(2026, 6, 24)
                });
                _repository.MonthlyRows.Add(new EntityAnalyticsMonthlyRow
                {
                    EntityType = EntityTypeCode.Customer,
                    EntityId = entityId,
                    PeriodYear = 2026,
                    PeriodMonth = 6,
                    KpiId = "CU-KPI-009",
                    NumericValue = omzet + i,
                    IsClosed = false
                });
                _repository.MonthlyRows.Add(new EntityAnalyticsMonthlyRow
                {
                    EntityType = EntityTypeCode.Customer,
                    EntityId = entityId,
                    PeriodYear = 2026,
                    PeriodMonth = 5,
                    KpiId = "CU-KPI-009",
                    NumericValue = omzet,
                    IsClosed = true
                });
            }
        }

        private void SeedReplayPeerGroupFromMonthlyOnly(string wilayah, int count, decimal omzet)
        {
            for (var i = 1; i <= count; i++)
            {
                var entityId = $"C00{i}";
                _repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
                {
                    EntityType = EntityTypeCode.Customer,
                    EntityId = entityId,
                    EntityCode = entityId,
                    KpiId = EntityAnalyticsMetaKpiIds.Wilayah,
                    TextValue = wilayah,
                    GeneratedAt = new DateTime(2024, 3, 31)
                });
                _repository.MonthlyRows.Add(new EntityAnalyticsMonthlyRow
                {
                    EntityType = EntityTypeCode.Customer,
                    EntityId = entityId,
                    EntityCode = entityId,
                    PeriodYear = 2024,
                    PeriodMonth = 3,
                    KpiId = "CU-KPI-009",
                    NumericValue = omzet + i,
                    IsClosed = true
                });
            }
        }

        private static void SeedSalesmanPeerGroup(PopulationTrackingRepository repository, int count)
        {
            for (var i = 1; i <= count; i++)
            {
                var entityId = $"SP00{i}";
                repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
                {
                    EntityType = EntityTypeCode.Salesman,
                    EntityId = entityId,
                    EntityCode = entityId,
                    KpiId = EntityAnalyticsMetaKpiIds.IsActive,
                    NumericValue = 1m,
                    GeneratedAt = new DateTime(2026, 6, 24)
                });
                repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
                {
                    EntityType = EntityTypeCode.Salesman,
                    EntityId = entityId,
                    EntityCode = entityId,
                    KpiId = "SF-KPI-008",
                    NumericValue = 1_000_000m + i,
                    GeneratedAt = new DateTime(2026, 6, 24)
                });
                repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
                {
                    EntityType = EntityTypeCode.Salesman,
                    EntityId = entityId,
                    EntityCode = entityId,
                    KpiId = "SF-KPI-009",
                    NumericValue = 80m + i,
                    GeneratedAt = new DateTime(2026, 6, 24)
                });
                repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
                {
                    EntityType = EntityTypeCode.Salesman,
                    EntityId = entityId,
                    EntityCode = entityId,
                    KpiId = "SF-KPI-010",
                    NumericValue = 50_000m,
                    GeneratedAt = new DateTime(2026, 6, 24)
                });
                repository.MonthlyRows.Add(new EntityAnalyticsMonthlyRow
                {
                    EntityType = EntityTypeCode.Salesman,
                    EntityId = entityId,
                    PeriodYear = 2026,
                    PeriodMonth = 6,
                    KpiId = "SF-KPI-008",
                    NumericValue = 1_000_000m + i,
                    IsClosed = false
                });
                repository.MonthlyRows.Add(new EntityAnalyticsMonthlyRow
                {
                    EntityType = EntityTypeCode.Salesman,
                    EntityId = entityId,
                    PeriodYear = 2026,
                    PeriodMonth = 5,
                    KpiId = "SF-KPI-008",
                    NumericValue = 900_000m,
                    IsClosed = true
                });
            }
        }

        private class RadarTestRepository : EntityAnalyticsRepositoryStubBase
        {
            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
                => CurrentRows.Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && !EntityAnalyticsMetaKpiIds.IsMetaOrDimension(r.KpiId)).ToList();

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId)
                => new EntityIdentity { EntityType = entityType, EntityId = entityId, EntityCode = entityId, DisplayName = entityId, IsActive = true };

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => new DateTime(2026, 6, 24);

            public override bool HasAnyCurrentMetrics(string entityType) => CurrentRows.Any();
        }

        private sealed class PopulationTrackingRepository : RadarTestRepository
        {
            public string LastDimensionKpiId { get; private set; }

            public override IReadOnlyList<EntityPopulationRow> GetActivePopulation(string entityType, string dimensionKpiId = null)
            {
                LastDimensionKpiId = dimensionKpiId;
                return base.GetActivePopulation(entityType, dimensionKpiId);
            }
        }
    }
}
