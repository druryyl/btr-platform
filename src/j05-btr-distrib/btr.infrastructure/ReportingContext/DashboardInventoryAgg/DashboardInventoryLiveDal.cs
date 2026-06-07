using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardInventoryAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SupportContext.TglJamAgg;

namespace btr.infrastructure.ReportingContext.DashboardInventoryAgg
{
    public class DashboardInventoryLiveDal
    {
        private readonly IStokBalanceViewDal _stokBalanceViewDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly DashboardInventoryAggregator _aggregator;

        public DashboardInventoryLiveDal(
            IStokBalanceViewDal stokBalanceViewDal,
            ITglJamDal tglJamDal,
            DashboardInventoryAggregator aggregator)
        {
            _stokBalanceViewDal = stokBalanceViewDal;
            _tglJamDal = tglJamDal;
            _aggregator = aggregator;
        }

        public DashboardInventoryResponse GetSummary()
        {
            var rows = _stokBalanceViewDal.ListData()?.ToList()
                       ?? new List<StokBalanceView>();
            var aggregate = _aggregator.Aggregate(rows, _tglJamDal.Now);
            return MapToResponse(aggregate);
        }

        private static DashboardInventoryResponse MapToResponse(DashboardInventoryAggregateResult aggregate)
        {
            var breakdown = aggregate.Breakdown ?? new List<DashboardInventoryBreakdownRow>();

            var topCategories = breakdown
                .Where(r => r.DimensionType == DashboardInventoryAggregator.DimensionCategory && r.IsTop10)
                .OrderBy(r => r.Top10Rank ?? int.MaxValue)
                .Select(r => new DashboardInventoryRankingItem
                {
                    Rank = r.Top10Rank ?? 0,
                    Name = r.Name,
                    InventoryValue = r.InventoryValue
                })
                .ToList();

            var topSuppliers = breakdown
                .Where(r => r.DimensionType == DashboardInventoryAggregator.DimensionSupplier && r.IsTop10)
                .OrderBy(r => r.Top10Rank ?? int.MaxValue)
                .Select(r => new DashboardInventoryRankingItem
                {
                    Rank = r.Top10Rank ?? 0,
                    Name = r.Name,
                    InventoryValue = r.InventoryValue
                })
                .ToList();

            return new DashboardInventoryResponse
            {
                TotalInventoryValue = aggregate.TotalInventoryValue,
                TotalItem = aggregate.TotalItem,
                GeneratedAt = aggregate.GeneratedAt,
                TopCategories = topCategories,
                TopSuppliers = topSuppliers,
                CategoryBreakdown = MapBreakdown(topCategories),
                SupplierBreakdown = MapBreakdown(topSuppliers)
            };
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
