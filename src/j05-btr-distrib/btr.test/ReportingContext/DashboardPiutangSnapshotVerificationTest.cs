using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.DashboardPiutangAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardPiutangSnapshotVerificationTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6);
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);

        [Fact]
        public void Aggregator_MatchesLiveDal_ForEquivalentOpenBalanceRows()
        {
            var scenarios = new[]
            {
                OpenRow("C001", "Alpha Corp", FixedToday.AddDays(5), 3_000_000m),
                OpenRow("C001", "Alpha Corp", FixedToday.AddDays(-10), 500_000m),
                OpenRow("C002", "Beta Ltd", FixedToday.AddDays(-45), 1_250_000m),
                OpenRow(null, "Standalone", FixedToday.AddDays(-95), 800_000m),
                OpenRow("C003", "Gamma Inc", FixedToday, 1m),
                OpenRow("C004", "Delta Co", FixedToday.AddDays(-5), 0m),
            };

            var openBalances = scenarios
                .Where(r => r.KurangBayar > 1)
                .Select(r => new PiutangOpenBalanceDto
                {
                    CustomerCode = r.CustomerCode,
                    CustomerName = r.CustomerName,
                    JatuhTempo = r.JatuhTempo,
                    KurangBayar = r.KurangBayar
                })
                .ToList();

            var wilayahRows = scenarios.Select(r => new PiutangSalesWilayahDto
            {
                CustomerCode = r.CustomerCode,
                CustomerName = r.CustomerName,
                JatuhTempo = r.JatuhTempo,
                KurangBayar = r.KurangBayar
            }).ToList();

            var aggregator = new DashboardPiutangAggregator();
            var aggregate = aggregator.Aggregate(openBalances, FixedToday, FixedGeneratedAt);

            var liveDal = new DashboardPiutangLiveDal(
                new StubPiutangSalesWilayahDal(wilayahRows),
                new StubTglJamDal(FixedGeneratedAt));

            var live = liveDal.GetSummary();

            aggregate.TotalPiutang.Should().Be(live.TotalPiutang);
            aggregate.TotalCustomer.Should().Be(live.TotalCustomer);
            aggregate.OverdueCustomer.Should().Be(live.OverdueCustomer);

            aggregate.AgingBuckets.Should().HaveCount(live.AgingBuckets.Count);
            foreach (var bucket in aggregate.AgingBuckets)
            {
                var liveBucket = live.AgingBuckets.Single(b => b.BucketKey == bucket.BucketKey);
                liveBucket.Amount.Should().Be(bucket.Amount);
            }

            aggregate.TopCustomers.Should().HaveCount(live.TopCustomers.Count);
            for (var i = 0; i < aggregate.TopCustomers.Count; i++)
            {
                aggregate.TopCustomers[i].CustomerName.Should().Be(live.TopCustomers[i].CustomerName);
                aggregate.TopCustomers[i].OutstandingBalance.Should().Be(live.TopCustomers[i].OutstandingBalance);
                aggregate.TopCustomers[i].Rank.Should().Be(live.TopCustomers[i].Rank);
            }
        }

        private static (string CustomerCode, string CustomerName, DateTime JatuhTempo, decimal KurangBayar) OpenRow(
            string customerCode,
            string customerName,
            DateTime jatuhTempo,
            decimal kurangBayar)
        {
            return (customerCode, customerName, jatuhTempo, kurangBayar);
        }

        private sealed class StubPiutangSalesWilayahDal : IPiutangSalesWilayahDal
        {
            private readonly IEnumerable<PiutangSalesWilayahDto> _rows;

            public StubPiutangSalesWilayahDal(IEnumerable<PiutangSalesWilayahDto> rows)
            {
                _rows = rows;
            }

            public IEnumerable<PiutangSalesWilayahDto> ListData(Periode periode) => _rows;
        }

        private sealed class StubTglJamDal : ITglJamDal
        {
            public StubTglJamDal(DateTime now)
            {
                Now = now;
            }

            public DateTime Now { get; }
        }
    }
}
