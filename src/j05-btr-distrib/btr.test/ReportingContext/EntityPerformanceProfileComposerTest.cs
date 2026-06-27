using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
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
    public class EntityPerformanceProfileComposerTest
    {
        private readonly FakeRepository _repository = new FakeRepository();
        private readonly EntityTypeRegistry _entityTypes = new EntityTypeRegistry();
        private readonly EntityAnalyticsKpiRegistry _registry;
        private readonly EntityPerformanceProfileComposer _composer;

        public EntityPerformanceProfileComposerTest()
        {
            _entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = "customer-default",
                RelationshipPackId = CustomerRelationshipCatalog.PackId,
                PeerGroupRuleId = PeerGroupResolver.CustomerWilayah,
                ProfileRouteTemplate = "/analytics/customers/{id}"
            });
            _entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Item,
                ProfileRouteTemplate = "/analytics/items/{id}"
            });
            _registry = new EntityAnalyticsKpiRegistry(_entityTypes);
            new CustomerEntityAnalyticsRegistrar().Register(
                _entityTypes,
                _registry,
                new EntityAnalyticsDimensionLabelRegistry());

            var options = Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions());
            var relationships = new EntityRelationshipDefinitionRegistry(_entityTypes);
            CustomerRelationshipCatalog.Register(relationships);
            var radarEngine = new EntityRadarEngine(_repository, _registry, _entityTypes);
            _composer = new EntityPerformanceProfileComposer(
                _repository,
                _registry,
                _entityTypes,
                Array.Empty<IEntityProfileEvidenceResolver>(),
                new EntityKpiEnvelopeFormatter(),
                new EntityTrendEngine(_repository, _registry, _entityTypes, options),
                new EntityRankingEngine(_repository, _registry, _entityTypes, options),
                new EntityAttentionEngine(_repository),
                new EntityRelationshipEngine(_repository, relationships, _entityTypes),
                new EntityComparisonEngine(
                    _repository,
                    _registry,
                    _entityTypes,
                    new EntityKpiEnvelopeFormatter(),
                    new EntityTrendEngine(_repository, _registry, _entityTypes, options),
                    new EntityRankingEngine(_repository, _registry, _entityTypes, options),
                    new EntityAttentionEngine(_repository),
                    new EntityRelationshipEngine(_repository, relationships, _entityTypes),
                    radarEngine),
                radarEngine);
        }

        [Fact]
        public void Build_NoSnapshotData_ReturnsEmptySafeProfile()
        {
            var result = _composer.Build(EntityTypeCode.Customer, "C001");

            result.IsAvailable.Should().BeFalse();
            result.EntityType.Should().Be(EntityTypeCode.Customer);
            result.EntityId.Should().Be("C001");
            result.SnapshotVersion.Should().Be(EntityAnalyticsConstants.CurrentSnapshotVersion);
            result.ContractVersion.Should().NotBeNullOrEmpty();
            result.Overview.IsAvailable.Should().BeTrue();
            result.KpiSummary.IsAvailable.Should().BeFalse();
            result.KpiSummary.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.NoSnapshotData);
            result.Comparison.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.NoRegisteredKpis);
            result.Trend.IsAvailable.Should().BeFalse();
            result.Trend.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.NoSnapshotData);
            result.Radar.IsAvailable.Should().BeFalse();
            result.Ranking.IsAvailable.Should().BeFalse();
            result.Ranking.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.NoSnapshotData);
            result.Attention.IsAvailable.Should().BeFalse();
            result.Attention.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.NoSnapshotData);
            result.RelatedEntities.IsAvailable.Should().BeFalse();
            result.RelatedEntities.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.NoSnapshotData);
            result.Evidence.IsAvailable.Should().BeFalse();
        }

        [Fact]
        public void Build_UnknownEntityType_Throws()
        {
            Action act = () => _composer.Build("Warehouse", "W001");

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Build_EmptyEntityId_Throws()
        {
            Action act = () => _composer.Build(EntityTypeCode.Customer, "  ");

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Build_WithRegisteredKpiAndSnapshotData_GroupsKpiSummary()
        {
            _registry.RegisterMetadata(new EntityKpiMetadata
            {
                KpiId = "TEST-KPI-001",
                Category = EntityKpiCategory.Financial,
                DisplayName = "Test Omzet",
                Unit = "IDR",
                ValueType = "Numeric",
                DisplayPrecision = 0,
                Direction = "HigherIsBetter",
                PeriodSemantics = "MTD"
            });
            _repository.Metrics.Add(new EntityAnalyticsCurrentRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = "C001",
                EntityCode = "C001",
                KpiId = "TEST-KPI-001",
                NumericValue = 1000000m,
                GeneratedAt = new DateTime(2026, 6, 1)
            });

            var result = _composer.Build(EntityTypeCode.Customer, "C001");

            result.IsAvailable.Should().BeTrue();
            result.KpiSummary.IsAvailable.Should().BeTrue();
            result.KpiSummary.Categories.Should().ContainSingle(c => c.Category == "Financial");
            result.KpiSummary.Categories[0].Kpis[0].FormattedValue.Should().Be("1,000,000");
            result.KpiSummary.Categories[0].Kpis[0].ValueType.Should().Be("Numeric");
        }

        [Fact]
        public void Build_WithRankingHistory_ReturnsRankingSection()
        {
            _repository.RankingRows.Add(new EntityAnalyticsRankingRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = "C001",
                EntityCode = "C001",
                RankMetricKpiId = "CU-KPI-009",
                PeriodYear = 2026,
                PeriodMonth = 6,
                RankPosition = 5,
                PopulationSize = 100,
                Percentile = 96m,
                GeneratedAt = new DateTime(2026, 6, 24)
            });
            _repository.Metrics.Add(new EntityAnalyticsCurrentRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = "C001",
                EntityCode = "C001",
                KpiId = "CU-KPI-009",
                NumericValue = 1000000m,
                GeneratedAt = new DateTime(2026, 6, 24)
            });

            var result = _composer.Build(EntityTypeCode.Customer, "C001");

            result.Ranking.IsAvailable.Should().BeTrue();
            result.Ranking.Series.Should().ContainSingle();
            result.Ranking.Series[0].CurrentRank.Should().Be(5);
            result.Ranking.Series[0].RankingDirection.Should().Be("HigherIsBetter");
            result.Trend.IsAvailable.Should().BeFalse();
        }

        [Fact]
        public void Build_WithAttentionHistory_ReturnsAttentionSection()
        {
            _repository.AttentionRows.Add(new EntityAnalyticsAttentionEventRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = "C001",
                EntityCode = "C001",
                SignalCode = DashboardCustomerAggregator.SignalOverdue,
                SignalTitle = "Overdue",
                SignalCategory = "Finance",
                FirstSeenPeriodYear = 2026,
                FirstSeenPeriodMonth = 4,
                LastSeenPeriodYear = 2026,
                LastSeenPeriodMonth = 6,
                ConsecutivePeriods = 3,
                TotalOccurrences = 3,
                IsActive = true,
                GeneratedAt = new DateTime(2026, 6, 24)
            });
            _repository.Metrics.Add(new EntityAnalyticsCurrentRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = "C001",
                EntityCode = "C001",
                KpiId = "CU-KPI-009",
                NumericValue = 1000000m,
                GeneratedAt = new DateTime(2026, 6, 24)
            });

            var result = _composer.Build(EntityTypeCode.Customer, "C001");

            result.Attention.IsAvailable.Should().BeTrue();
            result.Attention.ActiveSignalCount.Should().Be(1);
            result.Attention.Events.Should().ContainSingle();
            result.Attention.Events[0].SignalCode.Should().Be(DashboardCustomerAggregator.SignalOverdue);
            result.Attention.Events[0].ConsecutivePeriods.Should().Be(3);
            result.Attention.Events[0].FirstSeen.Should().Be("Apr 2026");
        }

        [Fact]
        public void Build_WithRelationshipRollups_ReturnsRelatedEntitiesSection()
        {
            _repository.RelationshipRows.Add(new EntityAnalyticsRelationshipRow
            {
                SourceEntityType = EntityTypeCode.Customer,
                SourceEntityId = "C001",
                SourceEntityCode = "C001",
                RelationshipCode = CustomerRelationshipCatalog.TopItemsByOmzet,
                TargetEntityType = EntityTypeCode.Item,
                TargetEntityId = "I1",
                TargetEntityCode = "BRG1",
                TargetDisplayName = "Item 1",
                Rank = 1,
                MetricValue = 500000m,
                PeriodYear = 2026,
                PeriodMonth = 6
            });
            _repository.Metrics.Add(new EntityAnalyticsCurrentRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = "C001",
                EntityCode = "C001",
                KpiId = "CU-KPI-009",
                NumericValue = 1000000m,
                GeneratedAt = new DateTime(2026, 6, 24)
            });

            var result = _composer.Build(EntityTypeCode.Customer, "C001");

            result.RelatedEntities.IsAvailable.Should().BeTrue();
            result.RelatedEntities.Blocks.Should().ContainSingle();
            result.RelatedEntities.Blocks[0].RelationshipLabel.Should().Be("Top Items");
            result.RelatedEntities.Blocks[0].Rows[0].ProfileRoute.Should().Be("/analytics/items/I1");
        }

        [Fact]
        public void BuildDisabledProfile_AllSectionsUnavailable()
        {
            var result = EntityPerformanceProfileComposer.BuildDisabledProfile(EntityTypeCode.Salesman, "S001");

            result.IsAvailable.Should().BeFalse();
            result.Overview.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.EntityTypeDisabled);
            result.Comparison.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.EntityTypeDisabled);
        }

        private sealed class FakeRepository : EntityAnalyticsRepositoryStubBase
        {
            public List<EntityAnalyticsCurrentRow> Metrics { get; } = new List<EntityAnalyticsCurrentRow>();

            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
            {
                return Metrics.FindAll(m =>
                    m.EntityType == entityType && m.EntityId == entityId);
            }

            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetricsBatch(
                string entityType,
                IEnumerable<string> entityIds)
            {
                var idSet = new HashSet<string>(
                    (entityIds ?? Array.Empty<string>()).Select(id => id?.Trim()).Where(id => !string.IsNullOrWhiteSpace(id)),
                    StringComparer.OrdinalIgnoreCase);

                return Metrics.FindAll(m =>
                    m.EntityType == entityType && idSet.Contains(m.EntityId));
            }

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId)
            {
                if (!Metrics.Any(m => m.EntityType == entityType && m.EntityId == entityId))
                    return null;

                return new EntityIdentity
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityCode = entityId,
                    DisplayName = entityId,
                    IsActive = true
                };
            }

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId)
            {
                return Metrics.Count > 0 ? Metrics[0].GeneratedAt : (DateTime?)null;
            }

            public override bool HasAnyCurrentMetrics(string entityType) => Metrics.Count > 0;
        }
    }
}
