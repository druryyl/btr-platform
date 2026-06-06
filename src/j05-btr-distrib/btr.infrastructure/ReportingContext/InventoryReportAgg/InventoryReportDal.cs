using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.InventoryReportAgg.Contracts;
using btr.application.ReportingContext.InventoryReportAgg.Queries;
using btr.application.SupportContext.TglJamAgg;

namespace btr.infrastructure.ReportingContext.InventoryReportAgg
{
    public class InventoryReportDal : IInventoryReportDal
    {
        private const string InTransitWarehouseName = "In-Transit";

        private readonly IStokBalanceViewDal _stokBalanceViewDal;
        private readonly ITglJamDal _tglJamDal;

        public InventoryReportDal(
            IStokBalanceViewDal stokBalanceViewDal,
            ITglJamDal tglJamDal)
        {
            _stokBalanceViewDal = stokBalanceViewDal;
            _tglJamDal = tglJamDal;
        }

        public InventoryReportResponse GetReport()
        {
            var filteredRows = _stokBalanceViewDal.ListData()?.ToList()
                               ?? new List<StokBalanceView>();

            filteredRows = filteredRows
                .Where(x => x.WarehouseName != InTransitWarehouseName)
                .ToList();

            var grouped = (
                from row in filteredRows
                group row by row.BrgId into g
                select new
                {
                    Qty = g.Sum(x => x.Qty),
                    NilaiSediaan = g.Sum(x => x.Hpp * x.Qty)
                }).ToList();

            var summary = new InventoryReportSummary
            {
                TotalInventoryValue = grouped.Sum(x => x.NilaiSediaan),
                TotalItem = grouped.Count(x => x.Qty > 0),
            };

            var rows = filteredRows
                .Where(x => x.Qty > 0)
                .OrderBy(x => x.BrgCode)
                .ThenBy(x => x.WarehouseName)
                .Select(MapRow)
                .ToList();

            return new InventoryReportResponse
            {
                GeneratedAt = _tglJamDal.Now,
                Summary = summary,
                Rows = rows,
            };
        }

        private static InventoryReportRow MapRow(StokBalanceView row)
        {
            var code = row.BrgCode?.Trim() ?? string.Empty;
            var name = row.BrgName?.Trim() ?? string.Empty;
            var itemDisplay = string.IsNullOrEmpty(code)
                ? name
                : string.IsNullOrEmpty(name)
                    ? code
                    : $"{code} — {name}";

            return new InventoryReportRow
            {
                BrgId = row.BrgId ?? string.Empty,
                ItemDisplay = itemDisplay,
                WarehouseName = row.WarehouseName ?? string.Empty,
                Qty = row.Qty,
                Hpp = row.Hpp,
                NilaiSediaan = row.Hpp * row.Qty,
            };
        }
    }
}
