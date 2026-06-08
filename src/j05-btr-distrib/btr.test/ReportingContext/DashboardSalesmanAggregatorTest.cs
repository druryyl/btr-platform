using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardSalesmanAggregatorTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6);
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode FixedPeriode = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private readonly DashboardSalesmanAggregator _aggregator = new DashboardSalesmanAggregator();

        [Fact]
        public void Aggregate_TopOmzetRanking_OrderedByGrandTotalThenName()
        {
            var result = Aggregate(
                faktur: new[]
                {
                    Faktur("SP001", "S001", "Zulu", 300m),
                    Faktur("SP002", "S002", "Alpha", 500m),
                    Faktur("SP003", "S003", "Beta", 500m),
                });

            result.TopOmzet.Should().HaveCount(3);
            result.TopOmzet[0].SalesPersonName.Should().Be("Alpha");
            result.TopOmzet[0].CompletedOmzet.Should().Be(500m);
            result.TopOmzet[1].SalesPersonName.Should().Be("Beta");
            result.TopOmzet[2].SalesPersonName.Should().Be("Zulu");
        }

        [Fact]
        public void Aggregate_TopOmzet_ExcludesZeroOmzet()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("SP001", "S001", "Active", 100m) },
                salespeople: new[] { SalesPerson("SP001", "S001", "Active"), SalesPerson("SP002", "S002", "Inactive") });

            result.TopOmzet.Should().ContainSingle();
            result.TopOmzet[0].SalesPersonName.Should().Be("Active");
        }

        [Fact]
        public void Aggregate_TopAchievement_OrderedByPercentExcludesNullAndZeroOmzet()
        {
            var targets = new Dictionary<string, decimal?>
            {
                ["SP001"] = 100m,
                ["SP002"] = 100m,
                ["SP003"] = 100m,
            };

            var result = Aggregate(
                faktur: new[]
                {
                    Faktur("SP001", "S001", "Low", 79m),
                    Faktur("SP002", "S002", "High", 120m),
                },
                targets: targets,
                salespeople: new[]
                {
                    SalesPerson("SP001", "S001", "Low"),
                    SalesPerson("SP002", "S002", "High"),
                    SalesPerson("SP003", "S003", "No Sales"),
                });

            result.TopAchievement.Should().HaveCount(2);
            result.TopAchievement[0].SalesPersonName.Should().Be("High");
            result.TopAchievement[0].AchievementPercent.Should().Be(120m);
            result.TopAchievement[1].SalesPersonName.Should().Be("Low");
        }

        [Fact]
        public void Aggregate_TopPiutang_MatchesGroupingAndExcludesZero()
        {
            var result = Aggregate(
                piutang: new[]
                {
                    Piutang("SP001", "Alpha", "C001", FixedToday.AddDays(-10), 100m),
                    Piutang("SP001", "Alpha", "C002", FixedToday.AddDays(-20), 50m),
                    Piutang("SP002", "Beta", "C003", FixedToday.AddDays(-10), 300m),
                });

            result.TopPiutang.Should().HaveCount(2);
            result.TopPiutang[0].SalesPersonName.Should().Be("Beta");
            result.TopPiutang[0].OutstandingBalance.Should().Be(300m);
            result.TopPiutang[1].OutstandingBalance.Should().Be(150m);
        }

        [Fact]
        public void Aggregate_GroupsBySalesPersonIdNotName()
        {
            var result = Aggregate(
                faktur: new[]
                {
                    Faktur("SP001", "S001", "Name A", 100m),
                    Faktur("SP001", "S001", "Name B", 200m),
                });

            result.ActiveSalesmanCount.Should().Be(1);
            result.TotalTeamOmzet.Should().Be(300m);
        }

        [Fact]
        public void Aggregate_BelowTarget_EmitsSignalFor79Percent()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("SP001", "S001", "Critical Rep", 79m) },
                targets: new Dictionary<string, decimal?> { ["SP001"] = 100m },
                salespeople: new[] { SalesPerson("SP001", "S001", "Critical Rep") });

            result.BelowTargetCount.Should().Be(1);
            result.AttentionList.Should().ContainSingle(a =>
                a.SignalKey == DashboardSalesmanAggregator.SignalBelowTarget &&
                a.SalesPersonName == "Critical Rep");
        }

        [Fact]
        public void Aggregate_BelowTarget_NoSignalWhenHealthy()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("SP001", "S001", "Healthy Rep", 105m) },
                targets: new Dictionary<string, decimal?> { ["SP001"] = 100m },
                salespeople: new[] { SalesPerson("SP001", "S001", "Healthy Rep") });

            result.BelowTargetCount.Should().Be(0);
            result.AttentionList.Should().NotContain(a =>
                a.SignalKey == DashboardSalesmanAggregator.SignalBelowTarget);
        }

        [Fact]
        public void Aggregate_NoTarget_WhenActivityWithoutTarget()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("SP001", "S001", "No Target Rep", 50m) },
                salespeople: new[] { SalesPerson("SP001", "S001", "No Target Rep") });

            result.NoTargetCount.Should().Be(1);
            result.AttentionList.Should().ContainSingle(a =>
                a.SignalKey == DashboardSalesmanAggregator.SignalNoTarget);
        }

        [Fact]
        public void Aggregate_NoTarget_NotWhenTargetExists()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("SP001", "S001", "Target Rep", 50m) },
                targets: new Dictionary<string, decimal?> { ["SP001"] = 100m },
                salespeople: new[] { SalesPerson("SP001", "S001", "Target Rep") });

            result.AttentionList.Should().NotContain(a =>
                a.SignalKey == DashboardSalesmanAggregator.SignalNoTarget);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardSalesmanAggregator.SignalBelowTarget);
        }

        [Fact]
        public void Aggregate_NoTarget_NotWhenInactiveZeroActivity()
        {
            var result = Aggregate(
                salespeople: new[] { SalesPerson("SP001", "S001", "Idle Rep") });

            result.NoTargetCount.Should().Be(0);
        }

        [Fact]
        public void Aggregate_HighOverdueExposure_WhenPastDueRow()
        {
            var result = Aggregate(
                piutang: new[]
                {
                    Piutang("SP001", "Overdue Rep", "C001", FixedToday.AddDays(-10), 200m),
                },
                salespeople: new[] { SalesPerson("SP001", "S001", "Overdue Rep") });

            result.HighOverdueExposureCount.Should().Be(1);
            result.AttentionList.Should().ContainSingle(a =>
                a.SignalKey == DashboardSalesmanAggregator.SignalHighOverdueExposure &&
                a.ValueText.Contains("overdue customers"));
        }

        [Fact]
        public void Aggregate_HighPiutangExposure_WithSharePercent()
        {
            var result = Aggregate(
                piutang: new[]
                {
                    Piutang("SP001", "Piutang Rep", "C001", FixedToday, 400m),
                    Piutang("SP002", "Other Rep", "C002", FixedToday, 100m),
                },
                salespeople: new[]
                {
                    SalesPerson("SP001", "S001", "Piutang Rep"),
                    SalesPerson("SP002", "S002", "Other Rep"),
                });

            result.HighPiutangExposureCount.Should().Be(2);
            result.AttentionList.Should().Contain(a =>
                a.SignalKey == DashboardSalesmanAggregator.SignalHighPiutangExposure &&
                a.SalesPersonName == "Piutang Rep" &&
                a.ValueText.Contains("80.0%"));
        }

        [Fact]
        public void Aggregate_CustomerConcentration_ShowsTopCustomerPercent()
        {
            var result = Aggregate(
                faktur: new[]
                {
                    Faktur("SP001", "S001", "Conc Rep", 100m, "C001", "Top Customer"),
                    Faktur("SP001", "S001", "Conc Rep", 50m, "C002", "Small Customer"),
                },
                salespeople: new[] { SalesPerson("SP001", "S001", "Conc Rep") });

            result.CustomerConcentrationCount.Should().Be(1);
            result.AttentionList.Should().ContainSingle(a =>
                a.SignalKey == DashboardSalesmanAggregator.SignalCustomerConcentration &&
                a.ValueText == "66.7% top customer");
        }

        [Fact]
        public void Aggregate_DormantPortfolio_AttributedToLastInvoicingSalesman()
        {
            var result = Aggregate(
                lastFaktur: new[]
                {
                    LastFaktur("C001", "Dormant Co", FixedToday.AddDays(-91), "SP001", "Rep A"),
                },
                salespeople: new[] { SalesPerson("SP001", "S001", "Rep A") });

            result.DormantPortfolioCount.Should().Be(1);
            result.AttentionList.Should().ContainSingle(a =>
                a.SignalKey == DashboardSalesmanAggregator.SignalDormantCustomerPortfolio &&
                a.SalesPersonName == "Rep A");
        }

        [Fact]
        public void Aggregate_Dormant_NotWhenActiveThisMonth()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("SP001", "S001", "Rep A", 100m, "C001", "Active Co") },
                lastFaktur: new[]
                {
                    LastFaktur("C001", "Active Co", FixedToday.AddDays(-120), "SP001", "Rep A"),
                },
                salespeople: new[] { SalesPerson("SP001", "S001", "Rep A") });

            result.DormantPortfolioCount.Should().Be(0);
        }

        [Fact]
        public void Aggregate_Dormant_NotWhenNoHistory()
        {
            var result = Aggregate();
            result.DormantPortfolioCount.Should().Be(0);
        }

        [Fact]
        public void Aggregate_MultiSignal_TwoRowsForSameRep()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("SP001", "S001", "Multi Rep", 50m) },
                piutang: new[]
                {
                    Piutang("SP001", "Multi Rep", "C001", FixedToday.AddDays(-10), 100m),
                },
                targets: new Dictionary<string, decimal?> { ["SP001"] = 100m },
                salespeople: new[] { SalesPerson("SP001", "S001", "Multi Rep") });

            result.AttentionList
                .Where(a => a.SalesPersonName == "Multi Rep")
                .Select(a => a.SignalKey)
                .Should().BeEquivalentTo(new[]
                {
                    DashboardSalesmanAggregator.SignalBelowTarget,
                    DashboardSalesmanAggregator.SignalHighOverdueExposure,
                    DashboardSalesmanAggregator.SignalHighPiutangExposure,
                    DashboardSalesmanAggregator.SignalCustomerConcentration,
                });
        }

        [Fact]
        public void Aggregate_SegmentationWilayah_UnknownWhenBlank()
        {
            var result = Aggregate(
                salespeople: new[] { SalesPerson("SP001", "S001", "Rep", wilayah: "") });

            result.Segmentation.Should().Contain(s =>
                s.SegmentType == "Wilayah" &&
                s.SegmentLabel == "Unknown" &&
                s.SalesmanCount == 1);
        }

        [Fact]
        public void Aggregate_ActiveVsInactive_CountsCorrectly()
        {
            var result = Aggregate(
                faktur: new[] { Faktur("SP001", "S001", "Active Rep", 100m) },
                salespeople: new[]
                {
                    SalesPerson("SP001", "S001", "Active Rep"),
                    SalesPerson("SP002", "S002", "Inactive Rep"),
                });

            result.ActiveSalesmanCount.Should().Be(1);
            result.Segmentation.Should().Contain(s =>
                s.SegmentType == "Activity" && s.SegmentKey == "Active" && s.SalesmanCount == 1);
            result.Segmentation.Should().Contain(s =>
                s.SegmentType == "Activity" && s.SegmentKey == "Inactive" && s.SalesmanCount == 1);
        }

        [Fact]
        public void Aggregate_ConcentrationCompanyPercent_TopRepOmzetOverTeamTotal()
        {
            var result = Aggregate(
                faktur: new[]
                {
                    Faktur("SP001", "S001", "Top", 400m),
                    Faktur("SP002", "S002", "Other A", 300m),
                    Faktur("SP003", "S003", "Other B", 300m),
                });

            result.TopOmzetSalesmanPercent.Should().Be(40m);
        }

        private DashboardSalesmanAggregateResult Aggregate(
            IEnumerable<FakturView> faktur = null,
            IEnumerable<PiutangOpenBalanceWithSalesmanDto> piutang = null,
            IEnumerable<CustomerLastFakturWithSalesmanDto> lastFaktur = null,
            IEnumerable<SalesPersonModel> salespeople = null,
            IReadOnlyDictionary<string, decimal?> targets = null)
        {
            return _aggregator.Aggregate(
                faktur ?? Array.Empty<FakturView>(),
                piutang ?? Array.Empty<PiutangOpenBalanceWithSalesmanDto>(),
                lastFaktur ?? Array.Empty<CustomerLastFakturWithSalesmanDto>(),
                salespeople ?? Array.Empty<SalesPersonModel>(),
                targets ?? new Dictionary<string, decimal?>(),
                FixedPeriode,
                FixedToday,
                FixedGeneratedAt);
        }

        private static FakturView Faktur(
            string salesPersonId,
            string salesPersonCode,
            string salesPersonName,
            decimal grandTotal,
            string customerCode = "C001",
            string customerName = "Customer")
        {
            return new FakturView
            {
                SalesPersonId = salesPersonId,
                SalesPersonCode = salesPersonCode,
                SalesPersonName = salesPersonName,
                CustomerCode = customerCode,
                Customer = customerName,
                GrandTotal = grandTotal,
                Tgl = FixedToday
            };
        }

        private static PiutangOpenBalanceWithSalesmanDto Piutang(
            string salesPersonId,
            string salesPersonName,
            string customerCode,
            DateTime jatuhTempo,
            decimal kurangBayar)
        {
            return new PiutangOpenBalanceWithSalesmanDto
            {
                SalesPersonId = salesPersonId,
                SalesPersonName = salesPersonName,
                CustomerCode = customerCode,
                CustomerName = customerCode,
                JatuhTempo = jatuhTempo,
                KurangBayar = kurangBayar
            };
        }

        private static CustomerLastFakturWithSalesmanDto LastFaktur(
            string customerCode,
            string customerName,
            DateTime lastDate,
            string salesPersonId,
            string salesPersonName)
        {
            return new CustomerLastFakturWithSalesmanDto
            {
                CustomerCode = customerCode,
                CustomerName = customerName,
                LastFakturDate = lastDate,
                SalesPersonId = salesPersonId,
                SalesPersonName = salesPersonName
            };
        }

        private static SalesPersonModel SalesPerson(
            string id,
            string code,
            string name,
            string wilayah = "Jakarta",
            string segment = "Retail")
        {
            return new SalesPersonModel
            {
                SalesPersonId = id,
                SalesPersonCode = code,
                SalesPersonName = name,
                WilayahName = wilayah,
                SegmentName = segment
            };
        }
    }
}
