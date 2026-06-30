using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityPopulationMapEngineTest
    {
        private readonly PopulationMapTestRepository _repository = new PopulationMapTestRepository();
        private readonly EntityTypeRegistry _entityTypes = new EntityTypeRegistry();
        private readonly EntityAnalyticsKpiRegistry _kpiRegistry;
        private readonly EntityPopulationMapEngine _engine;
        private static readonly DateTime GeneratedAt = new DateTime(2026, 6, 24, 8, 0, 0);

        public EntityPopulationMapEngineTest()
        {
            _entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = CustomerEntityAnalyticsRegistrar.KpiPackId,
                PeerGroupRuleId = PeerGroupResolver.CustomerWilayah
            });
            _kpiRegistry = new EntityAnalyticsKpiRegistry(_entityTypes);
            new CustomerEntityAnalyticsRegistrar().Register(
                _entityTypes,
                _kpiRegistry,
                new EntityAnalyticsDimensionLabelRegistry());
            _engine = new EntityPopulationMapEngine(
                _repository,
                _kpiRegistry,
                _entityTypes,
                new EntityKpiEnvelopeFormatter());
        }

        [Fact]
        public void BuildPopulationMap_WithPopulation_DoesNotCallPerEntityMethods()
        {
            SeedPopulation(count: 10);

            var result = _engine.BuildPopulationMap(new PopulationMapRequest
            {
                EntityType = EntityTypeCode.Customer
            });

            result.Points.Should().HaveCount(10);
            _repository.TryResolveIdentityCallCount.Should().Be(0);
            _repository.GetLatestGeneratedAtCallCount.Should().Be(0);
            _repository.GetLatestGeneratedAtForEntityTypeCallCount.Should().Be(1);
            _repository.GetActivePopulationCallCount.Should().Be(1);
            _repository.GetCurrentKpiPopulationCallCount.Should().Be(2);
            _repository.GetActiveAttentionCountsCallCount.Should().Be(1);
        }

        [Fact]
        public void BuildPopulationMap_UsesBatchDisplayName()
        {
            SeedEntity(
                entityId: "C001",
                entityCode: "CUST001",
                displayName: "Alpha Customer",
                axisX: 100m,
                axisY: 200m);

            var result = _engine.BuildPopulationMap(new PopulationMapRequest
            {
                EntityType = EntityTypeCode.Customer
            });

            var point = result.Points.Single(p => p.EntityId == "C001");
            point.DisplayName.Should().Be("Alpha Customer");
        }

        [Fact]
        public void BuildPopulationMap_FallsBackToEntityCode()
        {
            SeedEntity(
                entityId: "C002",
                entityCode: "CUST002",
                displayName: null,
                axisX: 150m,
                axisY: 250m);

            var result = _engine.BuildPopulationMap(new PopulationMapRequest
            {
                EntityType = EntityTypeCode.Customer
            });

            var point = result.Points.Single(p => p.EntityId == "C002");
            point.DisplayName.Should().Be("CUST002");
        }

        [Fact]
        public void BuildPopulationMap_PreservesAxisAndAttentionValues()
        {
            SeedEntity(
                entityId: "C003",
                entityCode: "CUST003",
                displayName: "Gamma Customer",
                axisX: 300m,
                axisY: 400m,
                dimensionValue: "Jakarta",
                attentionCount: 2);

            var result = _engine.BuildPopulationMap(new PopulationMapRequest
            {
                EntityType = EntityTypeCode.Customer,
                DimensionFilter = "Jakarta",
                AttentionOnly = true
            });

            result.GeneratedAt.Should().Be(GeneratedAt);
            result.TotalPopulationCount.Should().Be(1);
            result.FilteredPopulationCount.Should().Be(1);

            var point = result.Points.Single();
            point.AxisX.Should().Be(300m);
            point.AxisY.Should().Be(400m);
            point.ActiveAttentionCount.Should().Be(2);
            point.DimensionValue.Should().Be("Jakarta");
            point.MatchesFilter.Should().BeTrue();
            point.IsActive.Should().BeTrue();
        }

        private void SeedPopulation(int count)
        {
            for (var i = 1; i <= count; i++)
            {
                SeedEntity(
                    entityId: $"C{i:D3}",
                    entityCode: $"CUST{i:D3}",
                    displayName: $"Customer {i}",
                    axisX: 100m + i,
                    axisY: 200m + i);
            }
        }

        private void SeedEntity(
            string entityId,
            string entityCode,
            string displayName,
            decimal axisX,
            decimal axisY,
            string dimensionValue = "Jakarta",
            int attentionCount = 0)
        {
            _repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = entityId,
                EntityCode = entityCode,
                KpiId = EntityAnalyticsMetaKpiIds.IsActive,
                NumericValue = 1m,
                GeneratedAt = GeneratedAt
            });

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                _repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
                {
                    EntityType = EntityTypeCode.Customer,
                    EntityId = entityId,
                    EntityCode = entityCode,
                    KpiId = EntityAnalyticsMetaKpiIds.DisplayName,
                    TextValue = displayName,
                    GeneratedAt = GeneratedAt
                });
            }

            if (!string.IsNullOrWhiteSpace(dimensionValue))
            {
                _repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
                {
                    EntityType = EntityTypeCode.Customer,
                    EntityId = entityId,
                    EntityCode = entityCode,
                    KpiId = EntityAnalyticsMetaKpiIds.Wilayah,
                    TextValue = dimensionValue,
                    GeneratedAt = GeneratedAt
                });
            }

            _repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = entityId,
                EntityCode = entityCode,
                KpiId = "CU-KPI-009",
                NumericValue = axisX,
                GeneratedAt = GeneratedAt
            });

            _repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = entityId,
                EntityCode = entityCode,
                KpiId = "CU-KPI-010",
                NumericValue = axisY,
                GeneratedAt = GeneratedAt
            });

            for (var i = 0; i < attentionCount; i++)
            {
                _repository.AttentionRows.Add(new EntityAnalyticsAttentionEventRow
                {
                    EntityType = EntityTypeCode.Customer,
                    EntityId = entityId,
                    EntityCode = entityCode,
                    SignalCode = $"SIG-{entityId}-{i}",
                    IsActive = true
                });
            }
        }

        private sealed class PopulationMapTestRepository : EntityAnalyticsRepositoryStubBase
        {
            public int TryResolveIdentityCallCount { get; private set; }
            public int GetLatestGeneratedAtCallCount { get; private set; }
            public int GetLatestGeneratedAtForEntityTypeCallCount { get; private set; }
            public int GetActivePopulationCallCount { get; private set; }
            public int GetCurrentKpiPopulationCallCount { get; private set; }
            public int GetActiveAttentionCountsCallCount { get; private set; }

            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
                => CurrentRows.Where(r => string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && !EntityAnalyticsMetaKpiIds.IsMetaOrDimension(r.KpiId)).ToList();

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId)
            {
                TryResolveIdentityCallCount++;
                return null;
            }

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId)
            {
                GetLatestGeneratedAtCallCount++;
                return GeneratedAt;
            }

            public override DateTime? GetLatestGeneratedAtForEntityType(string entityType)
            {
                GetLatestGeneratedAtForEntityTypeCallCount++;
                return base.GetLatestGeneratedAtForEntityType(entityType);
            }

            public override bool HasAnyCurrentMetrics(string entityType) => CurrentRows.Any();

            public override IReadOnlyList<EntityPopulationRow> GetActivePopulation(string entityType, string dimensionKpiId = null)
            {
                GetActivePopulationCallCount++;
                return base.GetActivePopulation(entityType, dimensionKpiId);
            }

            public override IReadOnlyList<EntityAnalyticsPeriodMetricRow> GetCurrentKpiPopulation(string entityType, string kpiId)
            {
                GetCurrentKpiPopulationCallCount++;
                return base.GetCurrentKpiPopulation(entityType, kpiId);
            }

            public override IReadOnlyDictionary<string, int> GetActiveAttentionCounts(string entityType)
            {
                GetActiveAttentionCountsCallCount++;
                return base.GetActiveAttentionCounts(entityType);
            }
        }
    }
}
