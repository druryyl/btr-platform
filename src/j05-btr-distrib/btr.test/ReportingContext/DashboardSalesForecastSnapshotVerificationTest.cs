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
    public class DashboardSalesForecastSnapshotVerificationTest
    {
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 15, 10, 0, 0);
        private static readonly DateTime BusinessDate = new DateTime(2026, 6, 15);
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private readonly DashboardSalesFakturAggregator _salesAggregator = new DashboardSalesFakturAggregator();
        private readonly DashboardSalesForecastAggregator _forecastAggregator = new DashboardSalesForecastAggregator();

        [Fact]
        public void AfterRefresh_ForecastCurrentSales_EqualsSalesTotalOmzet()
        {
            var rows = new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 2), 1_000_000m),
                Faktur("FK002", new DateTime(2026, 6, 10), 2_500_000m),
                Faktur("FK003", new DateTime(2026, 6, 14), 750_000m),
            };

            var sales = _salesAggregator.Aggregate(rows, June2026, 10_000_000m, FixedGeneratedAt);
            var forecast = _forecastAggregator.Aggregate(
                rows,
                June2026,
                10_000_000m,
                BusinessDate,
                FixedGeneratedAt);

            forecast.CurrentSales.Should().Be(sales.TotalOmzet);
        }

        [Fact]
        public void AfterRefresh_ForecastTotalTarget_EqualsSalesTotalTarget()
        {
            var rows = new[] { Faktur("FK001", new DateTime(2026, 6, 5), 500_000m) };

            var sales = _salesAggregator.Aggregate(rows, June2026, 8_000_000m, FixedGeneratedAt);
            var forecast = _forecastAggregator.Aggregate(
                rows,
                June2026,
                8_000_000m,
                BusinessDate,
                FixedGeneratedAt);

            forecast.TotalTarget.Should().Be(sales.TotalTarget);
        }

        [Fact]
        public void AfterRefresh_DailyPaceRowCount_EqualsDaysInMonth()
        {
            var forecast = _forecastAggregator.Aggregate(
                Array.Empty<FakturView>(),
                June2026,
                0m,
                BusinessDate,
                FixedGeneratedAt);

            var expectedDays = SalesOmzetChartDayGrouper.BuildBuckets(June2026).Count;
            forecast.DailyPace.Should().HaveCount(expectedDays);
        }

        [Fact]
        public void AfterRefresh_ElapsedDailyBucketsSumToCurrentSales()
        {
            var rows = new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 1), 100_000m),
                Faktur("FK002", new DateTime(2026, 6, 8), 200_000m),
                Faktur("FK003", new DateTime(2026, 6, 15), 300_000m),
            };

            var forecast = _forecastAggregator.Aggregate(
                rows,
                June2026,
                5_000_000m,
                BusinessDate,
                FixedGeneratedAt);

            forecast.DailyPace
                .Where(d => d.IsElapsed)
                .Sum(d => d.ActualAmount)
                .Should()
                .Be(forecast.CurrentSales);
        }

        private static FakturView Faktur(string code, DateTime tgl, decimal grandTotal) =>
            new FakturView
            {
                FakturCode = code,
                Tgl = tgl,
                GrandTotal = grandTotal,
            };
    }
}
