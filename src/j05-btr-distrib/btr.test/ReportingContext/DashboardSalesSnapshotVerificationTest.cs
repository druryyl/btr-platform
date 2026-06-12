using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.Portal;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.infrastructure.ReportingContext.DashboardSalesAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardSalesSnapshotVerificationTest
    {
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        [Fact]
        public void Aggregator_TotalOmzet_MatchesSumOfFakturGrandTotal()
        {
            var rows = new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 2), "Alice", "C001", "Customer A", 1_000_000m),
                Faktur("FK002", new DateTime(2026, 6, 5), "Bob", "C002", "Customer B", 2_500_000m),
                Faktur("FK003", new DateTime(2026, 6, 15), "Alice", "C003", "Customer C", 750_000m),
            };

            var expectedTotal = rows.Sum(r => r.GrandTotal);
            var aggregator = new DashboardSalesFakturAggregator();
            var aggregate = aggregator.Aggregate(rows, June2026, totalTarget: 5_000_000m, FixedGeneratedAt);

            aggregate.TotalOmzet.Should().Be(expectedTotal);
            aggregate.TotalAchievement.Should().Be(expectedTotal);
            aggregate.TotalFaktur.Should().Be(rows.Length);
            aggregate.PipelineOmzet.Should().Be(0m);
        }

        [Fact]
        public void Aggregator_MatchesLiveDal_ForEquivalentFakturRows()
        {
            var rows = new[]
            {
                Faktur("FK001", new DateTime(2026, 6, 2), "Alice", "C001", "Customer A", 1_000_000m),
                Faktur("FK002", new DateTime(2026, 6, 5), "Bob", "C002", "Customer B", 2_500_000m),
                Faktur("FK003", new DateTime(2026, 6, 15), "Alice", "C003", "Customer C", 750_000m),
                Faktur("FK004", new DateTime(2026, 6, 20), "Charlie", "", "Customer D", 500_000m),
            };

            var aggregator = new DashboardSalesFakturAggregator();
            var aggregate = aggregator.Aggregate(rows, June2026, totalTarget: 5_000_000m, FixedGeneratedAt);

            var liveDal = new DashboardSalesLiveDal(
                new StubFakturViewDal(rows),
                new StubTargetDal(5_000_000m),
                new StubTglJamDal(FixedGeneratedAt),
                new StubBusinessDateProvider(FixedGeneratedAt),
                aggregator);

            var live = liveDal.GetSummary();

            live.TotalOmzet.Should().Be(aggregate.TotalOmzet);
            live.CompletedOmzet.Should().Be(aggregate.CompletedOmzet);
            live.PipelineOmzet.Should().Be(0m);
            live.TotalFaktur.Should().Be(aggregate.TotalFaktur);
            live.TotalCustomer.Should().Be(aggregate.TotalCustomer);
            live.TotalTarget.Should().Be(aggregate.TotalTarget);
            live.TotalAchievement.Should().Be(aggregate.TotalAchievement);
            live.AchievementPercent.Should().Be(aggregate.AchievementPercent);

            live.WeeklyTrend.Sum(w => w.RecognizedAmount).Should().Be(
                aggregate.WeekTrend.Sum(w => w.RecognizedAmount));

            live.TopSalesmanRanking.Should().HaveCount(aggregate.TopSalesman.Count);
            for (var i = 0; i < live.TopSalesmanRanking.Count; i++)
            {
                live.TopSalesmanRanking[i].SalesPersonName.Should().Be(aggregate.TopSalesman[i].SalesPersonName);
                live.TopSalesmanRanking[i].CompletedOmzet.Should().Be(aggregate.TopSalesman[i].CompletedOmzet);
                live.TopSalesmanRanking[i].Rank.Should().Be(aggregate.TopSalesman[i].Rank);
            }
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

        private sealed class StubFakturViewDal : IFakturViewDal
        {
            private readonly IEnumerable<FakturView> _rows;

            public StubFakturViewDal(IEnumerable<FakturView> rows)
            {
                _rows = rows;
            }

            public IEnumerable<FakturView> ListData(Periode filter) => _rows;

            public IEnumerable<FakturView> ListTerhapus(Periode periode) => Array.Empty<FakturView>();
        }

        private sealed class StubTargetDal : btr.application.SalesContext.SalesOmzetAgg.Contracts.ISalesOmzetTargetDal
        {
            private readonly decimal _totalTarget;

            public StubTargetDal(decimal totalTarget)
            {
                _totalTarget = totalTarget;
            }

            public decimal SumTargetAmountForMonth(int year, int month) => _totalTarget;

            public decimal? GetTargetAmount(string salesPersonId, int year, int month) => null;

            public System.Collections.Generic.IReadOnlyDictionary<string, decimal?> ListTargetsForMonth(int year, int month)
                => new System.Collections.Generic.Dictionary<string, decimal?>();
        }

        private sealed class StubTglJamDal : btr.application.SupportContext.TglJamAgg.ITglJamDal
        {
            public StubTglJamDal(DateTime now)
            {
                Now = now;
            }

            public DateTime Now { get; }
        }

        private sealed class StubBusinessDateProvider : IBusinessDateProvider
        {
            public StubBusinessDateProvider(DateTime today)
            {
                Today = today.Date;
            }

            public DateTime Today { get; }

            public bool IsPresentationActive => false;
        }
    }
}
