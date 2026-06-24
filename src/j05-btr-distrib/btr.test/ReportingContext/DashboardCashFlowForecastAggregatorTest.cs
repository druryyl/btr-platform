using System;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardCashFlowForecastAggregatorTest
    {
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 15, 10, 0, 0);
        private static readonly DateTime BusinessDate = new DateTime(2026, 6, 15);
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private readonly DashboardCashFlowForecastAggregator _aggregator = new DashboardCashFlowForecastAggregator();
        private readonly DashboardCollectionAggregator _collectionAggregator = new DashboardCollectionAggregator();

        [Fact]
        public void Aggregate_ElapsedDailyCashBucketsSumToCashCollectedMtd()
        {
            var pelunasan = new[]
            {
                Pelunasan(new DateTime(2026, 6, 2), 1_000_000m, 1_100_000m),
                Pelunasan(new DateTime(2026, 6, 5), 500_000m, 600_000m),
                Pelunasan(new DateTime(2026, 6, 10), 250_000m, 300_000m),
            };

            var collection = BuildCollectionResult(pelunasan, Array.Empty<FakturView>());
            var result = Aggregate(pelunasan, Array.Empty<FakturView>(), Array.Empty<PiutangOpenBalanceDto>(), collection);

            var elapsedCashSum = result.DailyPace
                .Where(d => d.IsElapsed)
                .Sum(d => d.ActualCashAmount);

            elapsedCashSum.Should().Be(result.CashCollectedMtd);
            result.CashCollectedMtd.Should().Be(pelunasan.Sum(p => p.BayarTunai));
        }

        [Fact]
        public void Aggregate_Traceability_MatchesCollectionAggregatorKpis()
        {
            var pelunasan = new[]
            {
                Pelunasan(new DateTime(2026, 6, 3), 2_000_000m, 2_200_000m),
            };
            var faktur = new[] { Faktur("FK001", new DateTime(2026, 6, 1), 10_000_000m) };

            var collection = BuildCollectionResult(pelunasan, faktur);
            var result = Aggregate(pelunasan, faktur, Array.Empty<PiutangOpenBalanceDto>(), collection);

            result.CashCollectedMtd.Should().Be(collection.CashCollectedMtd);
            result.MonthCollections.Should().Be(collection.MonthCollections);
            result.MonthFakturOmzet.Should().Be(collection.MonthFakturOmzet);
        }

        [Fact]
        public void Aggregate_OutstandingDueRemaining_FiltersByDueDateWindow()
        {
            var openBalances = new[]
            {
                OpenBalance("C001", new DateTime(2026, 6, 20), 5_000_000m),
                OpenBalance("C002", new DateTime(2026, 6, 10), 1_000_000m),
                OpenBalance("C003", new DateTime(2026, 7, 5), 2_000_000m),
            };

            var collection = BuildCollectionResult(Array.Empty<PenerimaanPelunasanSalesDto>(), Array.Empty<FakturView>());
            var result = Aggregate(
                Array.Empty<PenerimaanPelunasanSalesDto>(),
                Array.Empty<FakturView>(),
                openBalances,
                collection);

            result.OutstandingDueRemaining.Should().Be(5_000_000m);
        }

        [Fact]
        public void Aggregate_CollectionRisks_CappedAtTen()
        {
            var openBalances = Enumerable.Range(1, 15)
                .Select(i => OpenBalance($"C{i:D3}", BusinessDate.AddDays(3), 60_000_000m))
                .ToArray();

            var collection = BuildCollectionResult(Array.Empty<PenerimaanPelunasanSalesDto>(), Array.Empty<FakturView>());
            collection.OverdueExposure = 100_000_000m;
            collection.TopOverdueCustomers = openBalances
                .Select((r, i) => new DashboardCollectionTopOverdueCustomerRow
                {
                    Rank = i + 1,
                    CustomerCode = r.CustomerCode,
                    CustomerName = r.CustomerName,
                    OverdueBalance = 60_000_000m
                })
                .ToList();

            var result = Aggregate(
                Array.Empty<PenerimaanPelunasanSalesDto>(),
                Array.Empty<FakturView>(),
                openBalances,
                collection,
                largeDueSoonFloor: 50_000_000m);

            result.CollectionRisks.Should().HaveCountLessOrEqualTo(10);
            result.CollectionRisks.First().RiskKey.Should().Be(CashFlowCollectionRiskBuilder.RiskLargeDueSoon);
        }

        private DashboardCollectionAggregateResult BuildCollectionResult(
            PenerimaanPelunasanSalesDto[] pelunasan,
            FakturView[] faktur)
        {
            return _collectionAggregator.Aggregate(
                Array.Empty<PiutangOpenBalanceDto>(),
                Array.Empty<PiutangOpenBalanceWithSalesmanDto>(),
                Array.Empty<PiutangOpenBalanceWithWilayahDto>(),
                pelunasan,
                faktur,
                Array.Empty<CustomerLastFakturDto>(),
                Array.Empty<btr.domain.SalesContext.CustomerAgg.CustomerModel>(),
                Array.Empty<btr.domain.SalesContext.SalesPersonAgg.SalesPersonModel>(),
                June2026,
                BusinessDate,
                FixedGeneratedAt);
        }

        private DashboardCashFlowForecastAggregateResult Aggregate(
            PenerimaanPelunasanSalesDto[] pelunasan,
            FakturView[] faktur,
            PiutangOpenBalanceDto[] openBalances,
            DashboardCollectionAggregateResult collection,
            decimal largeDueSoonFloor = 50_000_000m)
        {
            return _aggregator.Aggregate(
                pelunasan,
                faktur,
                openBalances,
                Array.Empty<PiutangOpenBalanceWithSalesmanDto>(),
                Array.Empty<PiutangOpenBalanceWithWilayahDto>(),
                collection,
                June2026,
                BusinessDate,
                FixedGeneratedAt,
                largeDueSoonFloor);
        }

        private static PenerimaanPelunasanSalesDto Pelunasan(DateTime date, decimal cash, decimal total) =>
            new PenerimaanPelunasanSalesDto
            {
                LunasDate = date,
                BayarTunai = cash,
                TotalBayar = total
            };

        private static FakturView Faktur(string id, DateTime date, decimal amount) =>
            new FakturView
            {
                FakturId = id,
                Tgl = date,
                GrandTotal = amount
            };

        private static PiutangOpenBalanceDto OpenBalance(string code, DateTime dueDate, decimal balance) =>
            new PiutangOpenBalanceDto
            {
                CustomerCode = code,
                CustomerName = code,
                JatuhTempo = dueDate,
                KurangBayar = balance
            };
    }
}
