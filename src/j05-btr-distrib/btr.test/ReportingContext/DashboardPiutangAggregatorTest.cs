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
                Row("CUST001", "C001", "Alpha", FixedToday.AddDays(10), 500_000m),
                Row("CUST002", "C002", "Beta", FixedToday.AddDays(10), 1m),
                Row("CUST003", "C003", "Gamma", FixedToday.AddDays(10), 0m),
            }, FixedToday, FixedGeneratedAt);

            result.TotalPiutang.Should().Be(500_000m);
            result.TotalCustomer.Should().Be(1);
        }

        [Fact]
        public void Aggregate_UsesCustomerCode_ForDistinctCount()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("CUST001", "C001", "Name A", FixedToday.AddDays(10), 100_000m),
                Row("CUST001", "C001", "Name B", FixedToday.AddDays(10), 200_000m),
                Row(null, null, "Standalone", FixedToday.AddDays(10), 300_000m),
            }, FixedToday, FixedGeneratedAt);

            result.TotalCustomer.Should().Be(2);
            result.TotalPiutang.Should().Be(600_000m);
        }

        [Fact]
        public void Aggregate_ReturnsAllFiveAgingBuckets()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("CUST001", "C001", "Alpha", FixedToday, 100_000m),
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
                Row("CUST001", "C001", "Alpha", FixedToday, 100_000m),
                Row("CUST001", "C001", "Alpha", FixedToday.AddDays(-15), 200_000m),
                Row("CUST002", "C002", "Beta", FixedToday.AddDays(-45), 300_000m),
                Row("CUST003", "C003", "Gamma", FixedToday.AddDays(-75), 400_000m),
                Row("CUST004", "C004", "Delta", FixedToday.AddDays(-120), 500_000m),
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
                Row("CUST001", "C001", "Alpha", FixedToday.AddDays(-daysOverdue), 100_000m),
            }, FixedToday, FixedGeneratedAt);

            var bucket = result.AgingBuckets.Single(b => b.Amount > 0);
            bucket.BucketKey.Should().Be(expectedBucket);
        }

        [Fact]
        public void Aggregate_CountsOverdueCustomers_WithNonCurrentExposure()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("CUST001", "C001", "Current Only", FixedToday.AddDays(5), 100_000m),
                Row("CUST002", "C002", "Overdue", FixedToday.AddDays(-10), 200_000m),
                Row("CUST002", "C002", "Overdue", FixedToday.AddDays(-20), 50_000m),
            }, FixedToday, FixedGeneratedAt);

            result.OverdueCustomer.Should().Be(1);
        }

        [Fact]
        public void Aggregate_OverduePiutang_EqualsTotalMinusCurrentBucket()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("CUST001", "C001", "Alpha", FixedToday, 100_000m),
                Row("CUST002", "C002", "Beta", FixedToday.AddDays(-15), 200_000m),
            }, FixedToday, FixedGeneratedAt);

            var current = result.AgingBuckets.Single(b => b.BucketKey == "Current").Amount;
            result.OverduePiutang.Should().Be(result.TotalPiutang - current);
        }

        [Fact]
        public void Aggregate_AgingOver90Percent_IsNullWhenTotalPiutangZero()
        {
            var result = _aggregator.Aggregate(Array.Empty<PiutangOpenBalanceDto>(), FixedToday, FixedGeneratedAt);

            result.AgingOver90Percent.Should().BeNull();
            result.Top10CustomerConcentrationPercent.Should().BeNull();
            result.Top20CustomerConcentrationPercent.Should().BeNull();
        }

        [Fact]
        public void Aggregate_AgingOver90Percent_CalculatesCorrectRatio()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("CUST001", "C001", "Alpha", FixedToday.AddDays(-120), 250_000m),
                Row("CUST002", "C002", "Beta", FixedToday, 750_000m),
            }, FixedToday, FixedGeneratedAt);

            result.AgingOver90Amount.Should().Be(250_000m);
            result.AgingOver90Percent.Should().Be(25m);
        }

        [Fact]
        public void Aggregate_CustomerAgingBuckets_SumToCustomerTotal()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("CUST001", "C001", "Alpha", FixedToday, 100_000m),
                Row("CUST001", "C001", "Alpha", FixedToday.AddDays(-15), 200_000m),
                Row("CUST001", "C001", "Alpha", FixedToday.AddDays(-120), 50_000m),
            }, FixedToday, FixedGeneratedAt);

            var customer = result.CustomerAging.Single();
            customer.TotalPiutang.Should().Be(350_000m);
            (customer.CurrentAmount + customer.Aging30Amount + customer.Aging60Amount
                + customer.Aging90Amount + customer.AgingOver90Amount).Should().Be(customer.TotalPiutang);
        }

        [Fact]
        public void Aggregate_CustomerAgingTotals_SumToTotalPiutang_WhenAllRowsHaveCustomerId()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("CUST001", "C001", "Alpha", FixedToday, 100_000m),
                Row("CUST002", "C002", "Beta", FixedToday.AddDays(-45), 300_000m),
            }, FixedToday, FixedGeneratedAt);

            result.CustomerAging.Sum(c => c.TotalPiutang).Should().Be(result.TotalPiutang);
        }

        [Fact]
        public void Aggregate_SkipsRowsWithoutCustomerId_FromCustomerAging()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("CUST001", "C001", "Alpha", FixedToday, 100_000m),
                Row(null, null, "No Id", FixedToday, 200_000m),
            }, FixedToday, FixedGeneratedAt);

            result.CustomerAging.Should().HaveCount(1);
            result.SkippedCustomerIdRowCount.Should().Be(1);
            result.TotalPiutang.Should().Be(300_000m);
        }

        [Fact]
        public void Aggregate_TieBreaksEqualBalance_ByCustomerNameAscending()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("CUST001", "C001", "Zulu Corp", FixedToday, 500_000m),
                Row("CUST002", "C002", "Alpha Corp", FixedToday, 500_000m),
                Row("CUST003", "C003", "Mike Corp", FixedToday, 500_000m),
            }, FixedToday, FixedGeneratedAt);

            result.TopCustomerRisk.Should().HaveCount(3);
            result.TopCustomerRisk[0].CustomerName.Should().Be("Alpha Corp");
            result.TopCustomerRisk[1].CustomerName.Should().Be("Mike Corp");
            result.TopCustomerRisk[2].CustomerName.Should().Be("Zulu Corp");
        }

        [Fact]
        public void Aggregate_ReturnsTopTwenty_OrderedByBalanceThenName()
        {
            var rows = Enumerable.Range(1, 25)
                .Select(i => Row(
                    $"CUST{i:D3}",
                    $"C{i:D3}",
                    $"Customer {i:D2}",
                    FixedToday.AddDays(-10),
                    i * 10_000m))
                .ToList();

            var result = _aggregator.Aggregate(rows, FixedToday, FixedGeneratedAt);

            result.TopCustomerRisk.Should().HaveCount(20);
            result.TopCustomerRisk[0].Rank.Should().Be(1);
            result.TopCustomerRisk[0].CustomerName.Should().Be("Customer 25");
            result.TopCustomerRisk[0].TotalPiutang.Should().Be(250_000m);
            result.TopCustomerRisk.Should().BeInDescendingOrder(c => c.TotalPiutang);
        }

        [Fact]
        public void Aggregate_TopConcentrationPercents_AreCalculatedFromRankedTotals()
        {
            var rows = Enumerable.Range(1, 15)
                .Select(i => Row(
                    $"CUST{i:D3}",
                    $"C{i:D3}",
                    $"Customer {i:D2}",
                    FixedToday,
                    i * 100_000m))
                .ToList();

            var result = _aggregator.Aggregate(rows, FixedToday, FixedGeneratedAt);

            var total = result.TotalPiutang;
            var ranked = result.CustomerAging
                .OrderByDescending(c => c.TotalPiutang)
                .ThenBy(c => c.CustomerName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var expectedTop10 = ranked.Take(10).Sum(c => c.TotalPiutang) / total * 100m;
            var expectedTop20 = ranked.Take(20).Sum(c => c.TotalPiutang) / total * 100m;

            result.Top10CustomerConcentrationPercent.Should().Be(expectedTop10);
            result.Top20CustomerConcentrationPercent.Should().Be(expectedTop20);
            result.Top20CustomerConcentrationPercent.Should().BeGreaterThanOrEqualTo(
                result.Top10CustomerConcentrationPercent.GetValueOrDefault());
        }

        [Fact]
        public void Aggregate_SetsGeneratedAt_FromInput()
        {
            var result = _aggregator.Aggregate(Array.Empty<PiutangOpenBalanceDto>(), FixedToday, FixedGeneratedAt);
            result.GeneratedAt.Should().Be(FixedGeneratedAt);
        }

        private static PiutangOpenBalanceDto Row(
            string customerId,
            string customerCode,
            string customerName,
            DateTime jatuhTempo,
            decimal kurangBayar)
        {
            return new PiutangOpenBalanceDto
            {
                CustomerId = customerId,
                CustomerCode = customerCode,
                CustomerName = customerName,
                JatuhTempo = jatuhTempo,
                KurangBayar = kurangBayar
            };
        }
    }
}
