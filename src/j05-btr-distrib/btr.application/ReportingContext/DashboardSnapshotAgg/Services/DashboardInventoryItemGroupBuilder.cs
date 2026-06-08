using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class DashboardInventoryItemGroupBuilder
    {
        public const string InTransitWarehouseName = "In-Transit";
        public const string UnknownLabel = "Unknown";

        public static List<DashboardInventoryItemGroup> BuildItemGroups(IEnumerable<StokBalanceView> rows)
        {
            var filtered = (rows ?? Enumerable.Empty<StokBalanceView>())
                .Where(x => x.WarehouseName != InTransitWarehouseName)
                .ToList();

            return (
                from row in filtered
                group row by row.BrgId into g
                let qty = g.Sum(x => x.Qty)
                where qty > 0
                let first = g.FirstOrDefault()
                select new DashboardInventoryItemGroup
                {
                    BrgId = g.Key ?? string.Empty,
                    BrgCode = first?.BrgCode ?? string.Empty,
                    BrgName = first?.BrgName ?? string.Empty,
                    Qty = qty,
                    InventoryValue = g.Sum(x => x.Hpp * x.Qty),
                    CategoryName = NormalizeDimensionName(g.Select(x => x.KategoriName).FirstOrDefault()),
                    SupplierName = NormalizeDimensionName(g.Select(x => x.SupplierName).FirstOrDefault())
                }).ToList();
        }

        public static string NormalizeDimensionName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? UnknownLabel : name.Trim();
        }
    }

    public sealed class DashboardInventoryItemGroup
    {
        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public decimal Qty { get; set; }

        public decimal InventoryValue { get; set; }

        public string CategoryName { get; set; }

        public string SupplierName { get; set; }
    }
}
