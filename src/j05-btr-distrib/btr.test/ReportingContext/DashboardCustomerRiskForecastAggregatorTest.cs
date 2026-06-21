using System;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.CustomerAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardCustomerRiskForecastAggregatorTest
    {
        private static readonly DateTime BusinessDate = new DateTime(2026, 6, 15);
        private static readonly DateTime GeneratedAt = new DateTime(2026, 6, 15, 10, 0, 0);

        private readonly DashboardCustomerRiskForecastAggregator _aggregator =
            new DashboardCustomerRiskForecastAggregator();

        [Fact]
        public void Aggregate_EndToEndSyntheticPortfolio_KpiCountsMatchRows()
        {
            var options = new CustomerRiskForecastOptions
            {
                HorizonDays = 30,
                PriorMonthOmzetFloorIdr = 1_000_000m,
                MaxTopCustomers = 20,
                MaxAttentionRows = 25,
                MaxRecommendations = 15
            };

            var result = _aggregator.Aggregate(
                new[]
                {
                    OpenBalance("C001", "Customer A", new DateTime(2026, 6, 20), 8_000_000m),
                    OpenBalance("C002", "Customer B", new DateTime(2026, 5, 1), 12_000_000m)
                },
                new[]
                {
                    Omzet("C001", 4_000_000m, 10_000_000m),
                    Omzet("C002", 1_000_000m, 8_000_000m)
                },
                new[]
                {
                    LastFaktur("C001", BusinessDate.AddDays(-10)),
                    LastFaktur("C002", BusinessDate.AddDays(-70))
                },
                new[]
                {
                    Customer("C001", 5_000_000m),
                    Customer("C002", 20_000_000m)
                },
                Array.Empty<CustomerPelunasanSummaryDto>(),
                new[]
                {
                    PaymentBehavior("C001", 10m),
                    PaymentBehavior("C002", 15m)
                },
                new[]
                {
                    Faktur("C001", BusinessDate.AddDays(-5), 4_000_000m),
                    Faktur("C002", BusinessDate.AddDays(-70), 1_000_000m)
                },
                BusinessDate,
                GeneratedAt,
                options);

            result.Kpi.TotalPiutang.Should().Be(20_000_000m);
            (result.Kpi.HealthyCount + result.Kpi.WatchCount + result.Kpi.AttentionCount +
                result.Kpi.HighRiskCount + result.Kpi.CriticalCount)
                .Should().Be(2);
            result.TopCustomers.Count.Should().BeLessOrEqualTo(20);
        }

        [Fact]
        public void Aggregate_TotalPiutang_Traceability()
        {
            var piutang = new[]
            {
                OpenBalance("C001", "A", BusinessDate.AddDays(10), 3_000_000m),
                OpenBalance("C002", "B", BusinessDate.AddDays(10), 7_000_000m)
            };

            var result = AggregateMinimal(piutang);
            result.Kpi.TotalPiutang.Should().Be(10_000_000m);
        }

        [Fact]
        public void Aggregate_CustomerWithoutHistory_Excluded()
        {
            var result = _aggregator.Aggregate(
                new[] { OpenBalance("C999", "No History", BusinessDate.AddDays(5), 1_000_000m) },
                Array.Empty<CustomerOmzetHistoryDto>(),
                Array.Empty<CustomerLastFakturDto>(),
                Array.Empty<CustomerModel>(),
                Array.Empty<CustomerPelunasanSummaryDto>(),
                Array.Empty<CustomerPaymentBehaviorDto>(),
                Array.Empty<FakturView>(),
                BusinessDate,
                GeneratedAt,
                new CustomerRiskForecastOptions());

            result.Kpi.HealthyCount.Should().Be(0);
            result.TopCustomers.Should().BeEmpty();
        }

        [Fact]
        public void Aggregate_Top20Cap_Enforced()
        {
            var options = new CustomerRiskForecastOptions { MaxTopCustomers = 20 };
            var lastFaktur = Enumerable.Range(1, 30)
                .Select(i => LastFaktur($"C{i:D3}", BusinessDate.AddDays(-10)))
                .ToArray();
            var piutang = Enumerable.Range(1, 30)
                .Select(i => OpenBalance($"C{i:D3}", $"Customer {i}", BusinessDate.AddDays(3), 5_000_000m))
                .ToArray();
            var payment = Enumerable.Range(1, 30)
                .Select(i => PaymentBehavior($"C{i:D3}", 10m))
                .ToArray();

            var result = _aggregator.Aggregate(
                piutang,
                Array.Empty<CustomerOmzetHistoryDto>(),
                lastFaktur,
                Array.Empty<CustomerModel>(),
                Array.Empty<CustomerPelunasanSummaryDto>(),
                payment,
                Array.Empty<FakturView>(),
                BusinessDate,
                GeneratedAt,
                options);

            result.TopCustomers.Count.Should().BeLessOrEqualTo(20);
        }

        private DashboardCustomerRiskForecastAggregateResult AggregateMinimal(
            PiutangOpenBalanceDto[] piutang) =>
            _aggregator.Aggregate(
                piutang,
                new[] { Omzet("C001", 1_000_000m, 1_000_000m), Omzet("C002", 1_000_000m, 1_000_000m) },
                new[] { LastFaktur("C001", BusinessDate.AddDays(-5)), LastFaktur("C002", BusinessDate.AddDays(-5)) },
                Array.Empty<CustomerModel>(),
                Array.Empty<CustomerPelunasanSummaryDto>(),
                Array.Empty<CustomerPaymentBehaviorDto>(),
                Array.Empty<FakturView>(),
                BusinessDate,
                GeneratedAt,
                new CustomerRiskForecastOptions());

        private static PiutangOpenBalanceDto OpenBalance(
            string code,
            string name,
            DateTime due,
            decimal amount) =>
            new PiutangOpenBalanceDto
            {
                CustomerCode = code,
                CustomerName = name,
                JatuhTempo = due,
                KurangBayar = amount
            };

        private static CustomerOmzetHistoryDto Omzet(string code, decimal current, decimal prior) =>
            new CustomerOmzetHistoryDto
            {
                CustomerCode = code,
                CustomerName = code,
                CurrentMonthOmzet = current,
                PriorMonthOmzet = prior
            };

        private static CustomerLastFakturDto LastFaktur(string code, DateTime date) =>
            new CustomerLastFakturDto
            {
                CustomerCode = code,
                CustomerName = code,
                LastFakturDate = date
            };

        private static CustomerModel Customer(string code, decimal plafond) =>
            new CustomerModel
            {
                CustomerCode = code,
                CustomerName = code,
                Plafond = plafond
            };

        private static CustomerPaymentBehaviorDto PaymentBehavior(string code, decimal lag) =>
            new CustomerPaymentBehaviorDto
            {
                CustomerCode = code,
                CustomerName = code,
                SettledFakturCount = 2,
                AvgPaymentLagDays = lag
            };

        private static FakturView Faktur(string code, DateTime date, decimal amount) =>
            new FakturView
            {
                CustomerCode = code,
                Customer = code,
                Tgl = date,
                GrandTotal = amount
            };
    }
}
