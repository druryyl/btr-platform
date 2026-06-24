using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
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
    public class CustomerEntityAnalyticsReconciliationTest
    {
        [Fact]
        public void ProducedKpis_MatchCu01TopOmzetAndCu05PortfolioRow()
        {
            var repository = new InMemoryRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 12, 0, 0);

            var portfolioCustomer = new DashboardCustomerPortfolioCustomerRow
            {
                CustomerCode = "C100",
                CustomerName = "Reconcile Customer",
                MtdOmzet = 2500000m,
                OpenBalance = 750000m,
                OverdueBalance = 125000m,
                IsActiveMtd = true
            };

            producer.Produce(new EntityAnalyticsProduceContext
            {
                RefreshLogId = "refresh-reconcile",
                GeneratedAt = generatedAt,
                BusinessDate = generatedAt.Date,
                DomainInput = new CustomerEntityAnalyticsProduceInput
                {
                    CustomerAggregate = new DashboardCustomerAggregateResult
                    {
                        TopOmzet = new List<DashboardCustomerTopOmzetRow>
                        {
                            new DashboardCustomerTopOmzetRow
                            {
                                CustomerCode = "C100",
                                OmzetAmount = 2500000m
                            }
                        },
                        TopPiutang = new List<DashboardCustomerTopPiutangRow>
                        {
                            new DashboardCustomerTopPiutangRow
                            {
                                CustomerCode = "C100",
                                OutstandingBalance = 750000m
                            }
                        }
                    },
                    PortfolioAggregate = new DashboardCustomerPortfolioAggregateResult
                    {
                        Customers = new List<DashboardCustomerPortfolioCustomerRow> { portfolioCustomer }
                    }
                }
            });

            var metrics = repository.GetCurrentMetrics(EntityTypeCode.Customer, "C100");
            metrics.Single(r => r.KpiId == "CU-KPI-009").NumericValue.Should().Be(2500000m);
            metrics.Single(r => r.KpiId == "CU-KPI-010").NumericValue.Should().Be(750000m);
            metrics.Single(r => r.KpiId == "FI-KPI-013").NumericValue.Should().Be(125000m);

            metrics.Single(r => r.KpiId == "CU-KPI-009").NumericValue
                .Should().Be(portfolioCustomer.MtdOmzet);
            metrics.Single(r => r.KpiId == "CU-KPI-010").NumericValue
                .Should().Be(portfolioCustomer.OpenBalance);

            var monthly = repository.GetHistory(EntityTypeCode.Customer, "C100", 2026, 6, 2026, 6);
            monthly.Single(r => r.KpiId == "CU-KPI-009").NumericValue.Should().Be(2500000m);
            monthly.Single(r => r.KpiId == "CU-KPI-010").NumericValue.Should().Be(750000m);
            monthly.Single(r => r.KpiId == "FI-KPI-013").NumericValue.Should().Be(125000m);
        }

        [Fact]
        public void ProducedRankings_MatchManualSortForOmzetAndBalance()
        {
            var repository = new InMemoryRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 12, 0, 0);

            var customers = new List<DashboardCustomerPortfolioCustomerRow>
            {
                new DashboardCustomerPortfolioCustomerRow
                {
                    CustomerCode = "C001",
                    CustomerName = "A",
                    MtdOmzet = 100m,
                    OpenBalance = 500m,
                    IsActiveMtd = true
                },
                new DashboardCustomerPortfolioCustomerRow
                {
                    CustomerCode = "C002",
                    CustomerName = "B",
                    MtdOmzet = 300m,
                    OpenBalance = 100m,
                    IsActiveMtd = true
                },
                new DashboardCustomerPortfolioCustomerRow
                {
                    CustomerCode = "C003",
                    CustomerName = "C",
                    MtdOmzet = 300m,
                    OpenBalance = 200m,
                    IsActiveMtd = true
                }
            };

            producer.Produce(new EntityAnalyticsProduceContext
            {
                RefreshLogId = "refresh-rank",
                GeneratedAt = generatedAt,
                BusinessDate = generatedAt.Date,
                DomainInput = new CustomerEntityAnalyticsProduceInput
                {
                    PortfolioAggregate = new DashboardCustomerPortfolioAggregateResult { Customers = customers }
                }
            });

            var omzetRanks = repository.RankingRows
                .Where(r => r.RankMetricKpiId == "CU-KPI-009")
                .ToDictionary(r => r.EntityId, r => r.RankPosition);

            omzetRanks["C002"].Should().Be(1);
            omzetRanks["C003"].Should().Be(1);
            omzetRanks["C001"].Should().Be(3);

            var balanceRanks = repository.RankingRows
                .Where(r => r.RankMetricKpiId == "CU-KPI-010")
                .ToDictionary(r => r.EntityId, r => r.RankPosition);

            balanceRanks["C002"].Should().Be(1);
            balanceRanks["C003"].Should().Be(2);
            balanceRanks["C001"].Should().Be(3);
        }

        private static CustomerEntityAnalyticsProducer CreateProducer(InMemoryRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = CustomerEntityAnalyticsRegistrar.KpiPackId,
                RelationshipPackId = CustomerRelationshipCatalog.PackId,
                PeerGroupRuleId = PeerGroupResolver.CustomerWilayah
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            new CustomerEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());

            var attentionSignals = new EntityAttentionSignalRegistry();
            CustomerAttentionSignalCatalog.Register(attentionSignals);

            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            CustomerRelationshipCatalog.Register(relationships);
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Salesman,
                ProfileRouteTemplate = "/analytics/salesmen/{code}"
            });
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Item,
                ProfileRouteTemplate = "/analytics/items/{code}"
            });
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                ProfileRouteTemplate = "/analytics/suppliers/{code}"
            });

            return new CustomerEntityAnalyticsProducer(
                repository,
                registry,
                new NoOpMonthCloseService(),
                new EntityRankingEngine(
                    repository,
                    registry,
                    entityTypes,
                    Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions())),
                new EntityAttentionEngine(repository),
                new EntityRelationshipEngine(repository, relationships, entityTypes),
                new EntityRadarEngine(repository, registry, entityTypes),
                attentionSignals);
        }

        private sealed class NoOpMonthCloseService : IEntityAnalyticsMonthCloseService
        {
            public void EnsurePriorMonthClosed(string entityType, EntityAnalyticsProduceContext context)
            {
            }
        }

        [Fact]
        public void EvidenceResolver_BuildsFilteredReportRoutes()
        {
            var resolver = new CustomerEntityAnalyticsEvidenceResolver();
            var evidence = resolver.BuildEvidence("C001", new EntityIdentity
            {
                EntityCode = "C001",
                DisplayName = "Alpha"
            });

            evidence.IsAvailable.Should().BeTrue();
            evidence.Links.Should().HaveCount(3);
            evidence.Links.Should().Contain(l => l.ReportRoute == "/reports/sales?customerCode=C001");
            evidence.Links.Should().Contain(l => l.ReportRoute == "/reports/piutang?customerCode=C001");
            evidence.Links.Should().Contain(l => l.ReportRoute == "/reports/customers?customerCode=C001");
        }

        [Fact]
        public void Composer_WithCustomerPack_BuildsEvidenceSection()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = CustomerEntityAnalyticsRegistrar.KpiPackId,
                RelationshipPackId = CustomerRelationshipCatalog.PackId
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new CustomerEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            var repository = new InMemoryRepository();
            repository.Seed(EntityTypeCode.Customer, "C001", "Alpha Customer", 1000m, 200m, 50m);

            var options = Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions());
            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            CustomerRelationshipCatalog.Register(relationships);
            var radarEngine = new EntityRadarEngine(repository, registry, entityTypes);
            var composer = new EntityPerformanceProfileComposer(
                repository,
                registry,
                entityTypes,
                new IEntityProfileEvidenceResolver[] { new CustomerEntityAnalyticsEvidenceResolver() },
                new EntityKpiEnvelopeFormatter(),
                new EntityTrendEngine(repository, registry, entityTypes, options),
                new EntityRankingEngine(repository, registry, entityTypes, options),
                new EntityAttentionEngine(repository),
                new EntityRelationshipEngine(repository, relationships, entityTypes),
                new EntityComparisonEngine(
                    repository,
                    registry,
                    entityTypes,
                    new EntityKpiEnvelopeFormatter(),
                    new EntityTrendEngine(repository, registry, entityTypes, options),
                    new EntityRankingEngine(repository, registry, entityTypes, options),
                    new EntityAttentionEngine(repository),
                    new EntityRelationshipEngine(repository, relationships, entityTypes),
                    radarEngine),
                radarEngine);

            var profile = composer.Build(EntityTypeCode.Customer, "C001");

            profile.IsAvailable.Should().BeTrue();
            profile.KpiSummary.IsAvailable.Should().BeTrue();
            profile.Evidence.IsAvailable.Should().BeTrue();
            profile.Evidence.Links.Should().NotBeEmpty();
            profile.Overview.DisplayName.Should().Be("Alpha Customer");
            profile.Comparison.IsAvailable.Should().BeTrue();
            profile.Comparison.Metrics.Should().NotBeEmpty();
        }

        private sealed class InMemoryRepository : EntityAnalyticsRepositoryStubBase
        {
            private readonly List<EntityAnalyticsCurrentRow> _rows = new List<EntityAnalyticsCurrentRow>();

            public void Seed(
                string entityType,
                string entityId,
                string displayName,
                decimal omzet,
                decimal openBalance,
                decimal overdue)
            {
                var generatedAt = new DateTime(2026, 6, 24);
                _rows.AddRange(new[]
                {
                    Row(entityType, entityId, "CU-KPI-009", omzet, generatedAt),
                    Row(entityType, entityId, "CU-KPI-010", openBalance, generatedAt),
                    Row(entityType, entityId, "FI-KPI-013", overdue, generatedAt),
                    Meta(entityType, entityId, EntityAnalyticsMetaKpiIds.DisplayName, displayName, generatedAt)
                });
            }

            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
            {
                return _rows
                    .Where(r => r.EntityType == entityType && r.EntityId == entityId)
                    .Where(r => !EntityAnalyticsMetaKpiIds.IsMetaOrDimension(r.KpiId))
                    .ToList();
            }

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId)
            {
                var rows = _rows.Where(r => r.EntityType == entityType && r.EntityId == entityId).ToList();
                return new EntityIdentity
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityCode = entityId,
                    DisplayName = rows.FirstOrDefault(r => r.KpiId == EntityAnalyticsMetaKpiIds.DisplayName)?.TextValue ?? entityId,
                    IsActive = true
                };
            }

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
                _rows.RemoveAll(r => r.EntityType == entityType);
                _rows.AddRange(rows);
                CurrentRows.Clear();
                CurrentRows.AddRange(rows);
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId)
            {
                return _rows
                    .Where(r => r.EntityType == entityType && r.EntityId == entityId)
                    .Select(r => r.GeneratedAt)
                    .Cast<DateTime?>()
                    .FirstOrDefault();
            }

            public override bool HasAnyCurrentMetrics(string entityType)
            {
                return _rows.Any(r => r.EntityType == entityType);
            }

            private static EntityAnalyticsCurrentRow Row(
                string entityType,
                string entityId,
                string kpiId,
                decimal value,
                DateTime generatedAt)
            {
                return new EntityAnalyticsCurrentRow
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityCode = entityId,
                    KpiId = kpiId,
                    NumericValue = value,
                    GeneratedAt = generatedAt
                };
            }

            private static EntityAnalyticsCurrentRow Meta(
                string entityType,
                string entityId,
                string kpiId,
                string text,
                DateTime generatedAt)
            {
                return new EntityAnalyticsCurrentRow
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityCode = entityId,
                    KpiId = kpiId,
                    TextValue = text,
                    GeneratedAt = generatedAt
                };
            }
        }
    }
}
