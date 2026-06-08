using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.CustomerAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardCustomerAggregatorTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6);
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode FixedPeriode = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private readonly DashboardCustomerAggregator _aggregator = new DashboardCustomerAggregator();

        [Fact]
        public void Aggregate_TopOmzetRanking_OrderedByGrandTotalThenName()
        {
            var result = Aggregate(
                faktur: new[]
                {
                    Faktur("C001", "Zulu", 300m),
                    Faktur("C002", "Alpha", 500m),
                    Faktur("C003", "Beta", 500m),
                });

            result.TopOmzet.Should().HaveCount(3);
            result.TopOmzet[0].CustomerName.Should().Be("Alpha");
            result.TopOmzet[0].OmzetAmount.Should().Be(500m);
            result.TopOmzet[1].CustomerName.Should().Be("Beta");
            result.TopOmzet[2].CustomerName.Should().Be("Zulu");
        }

        [Fact]
        public void Aggregate_TopPiutangRanking_MatchesPiutangGrouping()
        {
            var result = Aggregate(
                piutang: new[]
                {
                    Piutang("C001", "Alpha", FixedToday.AddDays(-10), 100m),
                    Piutang("C001", "Alpha", FixedToday.AddDays(-20), 50m),
                    Piutang("C002", "Beta", FixedToday.AddDays(-10), 300m),
                });

            result.TopPiutang.Should().HaveCount(2);
            result.TopPiutang[0].CustomerName.Should().Be("Beta");
            result.TopPiutang[0].OutstandingBalance.Should().Be(300m);
            result.TopPiutang[1].OutstandingBalance.Should().Be(150m);
        }

        [Fact]
        public void Aggregate_CustomerKeyFallback_CodePreferredOverName()
        {
            var result = Aggregate(
                faktur: new[]
                {
                    Faktur("C001", "Name A", 100m),
                    Faktur("C001", "Name B", 200m),
                    Faktur(null, "Standalone", 300m),
                });

            result.ActiveCustomerCount.Should().Be(2);
            result.TotalOmzet.Should().Be(600m);
        }

        [Fact]
        public void Aggregate_Dormant_IncludedWhen91DaysWithHistory()
        {
            var result = Aggregate(
                lastFaktur: new[]
                {
                    LastFaktur("C001", "Dormant Co", FixedToday.AddDays(-91)),
                });

            result.DormantCustomerCount.Should().Be(1);
            result.AttentionList.Should().ContainSingle(a =>
                a.SignalKey == DashboardCustomerAggregator.SignalDormant &&
                a.CustomerName == "Dormant Co");
        }

        [Fact]
        public void Aggregate_Dormant_NotIncludedWhen89Days()
        {
            var result = Aggregate(
                lastFaktur: new[]
                {
                    LastFaktur("C001", "Recent Co", FixedToday.AddDays(-89)),
                });

            result.DormantCustomerCount.Should().Be(0);
        }

        [Fact]
        public void Aggregate_Dormant_NotIncludedWhenNoHistory()
        {
            var result = Aggregate();
            result.DormantCustomerCount.Should().Be(0);
        }

        [Fact]
        public void Aggregate_Dormant_NotIncludedWhenActiveThisMonth()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("C001", "Active Co", 100m) },
                lastFaktur: new[]
                {
                    LastFaktur("C001", "Active Co", FixedToday.AddDays(-120)),
                });

            result.DormantCustomerCount.Should().Be(0);
            result.ActiveCustomerCount.Should().Be(1);
        }

        [Fact]
        public void Aggregate_OverdueSignal_AppearsInAttentionList()
        {
            var result = Aggregate(
                piutang: new[]
                {
                    Piutang("C001", "Overdue Co", FixedToday.AddDays(-10), 200m),
                });

            result.AttentionList.Should().ContainSingle(a =>
                a.SignalKey == DashboardCustomerAggregator.SignalOverdue &&
                a.ValueAmount == 200m);
        }

        [Fact]
        public void Aggregate_PlafondBreach_WhenBalanceExceedsPlafond()
        {
            var result = Aggregate(
                piutang: new[] { Piutang("C001", "Breach Co", FixedToday, 150m) },
                customers: new[] { Customer("C001", "Breach Co", plafond: 100m) });

            result.PlafondBreachCount.Should().Be(1);
            result.AttentionList.Should().ContainSingle(a =>
                a.SignalKey == DashboardCustomerAggregator.SignalPlafondBreach &&
                a.ValueAmount == 50m);
        }

        [Fact]
        public void Aggregate_PlafondZero_NoBreachEvenWithBalance()
        {
            var result = Aggregate(
                piutang: new[] { Piutang("C001", "No Limit", FixedToday, 500m) },
                customers: new[] { Customer("C001", "No Limit", plafond: 0m) });

            result.PlafondBreachCount.Should().Be(0);
        }

        [Fact]
        public void Aggregate_SuspendedWithSales_WhenSuspendAndCurrentFaktur()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("C001", "Suspended Co", 250m) },
                customers: new[] { Customer("C001", "Suspended Co", isSuspend: true) });

            result.SuspendedWithSalesCount.Should().Be(1);
            result.AttentionList.Should().ContainSingle(a =>
                a.SignalKey == DashboardCustomerAggregator.SignalSuspendedWithSales &&
                a.ValueAmount == 250m);
        }

        [Fact]
        public void Aggregate_SuspendedNoSales_NoSignal()
        {
            var result = Aggregate(
                customers: new[] { Customer("C001", "Suspended Idle", isSuspend: true) });

            result.SuspendedWithSalesCount.Should().Be(0);
        }

        [Fact]
        public void Aggregate_ConcentrationPercent_TopOmzetOverTotal()
        {
            var result = Aggregate(
                faktur: new[]
                {
                    Faktur("C001", "Top", 400m),
                    Faktur("C002", "Other A", 300m),
                    Faktur("C003", "Other B", 300m),
                });

            result.TopOmzetCustomerPercent.Should().Be(40m);
        }

        [Fact]
        public void Aggregate_SegmentationUnknown_WhenKlasifikasiBlank()
        {
            var result = Aggregate(
                customers: new[] { Customer("C001", "Unclassified", klasifikasi: "") });

            result.Segmentation.Should().Contain(s =>
                s.SegmentType == "Klasifikasi" &&
                s.SegmentLabel == "Unknown" &&
                s.CustomerCount == 1);
        }

        [Fact]
        public void Aggregate_AttentionList_MultiSignal_TwoRowsForSameCustomer()
        {
            var result = Aggregate(
                piutang: new[] { Piutang("C001", "Multi Signal", FixedToday.AddDays(-10), 150m) },
                customers: new[] { Customer("C001", "Multi Signal", plafond: 100m) });

            result.AttentionList
                .Where(a => a.CustomerName == "Multi Signal")
                .Select(a => a.SignalKey)
                .Should().BeEquivalentTo(new[]
                {
                    DashboardCustomerAggregator.SignalOverdue,
                    DashboardCustomerAggregator.SignalPlafondBreach
                });
        }

        private DashboardCustomerAggregateResult Aggregate(
            IEnumerable<FakturView> faktur = null,
            IEnumerable<PiutangOpenBalanceDto> piutang = null,
            IEnumerable<CustomerLastFakturDto> lastFaktur = null,
            IEnumerable<CustomerModel> customers = null)
        {
            return _aggregator.Aggregate(
                faktur ?? Array.Empty<FakturView>(),
                piutang ?? Array.Empty<PiutangOpenBalanceDto>(),
                lastFaktur ?? Array.Empty<CustomerLastFakturDto>(),
                customers ?? Array.Empty<CustomerModel>(),
                FixedPeriode,
                FixedToday,
                FixedGeneratedAt);
        }

        private static FakturView Faktur(string code, string name, decimal grandTotal)
        {
            return new FakturView
            {
                CustomerCode = code,
                Customer = name,
                GrandTotal = grandTotal,
                Tgl = FixedToday
            };
        }

        private static PiutangOpenBalanceDto Piutang(
            string code,
            string name,
            DateTime jatuhTempo,
            decimal kurangBayar)
        {
            return new PiutangOpenBalanceDto
            {
                CustomerCode = code,
                CustomerName = name,
                JatuhTempo = jatuhTempo,
                KurangBayar = kurangBayar
            };
        }

        private static CustomerLastFakturDto LastFaktur(string code, string name, DateTime lastDate)
        {
            return new CustomerLastFakturDto
            {
                CustomerCode = code,
                CustomerName = name,
                LastFakturDate = lastDate
            };
        }

        private static CustomerModel Customer(
            string code,
            string name,
            decimal plafond = 0m,
            bool isSuspend = false,
            string klasifikasi = "Retail",
            string wilayah = "Jakarta")
        {
            return new CustomerModel
            {
                CustomerCode = code,
                CustomerName = name,
                Plafond = plafond,
                IsSuspend = isSuspend,
                KlasifikasiName = klasifikasi,
                WilayahName = wilayah
            };
        }
    }
}
