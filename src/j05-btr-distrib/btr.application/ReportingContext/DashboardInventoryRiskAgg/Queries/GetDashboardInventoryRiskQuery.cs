using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardInventoryRiskAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardInventoryRiskAgg.Queries
{
    public class GetDashboardInventoryRiskQuery : IRequest<DashboardInventoryRiskResponse>
    {
    }

    public class DashboardInventoryRiskResponse
    {
        public bool IsAvailable { get; set; }

        public bool IsDataFresh { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public DashboardInventoryRiskAttentionCards AttentionCards { get; set; }

        public IList<DashboardInventoryRiskAgingBucket> AgingBuckets { get; set; }
            = new List<DashboardInventoryRiskAgingBucket>();

        public IList<DashboardInventoryRiskBreakdownItem> CategoryRiskExposure { get; set; }
            = new List<DashboardInventoryRiskBreakdownItem>();

        public IList<DashboardInventoryRiskBreakdownItem> SupplierRiskExposure { get; set; }
            = new List<DashboardInventoryRiskBreakdownItem>();

        public IList<DashboardInventoryRiskAttentionItem> AttentionList { get; set; }
            = new List<DashboardInventoryRiskAttentionItem>();

        public DashboardInventoryRiskRankings Rankings { get; set; }

        public DashboardInventoryRiskNavigationLinks Navigation { get; set; }
    }

    public class DashboardInventoryRiskAttentionCards
    {
        public decimal TotalInventoryValue { get; set; }

        public int DeadStockItemCount { get; set; }

        public decimal DeadStockValue { get; set; }

        public int SlowMovingItemCount { get; set; }

        public decimal SlowMovingValue { get; set; }

        public decimal AtRiskInventoryPercent { get; set; }

        public bool RequiresAttention { get; set; }
    }

    public class DashboardInventoryRiskAgingBucket
    {
        public string BucketKey { get; set; }

        public string BucketLabel { get; set; }

        public decimal Amount { get; set; }

        public int ItemCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardInventoryRiskBreakdownItem
    {
        public string Name { get; set; }

        public decimal AtRiskValue { get; set; }

        public int ItemCount { get; set; }

        public decimal? PercentOfAtRisk { get; set; }
    }

    public class DashboardInventoryRiskAttentionItem
    {
        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string KategoriName { get; set; }

        public string SupplierName { get; set; }

        public int Qty { get; set; }

        public decimal InventoryValue { get; set; }

        public int? DaysSinceLastFaktur { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public string ReportRoute { get; set; }
    }

    public class DashboardInventoryRiskRankingRow
    {
        public int Rank { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string KategoriName { get; set; }

        public string SupplierName { get; set; }

        public int Qty { get; set; }

        public decimal InventoryValue { get; set; }

        public int DaysSinceLastFaktur { get; set; }

        public decimal? PercentOfAtRisk { get; set; }

        public string ReportRoute { get; set; }
    }

    public class DashboardInventoryRiskRankings
    {
        public IList<DashboardInventoryRiskRankingRow> TopDead { get; set; }
            = new List<DashboardInventoryRiskRankingRow>();

        public IList<DashboardInventoryRiskRankingRow> TopSlow { get; set; }
            = new List<DashboardInventoryRiskRankingRow>();
    }

    public class DashboardInventoryRiskNavigationLinks
    {
        public string InventoryDashboardRoute { get; set; }

        public string InventoryReportRoute { get; set; }
    }

    public class GetDashboardInventoryRiskHandler
        : IRequestHandler<GetDashboardInventoryRiskQuery, DashboardInventoryRiskResponse>
    {
        private readonly IDashboardInventoryRiskDal _dal;

        public GetDashboardInventoryRiskHandler(IDashboardInventoryRiskDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardInventoryRiskResponse> Handle(
            GetDashboardInventoryRiskQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }
}
