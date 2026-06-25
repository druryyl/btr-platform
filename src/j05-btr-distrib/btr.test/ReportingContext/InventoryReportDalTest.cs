using System;
using System.Collections.Generic;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.InventoryReportAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class InventoryReportDalTest
    {
        private static readonly DateTime FixedNow = new DateTime(2026, 6, 6, 14, 30, 0);

        [Fact]
        public void GetReport_ExcludesInTransit_AndZeroQtyRows_FromDisplay()
        {
            var dal = new InventoryReportDal(
                new StubStokBalanceViewDal(new[]
                {
                    Row("BRG001", "A-001", "Product A", "Gudang Utama", 100, 10_000m),
                    Row("BRG001", "A-001", "Product A", "In-Transit", 50, 10_000m),
                    Row("BRG002", "B-001", "Product B", "Gudang Utama", 0, 5_000m),
                }),
                new StubTglJamDal(FixedNow));

            var result = dal.GetReport();

            result.Rows.Should().HaveCount(1);
            result.Rows[0].WarehouseName.Should().Be("Gudang Utama");
            result.Rows.Should().OnlyContain(r => r.Qty > 0);
        }

        [Fact]
        public void GetReport_GroupsByBrgId_ForSummaryTotals()
        {
            var dal = new InventoryReportDal(
                new StubStokBalanceViewDal(new[]
                {
                    Row("BRG001", "A-001", "Product A", "Gudang Utama", 100, 10_000m),
                    Row("BRG001", "A-001", "Product A", "Gudang Cabang", 50, 10_000m),
                    Row("BRG002", "B-001", "Product B", "Gudang Utama", 20, 5_000m),
                }),
                new StubTglJamDal(FixedNow));

            var result = dal.GetReport();

            result.Rows.Should().HaveCount(3);
            result.Summary.TotalInventoryValue.Should().Be(1_600_000m);
            result.Summary.TotalItem.Should().Be(2);
        }

        [Fact]
        public void GetReport_IncludesZeroQtyInGrouping_ButExcludesFromTotalItem()
        {
            var dal = new InventoryReportDal(
                new StubStokBalanceViewDal(new[]
                {
                    Row("BRG001", "A-001", "Product A", "Gudang Utama", 100, 10_000m),
                    Row("BRG002", "B-001", "Product B", "Gudang Utama", 0, 5_000m),
                }),
                new StubTglJamDal(FixedNow));

            var result = dal.GetReport();

            result.Summary.TotalItem.Should().Be(1);
            result.Summary.TotalInventoryValue.Should().Be(1_000_000m);
        }

        [Fact]
        public void GetReport_OrdersRows_ByBrgCodeThenWarehouse()
        {
            var dal = new InventoryReportDal(
                new StubStokBalanceViewDal(new[]
                {
                    Row("BRG002", "Z-002", "Zulu", "Warehouse B", 10, 1_000m),
                    Row("BRG001", "A-001", "Alpha", "Warehouse A", 10, 1_000m),
                }),
                new StubTglJamDal(FixedNow));

            var result = dal.GetReport();

            result.Rows[0].ItemDisplay.Should().Contain("A-001");
            result.Rows[1].ItemDisplay.Should().Contain("Z-002");
        }

        private static StokBalanceView Row(
            string brgId,
            string brgCode,
            string brgName,
            string warehouseName,
            int qty,
            decimal hpp)
        {
            return new StokBalanceView
            {
                BrgId = brgId,
                BrgCode = brgCode,
                BrgName = brgName,
                WarehouseName = warehouseName,
                Qty = qty,
                Hpp = hpp,
            };
        }

        private sealed class StubStokBalanceViewDal : IStokBalanceViewDal
        {
            private readonly IEnumerable<StokBalanceView> _rows;

            public StubStokBalanceViewDal(IEnumerable<StokBalanceView> rows)
            {
                _rows = rows;
            }

            public IEnumerable<StokBalanceView> ListData() => _rows;

            public IEnumerable<StokBalanceView> ListDataAsOf(DateTime asOfDate) => _rows;
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
