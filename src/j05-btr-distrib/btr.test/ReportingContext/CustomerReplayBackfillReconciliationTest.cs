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
    public class CustomerReplayBackfillReconciliationTest
    {
        private static readonly YearMonthPeriod ReplayPeriod = new YearMonthPeriod(2024, 3);

        [Fact]
        public void ReplayProduce_MonthlyKpisMatchPortfolioOmzetAndBalance()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);

            var portfolioCustomer = new DashboardCustomerPortfolioCustomerRow
            {
                CustomerCode = "C100",
                CustomerName = "Replay Customer",
                MtdOmzet = 2500000m,
                OpenBalance = 750000m,
                OverdueBalance = 125000m,
                IsActiveMtd = true
            };

            producer.Produce(CreateReplayContext(generatedAt, portfolioCustomer));

            repository.ReplaceCurrentMetricsCalled.Should().BeFalse();
            repository.MonthlyRows.Should().OnlyContain(r =>
                r.PeriodYear == ReplayPeriod.Year && r.PeriodMonth == ReplayPeriod.Month && r.IsClosed);

            repository.MonthlyRows.Single(r => r.KpiId == "CU-KPI-009").NumericValue
                .Should().Be(portfolioCustomer.MtdOmzet);
            repository.MonthlyRows.Single(r => r.KpiId == "CU-KPI-010").NumericValue
                .Should().Be(portfolioCustomer.OpenBalance);
            repository.MonthlyRows.Single(r => r.KpiId == "FI-KPI-013").NumericValue
                .Should().Be(portfolioCustomer.OverdueBalance);
        }

        [Fact]
        public void ReplayProduce_RankingsMatchManualSortForOmzetAndBalance()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);

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

            producer.Produce(CreateReplayContext(generatedAt, customers));

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

        [Fact]
        public void ReplayProduce_IdempotentReplaceProducesIdenticalMonthlyRows()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);
            var context = CreateReplayContext(generatedAt, new DashboardCustomerPortfolioCustomerRow
            {
                CustomerCode = "C100",
                CustomerName = "Replay Customer",
                MtdOmzet = 2500000m,
                OpenBalance = 750000m,
                OverdueBalance = 125000m,
                IsActiveMtd = true
            });

            producer.Produce(context);
            producer.Produce(context);

            repository.ReplaceMonthlyHistoryCallCount.Should().Be(2);
            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Single(r => r.KpiId == "CU-KPI-009").NumericValue.Should().Be(2500000m);
        }

        private static EntityAnalyticsProduceContext CreateReplayContext(
            DateTime generatedAt,
            DashboardCustomerPortfolioCustomerRow customer)
        {
            return CreateReplayContext(generatedAt, new List<DashboardCustomerPortfolioCustomerRow> { customer });
        }

        private static EntityAnalyticsProduceContext CreateReplayContext(
            DateTime generatedAt,
            IReadOnlyList<DashboardCustomerPortfolioCustomerRow> customers)
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
                        Customers = customers.ToList()
                    }
                },
                "refresh-replay",
                generatedAt);
        }

        private static CustomerEntityAnalyticsProducer CreateProducer(ReplayReconciliationRepository repository)
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
            new CustomerEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());

            var attentionSignals = new EntityAttentionSignalRegistry();
            CustomerAttentionSignalCatalog.Register(attentionSignals);

            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            CustomerRelationshipCatalog.Register(relationships);

            return new CustomerEntityAnalyticsProducer(
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
