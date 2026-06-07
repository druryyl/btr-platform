using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardSalesFakturAggregatorTest
    {
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private readonly DashboardSalesFakturAggregator _aggregator = new DashboardSalesFakturAggregator();

        [Fact]
        public void Aggregate_SumsGrandTotal_AndSetsPipelineToZero()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 3), "Alice", "C001", "Customer A", 1_000_000m),
                Faktur("FK002", new DateTime(2026, 6, 10), "Bob", "C002", "Customer B", 2_500_000m),
            }, June2026, totalTarget: 5_000_000m, FixedGeneratedAt);

            result.TotalOmzet.Should().Be(3_500_000m);
            result.CompletedOmzet.Should().Be(3_500_000m);
            result.PipelineOmzet.Should().Be(0m);
            result.TotalAchievement.Should().Be(3_500_000m);
            result.TotalFaktur.Should().Be(2);
            result.AchievementPercent.Should().Be(70.0m);
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(6);
        }

        [Fact]
        public void Aggregate_CountsDistinctCustomers_ByCodeWithNameFallback()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 1), "Alice", "C001", "Customer A", 100m),
                Faktur("FK002", new DateTime(2026, 6, 2), "Alice", "C001", "Customer A Duplicate", 200m),
                Faktur("FK003", new DateTime(2026, 6, 3), "Bob", "", "Customer B", 300m),
                Faktur("FK004", new DateTime(2026, 6, 4), "Bob", null, "Customer B", 400m),
            }, June2026, totalTarget: 0m, FixedGeneratedAt);

            result.TotalCustomer.Should().Be(2);
        }

        [Fact]
        public void Aggregate_GroupsWeeklyTrend_ByFakturDate()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 1), "Alice", "C001", "A", 100m),
                Faktur("FK002", new DateTime(2026, 6, 2), "Alice", "C002", "B", 200m),
                Faktur("FK003", new DateTime(2026, 6, 8), "Bob", "C003", "C", 500m),
            }, June2026, totalTarget: 0m, FixedGeneratedAt);

            result.WeekTrend.Should().NotBeEmpty();
            result.WeekTrend.Sum(w => w.RecognizedAmount).Should().Be(800m);

            var firstWeek = result.WeekTrend.First(w => w.WeekStart == new DateTime(2026, 6, 1));
            firstWeek.RecognizedAmount.Should().Be(300m);

            var secondWeek = result.WeekTrend.First(w => w.WeekStart == new DateTime(2026, 6, 8));
            secondWeek.RecognizedAmount.Should().Be(500m);
        }

        [Fact]
        public void Aggregate_BuildsTop10Salesman_ByGrandTotalDescending()
        {
            var rows = Enumerable.Range(1, 12)
                .Select(i => Faktur(
                    $"FK{i:D3}",
                    new DateTime(2026, 6, 1),
                    $"Sales {i}",
                    $"C{i:D3}",
                    $"Customer {i}",
                    i * 100_000m))
                .ToArray();

            var result = _aggregator.Aggregate(rows, June2026, totalTarget: 0m, FixedGeneratedAt);

            result.TopSalesman.Should().HaveCount(DashboardSalesFakturAggregator.TopSalesmanCount);
            result.TopSalesman[0].SalesPersonName.Should().Be("Sales 12");
            result.TopSalesman[0].CompletedOmzet.Should().Be(1_200_000m);
            result.TopSalesman[9].SalesPersonName.Should().Be("Sales 3");
            result.TopSalesman.Select(r => r.Rank).Should().BeEquivalentTo(Enumerable.Range(1, 10));
        }

        [Fact]
        public void Aggregate_ExcludesBlankSalesPerson_FromRanking()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 1), "Alice", "C001", "A", 1_000m),
                Faktur("FK002", new DateTime(2026, 6, 2), "", "C002", "B", 9_000m),
                Faktur("FK003", new DateTime(2026, 6, 3), "  ", "C003", "C", 8_000m),
            }, June2026, totalTarget: 0m, FixedGeneratedAt);

            result.TopSalesman.Should().ContainSingle();
            result.TopSalesman[0].SalesPersonName.Should().Be("Alice");
        }

        [Fact]
        public void Aggregate_WeekTrendLabels_MatchWeekGrouper()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 1), "Alice", "C001", "A", 100m),
            }, June2026, totalTarget: 0m, FixedGeneratedAt);

            var expectedBuckets = SalesOmzetChartWeekGrouper.BuildBuckets(June2026);
            result.WeekTrend.Should().HaveCount(expectedBuckets.Count);
            result.WeekTrend[0].WeekLabel.Should().Be(expectedBuckets[0].WeekLabel);
        }

        private static FakturView Faktur(
            string code,
            DateTime tgl,
            string salesPerson,
            string customerCode,
            string customer,
            decimal grandTotal)
        {
            return new FakturView
            {
                FakturCode = code,
                Tgl = tgl,
                SalesPersonName = salesPerson,
                CustomerCode = customerCode,
                Customer = customer,
                GrandTotal = grandTotal,
            };
        }
    }
}
