using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardInventoryAgg.Contracts;
using btr.application.ReportingContext.DashboardInventoryAgg.Queries;
using btr.application.SupportContext.TglJamAgg;

namespace btr.infrastructure.ReportingContext.DashboardInventoryAgg
{
    public class DashboardInventoryDal : IDashboardInventoryDal
    {
        private const string InTransitWarehouseName = "In-Transit";
        private const string UnknownLabel = "Unknown";

        private readonly IStokBalanceViewDal _stokBalanceViewDal;
        private readonly ITglJamDal _tglJamDal;

        public DashboardInventoryDal(
            IStokBalanceViewDal stokBalanceViewDal,
            ITglJamDal tglJamDal)
        {
            _stokBalanceViewDal = stokBalanceViewDal;
            _tglJamDal = tglJamDal;
        }

        public DashboardInventoryResponse GetSummary()
        {
            var itemGroups = BuildItemGroups();
            var topCategories = BuildTopCategories(itemGroups);
            var topSuppliers = BuildTopSuppliers(itemGroups);

            return new DashboardInventoryResponse
            {
                TotalInventoryValue = itemGroups.Sum(x => x.InventoryValue),
                TotalItem = itemGroups.Count,
                GeneratedAt = _tglJamDal.Now,
                CategoryBreakdown = MapBreakdown(topCategories),
                SupplierBreakdown = MapBreakdown(topSuppliers),
                TopCategories = topCategories,
                TopSuppliers = topSuppliers
            };
        }

        private sealed class ItemGroup
        {
            public string BrgId { get; set; }
            public decimal Qty { get; set; }
            public decimal InventoryValue { get; set; }
            public string CategoryName { get; set; }
            public string SupplierName { get; set; }
        }

        private List<ItemGroup> BuildItemGroups()
        {
            var rows = _stokBalanceViewDal.ListData()?.ToList()
                       ?? new List<StokBalanceView>();

            var filtered = rows
                .Where(x => x.WarehouseName != InTransitWarehouseName)
                .ToList();

            return (
                from row in filtered
                group row by row.BrgId into g
                let qty = g.Sum(x => x.Qty)
                where qty > 0
                select new ItemGroup
                {
                    BrgId = g.Key ?? string.Empty,
                    Qty = qty,
                    InventoryValue = g.Sum(x => x.Hpp * x.Qty),
                    CategoryName = NormalizeDimensionName(g.Select(x => x.KategoriName).FirstOrDefault()),
                    SupplierName = NormalizeDimensionName(g.Select(x => x.SupplierName).FirstOrDefault())
                }).ToList();
        }

        private static string NormalizeDimensionName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? UnknownLabel : name.Trim();
        }

        private static List<DashboardInventoryRankingItem> BuildTopCategories(List<ItemGroup> itemGroups)
        {
            return itemGroups
                .GroupBy(x => x.CategoryName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    Name = g.Key,
                    InventoryValue = g.Sum(x => x.InventoryValue)
                })
                .OrderByDescending(x => x.InventoryValue)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .Select((x, index) => new DashboardInventoryRankingItem
                {
                    Rank = index + 1,
                    Name = x.Name,
                    InventoryValue = x.InventoryValue
                })
                .ToList();
        }

        private static List<DashboardInventoryRankingItem> BuildTopSuppliers(List<ItemGroup> itemGroups)
        {
            return itemGroups
                .GroupBy(x => x.SupplierName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    Name = g.Key,
                    InventoryValue = g.Sum(x => x.InventoryValue)
                })
                .OrderByDescending(x => x.InventoryValue)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .Select((x, index) => new DashboardInventoryRankingItem
                {
                    Rank = index + 1,
                    Name = x.Name,
                    InventoryValue = x.InventoryValue
                })
                .ToList();
        }

        private static List<DashboardInventoryBreakdownItem> MapBreakdown(
            List<DashboardInventoryRankingItem> ranking)
            => ranking.Select(r => new DashboardInventoryBreakdownItem
            {
                Name = r.Name,
                InventoryValue = r.InventoryValue
            }).ToList();
    }
}
