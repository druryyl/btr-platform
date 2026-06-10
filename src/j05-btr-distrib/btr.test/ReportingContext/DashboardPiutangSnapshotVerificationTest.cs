using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardPiutangSnapshotVerificationTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6);
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);

        [Fact]
        public void Aggregator_ReconcilesCompanyLevelMetrics()
        {
            var openBalances = BuildScenarioRows();
            var aggregate = Aggregate(openBalances);

            AssertCompanyLevelReconciliation(aggregate);
        }

        [Fact]
        public void SnapshotRoundTrip_PreservesCustomerAgingAndTopRiskRowCounts()
        {
            var openBalances = BuildScenarioRows();
            var aggregate = Aggregate(openBalances);
            var snapshotDal = new InMemoryPiutangSnapshotDal();

            snapshotDal.ReplaceCurrent(aggregate, "refresh-log-1");
            var loaded = snapshotDal.GetCurrent();

            loaded.Should().NotBeNull();
            loaded.CustomerAging.Should().HaveCount(aggregate.CustomerAging.Count);
            loaded.CustomerAging.Should().HaveCount(
                openBalances
                    .Where(r => !string.IsNullOrWhiteSpace(r.CustomerId))
                    .Select(r => r.CustomerId)
                    .Distinct()
                    .Count());
            loaded.TopCustomerRisk.Should().HaveCountLessThanOrEqualTo(20);
            loaded.TopCustomerRisk.Should().HaveCount(aggregate.TopCustomerRisk.Count);

            AssertCompanyLevelReconciliation(loaded);
            loaded.TotalPiutang.Should().Be(aggregate.TotalPiutang);
            loaded.Top10CustomerConcentrationPercent.Should().Be(aggregate.Top10CustomerConcentrationPercent);
            loaded.Top20CustomerConcentrationPercent.Should().Be(aggregate.Top20CustomerConcentrationPercent);
        }

        private static List<PiutangOpenBalanceDto> BuildScenarioRows()
        {
            var scenarios = new[]
            {
                OpenRow("CUST001", "C001", "Alpha Corp", FixedToday.AddDays(5), 3_000_000m),
                OpenRow("CUST001", "C001", "Alpha Corp", FixedToday.AddDays(-10), 500_000m),
                OpenRow("CUST002", "C002", "Beta Ltd", FixedToday.AddDays(-45), 1_250_000m),
                OpenRow(null, null, "Standalone", FixedToday.AddDays(-95), 800_000m),
                OpenRow("CUST003", "C003", "Gamma Inc", FixedToday, 1m),
                OpenRow("CUST004", "C004", "Delta Co", FixedToday.AddDays(-5), 0m),
            };

            return scenarios
                .Where(r => r.KurangBayar > 1)
                .Select(r => new PiutangOpenBalanceDto
                {
                    CustomerId = r.CustomerId,
                    CustomerCode = r.CustomerCode,
                    CustomerName = r.CustomerName,
                    JatuhTempo = r.JatuhTempo,
                    KurangBayar = r.KurangBayar
                })
                .ToList();
        }

        private static DashboardPiutangAggregateResult Aggregate(IEnumerable<PiutangOpenBalanceDto> openBalances)
        {
            var aggregator = new DashboardPiutangAggregator();
            return aggregator.Aggregate(openBalances, FixedToday, FixedGeneratedAt);
        }

        private static void AssertCompanyLevelReconciliation(DashboardPiutangAggregateResult aggregate)
        {
            aggregate.AgingBuckets.Sum(b => b.Amount).Should().Be(aggregate.TotalPiutang);

            var current = aggregate.AgingBuckets.Single(b => b.BucketKey == "Current").Amount;
            aggregate.OverduePiutang.Should().Be(aggregate.TotalPiutang - current);

            aggregate.CustomerAging.Sum(c => c.TotalPiutang).Should().BeLessThanOrEqualTo(aggregate.TotalPiutang);
            aggregate.TopCustomerRisk.Should().HaveCountLessThanOrEqualTo(20);

            foreach (var customer in aggregate.CustomerAging)
            {
                (customer.CurrentAmount + customer.Aging30Amount + customer.Aging60Amount
                    + customer.Aging90Amount + customer.AgingOver90Amount).Should().Be(customer.TotalPiutang);
            }

            if (aggregate.Top20CustomerConcentrationPercent.HasValue
                && aggregate.Top10CustomerConcentrationPercent.HasValue)
            {
                aggregate.Top20CustomerConcentrationPercent.Should()
                    .BeGreaterThanOrEqualTo(aggregate.Top10CustomerConcentrationPercent.Value);
            }
        }

        private static (string CustomerId, string CustomerCode, string CustomerName, DateTime JatuhTempo, decimal KurangBayar) OpenRow(
            string customerId,
            string customerCode,
            string customerName,
            DateTime jatuhTempo,
            decimal kurangBayar)
        {
            return (customerId, customerCode, customerName, jatuhTempo, kurangBayar);
        }

        private sealed class InMemoryPiutangSnapshotDal : IDashboardPiutangSnapshotDal
        {
            private DashboardPiutangAggregateResult _current;

            public DashboardPiutangAggregateResult GetCurrent() => _current;

            public void ReplaceCurrent(DashboardPiutangAggregateResult result, string refreshLogId)
            {
                _current = result;
            }
        }
    }
}
