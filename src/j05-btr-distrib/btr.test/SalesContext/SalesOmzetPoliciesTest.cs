using System;
using System.Collections.Generic;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.application.SalesContext.SalesOmzetAgg.Snapshots;
using btr.domain.SalesContext.FakturControlAgg;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesOmzetPoliciesTest
    {
        private readonly SalesOmzetPeriodPolicy _periodPolicy = new SalesOmzetPeriodPolicy();
        private readonly SalesOmzetStatusPolicy _statusPolicy = new SalesOmzetStatusPolicy();
        private readonly SalesOmzetSnapshotBuilder _snapshotBuilder = new SalesOmzetSnapshotBuilder();

        private static Periode Jan2025() => new Periode(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));

        [Fact]
        public void Period_OmzetPeriod_HidesOutstandingWithSentinelOmzetDate()
        {
            var row = new SalesOmzetModel
            {
                SalesDate = new DateTime(2025, 1, 15),
                OmzetDate = SalesOmzetDates.Sentinel,
                OrderId = "ORD-1"
            };

            _periodPolicy.IsInPeriod(row, Jan2025(), SalesOmzetPeriodFilterMode.OmzetPeriod)
                .Should().BeFalse();
        }

        [Fact]
        public void Period_OmzetPeriod_IncludesRecognizedOmzetInRange()
        {
            var row = new SalesOmzetModel
            {
                SalesDate = new DateTime(2025, 1, 15),
                OmzetDate = new DateTime(2025, 2, 10)
            };

            var feb = new Periode(new DateTime(2025, 2, 1), new DateTime(2025, 2, 28));
            _periodPolicy.IsInPeriod(row, feb, SalesOmzetPeriodFilterMode.OmzetPeriod)
                .Should().BeTrue();
        }

        [Fact]
        public void Period_JanOrder_FebOmzet_VisibleInFebOmzet_NotInJanOmzet()
        {
            var row = new SalesOmzetModel
            {
                SalesDate = new DateTime(2025, 1, 15),
                OmzetDate = new DateTime(2025, 2, 10),
                OrderId = "ORD-1"
            };

            var jan = Jan2025();
            var feb = new Periode(new DateTime(2025, 2, 1), new DateTime(2025, 2, 28));

            _periodPolicy.IsInPeriod(row, jan, SalesOmzetPeriodFilterMode.OmzetPeriod)
                .Should().BeFalse();
            _periodPolicy.IsInPeriod(row, feb, SalesOmzetPeriodFilterMode.OmzetPeriod)
                .Should().BeTrue();
            _periodPolicy.IsInPeriod(row, jan, SalesOmzetPeriodFilterMode.SalesPeriod)
                .Should().BeTrue();
        }

        [Fact]
        public void Period_SalesPeriod_ShowsOutstandingInSalesMonth()
        {
            var row = new SalesOmzetModel
            {
                SalesDate = new DateTime(2025, 1, 15),
                OmzetDate = SalesOmzetDates.Sentinel,
                OrderId = "ORD-1"
            };

            _periodPolicy.IsInPeriod(row, Jan2025(), SalesOmzetPeriodFilterMode.SalesPeriod)
                .Should().BeTrue();
        }

        [Fact]
        public void Period_ToSqlWhere_StrictOmzetExcludesSentinel()
        {
            _periodPolicy.ToSqlWhere(SalesOmzetPeriodFilterMode.OmzetPeriod)
                .Should().Contain("3000-01-01");
        }

        [Fact]
        public void Status_Outstanding_WhenOrderedSaleWithoutFaktur()
        {
            var row = new SalesOmzetModel
            {
                SaleKind = SaleKindEnum.OrderedSale,
                OrderId = "ORD-1",
                FakturId = string.Empty,
                OmzetDate = SalesOmzetDates.Sentinel
            };

            _statusPolicy.Resolve(row).Should().Be(SalesOmzetStatusEnum.Outstanding);
        }

        [Fact]
        public void Status_PendingOmzet_WhenFakturWithoutKembaliFaktur()
        {
            var row = new SalesOmzetModel
            {
                SaleKind = SaleKindEnum.OrderedSale,
                OrderId = "ORD-1",
                FakturId = "FK-1",
                OmzetDate = SalesOmzetDates.Sentinel
            };

            _statusPolicy.Resolve(row).Should().Be(SalesOmzetStatusEnum.PendingOmzet);
        }

        [Fact]
        public void Status_Completed_WhenOmzetDateRecognized()
        {
            var row = new SalesOmzetModel
            {
                FakturId = "FK-1",
                OmzetDate = new DateTime(2025, 2, 5)
            };

            _statusPolicy.Resolve(row).Should().Be(SalesOmzetStatusEnum.Completed);
        }

        [Fact]
        public void Snapshot_OmzetDate_FromKembaliFakturStatusOnly()
        {
            var row = new SalesOmzetModel { OmzetDate = SalesOmzetDates.Sentinel };
            var statuses = new List<FakturControlStatusSnapshot>
            {
                new FakturControlStatusSnapshot
                {
                    StatusFaktur = StatusFakturEnum.Kirim,
                    StatusDate = new DateTime(2025, 1, 10)
                },
                new FakturControlStatusSnapshot
                {
                    StatusFaktur = StatusFakturEnum.KembaliFaktur,
                    StatusDate = new DateTime(2025, 2, 3)
                }
            };

            _snapshotBuilder.ApplyOmzetDate(row, statuses);
            row.OmzetDate.Should().Be(new DateTime(2025, 2, 3));
        }

        [Fact]
        public void Snapshot_SalesDate_NotOverwrittenOnApplyOrder()
        {
            var frozen = new DateTime(2025, 1, 5);
            var row = new SalesOmzetModel { SalesDate = frozen, OrderDate = SalesOmzetDates.Sentinel };
            var order = new OrderSnapshot
            {
                OrderId = "ORD-1",
                OrderDate = new DateTime(2025, 1, 20),
                OrderTotal = 500m
            };

            _snapshotBuilder.ApplyOrder(row, order);

            row.SalesDate.Should().Be(frozen);
            row.OrderDate.Should().Be(new DateTime(2025, 1, 20));
            row.OrderTotal.Should().Be(500m);
        }

        [Fact]
        public void Snapshot_SetSalesDateOnCreate_DirectSaleUsesFakturDate()
        {
            var row = new SalesOmzetModel();
            var faktur = new FakturSnapshot { FakturDate = new DateTime(2025, 3, 12) };

            _snapshotBuilder.SetSalesDateOnCreate(row, SaleKindEnum.DirectSale, null, faktur);

            row.SalesDate.Should().Be(new DateTime(2025, 3, 12));
        }
    }
}
