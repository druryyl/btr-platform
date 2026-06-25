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
    public class SupplierReplayBackfillReconciliationTest
    {
        private static readonly YearMonthPeriod ReplayPeriod = new YearMonthPeriod(2024, 3);

        [Fact]
        public void ReplayProduce_MonthlyKpisMatchPu01PrincipalExposureRow()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);

            var portfolioSupplier = new DashboardPurchasingManagementPortfolioRow
            {
                SupplierId = "S100",
                SupplierCode = "SUP100",
                SupplierName = "Replay Principal",
                MtdPurchaseAmount = 1_250_000m,
                MtdInvoiceCount = 3,
                PostedPercent = 66.6667m,
                IsActiveMtd = true
            };

            producer.Produce(CreateReplayContext(generatedAt, portfolioSupplier));

            repository.ReplaceCurrentMetricsCalled.Should().BeFalse();
            repository.MonthlyRows.Should().OnlyContain(r =>
                r.PeriodYear == ReplayPeriod.Year && r.PeriodMonth == ReplayPeriod.Month && r.IsClosed);

            repository.MonthlyRows.Single(r => r.KpiId == "PU-KPI-001").NumericValue
                .Should().Be(portfolioSupplier.MtdPurchaseAmount);
            repository.MonthlyRows.Single(r => r.KpiId == "PU-KPI-002").NumericValue
                .Should().Be(portfolioSupplier.MtdInvoiceCount);
            repository.MonthlyRows.Single(r => r.KpiId == "PU-KPI-003").NumericValue
                .Should().Be(portfolioSupplier.PostedPercent);
        }

        [Fact]
        public void ReplayProduce_RankingsMatchManualSortForPurchaseAmount()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);

            var suppliers = new List<DashboardPurchasingManagementPortfolioRow>
            {
                new DashboardPurchasingManagementPortfolioRow
                {
                    SupplierId = "S001",
                    SupplierCode = "SUPA",
                    MtdPurchaseAmount = 100m,
                    MtdInvoiceCount = 1,
                    PostedPercent = 50m,
                    IsActiveMtd = true
                },
                new DashboardPurchasingManagementPortfolioRow
                {
                    SupplierId = "S002",
                    SupplierCode = "SUPB",
                    MtdPurchaseAmount = 300m,
                    MtdInvoiceCount = 4,
                    PostedPercent = 90m,
                    IsActiveMtd = true
                },
                new DashboardPurchasingManagementPortfolioRow
                {
                    SupplierId = "S003",
                    SupplierCode = "SUPC",
                    MtdPurchaseAmount = 300m,
                    MtdInvoiceCount = 2,
                    PostedPercent = 80m,
                    IsActiveMtd = true
                }
            };

            producer.Produce(CreateReplayContext(generatedAt, suppliers));

            var purchaseRanks = repository.RankingRows
                .Where(r => r.RankMetricKpiId == "PU-KPI-001")
                .ToDictionary(r => r.EntityId, r => r.RankPosition);

            purchaseRanks["S002"].Should().Be(1);
            purchaseRanks["S003"].Should().Be(1);
            purchaseRanks["S001"].Should().Be(3);
        }

        [Fact]
        public void ReplayProduce_IdempotentReplaceProducesIdenticalMonthlyRows()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);
            var context = CreateReplayContext(generatedAt, new DashboardPurchasingManagementPortfolioRow
            {
                SupplierId = "S100",
                SupplierCode = "SUP100",
                MtdPurchaseAmount = 1_250_000m,
                MtdInvoiceCount = 3,
                PostedPercent = 66.6667m,
                IsActiveMtd = true
            });

            producer.Produce(context);
            producer.Produce(context);

            repository.ReplaceMonthlyHistoryCallCount.Should().Be(2);
            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Single(r => r.KpiId == "PU-KPI-001").NumericValue.Should().Be(1_250_000m);
        }

        private static EntityAnalyticsProduceContext CreateReplayContext(
            DateTime generatedAt,
            DashboardPurchasingManagementPortfolioRow supplier)
        {
            return CreateReplayContext(generatedAt, new List<DashboardPurchasingManagementPortfolioRow> { supplier });
        }

        private static EntityAnalyticsProduceContext CreateReplayContext(
            DateTime generatedAt,
            IReadOnlyList<DashboardPurchasingManagementPortfolioRow> suppliers)
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
                        Portfolio = suppliers.ToList()
                    }
                },
                "refresh-replay",
                generatedAt);
        }

        private static SupplierEntityAnalyticsProducer CreateProducer(ReplayReconciliationRepository repository)
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
            new SupplierEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());

            var attentionSignals = new EntityAttentionSignalRegistry();
            SupplierAttentionSignalCatalog.Register(attentionSignals);

            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            SupplierRelationshipCatalog.Register(relationships);

            return new SupplierEntityAnalyticsProducer(
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
