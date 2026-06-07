using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardPiutangAggregatorTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6);
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);
        private readonly DashboardPiutangAggregator _aggregator = new DashboardPiutangAggregator();

        [Fact]
        public void Aggregate_ExcludesRows_WithKurangBayarNotGreaterThanOne()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("C001", "Alpha", FixedToday.AddDays(10), 500_000m),
                Row("C002", "Beta", FixedToday.AddDays(10), 1m),
                Row("C003", "Gamma", FixedToday.AddDays(10), 0m),
            }, FixedToday, FixedGeneratedAt);

            result.TotalPiutang.Should().Be(500_000m);
            result.TotalCustomer.Should().Be(1);
        }

        [Fact]
        public void Aggregate_UsesCustomerCode_ForDistinctCount()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("C001", "Name A", FixedToday.AddDays(10), 100_000m),
                Row("C001", "Name B", FixedToday.AddDays(10), 200_000m),
                Row(null, "Standalone", FixedToday.AddDays(10), 300_000m),
            }, FixedToday, FixedGeneratedAt);

            result.TotalCustomer.Should().Be(2);
            result.TotalPiutang.Should().Be(600_000m);
        }

        [Fact]
        public void Aggregate_ReturnsAllFiveAgingBuckets()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("C001", "Alpha", FixedToday, 100_000m),
            }, FixedToday, FixedGeneratedAt);

            result.AgingBuckets.Should().HaveCount(5);
            result.AgingBuckets.Select(b => b.BucketKey).Should().Contain(new[]
            {
                "Current", "Days1To30", "Days31To60", "Days61To90", "DaysOver90"
            });
        }

        [Fact]
        public void Aggregate_AgingBucketAmounts_SumToTotalPiutang()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("C001", "Alpha", FixedToday, 100_000m),
                Row("C001", "Alpha", FixedToday.AddDays(-15), 200_000m),
                Row("C002", "Beta", FixedToday.AddDays(-45), 300_000m),
                Row("C003", "Gamma", FixedToday.AddDays(-75), 400_000m),
                Row("C004", "Delta", FixedToday.AddDays(-120), 500_000m),
            }, FixedToday, FixedGeneratedAt);

            result.AgingBuckets.Sum(b => b.Amount).Should().Be(result.TotalPiutang);
            result.TotalPiutang.Should().Be(1_500_000m);
        }

        [Theory]
        [InlineData(0, "Current")]
        [InlineData(1, "Days1To30")]
        [InlineData(30, "Days1To30")]
        [InlineData(31, "Days31To60")]
        [InlineData(60, "Days31To60")]
        [InlineData(61, "Days61To90")]
        [InlineData(90, "Days61To90")]
        [InlineData(91, "DaysOver90")]
        public void Aggregate_AssignsAgingBucket_ByDueDateBoundary(int daysOverdue, string expectedBucket)
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("C001", "Alpha", FixedToday.AddDays(-daysOverdue), 100_000m),
            }, FixedToday, FixedGeneratedAt);

            var bucket = result.AgingBuckets.Single(b => b.Amount > 0);
            bucket.BucketKey.Should().Be(expectedBucket);
        }

        [Fact]
        public void Aggregate_CountsOverdueCustomers_WithNonCurrentExposure()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("C001", "Current Only", FixedToday.AddDays(5), 100_000m),
                Row("C002", "Overdue", FixedToday.AddDays(-10), 200_000m),
                Row("C002", "Overdue", FixedToday.AddDays(-20), 50_000m),
            }, FixedToday, FixedGeneratedAt);

            result.OverdueCustomer.Should().Be(1);
        }

        [Fact]
        public void Aggregate_ReturnsTopTen_OrderedByBalanceThenName()
        {
            var rows = Enumerable.Range(1, 12)
                .Select(i => Row($"C{i:D3}", $"Customer {i:D2}", FixedToday.AddDays(-10), i * 10_000m))
                .ToList();

            var result = _aggregator.Aggregate(rows, FixedToday, FixedGeneratedAt);

            result.TopCustomers.Should().HaveCount(10);
            result.TopCustomers[0].Rank.Should().Be(1);
            result.TopCustomers[0].CustomerName.Should().Be("Customer 12");
            result.TopCustomers[0].OutstandingBalance.Should().Be(120_000m);
            result.TopCustomers.Should().BeInDescendingOrder(c => c.OutstandingBalance);
        }

        [Fact]
        public void Aggregate_SetsGeneratedAt_FromInput()
        {
            var result = _aggregator.Aggregate(Array.Empty<PiutangOpenBalanceDto>(), FixedToday, FixedGeneratedAt);
            result.GeneratedAt.Should().Be(FixedGeneratedAt);
        }

        private static PiutangOpenBalanceDto Row(
            string customerCode,
            string customerName,
            DateTime jatuhTempo,
            decimal kurangBayar)
        {
            return new PiutangOpenBalanceDto
            {
                CustomerCode = customerCode,
                CustomerName = customerName,
                JatuhTempo = jatuhTempo,
                KurangBayar = kurangBayar
            };
        }
    }
}
