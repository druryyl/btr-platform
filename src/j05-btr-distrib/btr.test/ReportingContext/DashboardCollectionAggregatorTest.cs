using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.CustomerAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardCollectionKeyResolverTest
    {
        [Fact]
        public void ResolveCustomerKey_PrefersCodeOverName()
        {
            DashboardCollectionKeyResolver.ResolveCustomerKey("C001", "Name A")
                .Should().Be("C001");
        }

        [Fact]
        public void ResolveCustomerKey_FallsBackToName()
        {
            DashboardCollectionKeyResolver.ResolveCustomerKey(null, "Standalone")
                .Should().Be("Standalone");
        }

        [Fact]
        public void ResolveSalesPersonId_PrefersId()
        {
            DashboardCollectionKeyResolver.ResolveSalesPersonId("SP001", "Rep A")
                .Should().Be("SP001");
        }

        [Fact]
        public void ResolveWilayahKey_BlankId_ReturnsUnknown()
        {
            DashboardCollectionKeyResolver.ResolveWilayahKey("")
                .Should().Be(DashboardCollectionKeyResolver.UnknownWilayahKey);
        }

        [Fact]
        public void ResolveWilayahDisplayName_UsesNameWhenPresent()
        {
            DashboardCollectionKeyResolver.ResolveWilayahDisplayName("W1", "Jakarta")
                .Should().Be("Jakarta");
        }
    }

    public class DashboardCollectionAggregatorTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6);
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode FixedPeriode = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private readonly DashboardCollectionAggregator _aggregator = new DashboardCollectionAggregator();

        [Fact]
        public void Aggregate_OverdueExposure_SumsNonCurrentRows()
        {
            var result = Aggregate(
                piutang: new[]
                {
                    Open("C001", "Alpha", FixedToday.AddDays(10), 100m),
                    Open("C002", "Beta", FixedToday.AddDays(-10), 200m),
                    Open("C003", "Gamma", FixedToday.AddDays(-95), 300m),
                });

            result.OverdueExposure.Should().Be(500m);
        }

        [Fact]
        public void Aggregate_AgingRiskBuckets_SumToOverdueExposure_ExcludeCurrent()
        {
            var result = Aggregate(
                piutang: new[]
                {
                    Open("C001", "Alpha", FixedToday.AddDays(10), 100m),
                    Open("C002", "Beta", FixedToday.AddDays(-10), 200m),
                    Open("C003", "Gamma", FixedToday.AddDays(-95), 300m),
                });

            result.AgingRiskSummary.Sum(b => b.Amount).Should().Be(result.OverdueExposure);
            result.AgingRiskSummary.Should().HaveCount(4);
            result.AgingRiskSummary.Should().NotContain(b => b.BucketKey == "Current");
        }

        [Fact]
        public void Aggregate_AgingOver90Exposure_MatchesDaysOver90Bucket()
        {
            var result = Aggregate(
                piutang: new[]
                {
                    Open("C001", "Alpha", FixedToday.AddDays(-95), 300m),
                });

            result.AgingOver90Exposure.Should().Be(300m);
            result.AgingRiskSummary.Single(b => b.BucketKey == "DaysOver90").Amount.Should().Be(300m);
        }

        [Fact]
        public void Aggregate_OverdueConcentration_TopCustomerOverTotal()
        {
            var result = Aggregate(
                piutang: new[]
                {
                    Open("C001", "Alpha", FixedToday.AddDays(-10), 200m),
                    Open("C002", "Beta", FixedToday.AddDays(-20), 800m),
                });

            result.OverdueConcentrationPercent.Should().Be(80m);
        }

        [Fact]
        public void Aggregate_CashCollectedMtd_SumsBayarTunaiOnly()
        {
            var result = Aggregate(
                pelunasan: new[]
                {
                    Pelunasan("SP1", "Rep A", 100m, 50m, 0m, 0m, 0m),
                    Pelunasan("SP2", "Rep B", 200m, 0m, 0m, 0m, 0m),
                });

            result.CashCollectedMtd.Should().Be(300m);
        }

        [Fact]
        public void Aggregate_RecoveryVsBilling_UsesTotalBayarOverOmzet()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("SP1", "Rep A", 1000m) },
                pelunasan: new[] { Pelunasan("SP1", "Rep A", 400m, 200m, 0m, 0m, 0m) });

            result.MonthCollections.Should().Be(600m);
            result.RecoveryVsBillingPercent.Should().Be(60m);
        }

        [Fact]
        public void Aggregate_PaymentMixPercents_SumTo100WhenSettlementPositive()
        {
            var result = Aggregate(
                pelunasan: new[]
                {
                    Pelunasan("SP1", "Rep A", 500m, 300m, 100m, 50m, 50m),
                });

            var sum = result.PaymentMixCashPercent + result.PaymentMixGiroPercent + result.PaymentMixAdjustmentPercent;
            sum.Should().BeApproximately(100m, 0.01m);
        }

        [Fact]
        public void Aggregate_TopOverdueCustomers_RankedByOverdueNotTotal()
        {
            var result = Aggregate(
                piutang: new[]
                {
                    Open("C001", "Alpha", FixedToday.AddDays(10), 500m),
                    Open("C001", "Alpha", FixedToday.AddDays(-10), 100m),
                    Open("C002", "Beta", FixedToday.AddDays(-10), 300m),
                });

            result.TopOverdueCustomers[0].CustomerName.Should().Be("Beta");
            result.TopOverdueCustomers[0].OverdueBalance.Should().Be(300m);
            result.TopOverdueCustomers[1].OverdueBalance.Should().Be(100m);
        }

        [Fact]
        public void Aggregate_ChronicOverdue_SuppressesGenericOverdue()
        {
            var result = Aggregate(
                piutang: new[]
                {
                    Open("C001", "Alpha", FixedToday.AddDays(-95), 300m),
                });

            result.AttentionList.Should().ContainSingle(a =>
                a.SignalKey == DashboardCollectionAggregator.SignalChronicOverdue);
            result.AttentionList.Should().NotContain(a =>
                a.SignalKey == DashboardCollectionAggregator.SignalOverdue);
        }

        [Fact]
        public void Aggregate_LegacyDebt_DormantWithOpenBalance()
        {
            var result = Aggregate(
                piutang: new[] { Open("C001", "Legacy Co", FixedToday.AddDays(-10), 150m) },
                lastFaktur: new[] { LastFaktur("C001", "Legacy Co", FixedToday.AddDays(-91)) });

            result.LegacyDebtCount.Should().Be(1);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardCollectionAggregator.SignalLegacyDebt);
        }

        [Fact]
        public void Aggregate_PlafondBreachOverdue_WhenOverdueAndBreach()
        {
            var result = Aggregate(
                piutang: new[] { Open("C001", "Breach Co", FixedToday.AddDays(-10), 150m) },
                customers: new[] { Customer("C001", "Breach Co", 100m) });

            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardCollectionAggregator.SignalPlafondBreachOverdue);
        }

        [Fact]
        public void Aggregate_HighOverdueWorkload_WhenRepHasOverdue()
        {
            var result = Aggregate(
                piutangWithSalesman: new[]
                {
                    OpenWithSalesman("SP1", "Rep A", "C001", "Alpha", FixedToday.AddDays(-10), 200m),
                },
                salespeople: new[] { SalesPerson("SP1", "R01", "Rep A") });

            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardCollectionAggregator.SignalHighOverdueWorkload &&
                a.EntityName == "Rep A");
        }

        [Fact]
        public void Aggregate_LowRecoveryVsBilling_WhenCollectionsBelowOmzet()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("SP1", "Rep A", 100m) },
                pelunasan: new[] { Pelunasan("SP1", "Rep A", 60m, 0m, 0m, 0m, 0m) },
                salespeople: new[] { SalesPerson("SP1", "R01", "Rep A") });

            result.LowRecoveryVsBillingCount.Should().Be(1);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardCollectionAggregator.SignalLowRecoveryVsBilling);
        }

        [Fact]
        public void Aggregate_LowRecoveryHealthyRep_NoSignal()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("SP1", "Rep A", 100m) },
                pelunasan: new[] { Pelunasan("SP1", "Rep A", 100m, 0m, 0m, 0m, 0m) },
                salespeople: new[] { SalesPerson("SP1", "R01", "Rep A") });

            result.LowRecoveryVsBillingCount.Should().Be(0);
        }

        [Fact]
        public void Aggregate_WilayahHotspot_At20Percent_EmitsSignal()
        {
            var result = Aggregate(
                piutangWithWilayah: new[]
                {
                    OpenWithWilayah("W1", "East", "C001", "A", FixedToday.AddDays(-10), 200m),
                    OpenWithWilayah("W2", "West", "C002", "B", FixedToday.AddDays(-10), 800m),
                });

            result.WilayahHotspotCount.Should().Be(1);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardCollectionAggregator.SignalWilayahHotspot &&
                a.EntityName == "East");
        }

        [Fact]
        public void Aggregate_WilayahHotspot_At10Percent_NoSignal()
        {
            var result = Aggregate(
                piutangWithWilayah: new[]
                {
                    OpenWithWilayah("W1", "East", "C001", "A", FixedToday.AddDays(-10), 100m),
                    OpenWithWilayah("W2", "West", "C002", "B", FixedToday.AddDays(-10), 900m),
                });

            result.WilayahHotspotCount.Should().Be(0);
        }

        [Fact]
        public void Aggregate_ZeroOmzetMonth_RecoveryPercentNull()
        {
            var result = Aggregate(
                pelunasan: new[] { Pelunasan("SP1", "Rep A", 100m, 0m, 0m, 0m, 0m) });

            result.RecoveryVsBillingPercent.Should().BeNull();
        }

        [Fact]
        public void Aggregate_BlankWilayahId_GroupsAsUnknown()
        {
            var result = Aggregate(
                piutangWithWilayah: new[]
                {
                    OpenWithWilayah("", "", "C001", "A", FixedToday.AddDays(-10), 200m),
                });

            result.TopOverdueWilayah.Should().ContainSingle(r => r.WilayahName == "Unknown");
        }

        private DashboardCollectionAggregateResult Aggregate(
            IEnumerable<PiutangOpenBalanceDto> piutang = null,
            IEnumerable<PiutangOpenBalanceWithSalesmanDto> piutangWithSalesman = null,
            IEnumerable<PiutangOpenBalanceWithWilayahDto> piutangWithWilayah = null,
            IEnumerable<PenerimaanPelunasanSalesDto> pelunasan = null,
            IEnumerable<FakturView> faktur = null,
            IEnumerable<CustomerLastFakturDto> lastFaktur = null,
            IEnumerable<CustomerModel> customers = null,
            IEnumerable<SalesPersonModel> salespeople = null)
        {
            return _aggregator.Aggregate(
                piutang ?? Array.Empty<PiutangOpenBalanceDto>(),
                piutangWithSalesman ?? Array.Empty<PiutangOpenBalanceWithSalesmanDto>(),
                piutangWithWilayah ?? Array.Empty<PiutangOpenBalanceWithWilayahDto>(),
                pelunasan ?? Array.Empty<PenerimaanPelunasanSalesDto>(),
                faktur ?? Array.Empty<FakturView>(),
                lastFaktur ?? Array.Empty<CustomerLastFakturDto>(),
                customers ?? Array.Empty<CustomerModel>(),
                salespeople ?? Array.Empty<SalesPersonModel>(),
                FixedPeriode,
                FixedToday,
                FixedGeneratedAt);
        }

        private static PiutangOpenBalanceDto Open(string code, string name, DateTime jatuhTempo, decimal amount)
        {
            return new PiutangOpenBalanceDto
            {
                CustomerCode = code,
                CustomerName = name,
                JatuhTempo = jatuhTempo,
                KurangBayar = amount
            };
        }

        private static PiutangOpenBalanceWithSalesmanDto OpenWithSalesman(
            string salesId,
            string salesName,
            string code,
            string customerName,
            DateTime jatuhTempo,
            decimal amount)
        {
            return new PiutangOpenBalanceWithSalesmanDto
            {
                SalesPersonId = salesId,
                SalesPersonName = salesName,
                CustomerCode = code,
                CustomerName = customerName,
                JatuhTempo = jatuhTempo,
                KurangBayar = amount
            };
        }

        private static PiutangOpenBalanceWithWilayahDto OpenWithWilayah(
            string wilayahId,
            string wilayahName,
            string code,
            string customerName,
            DateTime jatuhTempo,
            decimal amount)
        {
            return new PiutangOpenBalanceWithWilayahDto
            {
                WilayahId = wilayahId,
                WilayahName = wilayahName,
                CustomerCode = code,
                CustomerName = customerName,
                JatuhTempo = jatuhTempo,
                KurangBayar = amount
            };
        }

        private static PenerimaanPelunasanSalesDto Pelunasan(
            string salesId,
            string salesName,
            decimal cash,
            decimal giro,
            decimal retur,
            decimal potongan,
            decimal materaiAdmin)
        {
            return new PenerimaanPelunasanSalesDto
            {
                SalesPersonId = salesId,
                SalesName = salesName,
                BayarTunai = cash,
                BayarGiro = giro,
                Retur = retur,
                Potongan = potongan,
                MateraiAdmin = materaiAdmin,
                TotalBayar = cash + giro
            };
        }

        private static FakturView Faktur(string salesId, string salesName, decimal grandTotal)
        {
            return new FakturView
            {
                SalesPersonId = salesId,
                SalesPersonName = salesName,
                GrandTotal = grandTotal
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

        private static CustomerModel Customer(string code, string name, decimal plafond)
        {
            return new CustomerModel
            {
                CustomerCode = code,
                CustomerName = name,
                Plafond = plafond
            };
        }

        private static SalesPersonModel SalesPerson(string id, string code, string name)
        {
            return new SalesPersonModel
            {
                SalesPersonId = id,
                SalesPersonCode = code,
                SalesPersonName = name
            };
        }
    }
}
