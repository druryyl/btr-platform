using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityTrendEngineTest
    {
        [Fact]
        public void BuildTrendSection_OrdersPointsChronologically()
        {
            var repository = new TrendRepository();
            repository.MonthlyRows.AddRange(new[]
            {
                Monthly("C001", 2026, 6, "CU-KPI-009", 1500000m, "MTD", false),
                Monthly("C001", 2026, 4, "CU-KPI-009", 900000m, "MTD", true),
                Monthly("C001", 2026, 5, "CU-KPI-009", 1200000m, "MTD", true)
            });

            var engine = CreateEngine(repository);
            var result = engine.BuildTrendSection(EntityTypeCode.Customer, "C001");

            result.IsAvailable.Should().BeTrue();
            var series = result.Series.Should().ContainSingle().Subject;
            series.KpiId.Should().Be("CU-KPI-009");
            series.Points.Select(p => p.PeriodMonth).Should().Equal(4, 5, 6);
            series.Points[2].PeriodLabel.Should().Be("Jun 2026 (MTD)");
            series.Points[1].IsClosed.Should().BeTrue();
        }

        [Fact]
        public void BuildTrendSection_NoHistory_ReturnsNoSnapshotData()
        {
            var engine = CreateEngine(new TrendRepository());
            var result = engine.BuildTrendSection(EntityTypeCode.Customer, "C001");

            result.IsAvailable.Should().BeFalse();
            result.UnavailableReason.Should().Be(EntityAnalyticsUnavailableReasons.NoSnapshotData);
        }

        [Fact]
        public void CloseMonth_BlocksFurtherUpsert()
        {
            var repository = new TrendRepository();
            repository.SaveMonthlyHistory(EntityTypeCode.Customer, new[]
            {
                Monthly("C001", 2026, 5, "CU-KPI-009", 100m, "MTD", false)
            }, "r1");

            repository.CloseMonth(EntityTypeCode.Customer, 2026, 5, "close-1");

            repository.SaveMonthlyHistory(EntityTypeCode.Customer, new[]
            {
                Monthly("C001", 2026, 5, "CU-KPI-009", 999m, "MTD", false)
            }, "r2");

            repository.MonthlyRows.Single().NumericValue.Should().Be(100m);
            repository.MonthlyRows.Single().IsClosed.Should().BeTrue();
        }

        private static EntityTrendEngine CreateEngine(TrendRepository repository)
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

            return new EntityTrendEngine(
                repository,
                registry,
                entityTypes,
                Options.Create(new EntityAnalyticsOptions { HistoryRetentionMonths = 36 }));
        }

        private static EntityAnalyticsMonthlyRow Monthly(
            string entityId,
            int year,
            int month,
            string kpiId,
            decimal value,
            string semantics,
            bool isClosed)
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
                PeriodSemantics = semantics,
                IsClosed = isClosed,
                GeneratedAt = new DateTime(year, month, 1)
            };
        }

        private sealed class TrendRepository : EntityAnalyticsRepositoryStubBase
        {
            public override IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
                => Array.Empty<EntityAnalyticsCurrentRow>();

            public override EntityIdentity TryResolveIdentity(string entityType, string entityId) => null;

            public override void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
            {
            }

            public override DateTime? GetLatestGeneratedAt(string entityType, string entityId) => null;

            public override bool HasAnyCurrentMetrics(string entityType) => false;
        }
    }
}
