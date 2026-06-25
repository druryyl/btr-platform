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
    public class SalesmanReplayBackfillReconciliationTest
    {
        private static readonly YearMonthPeriod ReplayPeriod = new YearMonthPeriod(2024, 3);

        [Fact]
        public void ReplayProduce_MonthlyKpisMatchPortfolioOmzetAndBalance()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);

            var portfolioRep = new DashboardSalesmanPortfolioRow
            {
                SalesPersonId = "SP100",
                SalesPersonCode = "R100",
                SalesPersonName = "Replay Rep",
                CompletedOmzet = 2500000m,
                AchievementPercent = 88m,
                OpenBalance = 750000m,
                IsActive = true
            };

            producer.Produce(CreateReplayContext(generatedAt, portfolioRep));

            repository.ReplaceCurrentMetricsCalled.Should().BeFalse();
            repository.MonthlyRows.Should().OnlyContain(r =>
                r.PeriodYear == ReplayPeriod.Year && r.PeriodMonth == ReplayPeriod.Month && r.IsClosed);

            repository.MonthlyRows.Single(r => r.KpiId == "SF-KPI-008").NumericValue
                .Should().Be(portfolioRep.CompletedOmzet);
            repository.MonthlyRows.Single(r => r.KpiId == "SF-KPI-009").NumericValue
                .Should().Be(portfolioRep.AchievementPercent);
            repository.MonthlyRows.Single(r => r.KpiId == "SF-KPI-010").NumericValue
                .Should().Be(portfolioRep.OpenBalance);
        }

        [Fact]
        public void ReplayProduce_RankingsMatchManualSortForOmzet()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);

            var reps = new List<DashboardSalesmanPortfolioRow>
            {
                new DashboardSalesmanPortfolioRow
                {
                    SalesPersonId = "SP001",
                    SalesPersonCode = "R001",
                    CompletedOmzet = 100m,
                    AchievementPercent = 50m,
                    OpenBalance = 500m,
                    IsActive = true
                },
                new DashboardSalesmanPortfolioRow
                {
                    SalesPersonId = "SP002",
                    SalesPersonCode = "R002",
                    CompletedOmzet = 300m,
                    AchievementPercent = 90m,
                    OpenBalance = 100m,
                    IsActive = true
                },
                new DashboardSalesmanPortfolioRow
                {
                    SalesPersonId = "SP003",
                    SalesPersonCode = "R003",
                    CompletedOmzet = 300m,
                    AchievementPercent = 80m,
                    OpenBalance = 200m,
                    IsActive = true
                }
            };

            producer.Produce(CreateReplayContext(generatedAt, reps));

            var omzetRanks = repository.RankingRows
                .Where(r => r.RankMetricKpiId == "SF-KPI-008")
                .ToDictionary(r => r.EntityId, r => r.RankPosition);

            omzetRanks["SP002"].Should().Be(1);
            omzetRanks["SP003"].Should().Be(1);
            omzetRanks["SP001"].Should().Be(3);
        }

        [Fact]
        public void ReplayProduce_IdempotentReplaceProducesIdenticalMonthlyRows()
        {
            var repository = new ReplayReconciliationRepository();
            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);
            var context = CreateReplayContext(generatedAt, new DashboardSalesmanPortfolioRow
            {
                SalesPersonId = "SP100",
                SalesPersonCode = "R100",
                CompletedOmzet = 2500000m,
                AchievementPercent = 88m,
                OpenBalance = 750000m,
                IsActive = true
            });

            producer.Produce(context);
            producer.Produce(context);

            repository.ReplaceMonthlyHistoryCallCount.Should().Be(2);
            repository.MonthlyRows.Should().HaveCount(3);
            repository.MonthlyRows.Single(r => r.KpiId == "SF-KPI-008").NumericValue.Should().Be(2500000m);
        }

        [Fact]
        public void ReplayProduce_FastPath_SkipL1Persist_WritesL2FromPersistedMonthly()
        {
            var repository = new ReplayReconciliationRepository();
            repository.MonthlyRows.AddRange(new[]
            {
                CreateMonthlyRow("SP001", "R001", "SF-KPI-008", 300m),
                CreateMonthlyRow("SP002", "R002", "SF-KPI-008", 100m),
                CreateMonthlyRow("SP003", "R003", "SF-KPI-008", 300m)
            });

            var producer = CreateProducer(repository);
            var generatedAt = new DateTime(2024, 3, 31, 12, 0, 0);
            var replay = EntityAnalyticsReplayContextFactory.Create(
                ReplayPeriod,
                EntityTypeCode.Salesman,
                "job-fast-path",
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
                "refresh-fast-path",
                generatedAt);

            producer.Produce(context);

            repository.ReplaceMonthlyHistoryCallCount.Should().Be(0);
            repository.RankingRows.Should().NotBeEmpty();

            var omzetRanks = repository.RankingRows
                .Where(r => r.RankMetricKpiId == "SF-KPI-008")
                .ToDictionary(r => r.EntityId, r => r.RankPosition);

            omzetRanks["SP001"].Should().Be(1);
            omzetRanks["SP003"].Should().Be(1);
            omzetRanks["SP002"].Should().Be(3);
        }

        private static EntityAnalyticsMonthlyRow CreateMonthlyRow(
            string entityId,
            string entityCode,
            string kpiId,
            decimal value)
        {
            return new EntityAnalyticsMonthlyRow
            {
                EntityType = EntityTypeCode.Salesman,
                EntityId = entityId,
                EntityCode = entityCode,
                PeriodYear = ReplayPeriod.Year,
                PeriodMonth = ReplayPeriod.Month,
                KpiId = kpiId,
                NumericValue = value,
                IsClosed = true,
                GeneratedAt = ReplayPeriod.PeriodEnd
            };
        }

        private static EntityAnalyticsProduceContext CreateReplayContext(
            DateTime generatedAt,
            DashboardSalesmanPortfolioRow rep)
        {
            return CreateReplayContext(generatedAt, new List<DashboardSalesmanPortfolioRow> { rep });
        }

        private static EntityAnalyticsProduceContext CreateReplayContext(
            DateTime generatedAt,
            IReadOnlyList<DashboardSalesmanPortfolioRow> reps)
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
                        Portfolio = reps.ToList()
                    }
                },
                "refresh-replay",
                generatedAt);
        }

        private static SalesmanEntityAnalyticsProducer CreateProducer(ReplayReconciliationRepository repository)
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
            new SalesmanEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());

            var attentionSignals = new EntityAttentionSignalRegistry();
            SalesmanAttentionSignalCatalog.Register(attentionSignals);

            var relationships = new EntityRelationshipDefinitionRegistry(entityTypes);
            SalesmanRelationshipCatalog.Register(relationships);

            return new SalesmanEntityAnalyticsProducer(
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
