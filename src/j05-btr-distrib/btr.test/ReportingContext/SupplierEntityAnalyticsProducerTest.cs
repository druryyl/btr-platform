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
    public class SupplierEntityAnalyticsProducerTest
    {
        [Fact]
        public void Produce_MapsPortfolioSuppliersToL0Rows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioSupplier()));

            repository.EntityType.Should().Be(EntityTypeCode.Supplier);
            repository.Rows.Should().NotBeEmpty();

            var kpiRows = repository.Rows
                .Where(r => !EntityAnalyticsMetaKpiIds.IsMetaOrDimension(r.KpiId))
                .ToList();

            kpiRows.Should().Contain(r => r.KpiId == "PU-KPI-001" && r.NumericValue == 1_500_000m);
            kpiRows.Should().Contain(r => r.KpiId == "PU-KPI-002" && r.NumericValue == 4m);
            kpiRows.Should().Contain(r => r.KpiId == "PU-KPI-003" && r.NumericValue == 75m);
            repository.Rows.Should().Contain(r =>
                r.EntityId == "S001" && r.EntityCode == "SUPA");
        }

        [Fact]
        public void Produce_EmitsUniqueEntityKpiPairs()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioSupplier()));

            var duplicates = repository.Rows
                .GroupBy(r => new { r.EntityId, r.KpiId })
                .Where(g => g.Count() > 1)
                .Select(g => $"{g.Key.EntityId}/{g.Key.KpiId}")
                .ToList();

            duplicates.Should().BeEmpty("each EntityId+KpiId pair must appear exactly once in L0 output");

            repository.Rows.Should().ContainSingle(r =>
                r.EntityId == "S001"
                && r.KpiId == EntityAnalyticsMetaKpiIds.InventoryValue
                && r.NumericValue == 500_000m);
        }

        [Fact]
        public void Produce_WritesL1MonthlyRowsForTrendEligibleKpis()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioSupplier()));

            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Should().Contain(r =>
                r.KpiId == "PU-KPI-001" && r.NumericValue == 1_500_000m);
        }

        [Fact]
        public void Produce_WritesL3AttentionLifecycleRows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioSupplier()));

            repository.AttentionRows.Should().ContainSingle();
            repository.AttentionRows[0].SignalCode.Should().Be(
                DashboardPurchasingManagementAggregator.SignalQualifiedBacklog);
            repository.AttentionRows[0].EntityId.Should().Be("S001");
        }

        [Fact]
        public void Produce_WritesL4RelationshipRows()
        {
            var repository = new RecordingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioSupplier()));

            repository.RelationshipRows.Should().NotBeEmpty();
            repository.RelationshipRows.Should().Contain(r =>
                r.RelationshipCode == SupplierRelationshipCatalog.TopCustomersByOmzet);
            repository.RelationshipRows.Should().Contain(r =>
                r.RelationshipCode == SupplierRelationshipCatalog.TopProductsByOmzet);
        }

        [Fact]
        public void SupplierDefaultPack_RegistersCatalogKpiIds()
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Supplier",
                KpiPackId = SupplierEntityAnalyticsRegistrar.KpiPackId
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new SupplierEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            registry.GetPackKpiIds(SupplierEntityAnalyticsRegistrar.KpiPackId)
                .Should().Contain(new[] { "PU-KPI-001", "PU-KPI-002", "PU-KPI-003" });

            registry.TryGetMetadata("PU-KPI-001", out var purchase).Should().BeTrue();
            purchase.TrendEligible.Should().BeTrue();
            purchase.RankEligible.Should().BeTrue();
        }

        private static SupplierEntityAnalyticsProducer CreateProducer(RecordingRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Supplier",
                KpiPackId = SupplierEntityAnalyticsRegistrar.KpiPackId,
                RelationshipPackId = SupplierRelationshipCatalog.PackId,
                PeerGroupRuleId = PeerGroupResolver.SupplierAllActive
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new SupplierEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            var rankingEngine = new EntityRankingEngine(
                repository,
                registry,
                entityTypes,
                Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions()));

            var attentionSignals = new EntityAttentionSignalRegistry();
            SupplierAttentionSignalCatalog.Register(attentionSignals);
            var attentionEngine = new EntityAttentionEngine(repository);

            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            SupplierRelationshipCatalog.Register(relationships);
            var relationshipEngine = new EntityRelationshipEngine(repository, relationships, entityTypes);
            var radarEngine = new EntityRadarEngine(repository, registry, entityTypes);

            return new SupplierEntityAnalyticsProducer(
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
            DashboardPurchasingManagementPortfolioRow supplier)
        {
            return new EntityAnalyticsProduceContext
            {
                RefreshLogId = "refresh-1",
                GeneratedAt = generatedAt,
                BusinessDate = generatedAt.Date,
                DomainInput = new SupplierEntityAnalyticsProduceInput
                {
                    ManagementAggregate = new DashboardPurchasingManagementAggregateResult
                    {
                        AttentionList = new List<DashboardPurchasingManagementAttentionRow>
                        {
                            new DashboardPurchasingManagementAttentionRow
                            {
                                SupplierId = supplier.SupplierId,
                                SupplierCode = supplier.SupplierCode,
                                EntityType = DashboardPurchasingManagementAggregator.EntityTypePrincipal,
                                EntityName = supplier.PrincipalName,
                                SignalKey = DashboardPurchasingManagementAggregator.SignalQualifiedBacklog,
                                SignalLabel = "Qualified Backlog"
                            }
                        },
                        TopPrincipal = new List<DashboardPurchasingManagementTopPrincipalRow>
                        {
                            new DashboardPurchasingManagementTopPrincipalRow
                            {
                                Rank = 1,
                                SupplierId = supplier.SupplierId,
                                SupplierCode = supplier.SupplierCode,
                                PrincipalName = supplier.PrincipalName,
                                MtdPurchaseAmount = supplier.MtdPurchaseAmount,
                                InventoryValue = supplier.InventoryValue,
                                PercentOfPurchase = supplier.PercentOfPurchase
                            }
                        },
                        Portfolio = new List<DashboardPurchasingManagementPortfolioRow> { supplier }
                    },
                    RelationshipAggregate = new DashboardSupplierRelationshipAggregateResult
                    {
                        BySupplierId = new Dictionary<string, DashboardSupplierRelationshipSupplierRollup>
                        {
                            [supplier.SupplierId] = new DashboardSupplierRelationshipSupplierRollup
                            {
                                SupplierId = supplier.SupplierId,
                                SupplierCode = supplier.SupplierCode,
                                TopItems = new List<DashboardSupplierRelationshipItemRow>
                                {
                                    new DashboardSupplierRelationshipItemRow
                                    {
                                        Rank = 1,
                                        BrgId = "I1",
                                        BrgCode = "BRG1",
                                        BrgName = "Item 1",
                                        MetricValue = 1000m
                                    }
                                },
                                TopCustomers = new List<DashboardSupplierRelationshipCustomerRow>
                                {
                                    new DashboardSupplierRelationshipCustomerRow
                                    {
                                        Rank = 1,
                                        CustomerCode = "C001",
                                        CustomerName = "Customer 1",
                                        MetricValue = 1000m
                                    }
                                },
                                TopSalesmen = new List<DashboardSupplierRelationshipSalesmanRow>
                                {
                                    new DashboardSupplierRelationshipSalesmanRow
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

        private static DashboardPurchasingManagementPortfolioRow CreatePortfolioSupplier()
        {
            return new DashboardPurchasingManagementPortfolioRow
            {
                SupplierId = "S001",
                SupplierCode = "SUPA",
                SupplierName = "Principal A",
                PrincipalName = "Principal A",
                MtdPurchaseAmount = 1_500_000m,
                MtdInvoiceCount = 4,
                PostedPercent = 75m,
                PercentOfPurchase = 30m,
                InventoryValue = 500_000m,
                PercentOfInventory = 12m,
                IsActiveMtd = true
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
