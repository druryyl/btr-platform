using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Sources;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SalesmanRepHistoryBackfillSourceTest
    {
        [Fact]
        public void MapToL1Rows_MapsSfKpi008009010()
        {
            var dal = new FakeRepHistoryDal(new[]
            {
                new DashboardSalesmanRepHistoryRow
                {
                    PeriodYear = 2024,
                    PeriodMonth = 3,
                    SalesPersonId = "SP001",
                    SalesPersonCode = "R001",
                    SalesPersonName = "Rep One",
                    CompletedOmzet = 1000m,
                    AchievementPercent = 85.5m,
                    OpenBalance = 250m
                }
            });

            var source = new SalesmanRepHistoryBackfillSource(dal);
            var generatedAt = new DateTime(2026, 6, 25, 10, 0, 0);

            var rows = source.MapToL1Rows(2024, 3, "refresh-1", generatedAt);

            rows.Should().HaveCount(3);
            rows.Should().Contain(r => r.KpiId == "SF-KPI-008" && r.NumericValue == 1000m);
            rows.Should().Contain(r => r.KpiId == "SF-KPI-009" && r.NumericValue == 85.5m);
            rows.Should().Contain(r => r.KpiId == "SF-KPI-010" && r.NumericValue == 250m);
            rows.Should().OnlyContain(r =>
                r.EntityType == EntityTypeCode.Salesman
                && r.EntityId == "SP001"
                && r.PeriodYear == 2024
                && r.PeriodMonth == 3
                && r.IsClosed);
        }

        [Fact]
        public void HasCoverage_ReturnsTrueWhenRowsExist()
        {
            var dal = new FakeRepHistoryDal(new[]
            {
                new DashboardSalesmanRepHistoryRow { PeriodYear = 2024, PeriodMonth = 1 }
            });
            var source = new SalesmanRepHistoryBackfillSource(dal);

            source.HasCoverage(2024, 1).Should().BeTrue();
            source.HasCoverage(2024, 2).Should().BeFalse();
        }

        private sealed class FakeRepHistoryDal : ISalesmanRepHistoryBackfillDal
        {
            private readonly IReadOnlyList<DashboardSalesmanRepHistoryRow> _rows;

            public FakeRepHistoryDal(IReadOnlyList<DashboardSalesmanRepHistoryRow> rows)
            {
                _rows = rows;
            }

            public bool HasCoverage(int periodYear, int periodMonth) =>
                ListForPeriod(periodYear, periodMonth).Count > 0;

            public IReadOnlyList<DashboardSalesmanRepHistoryRow> ListForPeriod(int periodYear, int periodMonth)
            {
                var result = new List<DashboardSalesmanRepHistoryRow>();
                foreach (var row in _rows)
                {
                    if (row.PeriodYear == periodYear && row.PeriodMonth == periodMonth)
                        result.Add(row);
                }

                return result;
            }
        }
    }
}
