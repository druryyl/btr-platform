using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SalesmanEntityAnalyticsProducerTest
    {
        [Fact]
        public void Produce_MapsPortfolioRepsToL0Rows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioRep()));

            repository.EntityType.Should().Be(EntityTypeCode.Salesman);
            repository.Rows.Should().NotBeEmpty();

            var kpiRows = repository.Rows
                .Where(r => !EntityAnalyticsMetaKpiIds.IsMetaOrDimension(r.KpiId))
                .ToList();

            kpiRows.Should().Contain(r => r.KpiId == "SF-KPI-008" && r.NumericValue == 2500000m);
            kpiRows.Should().Contain(r => r.KpiId == "SF-KPI-009" && r.NumericValue == 92.5m);
            kpiRows.Should().Contain(r => r.KpiId == "SF-KPI-010" && r.NumericValue == 750000m);

            repository.Rows.Should().Contain(r =>
                r.EntityId == "SP-ID-1" && r.EntityCode == "SP01");
            repository.Rows.Should().Contain(r =>
                r.KpiId == EntityAnalyticsMetaKpiIds.DisplayName && r.TextValue == "Rep Alpha");
        }

        [Fact]
        public void Produce_WritesL1MonthlyRowsForTrendEligibleKpis()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioRep()));

            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Should().OnlyContain(r =>
                r.PeriodYear == 2026 && r.PeriodMonth == 6 && r.IsClosed == false);
            repository.MonthlyRows.Should().Contain(r =>
                r.KpiId == "SF-KPI-008" && r.NumericValue == 2500000m);
        }

        [Fact]
        public void Produce_WritesL3AttentionLifecycleRows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioRep()));

            repository.AttentionRows.Should().ContainSingle();
            repository.AttentionRows[0].SignalCode.Should().Be(DashboardSalesmanAggregator.SignalBelowTarget);
            repository.AttentionRows[0].EntityId.Should().Be("SP-ID-1");
        }

        [Fact]
        public void Produce_WritesL4RelationshipRows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioRep()));

            repository.RelationshipRows.Should().NotBeEmpty();
            repository.RelationshipRows.Should().Contain(r =>
                r.RelationshipCode == SalesmanRelationshipCatalog.TopItemsByOmzet);
            repository.RelationshipRows.Should().Contain(r =>
                r.RelationshipCode == SalesmanRelationshipCatalog.TopPrincipalsByOmzet);
        }

        [Fact]
        public void SalesmanDefaultPack_RegistersCatalogKpiIds()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Salesman,
                DisplayName = "Salesman",
                KpiPackId = SalesmanEntityAnalyticsRegistrar.KpiPackId
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new SalesmanEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            registry.GetPackKpiIds(SalesmanEntityAnalyticsRegistrar.KpiPackId)
                .Should().Contain(new[] { "SF-KPI-008", "SF-KPI-009", "SF-KPI-010" });

            registry.TryGetMetadata("SF-KPI-008", out var omzet).Should().BeTrue();
            omzet.TrendEligible.Should().BeTrue();
            omzet.RankEligible.Should().BeTrue();
        }

        private static SalesmanEntityAnalyticsProducer CreateProducer(RecordingRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Salesman,
                DisplayName = "Salesman",
                KpiPackId = SalesmanEntityAnalyticsRegistrar.KpiPackId,
                RelationshipPackId = SalesmanRelationshipCatalog.PackId,
                PeerGroupRuleId = PeerGroupResolver.SalesmanAllActive
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new SalesmanEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            var rankingEngine = new EntityRankingEngine(
                repository,
                registry,
                entityTypes,
                Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions()));

            var attentionSignals = new EntityAttentionSignalRegistry();
            SalesmanAttentionSignalCatalog.Register(attentionSignals);
            var attentionEngine = new EntityAttentionEngine(repository);

            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            SalesmanRelationshipCatalog.Register(relationships);
            var relationshipEngine = new EntityRelationshipEngine(repository, relationships, entityTypes);
            var radarEngine = new EntityRadarEngine(repository, registry, entityTypes);

            return new SalesmanEntityAnalyticsProducer(
                repository,
                registry,
                new NoOpMonthCloseService(),
                rankingEngine,
                attentionEngine,
                relationshipEngine,
                radarEngine,
                attentionSignals);
        }

        private static EntityAnalyticsProduceContext CreateContext(
            DateTime generatedAt,
            DashboardSalesmanPortfolioRow rep)
        {
            return new EntityAnalyticsProduceContext
            {
                RefreshLogId = "refresh-1",
                GeneratedAt = generatedAt,
                BusinessDate = generatedAt.Date,
                DomainInput = new SalesmanEntityAnalyticsProduceInput
                {
                    SalesmanAggregate = new DashboardSalesmanAggregateResult
                    {
                        AttentionList = new List<DashboardSalesmanAttentionRow>
                        {
                            new DashboardSalesmanAttentionRow
                            {
                                SalesPersonId = rep.SalesPersonId,
                                SalesPersonCode = rep.SalesPersonCode,
                                SignalKey = DashboardSalesmanAggregator.SignalBelowTarget,
                                SignalLabel = "Below Target"
                            }
                        },
                        PrincipalAchievement = new List<DashboardSalesmanPrincipalAchievementRow>
                        {
                            new DashboardSalesmanPrincipalAchievementRow
                            {
                                SalesPersonId = rep.SalesPersonId,
                                SupplierId = "SUP1",
                                SupplierName = "Principal 1",
                                CompletedOmzet = 500000m,
                                SortOrder = 1
                            }
                        },
                        Portfolio = new List<DashboardSalesmanPortfolioRow> { rep }
                    },
                    RelationshipAggregate = new DashboardSalesmanRelationshipAggregateResult
                    {
                        BySalesPersonId = new Dictionary<string, DashboardSalesmanRelationshipSalesmanRollup>
                        {
                            [rep.SalesPersonId] = new DashboardSalesmanRelationshipSalesmanRollup
                            {
                                SalesPersonId = rep.SalesPersonId,
                                SalesPersonCode = rep.SalesPersonCode,
                                TopItems = new List<DashboardSalesmanRelationshipItemRow>
                                {
                                    new DashboardSalesmanRelationshipItemRow
                                    {
                                        Rank = 1,
                                        BrgId = "I1",
                                        BrgCode = "BRG1",
                                        BrgName = "Item 1",
                                        MetricValue = 1000m
                                    }
                                },
                                TopCustomers = new List<DashboardSalesmanRelationshipCustomerRow>
                                {
                                    new DashboardSalesmanRelationshipCustomerRow
                                    {
                                        Rank = 1,
                                        CustomerCode = "C001",
                                        CustomerName = "Customer 1",
                                        MetricValue = 1000m
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static DashboardSalesmanPortfolioRow CreatePortfolioRep()
        {
            return new DashboardSalesmanPortfolioRow
            {
                SalesPersonId = "SP-ID-1",
                SalesPersonCode = "SP01",
                SalesPersonName = "Rep Alpha",
                WilayahName = "Jakarta",
                IsActive = true,
                CompletedOmzet = 2500000m,
                AchievementPercent = 92.5m,
                OpenBalance = 750000m,
                CustomerCount = 12,
                ActiveCustomerCount = 10,
                TopCustomers = new List<DashboardSalesmanPortfolioCustomerRow>
                {
                    new DashboardSalesmanPortfolioCustomerRow
                    {
                        Rank = 1,
                        CustomerCode = "C001",
                        CustomerName = "Customer 1",
                        MetricValue = 500000m
                    }
                }
            };
        }

        private sealed class NoOpMonthCloseService : IEntityAnalyticsMonthCloseService
        {
            public void EnsurePriorMonthClosed(string entityType, EntityAnalyticsProduceContext context)
            {
            }
        }

        private sealed class RecordingRepository : EntityAnalyticsRepositoryStubBase
        {
            public string EntityType { get; private set; }

            public List<EntityAnalyticsCurrentRow> Rows { get; } = new List<EntityAnalyticsCurrentRow>();

            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
            {
                return Rows.Where(r =>
                        r.EntityType == entityType &&
                        (string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(r.EntityCode, entityId, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
                EntityType = entityType;
                Rows.Clear();
                Rows.AddRange(rows);
                CurrentRows.Clear();
                CurrentRows.AddRange(rows);
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => Rows.Count > 0;
        }
    }
}
