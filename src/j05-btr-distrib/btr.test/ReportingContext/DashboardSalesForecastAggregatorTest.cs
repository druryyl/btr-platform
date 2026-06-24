using System;
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
    public class DashboardSalesForecastAggregatorTest
    {
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 15, 10, 0, 0);
        private static readonly DateTime BusinessDate = new DateTime(2026, 6, 15);
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private readonly DashboardSalesForecastAggregator _aggregator = new DashboardSalesForecastAggregator();
        private readonly DashboardSalesFakturAggregator _salesAggregator = new DashboardSalesFakturAggregator();

        [Fact]
        public void Aggregate_ElapsedDailyBucketsSumToCurrentSales()
        {
            var rows = new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 2), 1_000_000m),
                Faktur("FK002", new DateTime(2026, 6, 5), 2_500_000m),
                Faktur("FK003", new DateTime(2026, 6, 10), 750_000m),
            };

            var result = Aggregate(rows);

            var elapsedSum = result.DailyPace
                .Where(d => d.IsElapsed)
                .Sum(d => d.ActualAmount);

            elapsedSum.Should().Be(result.CurrentSales);
            result.CurrentSales.Should().Be(rows.Sum(r => r.GrandTotal));
        }

        [Fact]
        public void Aggregate_CurrentSales_MatchesSalesFakturAggregatorTotalOmzet()
        {
            var rows = new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 2), 1_000_000m),
                Faktur("FK002", new DateTime(2026, 6, 20), 500_000m),
            };

            var forecast = Aggregate(rows);
            var sales = _salesAggregator.Aggregate(rows, June2026, 5_000_000m, FixedGeneratedAt);

            forecast.CurrentSales.Should().Be(sales.TotalOmzet);
            forecast.TotalTarget.Should().Be(sales.TotalTarget);
        }

        [Fact]
        public void Aggregate_Recent7DayAverage_FallsBackToMtdWhenDaysElapsedLessThan7()
        {
            var businessDate = new DateTime(2026, 6, 5);
            var rows = new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 1), 100_000m),
                Faktur("FK002", new DateTime(2026, 6, 3), 200_000m),
                Faktur("FK003", new DateTime(2026, 6, 5), 300_000m),
            };

            var result = _aggregator.Aggregate(
                rows,
                June2026,
                totalTarget: 10_000_000m,
                businessDate,
                FixedGeneratedAt);

            result.BestCaseSales.Should().Be(result.ForecastSales);
            result.WorstCaseSales.Should().Be(result.ForecastSales);
        }

        [Fact]
        public void Aggregate_BestCaseGreaterOrEqualExpected_GreaterOrEqualWorstCase()
        {
            var rows = Enumerable.Range(1, 15)
                .Select(i => Faktur($"FK{i:D3}", new DateTime(2026, 6, i), i * 100_000m))
                .ToArray();

            var result = Aggregate(rows);

            result.BestCaseSales.Should().BeGreaterOrEqualTo(result.ForecastSales);
            result.ForecastSales.Should().BeGreaterOrEqualTo(result.WorstCaseSales);
        }

        [Fact]
        public void Aggregate_DailyPaceRowCount_EqualsDaysInMonth()
        {
            var result = Aggregate(Array.Empty<FakturView>());

            var expectedDays = SalesOmzetChartDayGrouper.BuildBuckets(June2026).Count;
            result.DailyPace.Should().HaveCount(expectedDays);
        }

        [Fact]
        public void Aggregate_FutureDayBuckets_HaveZeroActualAmount()
        {
            var rows = new[] { Faktur("FK001", new DateTime(2026, 6, 20), 1_000_000m) };
            var result = Aggregate(rows);

            result.DailyPace
                .Where(d => !d.IsElapsed)
                .Should().OnlyContain(d => d.ActualAmount == 0m);
        }

        [Fact]
        public void Aggregate_ProjectedDailyAmount_EqualsMtdDailyAverageForAllDays()
        {
            var rows = new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 1), 1_500_000m),
                Faktur("FK002", new DateTime(2026, 6, 10), 1_500_000m),
            };

            var result = Aggregate(rows);

            result.DailyPace.Should().OnlyContain(d =>
                d.ProjectedDailyAmount == result.DailyAverageSales);
        }

        private DashboardSalesForecastAggregateResult Aggregate(FakturView[] rows) =>
            _aggregator.Aggregate(rows, June2026, 10_000_000m, BusinessDate, FixedGeneratedAt);

        private static FakturView Faktur(string code, DateTime tgl, decimal grandTotal) =>
            new FakturView
            {
                FakturCode = code,
                Tgl = tgl,
                GrandTotal = grandTotal,
            };
    }
}
