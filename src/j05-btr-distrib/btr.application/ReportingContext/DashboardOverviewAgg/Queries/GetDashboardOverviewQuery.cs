using System;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardOverviewAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardOverviewAgg.Queries
{
    public class GetDashboardOverviewQuery : IRequest<DashboardOverviewResponse>
    {
    }

    public class DashboardOverviewResponse
    {
        public DashboardOverviewSalesSection Sales { get; set; }

        public DashboardOverviewPiutangSection Piutang { get; set; }

        public DashboardOverviewInventorySection Inventory { get; set; }

        public bool HasUnavailableDomain { get; set; }
    }

    public class DashboardOverviewSalesSection
    {
        public decimal TotalOmzet { get; set; }

        public int TotalFaktur { get; set; }

        public int TotalCustomer { get; set; }

        public DateTime GeneratedAt { get; set; }
    }

    public class DashboardOverviewPiutangSection
    {
        public decimal TotalPiutang { get; set; }

        public int TotalCustomer { get; set; }

        public DateTime GeneratedAt { get; set; }
    }

    public class DashboardOverviewInventorySection
    {
        public decimal TotalInventoryValue { get; set; }

        public int TotalItem { get; set; }

        public DateTime GeneratedAt { get; set; }
    }

    public class GetDashboardOverviewHandler
        : IRequestHandler<GetDashboardOverviewQuery, DashboardOverviewResponse>
    {
        private readonly IDashboardOverviewDal _dal;

        public GetDashboardOverviewHandler(IDashboardOverviewDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardOverviewResponse> Handle(
            GetDashboardOverviewQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetOverview());
        }
    }
}
