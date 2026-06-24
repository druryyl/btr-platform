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
    public class ItemEntityAnalyticsProducerTest
    {
        [Fact]
        public void Produce_MapsPortfolioItemsToL0Rows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioItem()));

            repository.EntityType.Should().Be(EntityTypeCode.Item);
            repository.Rows.Should().NotBeEmpty();
            repository.Rows.Should().Contain(r =>
                r.EntityId == "B001" && r.EntityCode == "BRG1" && r.KpiId == "IN-KPI-001" && r.NumericValue == 250_000m);
            repository.Rows.Should().Contain(r =>
                r.KpiId == EntityAnalyticsMetaKpiIds.QtyOnHand && r.NumericValue == 120m);
        }

        [Fact]
        public void Produce_WritesL1MonthlyRowsForTrendEligibleKpisOnly()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            var eligible = CreatePortfolioItem();
            eligible.IsTrendEligible = true;
            var dormant = CreatePortfolioItem();
            dormant.BrgId = "B002";
            dormant.BrgCode = "BRG2";
            dormant.IsTrendEligible = false;

            producer.Produce(CreateContext(generatedAt, eligible, dormant));

            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Should().OnlyContain(r => r.EntityId == "B001");
        }

        [Fact]
        public void Produce_WritesL3AttentionLifecycleRows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioItem()));

            repository.AttentionRows.Should().ContainSingle();
            repository.AttentionRows[0].SignalCode.Should().Be(
                DashboardInventoryRiskAggregator.SignalDeadStock);
            repository.AttentionRows[0].EntityId.Should().Be("B001");
        }

        [Fact]
        public void Produce_WritesL4RelationshipRows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioItem()));

            repository.RelationshipRows.Should().NotBeEmpty();
            repository.RelationshipRows.Should().Contain(r =>
                r.RelationshipCode == ItemRelationshipCatalog.TopCustomersByOmzet);
            repository.RelationshipRows.Should().Contain(r =>
                r.RelationshipCode == ItemRelationshipCatalog.PrimarySupplier);
        }

        [Fact]
        public void ItemDefaultPack_RegistersCatalogKpiIds()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Item,
                DisplayName = "Item",
                KpiPackId = ItemEntityAnalyticsRegistrar.KpiPackId
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new ItemEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            registry.GetPackKpiIds(ItemEntityAnalyticsRegistrar.KpiPackId)
                .Should().Contain(new[] { "IN-KPI-001", "IN-KPI-020", "IN-KPI-021" });

            registry.TryGetMetadata("IN-KPI-001", out var inventoryValue).Should().BeTrue();
            inventoryValue.TrendEligible.Should().BeTrue();
            inventoryValue.RankEligible.Should().BeTrue();
        }

        private static ItemEntityAnalyticsProducer CreateProducer(RecordingRepository repository)
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
                    Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions())),
                new EntityAttentionEngine(repository),
                new EntityRelationshipEngine(repository, relationships, entityTypes),
                new EntityRadarEngine(repository, registry, entityTypes),
                attentionSignals);
        }

        private static EntityAnalyticsProduceContext CreateContext(
            DateTime generatedAt,
            params DashboardItemPortfolioRow[] portfolio)
        {
            var item = portfolio[0];
            return new EntityAnalyticsProduceContext
            {
                RefreshLogId = "refresh-1",
                GeneratedAt = generatedAt,
                BusinessDate = generatedAt.Date,
                DomainInput = new ItemEntityAnalyticsProduceInput
                {
                    Portfolio = portfolio,
                    RiskAggregate = new DashboardInventoryRiskAggregateResult
                    {
                        AttentionList = new List<DashboardInventoryRiskAttentionRow>
                        {
                            new DashboardInventoryRiskAttentionRow
                            {
                                BrgId = item.BrgId,
                                BrgCode = item.BrgCode,
                                BrgName = item.BrgName,
                                SignalKey = DashboardInventoryRiskAggregator.SignalDeadStock,
                                SignalLabel = "Dead Stock",
                                InventoryValue = item.InventoryValue,
                                Qty = (int)item.Qty,
                                DaysSinceLastFaktur = item.DaysSinceLastFaktur
                            }
                        }
                    },
                    RelationshipAggregate = new DashboardItemRelationshipAggregateResult
                    {
                        ByBrgId = new Dictionary<string, DashboardItemRelationshipItemRollup>
                        {
                            [item.BrgId] = new DashboardItemRelationshipItemRollup
                            {
                                BrgId = item.BrgId,
                                BrgCode = item.BrgCode,
                                TopCustomers = new List<DashboardItemRelationshipCustomerRow>
                                {
                                    new DashboardItemRelationshipCustomerRow
                                    {
                                        Rank = 1,
                                        CustomerCode = "C001",
                                        CustomerName = "Customer 1",
                                        MetricValue = 1000m
                                    }
                                },
                                TopSalesmen = new List<DashboardItemRelationshipSalesmanRow>
                                {
                                    new DashboardItemRelationshipSalesmanRow
                                    {
                                        Rank = 1,
                                        SalesPersonId = "SP1",
                                        SalesPersonCode = "SP01",
                                        SalesPersonName = "Rep 1",
                                        MetricValue = 1000m
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static DashboardItemPortfolioRow CreatePortfolioItem()
        {
            return new DashboardItemPortfolioRow
            {
                BrgId = "B001",
                BrgCode = "BRG1",
                BrgName = "Item One",
                CategoryName = "Category A",
                SupplierName = "Supplier A",
                SupplierId = "S001",
                SupplierCode = "SUPA",
                Qty = 120,
                InventoryValue = 250_000m,
                MovementClass = DashboardInventoryRiskAggregator.SignalDeadStock,
                DaysSinceLastFaktur = 200,
                DaysOfSupply = 45m,
                RecommendedPurchaseQty = 10m,
                DistinctCustomerCount = 3,
                IsTrendEligible = true,
                IsActive = false
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
