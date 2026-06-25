using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
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
    public class EntityAnalyticsReplayProducerTest
    {
        private static readonly YearMonthPeriod ReplayPeriod = new YearMonthPeriod(2024, 3);

        [Fact]
        public void Produce_WithReplay_SkipsL0()
        {
            var repository = new ReplayTrackingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 10, 0, 0);

            producer.Produce(CreateReplayContext(generatedAt, CreatePortfolioCustomer()));

            repository.ReplaceCurrentMetricsCalled.Should().BeFalse();
            repository.CurrentRows.Should().BeEmpty();
            repository.Rows.Should().BeEmpty();
        }

        [Fact]
        public void Produce_WithReplay_UsesReplaceMonthlyHistoryForPeriod()
        {
            var repository = new ReplayTrackingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 10, 0, 0);

            producer.Produce(CreateReplayContext(generatedAt, CreatePortfolioCustomer()));

            repository.ReplaceMonthlyHistoryCallCount.Should().Be(1);
            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Should().OnlyContain(r =>
                r.PeriodYear == ReplayPeriod.Year
                && r.PeriodMonth == ReplayPeriod.Month
                && r.IsClosed);
            repository.SaveMonthlyHistoryInvoked.Should().BeFalse();
        }

        [Fact]
        public void Produce_WithReplay_IdempotentReplace()
        {
            var repository = new ReplayTrackingRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 10, 0, 0);
            var context = CreateReplayContext(generatedAt, CreatePortfolioCustomer());

            producer.Produce(context);
            producer.Produce(context);

            repository.ReplaceMonthlyHistoryCallCount.Should().Be(2);
            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Single(r => r.KpiId == "CU-KPI-009").NumericValue.Should().Be(1500000m);
        }

        [Fact]
        public void Produce_WithReplay_WritesL2WhenMonthClosed()
        {
            var repository = new ReplayTrackingRepository();
            SeedActiveCurrent(repository, "C001");
            repository.CloseMonth(EntityTypeCode.Customer, ReplayPeriod.Year, ReplayPeriod.Month, "seed");

            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 10, 0, 0);

            producer.Produce(CreateReplayContext(generatedAt, CreatePortfolioCustomer()));

            repository.ReplaceRankingCallCount.Should().Be(1);
            repository.RankingRows.Should().NotBeEmpty();
            repository.RankingRows.Should().OnlyContain(r =>
                r.PeriodYear == ReplayPeriod.Year && r.PeriodMonth == ReplayPeriod.Month);
        }

        [Fact]
        public void MonthCloseService_WithReplay_IsNoOp()
        {
            var repository = new MonthCloseTrackingRepository();
            var service = new EntityAnalyticsMonthCloseService(
                repository,
                Options.Create(new EntityAnalyticsOptions { HistoryRetentionMonths = 36 }));

            service.EnsurePriorMonthClosed(EntityTypeCode.Customer, new EntityAnalyticsProduceContext
            {
                BusinessDate = ReplayPeriod.PeriodEnd,
                GeneratedAt = ReplayPeriod.PeriodEnd,
                RefreshLogId = "refresh-replay",
                Replay = EntityAnalyticsReplayContextFactory.Create(
                    ReplayPeriod,
                    EntityTypeCode.Customer,
                    "job-1",
                    new EntityAnalyticsBackfillRequest())
            });

            repository.CloseMonthCalled.Should().BeFalse();
            repository.PurgeHistoryCalled.Should().BeFalse();
        }

        [Fact]
        public void MonthCloseService_WithoutReplay_ClosesPriorMonth()
        {
            var repository = new MonthCloseTrackingRepository();
            var service = new EntityAnalyticsMonthCloseService(
                repository,
                Options.Create(new EntityAnalyticsOptions { HistoryRetentionMonths = 36 }));

            service.EnsurePriorMonthClosed(EntityTypeCode.Customer, new EntityAnalyticsProduceContext
            {
                BusinessDate = new DateTime(2024, 3, 15),
                GeneratedAt = new DateTime(2024, 3, 15),
                RefreshLogId = "refresh-live"
            });

            repository.CloseMonthCalled.Should().BeTrue();
            repository.PurgeHistoryCalled.Should().BeTrue();
        }

        [Fact]
        public void SalesmanProduce_WithReplay_SkipsL0()
        {
            var repository = new ReplayTrackingRepository();
            var producer = CreateSalesmanProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 10, 0, 0);

            producer.Produce(CreateSalesmanReplayContext(generatedAt, CreatePortfolioRep()));

            repository.ReplaceCurrentMetricsCalled.Should().BeFalse();
        }

        [Fact]
        public void SalesmanProduce_WithReplay_UsesReplaceMonthlyHistoryForPeriod()
        {
            var repository = new ReplayTrackingRepository();
            var producer = CreateSalesmanProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 10, 0, 0);

            producer.Produce(CreateSalesmanReplayContext(generatedAt, CreatePortfolioRep()));

            repository.ReplaceMonthlyHistoryCallCount.Should().Be(1);
            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Should().OnlyContain(r =>
                r.PeriodYear == ReplayPeriod.Year
                && r.PeriodMonth == ReplayPeriod.Month
                && r.IsClosed);
        }

        [Fact]
        public void SalesmanProduce_WithReplay_SkipL1Persist_SkipsSecondL1Write()
        {
            var repository = new ReplayTrackingRepository();
            repository.MonthlyRows.Add(new EntityAnalyticsMonthlyRow
            {
                EntityType = EntityTypeCode.Salesman,
                EntityId = "SP001",
                EntityCode = "R001",
                PeriodYear = ReplayPeriod.Year,
                PeriodMonth = ReplayPeriod.Month,
                KpiId = "SF-KPI-008",
                NumericValue = 1500000m,
                IsClosed = true
            });

            var producer = CreateSalesmanProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 10, 0, 0);
            var replay = EntityAnalyticsReplayContextFactory.Create(
                ReplayPeriod,
                EntityTypeCode.Salesman,
                "job-replay",
                new EntityAnalyticsBackfillRequest());
            replay.SkipL1Persist = true;

            var context = EntityAnalyticsReplayContextFactory.CreateProduceContext(
                replay,
                new SalesmanEntityAnalyticsProduceInput
                {
                    SalesmanAggregate = new DashboardSalesmanAggregateResult
                    {
                        Portfolio = new List<DashboardSalesmanPortfolioRow>()
                    }
                },
                "refresh-replay",
                generatedAt);

            producer.Produce(context);

            repository.ReplaceMonthlyHistoryCallCount.Should().Be(0);
            repository.RankingRows.Should().NotBeEmpty();
        }

        [Fact]
        public void SupplierProduce_WithReplay_SkipsL0()
        {
            var repository = new ReplayTrackingRepository();
            var producer = CreateSupplierProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 10, 0, 0);

            producer.Produce(CreateSupplierReplayContext(generatedAt, CreatePortfolioSupplier()));

            repository.ReplaceCurrentMetricsCalled.Should().BeFalse();
        }

        [Fact]
        public void SupplierProduce_WithReplay_UsesReplaceMonthlyHistoryForPeriod()
        {
            var repository = new ReplayTrackingRepository();
            var producer = CreateSupplierProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 10, 0, 0);

            producer.Produce(CreateSupplierReplayContext(generatedAt, CreatePortfolioSupplier()));

            repository.ReplaceMonthlyHistoryCallCount.Should().Be(1);
            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Should().OnlyContain(r =>
                r.PeriodYear == ReplayPeriod.Year
                && r.PeriodMonth == ReplayPeriod.Month
                && r.IsClosed);
        }

        [Fact]
        public void SupplierProduce_WithReplay_IdempotentReplace()
        {
            var repository = new ReplayTrackingRepository();
            var producer = CreateSupplierProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 10, 0, 0);
            var context = CreateSupplierReplayContext(generatedAt, CreatePortfolioSupplier());

            producer.Produce(context);
            producer.Produce(context);

            repository.ReplaceMonthlyHistoryCallCount.Should().Be(2);
            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Single(r => r.KpiId == "PU-KPI-001").NumericValue.Should().Be(1_500_000m);
        }

        private static EntityAnalyticsProduceContext CreateSupplierReplayContext(
            DateTime generatedAt,
            DashboardPurchasingManagementPortfolioRow supplier)
        {
            var replay = EntityAnalyticsReplayContextFactory.Create(
                ReplayPeriod,
                EntityTypeCode.Supplier,
                "job-replay",
                new EntityAnalyticsBackfillRequest());

            return EntityAnalyticsReplayContextFactory.CreateProduceContext(
                replay,
                new SupplierEntityAnalyticsProduceInput
                {
                    ManagementAggregate = new DashboardPurchasingManagementAggregateResult
                    {
                        Portfolio = new List<DashboardPurchasingManagementPortfolioRow> { supplier }
                    }
                },
                "refresh-replay",
                generatedAt);
        }

        private static DashboardPurchasingManagementPortfolioRow CreatePortfolioSupplier()
        {
            return new DashboardPurchasingManagementPortfolioRow
            {
                SupplierId = "S001",
                SupplierCode = "SUPA",
                SupplierName = "Alpha Principal",
                MtdPurchaseAmount = 1_500_000m,
                MtdInvoiceCount = 4,
                PostedPercent = 75m,
                IsActiveMtd = true
            };
        }

        private static SupplierEntityAnalyticsProducer CreateSupplierProducer(ReplayTrackingRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Supplier,
                DisplayName = "Supplier",
                KpiPackId = SupplierEntityAnalyticsRegistrar.KpiPackId,
                PeerGroupRuleId = PeerGroupResolver.SupplierAllActive
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new SupplierEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            var rankingEngine = new EntityRankingEngine(
                repository,
                registry,
                entityTypes,
                Options.Create(new EntityAnalyticsOptions { HistoryRetentionMonths = 36 }));

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

        private static EntityAnalyticsProduceContext CreateSalesmanReplayContext(
            DateTime generatedAt,
            DashboardSalesmanPortfolioRow rep)
        {
            var replay = EntityAnalyticsReplayContextFactory.Create(
                ReplayPeriod,
                EntityTypeCode.Salesman,
                "job-replay",
                new EntityAnalyticsBackfillRequest());

            return EntityAnalyticsReplayContextFactory.CreateProduceContext(
                replay,
                new SalesmanEntityAnalyticsProduceInput
                {
                    SalesmanAggregate = new DashboardSalesmanAggregateResult
                    {
                        Portfolio = new List<DashboardSalesmanPortfolioRow> { rep }
                    }
                },
                "refresh-replay",
                generatedAt);
        }

        private static DashboardSalesmanPortfolioRow CreatePortfolioRep()
        {
            return new DashboardSalesmanPortfolioRow
            {
                SalesPersonId = "SP001",
                SalesPersonCode = "R001",
                SalesPersonName = "Alpha Rep",
                CompletedOmzet = 1500000m,
                AchievementPercent = 85m,
                OpenBalance = 500000m,
                IsActive = true
            };
        }

        private static SalesmanEntityAnalyticsProducer CreateSalesmanProducer(ReplayTrackingRepository repository)
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
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new SalesmanEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            var rankingEngine = new EntityRankingEngine(
                repository,
                registry,
                entityTypes,
                Options.Create(new EntityAnalyticsOptions { HistoryRetentionMonths = 36 }));

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

        private static void SeedActiveCurrent(ReplayTrackingRepository repository, string entityId)
        {
            repository.CurrentRows.Add(new EntityAnalyticsCurrentRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = entityId,
                EntityCode = entityId,
                KpiId = EntityAnalyticsMetaKpiIds.IsActive,
                NumericValue = 1m,
                GeneratedAt = DateTime.UtcNow
            });
        }

        private static EntityAnalyticsProduceContext CreateReplayContext(
            DateTime generatedAt,
            DashboardCustomerPortfolioCustomerRow customer)
        {
            var replay = EntityAnalyticsReplayContextFactory.Create(
                ReplayPeriod,
                EntityTypeCode.Customer,
                "job-replay",
                new EntityAnalyticsBackfillRequest());

            return EntityAnalyticsReplayContextFactory.CreateProduceContext(
                replay,
                new CustomerEntityAnalyticsProduceInput
                {
                    PortfolioAggregate = new DashboardCustomerPortfolioAggregateResult
                    {
                        Customers = new List<DashboardCustomerPortfolioCustomerRow> { customer }
                    }
                },
                "refresh-replay",
                generatedAt);
        }

        private static DashboardCustomerPortfolioCustomerRow CreatePortfolioCustomer()
        {
            return new DashboardCustomerPortfolioCustomerRow
            {
                CustomerCode = "C001",
                CustomerName = "Alpha Customer",
                MtdOmzet = 1500000m,
                OpenBalance = 500000m,
                OverdueBalance = 100000m,
                IsActiveMtd = true
            };
        }

        private static CustomerEntityAnalyticsProducer CreateProducer(ReplayTrackingRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = CustomerEntityAnalyticsRegistrar.KpiPackId,
                PeerGroupRuleId = PeerGroupResolver.CustomerWilayah
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            var dimensionLabels = new EntityAnalyticsDimensionLabelRegistry();
            new CustomerEntityAnalyticsRegistrar().Register(entityTypes, registry, dimensionLabels);

            var rankingEngine = new EntityRankingEngine(
                repository,
                registry,
                entityTypes,
                Options.Create(new EntityAnalyticsOptions { HistoryRetentionMonths = 36 }));

            var attentionSignals = new EntityAttentionSignalRegistry();
            CustomerAttentionSignalCatalog.Register(attentionSignals);
            var attentionEngine = new EntityAttentionEngine(repository);

            var entityTypesForRelationships = new EntityTypeRegistry();
            entityTypesForRelationships.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                RelationshipPackId = CustomerRelationshipCatalog.PackId
            });
            var relationships = new EntityRelationshipDefinitionRegistry(entityTypesForRelationships);
            CustomerRelationshipCatalog.Register(relationships);
            var relationshipEngine = new EntityRelationshipEngine(repository, relationships, entityTypesForRelationships);
            var radarEngine = new EntityRadarEngine(repository, registry, entityTypes);

            return new CustomerEntityAnalyticsProducer(
                repository,
                registry,
                new NoOpMonthCloseService(),
                rankingEngine,
                attentionEngine,
                relationshipEngine,
                radarEngine,
                attentionSignals);
        }

        private sealed class NoOpMonthCloseService : IEntityAnalyticsMonthCloseService
        {
            public void EnsurePriorMonthClosed(string entityType, EntityAnalyticsProduceContext context)
            {
            }
        }

        private sealed class ReplayTrackingRepository : EntityAnalyticsRepositoryStubBase
        {
            public List<EntityAnalyticsCurrentRow> Rows { get; } = new List<EntityAnalyticsCurrentRow>();
            public bool SaveMonthlyHistoryInvoked { get; private set; }

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
                Rows.Clear();
                Rows.AddRange(rows);
                CurrentRows.Clear();
                CurrentRows.AddRange(rows);
            }

            public override void SaveMonthlyHistory(
                string entityType,
                IEnumerable<EntityAnalyticsMonthlyRow> rows,
                string refreshLogId)
            {
                SaveMonthlyHistoryInvoked = true;
                base.SaveMonthlyHistory(entityType, rows, refreshLogId);
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => CurrentRows.Count > 0;
        }

        private sealed class MonthCloseTrackingRepository : EntityAnalyticsRepositoryStubBase
        {
            public bool CloseMonthCalled { get; private set; }
            public bool PurgeHistoryCalled { get; private set; }

            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
                => Array.Empty<EntityAnalyticsCurrentRow>();

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override void ReplaceCurrentMetrics(
                string entityType,
                IEnumerable<EntityAnalyticsCurrentRow> rows,
                string refreshLogId)
            {
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => false;

            public override void CloseMonth(string entityType, int periodYear, int periodMonth, string refreshLogId)
            {
                CloseMonthCalled = true;
                base.CloseMonth(entityType, periodYear, periodMonth, refreshLogId);
            }

            public override void PurgeHistoryOlderThan(string entityType, int retentionMonths)
            {
                PurgeHistoryCalled = true;
            }
        }
    }
}
