using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityRankingEngineTest
    {
        [Fact]
        public void ComputeAndPersistRanks_PersistsCompetitionRanking()
        {
            var repository = new RankingRepository();
            SeedPopulation(repository);

            var engine = CreateEngine(repository);
            engine.ComputeAndPersistRanks(EntityTypeCode.Customer, 2026, 6, "r1", new DateTime(2026, 6, 24));

            repository.RankingRows.Should().HaveCount(6);
            repository.RankingRows.Where(r => r.RankMetricKpiId == "CU-KPI-009").Should().HaveCount(3);

            var omzetRanks = repository.RankingRows
                .Where(r => r.RankMetricKpiId == "CU-KPI-009")
                .ToDictionary(r => r.EntityId, r => r.RankPosition);

            omzetRanks["C002"].Should().Be(1);
            omzetRanks["C001"].Should().Be(2);
            omzetRanks["C003"].Should().Be(3);
        }

        [Fact]
        public void ComputeAndPersistRanks_ExcludesInactiveAndNull()
        {
            var repository = new RankingRepository();
            repository.MonthlyRows.Add(Monthly("C001", 2026, 6, "CU-KPI-009", 100m));
            repository.MonthlyRows.Add(Monthly("C002", 2026, 6, "CU-KPI-009", null));
            repository.CurrentRows.Add(ActiveMeta("C001", true));
            repository.CurrentRows.Add(ActiveMeta("C002", true));

            var engine = CreateEngine(repository);
            engine.ComputeAndPersistRanks(EntityTypeCode.Customer, 2026, 6, "r1", DateTime.UtcNow);

            repository.RankingRows.Should().ContainSingle();
            repository.RankingRows[0].EntityId.Should().Be("C001");
            repository.RankingRows[0].PopulationSize.Should().Be(1);
        }

        [Fact]
        public void ComputeAndPersistRanks_SkipsClosedMonth()
        {
            var repository = new RankingRepository();
            SeedPopulation(repository);
            repository.CloseMonth(EntityTypeCode.Customer, 2026, 6, "close");

            var engine = CreateEngine(repository);
            engine.ComputeAndPersistRanks(EntityTypeCode.Customer, 2026, 6, "r1", DateTime.UtcNow);

            repository.RankingRows.Should().BeEmpty();
        }

        [Fact]
        public void ComputeAndPersistRanks_IsIdempotent()
        {
            var repository = new RankingRepository();
            SeedPopulation(repository);
            var engine = CreateEngine(repository);

            engine.ComputeAndPersistRanks(EntityTypeCode.Customer, 2026, 6, "r1", DateTime.UtcNow);
            engine.ComputeAndPersistRanks(EntityTypeCode.Customer, 2026, 6, "r2", DateTime.UtcNow);

            repository.RankingRows.Should().HaveCount(6);
            repository.RankingRows.Should().OnlyContain(r => r.LastRefreshLogId == "r2");
        }

        [Fact]
        public void BuildRankingSection_ReturnsSeriesWithBestWorst()
        {
            var repository = new RankingRepository();
            repository.RankingRows.AddRange(new[]
            {
                RankRow("C001", "CU-KPI-009", 2026, 4, 5, 10, 60m),
                RankRow("C001", "CU-KPI-009", 2026, 5, 2, 10, 90m),
                RankRow("C001", "CU-KPI-009", 2026, 6, 3, 10, 80m)
            });

            var engine = CreateEngine(repository);
            var section = engine.BuildRankingSection(EntityTypeCode.Customer, "C001");

            section.IsAvailable.Should().BeTrue();
            var series = section.Series.Should().ContainSingle().Subject;
            series.KpiId.Should().Be("CU-KPI-009");
            series.CurrentRank.Should().Be(3);
            series.BestRank.Should().Be(2);
            series.WorstRank.Should().Be(5);
            series.Points.Should().HaveCount(3);
            series.RankingDirection.Should().Be("HigherIsBetter");
        }

        [Fact]
        public void BuildRankingSection_NoData_ReturnsNoSnapshotData()
        {
            var engine = CreateEngine(new RankingRepository());
            var section = engine.BuildRankingSection(EntityTypeCode.Customer, "C001");

            section.IsAvailable.Should().BeFalse();
            section.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.NoSnapshotData);
        }

        private static void SeedPopulation(RankingRepository repository)
        {
            repository.MonthlyRows.Add(Monthly("C001", 2026, 6, "CU-KPI-009", 100m));
            repository.MonthlyRows.Add(Monthly("C002", 2026, 6, "CU-KPI-009", 300m));
            repository.MonthlyRows.Add(Monthly("C003", 2026, 6, "CU-KPI-009", 50m));
            repository.MonthlyRows.Add(Monthly("C001", 2026, 6, "CU-KPI-010", 500m));
            repository.MonthlyRows.Add(Monthly("C002", 2026, 6, "CU-KPI-010", 100m));
            repository.MonthlyRows.Add(Monthly("C003", 2026, 6, "CU-KPI-010", 200m));

            foreach (var id in new[] { "C001", "C002", "C003" })
                repository.CurrentRows.Add(ActiveMeta(id, true));
        }

        private static EntityRankingEngine CreateEngine(RankingRepository repository)
        {
            var entityTypes = new EntityTypeRegistry();
            entityTypes.Register(new EntityTypeRegistration
            {
                EntityTypeCode = EntityTypeCode.Customer,
                DisplayName = "Customer",
                KpiPackId = CustomerEntityAnalyticsRegistrar.KpiPackId
            });

            var registry = new EntityAnalyticsKpiRegistry(entityTypes);
            new CustomerEntityAnalyticsRegistrar().Register(
                entityTypes,
                registry,
                new EntityAnalyticsDimensionLabelRegistry());

            return new EntityRankingEngine(
                repository,
                registry,
                entityTypes,
                Options.Create(new EntityAnalyticsOptions { HistoryRetentionMonths = 36 }));
        }

        private static EntityAnalyticsMonthlyRow Monthly(
            string entityId, int year, int month, string kpiId, decimal? value)
        {
            return new EntityAnalyticsMonthlyRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = entityId,
                EntityCode = entityId,
                PeriodYear = year,
                PeriodMonth = month,
                KpiId = kpiId,
                NumericValue = value,
                PeriodSemantics = "MTD"
            };
        }

        private static EntityAnalyticsCurrentRow ActiveMeta(string entityId, bool isActive)
        {
            return new EntityAnalyticsCurrentRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = entityId,
                EntityCode = entityId,
                KpiId = EntityAnalyticsMetaKpiIds.IsActive,
                NumericValue = isActive ? 1m : 0m
            };
        }

        private static EntityAnalyticsRankingRow RankRow(
            string entityId,
            string kpiId,
            int year,
            int month,
            int rank,
            int population,
            decimal percentile)
        {
            return new EntityAnalyticsRankingRow
            {
                EntityType = EntityTypeCode.Customer,
                EntityId = entityId,
                EntityCode = entityId,
                RankMetricKpiId = kpiId,
                PeriodYear = year,
                PeriodMonth = month,
                RankPosition = rank,
                PopulationSize = population,
                Percentile = percentile,
                GeneratedAt = DateTime.UtcNow
            };
        }

        private sealed class RankingRepository : EntityAnalyticsRepositoryStubBase
        {
            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
                => CurrentRows.Where(r => r.EntityType == entityType && r.EntityId == entityId).ToList();

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
