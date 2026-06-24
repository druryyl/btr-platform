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
    public class ItemEntityAnalyticsReconciliationTest
    {
        [Fact]
        public void ProducedKpis_MatchIn02AttentionRow()
        {
            var repository = new InMemoryRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 12, 0, 0);

            var portfolioItem = new DashboardItemPortfolioRow
            {
                BrgId = "B001",
                BrgCode = "BRG1",
                BrgName = "Item One",
                CategoryName = "Category A",
                SupplierName = "Supplier A",
                Qty = 85,
                InventoryValue = 175_000m,
                MovementClass = DashboardInventoryRiskAggregator.SignalSlowMoving,
                DaysSinceLastFaktur = 120,
                DaysOfSupply = 32.5m,
                RecommendedPurchaseQty = 15m,
                DistinctCustomerCount = 2,
                IsTrendEligible = true,
                IsActive = false
            };

            producer.Produce(new EntityAnalyticsProduceContext
            {
                RefreshLogId = "refresh-reconcile",
                GeneratedAt = generatedAt,
                BusinessDate = generatedAt.Date,
                DomainInput = new ItemEntityAnalyticsProduceInput
                {
                    Portfolio = new List<DashboardItemPortfolioRow> { portfolioItem },
                    RiskAggregate = new DashboardInventoryRiskAggregateResult
                    {
                        TopSlow = new List<DashboardInventoryRiskTopRow>
                        {
                            new DashboardInventoryRiskTopRow
                            {
                                Rank = 1,
                                BrgId = "B001",
                                BrgCode = "BRG1",
                                BrgName = "Item One",
                                Qty = 85,
                                InventoryValue = 175_000m,
                                DaysSinceLastFaktur = 120
                            }
                        },
                        AttentionList = new List<DashboardInventoryRiskAttentionRow>
                        {
                            new DashboardInventoryRiskAttentionRow
                            {
                                BrgId = "B001",
                                BrgCode = "BRG1",
                                BrgName = "Item One",
                                Qty = 85,
                                InventoryValue = 175_000m,
                                DaysSinceLastFaktur = 120,
                                SignalKey = DashboardInventoryRiskAggregator.SignalSlowMoving,
                                SignalLabel = "Slow Moving"
                            }
                        }
                    }
                }
            });

            var metrics = repository.GetCurrentMetrics(EntityTypeCode.Item, "BRG1");
            metrics.Single(r => r.KpiId == "IN-KPI-001").NumericValue.Should().Be(175_000m);
            metrics.Single(r => r.KpiId == EntityAnalyticsMetaKpiIds.QtyOnHand).NumericValue.Should().Be(85m);
            metrics.Single(r => r.KpiId == EntityAnalyticsMetaKpiIds.DaysSinceLastFaktur).NumericValue.Should().Be(120m);
            metrics.Single(r => r.KpiId == "IN-KPI-020").NumericValue.Should().Be(32.5m);
            metrics.Single(r => r.KpiId == "IN-KPI-021").NumericValue.Should().Be(15m);
            metrics.Single(r => r.KpiId == EntityAnalyticsMetaKpiIds.MovementClass).TextValue
                .Should().Be(DashboardInventoryRiskAggregator.SignalSlowMoving);

            metrics.Single(r => r.KpiId == "IN-KPI-001").NumericValue
                .Should().Be(portfolioItem.InventoryValue);
            metrics.Single(r => r.KpiId == EntityAnalyticsMetaKpiIds.QtyOnHand).NumericValue
                .Should().Be(portfolioItem.Qty);
        }

        private static ItemEntityAnalyticsProducer CreateProducer(InMemoryRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Item,
                DisplayName = "Item",
                KpiPackId = ItemEntityAnalyticsRegistrar.KpiPackId,
                RelationshipPackId = ItemRelationshipCatalog.PackId,
                PeerGroupRuleId = PeerGroupResolver.ItemCategory
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new ItemEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            return new ItemEntityAnalyticsProducer(
                repository,
                registry,
                new NoOpMonthCloseService(),
                new EntityRankingEngine(
                    repository,
                    registry,
                    entityTypes,
                    Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions())),
                new EntityAttentionEngine(repository),
                new EntityRelationshipEngine(
                    repository,
                    new EntityRelationshipDefinitionRegistry(entityTypes),
                    entityTypes),
                new EntityRadarEngine(repository, registry, entityTypes),
                new EntityAttentionSignalRegistry());
        }

        private sealed class NoOpMonthCloseService : IEntityAnalyticsMonthCloseService
        {
            public void EnsurePriorMonthClosed(string entityType, EntityAnalyticsProduceContext context)
            {
            }
        }

        private sealed class InMemoryRepository : EntityAnalyticsRepositoryStubBase
        {
            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
            {
                return CurrentRows.Where(r =>
                        r.EntityType == entityType &&
                        (string.Equals(r.EntityId, entityId, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(r.EntityCode, entityId, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
                CurrentRows.Clear();
                CurrentRows.AddRange(rows);
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => CurrentRows.Count > 0;
        }
    }
}
