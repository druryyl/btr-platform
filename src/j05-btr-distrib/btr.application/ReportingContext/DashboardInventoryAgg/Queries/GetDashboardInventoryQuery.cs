using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardInventoryAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardInventoryAgg.Queries
{
    public class GetDashboardInventoryQuery : IRequest<DashboardInventoryResponse>
    {
    }

    public class DashboardInventoryResponse
    {
        public decimal TotalInventoryValue { get; set; }

        public int TotalItem { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<DashboardInventoryBreakdownItem> CategoryBreakdown { get; set; }

        public List<DashboardInventoryBreakdownItem> SupplierBreakdown { get; set; }

        public List<DashboardInventoryRankingItem> TopCategories { get; set; }

        public List<DashboardInventoryRankingItem> TopSuppliers { get; set; }
    }

    public class DashboardInventoryBreakdownItem
    {
        public string Name { get; set; }

        public decimal InventoryValue { get; set; }
    }

    public class DashboardInventoryRankingItem
    {
        public int Rank { get; set; }

        public string Name { get; set; }

        public decimal InventoryValue { get; set; }
    }

    public class GetDashboardInventoryHandler
        : IRequestHandler<GetDashboardInventoryQuery, DashboardInventoryResponse>
    {
        private readonly IDashboardInventoryDal _dal;

        public GetDashboardInventoryHandler(IDashboardInventoryDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardInventoryResponse> Handle(
            GetDashboardInventoryQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }
}
