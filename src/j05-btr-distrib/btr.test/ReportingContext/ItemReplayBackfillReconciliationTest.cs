using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.ReportingContext
{
    public class ItemReplayBackfillReconciliationTest
    {
        private static readonly YearMonthPeriod ReplayPeriod = new YearMonthPeriod(2024, 3);

        [Fact]
        public void ReplayProduce_MonthlyKpisMatchPortfolioInventoryMetrics()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);

            var portfolioItem = new DashboardItemPortfolioRow
            {
                BrgId = "B100",
                BrgCode = "BRG100",
                BrgName = "Replay Item",
                InventoryValue = 175_000m,
                DaysOfSupply = 32.5m,
                RecommendedPurchaseQty = 15m,
                Qty = 85,
                IsTrendEligible = true,
                IsActive = true
            };

            producer.Produce(CreateReplayContext(generatedAt, portfolioItem));

            repository.ReplaceCurrentMetricsCalled.Should().BeFalse();
            repository.MonthlyRows.Should().OnlyContain(r =>
                r.PeriodYear == ReplayPeriod.Year && r.PeriodMonth == ReplayPeriod.Month && r.IsClosed);

            repository.MonthlyRows.Single(r => r.KpiId == "IN-KPI-001").NumericValue
                .Should().Be(portfolioItem.InventoryValue);
            repository.MonthlyRows.Single(r => r.KpiId == "IN-KPI-020").NumericValue
                .Should().Be(portfolioItem.DaysOfSupply);
            repository.MonthlyRows.Single(r => r.KpiId == "IN-KPI-021").NumericValue
                .Should().Be(portfolioItem.RecommendedPurchaseQty);
        }

        [Fact]
        public void ReplayProduce_SkipsL1ForDormantSku()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);

            var active = new DashboardItemPortfolioRow
            {
                BrgId = "B001",
                BrgCode = "BRGA",
                InventoryValue = 100m,
                DaysOfSupply = 10m,
                RecommendedPurchaseQty = 5m,
                IsTrendEligible = true,
                IsActive = true
            };
            var dormant = new DashboardItemPortfolioRow
            {
                BrgId = "B002",
                BrgCode = "BRGB",
                InventoryValue = 50m,
                IsTrendEligible = false,
                IsActive = false
            };

            producer.Produce(CreateReplayContext(generatedAt, new List<DashboardItemPortfolioRow> { active, dormant }));

            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Should().OnlyContain(r => r.EntityId == "B001");
        }

        [Fact]
        public void ReplayProduce_RankingsMatchManualSortForDaysOfSupply()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);

            var items = new List<DashboardItemPortfolioRow>
            {
                new DashboardItemPortfolioRow
                {
                    BrgId = "B001",
                    BrgCode = "BRGA",
                    InventoryValue = 100m,
                    DaysOfSupply = 10m,
                    RecommendedPurchaseQty = 1m,
                    IsTrendEligible = true,
                    IsActive = true
                },
                new DashboardItemPortfolioRow
                {
                    BrgId = "B002",
                    BrgCode = "BRGB",
                    InventoryValue = 300m,
                    DaysOfSupply = 20m,
                    RecommendedPurchaseQty = 2m,
                    IsTrendEligible = true,
                    IsActive = true
                },
                new DashboardItemPortfolioRow
                {
                    BrgId = "B003",
                    BrgCode = "BRGC",
                    InventoryValue = 300m,
                    DaysOfSupply = 30m,
                    RecommendedPurchaseQty = 3m,
                    IsTrendEligible = true,
                    IsActive = true
                }
            };

            producer.Produce(CreateReplayContext(generatedAt, items));

            var inventoryRanks = repository.RankingRows
                .Where(r => r.RankMetricKpiId == "IN-KPI-020")
                .ToDictionary(r => r.EntityId, r => r.RankPosition);

            inventoryRanks["B003"].Should().Be(1);
            inventoryRanks["B002"].Should().Be(2);
            inventoryRanks["B001"].Should().Be(3);
        }

        [Fact]
        public void ReplayProduce_IdempotentReplaceProducesIdenticalMonthlyRows()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);
            var context = CreateReplayContext(generatedAt, new DashboardItemPortfolioRow
            {
                BrgId = "B100",
                BrgCode = "BRG100",
                InventoryValue = 175_000m,
                DaysOfSupply = 32.5m,
                RecommendedPurchaseQty = 15m,
                IsTrendEligible = true,
                IsActive = true
            });

            producer.Produce(context);
            producer.Produce(context);

            repository.ReplaceMonthlyHistoryCallCount.Should().Be(2);
            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Single(r => r.KpiId == "IN-KPI-001").NumericValue.Should().Be(175_000m);
        }

        private static EntityAnalyticsProduceContext CreateReplayContext(
            DateTime generatedAt,
            DashboardItemPortfolioRow item)
        {
            return CreateReplayContext(generatedAt, new List<DashboardItemPortfolioRow> { item });
        }

        private static EntityAnalyticsProduceContext CreateReplayContext(
            DateTime generatedAt,
            IReadOnlyList<DashboardItemPortfolioRow> items)
        {
            var replay = EntityAnalyticsReplayContextFactory.Create(
                ReplayPeriod,
                EntityTypeCode.Item,
                "job-replay",
                new EntityAnalyticsBackfillRequest());

            return EntityAnalyticsReplayContextFactory.CreateProduceContext(
                replay,
                new ItemEntityAnalyticsProduceInput
                {
                    Portfolio = items.ToList()
                },
                "refresh-replay",
                generatedAt);
        }

        private static ItemEntityAnalyticsProducer CreateProducer(ReplayReconciliationRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Item,
                DisplayName = "Item",
                KpiPackId = ItemEntityAnalyticsRegistrar.KpiPackId,
                PeerGroupRuleId = PeerGroupResolver.ItemCategory
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            new ItemEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());

            var attentionSignals = new EntityAttentionSignalRegistry();
            ItemAttentionSignalCatalog.Register(attentionSignals);

            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            ItemRelationshipCatalog.Register(relationships);

            return new ItemEntityAnalyticsProducer(
                repository,
                registry,
                new NoOpMonthCloseService(),
                new EntityRankingEngine(
                    repository,
                    registry,
                    entityTypes,
                    Options.Create(new EntityAnalyticsOptions { HistoryRetentionMonths = 36 })),
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

        private sealed class ReplayReconciliationRepository : EntityAnalyticsRepositoryStubBase
        {
            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
            {
                return CurrentRows.Where(r => r.EntityType == entityType && r.EntityId == entityId).ToList();
            }

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override void ReplaceCurrentMetrics(
                string entityType,
                IEnumerable<EntityAnalyticsCurrentRow> rows,
                string refreshLogId)
            {
                ReplaceCurrentMetricsCalled = true;
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => CurrentRows.Count > 0;
        }
    }
}
