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
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
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

        [Fact]
        public void Produce_ExposesPacingAchievementAndYoyMtdGrowthToL0()
        {
            var repository = new RecordingRepository();
            var producer = CreateTimeAwareProducer(
                repository,
                salesOut: SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                target: Targets(2026, 6, TargetRow("S001", "Principal A", 800m)),
                periodYear: 2026,
                currentRangeAmount: 1_500_000m,
                priorRangeAmount: 1_000_000m);
            var generatedAt = new DateTime(2026, 6, 15, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioSupplier()));

            repository.Rows.Should().ContainSingle(r =>
                r.EntityId == "S001"
                && r.KpiId == PrincipalKpiCatalog.PacingAchievementPercentageId
                && r.NumericValue == 250m);
            repository.Rows.Should().ContainSingle(r =>
                r.EntityId == "S001"
                && r.KpiId == PrincipalKpiCatalog.YoyMtdGrowthId
                && r.NumericValue == 50m);
        }

        [Fact]
        public void Produce_SuppressesTimeAwareKpisWhenConfidenceGuardsBreached()
        {
            var repository = new RecordingRepository();
            var producer = CreateTimeAwareProducer(
                repository,
                salesOut: SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                target: Targets(2026, 6, TargetRow("S001", "Principal A", 800m)),
                periodYear: 2026,
                currentRangeAmount: 1200m,
                priorRangeAmount: 500_000m);
            var generatedAt = new DateTime(2026, 6, 3, 10, 0, 0);
            producer.Produce(CreateContext(generatedAt, CreatePortfolioSupplier()));

            repository.Rows.Should().ContainSingle(r =>
                r.EntityId == "S001"
                && r.KpiId == PrincipalKpiCatalog.PacingAchievementPercentageId
                && r.NumericValue == null);
            repository.Rows.Should().ContainSingle(r =>
                r.EntityId == "S001"
                && r.KpiId == PrincipalKpiCatalog.YoyMtdGrowthId
                && r.NumericValue == null);
        }

        [Fact]
        public void Produce_TimeAwareKpisDoNotAlterExistingPrincipalKpis()
        {
            var repository = new RecordingRepository();
            var producer = CreateTimeAwareProducer(
                repository,
                salesOut: SalesOut(2026, 6, OwnedSalesOut("S001", "Principal A", 1000m)),
                target: Targets(2026, 6, TargetRow("S001", "Principal A", 800m)),
                periodYear: 2026,
                currentRangeAmount: 1_500_000m,
                priorRangeAmount: 1_000_000m);
            var generatedAt = new DateTime(2026, 6, 15, 10, 0, 0);

            producer.Produce(CreateContext(generatedAt, CreatePortfolioSupplier()));

            repository.Rows.Should().ContainSingle(r =>
                r.EntityId == "S001" && r.KpiId == PrincipalKpiCatalog.SalesOutId && r.NumericValue == 1000m);
            repository.Rows.Should().ContainSingle(r =>
                r.EntityId == "S001" && r.KpiId == PrincipalKpiCatalog.TargetId && r.NumericValue == 800m);

            var duplicates = repository.Rows
                .GroupBy(r => new { r.EntityId, r.KpiId })
                .Where(g => g.Count() > 1)
                .Select(g => $"{g.Key.EntityId}/{g.Key.KpiId}")
                .ToList();
            duplicates.Should().BeEmpty();
        }

        private static SupplierEntityAnalyticsProducer CreateTimeAwareProducer(
            RecordingRepository repository,
            PrincipalSalesOutAggregateResult salesOut,
            PrincipalTargetAggregateResult target,
            int periodYear,
            decimal currentRangeAmount,
            decimal priorRangeAmount)
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
            new SupplierEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());

            var rankingEngine = new EntityRankingEngine(
                repository,
                registry,
                entityTypes,
                Microsoft.Extensions.Options.Options.Create(new EntityAnalyticsOptions()));

            var attentionSignals = new EntityAttentionSignalRegistry();
            SupplierAttentionSignalCatalog.Register(attentionSignals);

            return new SupplierEntityAnalyticsProducer(
                repository,
                registry,
                new NoOpMonthCloseService(),
                rankingEngine,
                new EntityAttentionEngine(repository),
                new EntityRelationshipEngine(
                    repository,
                    new EntityRelationshipDefinitionRegistry(entityTypes),
                    entityTypes),
                new EntityRadarEngine(repository, registry, entityTypes),
                attentionSignals,
                new FakeSalesOutSnapshotDal(salesOut),
                null,
                null,
                new FakeTargetSnapshotDal(target),
                null,
                null,
                null,
                null,
                null,
                new PrincipalPacingAchievementComposer(),
                new PrincipalYoyMtdGrowthComposer(
                    new FakeRangeEvidenceDal(periodYear, currentRangeAmount, priorRangeAmount)),
                new PrincipalConfidenceGuard(registry));
        }

        private static PrincipalSalesOutAggregateResult SalesOut(
            int year,
            int month,
            params PrincipalSalesOutRow[] rows)
        {
            return new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = year,
                PeriodMonth = month,
                Principals = rows.ToList()
            };
        }

        private static PrincipalSalesOutRow OwnedSalesOut(string supplierId, string name, decimal amount)
        {
            return new PrincipalSalesOutRow
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                SupplierId = supplierId,
                SupplierName = name,
                SalesOutAmount = amount
            };
        }

        private static PrincipalTargetAggregateResult Targets(
            int year,
            int month,
            params PrincipalTargetRow[] rows)
        {
            return new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = year,
                PeriodMonth = month,
                Principals = rows.ToList()
            };
        }

        private static PrincipalTargetRow TargetRow(string supplierId, string name, decimal targetAmount)
        {
            return new PrincipalTargetRow
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                SupplierId = supplierId,
                SupplierName = name,
                TargetAmount = targetAmount
            };
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

        private sealed class FakeSalesOutSnapshotDal : IPrincipalSalesOutSnapshotDal
        {
            public FakeSalesOutSnapshotDal(PrincipalSalesOutAggregateResult current)
            {
                Current = current;
            }

            public PrincipalSalesOutAggregateResult Current { get; set; }

            public PrincipalSalesOutAggregateResult GetCurrent() => Current;

            public void ReplaceCurrent(PrincipalSalesOutAggregateResult result, string refreshLogId)
            {
                Current = result;
            }
        }

        private sealed class FakeTargetSnapshotDal : IPrincipalTargetSnapshotDal
        {
            public FakeTargetSnapshotDal(PrincipalTargetAggregateResult current)
            {
                Current = current;
            }

            public PrincipalTargetAggregateResult Current { get; set; }

            public PrincipalTargetAggregateResult GetCurrent() => Current;

            public void ReplaceCurrent(PrincipalTargetAggregateResult result, string refreshLogId)
            {
                Current = result;
            }
        }

        private sealed class FakeRangeEvidenceDal : IPrincipalSalesOutRangeEvidenceDal
        {
            private readonly int _periodYear;
            private readonly decimal _currentAmount;
            private readonly decimal _priorAmount;

            public FakeRangeEvidenceDal(int periodYear, decimal currentAmount, decimal priorAmount)
            {
                _periodYear = periodYear;
                _currentAmount = currentAmount;
                _priorAmount = priorAmount;
            }

            public IReadOnlyList<PrincipalSalesOutRangeEvidenceRow> ListSalesOutByRange(
                DateTime startDate,
                DateTime endDate)
            {
                var amount = startDate.Year == _periodYear ? _currentAmount : _priorAmount;
                return new List<PrincipalSalesOutRangeEvidenceRow>
                {
                    new PrincipalSalesOutRangeEvidenceRow
                    {
                        SupplierId = "S001",
                        SupplierName = "Principal A",
                        SalesOutAmount = amount,
                        LineCount = 1
                    }
                };
            }
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
