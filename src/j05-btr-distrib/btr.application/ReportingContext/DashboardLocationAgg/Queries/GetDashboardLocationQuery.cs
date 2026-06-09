using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardLocationAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardLocationAgg.Queries
{
    public class GetDashboardLocationQuery : IRequest<DashboardLocationResponse>
    {
    }

    public class DashboardLocationResponse
    {
        public bool IsAvailable { get; set; }

        public bool IsDataFresh { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public DashboardLocationAttentionCards AttentionCards { get; set; }

        public IList<DashboardLocationRankingRow> TopWarehouseInventory { get; set; }
            = new List<DashboardLocationRankingRow>();

        public IList<DashboardLocationRankingRow> TopWarehouseAtRisk { get; set; }
            = new List<DashboardLocationRankingRow>();

        public IList<DashboardLocationRankingRow> TopWarehouseSales { get; set; }
            = new List<DashboardLocationRankingRow>();

        public IList<DashboardLocationRankingRow> TopWarehousePurchasing { get; set; }
            = new List<DashboardLocationRankingRow>();

        public IList<DashboardLocationWilayahRankingRow> TopWilayahSales { get; set; }
            = new List<DashboardLocationWilayahRankingRow>();

        public IList<DashboardLocationAttentionItem> AttentionList { get; set; }
            = new List<DashboardLocationAttentionItem>();

        public DashboardLocationNavigationLinks Navigation { get; set; }
    }

    public class DashboardLocationAttentionCards
    {
        public decimal? Top1WarehouseInventoryPercent { get; set; }

        public decimal? Top3WarehouseInventoryPercent { get; set; }

        public decimal? Top1WarehouseAtRiskPercent { get; set; }

        public decimal? Top1WarehouseSalesPercent { get; set; }

        public decimal? Top1WilayahSalesPercent { get; set; }

        public int InactiveWarehouseWithStockCount { get; set; }

        public int WarehouseNoSalesWithInventoryCount { get; set; }
    }

    public class DashboardLocationRankingRow
    {
        public int Rank { get; set; }

        public string EntityCode { get; set; }

        public string EntityName { get; set; }

        public decimal Amount { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public string ReportRoute { get; set; }
    }

    public class DashboardLocationWilayahRankingRow
    {
        public int Rank { get; set; }

        public string EntityCode { get; set; }

        public string EntityName { get; set; }

        public decimal Amount { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public string DashboardRoute { get; set; }
    }

    public class DashboardLocationAttentionItem
    {
        public string EntityType { get; set; }

        public string EntityCode { get; set; }

        public string EntityName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public decimal? ValueAmount { get; set; }

        public string ValueText { get; set; }

        public string ReportRoute { get; set; }
    }

    public class DashboardLocationNavigationLinks
    {
        public string InventoryDashboardRoute { get; set; }

        public string InventoryRiskDashboardRoute { get; set; }

        public string SalesDashboardRoute { get; set; }

        public string PurchasingDashboardRoute { get; set; }

        public string CollectionDashboardRoute { get; set; }

        public string CustomerDashboardRoute { get; set; }

        public string SalesmanDashboardRoute { get; set; }
    }

    public class GetDashboardLocationHandler : IRequestHandler<GetDashboardLocationQuery, DashboardLocationResponse>
    {
        private readonly IDashboardLocationDal _dal;

        public GetDashboardLocationHandler(IDashboardLocationDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardLocationResponse> Handle(
            GetDashboardLocationQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }
}
