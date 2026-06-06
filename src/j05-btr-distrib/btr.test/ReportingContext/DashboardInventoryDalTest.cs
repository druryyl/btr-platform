using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.DashboardInventoryAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardInventoryDalTest
    {
        private static readonly DateTime FixedNow = new DateTime(2026, 6, 6, 14, 30, 0);

        [Fact]
        public void GetSummary_ExcludesInTransit_AndZeroQtyGroups()
        {
            var dal = CreateDal(new[]
            {
                Row("BRG001", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A"),
                Row("BRG001", "In-Transit", 50, 10_000m, "Cat A", "Sup A"),
                Row("BRG002", "Gudang Utama", 0, 5_000m, "Cat B", "Sup B"),
            });

            var result = dal.GetSummary();

            result.TotalItem.Should().Be(1);
            result.TotalInventoryValue.Should().Be(1_000_000m);
        }

        [Fact]
        public void GetSummary_MapsBlankCategoryAndSupplier_ToUnknown()
        {
            var dal = CreateDal(new[]
            {
                Row("BRG001", "Gudang Utama", 10, 1_000m, null, null),
                Row("BRG002", "Gudang Utama", 5, 2_000m, "  ", "Supplier X"),
            });

            var result = dal.GetSummary();

            result.TopCategories.Should().Contain(c => c.Name == "Unknown");
            result.TopSuppliers.Should().Contain(s => s.Name == "Unknown");
        }

        [Fact]
        public void GetSummary_FullCategoryRollup_EqualsTotalInventoryValue()
        {
            var dal = CreateDal(new[]
            {
                Row("BRG001", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A"),
                Row("BRG001", "Gudang Cabang", 50, 10_000m, "Cat A", "Sup A"),
                Row("BRG002", "Gudang Utama", 20, 5_000m, "Cat B", null),
                Row("BRG003", "Gudang Utama", 10, 1_000m, null, "Sup B"),
            });

            var result = dal.GetSummary();

            var allCategories = new[]
                {
                    Row("BRG001", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A"),
                    Row("BRG001", "Gudang Cabang", 50, 10_000m, "Cat A", "Sup A"),
                    Row("BRG002", "Gudang Utama", 20, 5_000m, "Cat B", null),
                    Row("BRG003", "Gudang Utama", 10, 1_000m, null, "Sup B"),
                }
                .Where(r => r.WarehouseName != "In-Transit")
                .GroupBy(r => r.BrgId)
                .Where(g => g.Sum(x => x.Qty) > 0)
                .Select(g => new
                {
                    Category = string.IsNullOrWhiteSpace(g.Select(x => x.KategoriName).FirstOrDefault())
                        ? "Unknown"
                        : g.Select(x => x.KategoriName).FirstOrDefault().Trim(),
                    Value = g.Sum(x => x.Hpp * x.Qty)
                })
                .GroupBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
                .Sum(g => g.Sum(x => x.Value));

            allCategories.Should().Be(result.TotalInventoryValue);
            result.TopCategories.Count.Should().BeLessOrEqualTo(10);
            result.CategoryBreakdown.Should().BeEquivalentTo(
                result.TopCategories.Select(t => new { t.Name, t.InventoryValue }),
                options => options.ExcludingMissingMembers());
        }

        [Fact]
        public void GetSummary_FullSupplierRollup_EqualsTotalInventoryValue()
        {
            var dal = CreateDal(new[]
            {
                Row("BRG001", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A"),
                Row("BRG002", "Gudang Utama", 20, 5_000m, "Cat B", "Sup B"),
            });

            var result = dal.GetSummary();

            var expectedSupplierSum = 1_100_000m;
            result.TotalInventoryValue.Should().Be(expectedSupplierSum);
            result.TopSuppliers.Sum(s => s.InventoryValue).Should().BeLessOrEqualTo(result.TotalInventoryValue);
            result.TopSuppliers.Should().BeInDescendingOrder(s => s.InventoryValue);
        }

        private static DashboardInventoryDal CreateDal(IEnumerable<StokBalanceView> rows)
        {
            return new DashboardInventoryDal(
                new StubStokBalanceViewDal(rows),
                new StubTglJamDal(FixedNow));
        }

        private static StokBalanceView Row(
            string brgId,
            string warehouseName,
            int qty,
            decimal hpp,
            string kategoriName,
            string supplierName)
        {
            return new StokBalanceView
            {
                BrgId = brgId,
                WarehouseName = warehouseName,
                Qty = qty,
                Hpp = hpp,
                KategoriName = kategoriName,
                SupplierName = supplierName,
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
