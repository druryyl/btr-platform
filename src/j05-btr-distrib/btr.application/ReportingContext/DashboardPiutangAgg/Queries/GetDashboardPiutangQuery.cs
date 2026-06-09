using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardPiutangAgg.Contracts;
using btr.application.ReportingContext.Shared;
using MediatR;

namespace btr.application.ReportingContext.DashboardPiutangAgg.Queries
{
    public class GetDashboardPiutangQuery : IRequest<DashboardPiutangResponse>
    {
    }

    public class DashboardPiutangResponse
    {
        public decimal TotalPiutang { get; set; }

        public int TotalCustomer { get; set; }

        public DateTime GeneratedAt { get; set; }

        public int OverdueCustomer { get; set; }

        public List<DashboardPiutangAgingBucket> AgingBuckets { get; set; }
            = new List<DashboardPiutangAgingBucket>();

        public List<DashboardPiutangTopCustomer> TopCustomers { get; set; }
            = new List<DashboardPiutangTopCustomer>();
    }

    public class DashboardPiutangAgingBucket
    {
        public string BucketKey { get; set; }

        public string BucketLabel { get; set; }

        public decimal Amount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardPiutangTopCustomer
    {
        public int Rank { get; set; }

        public string CustomerName { get; set; }

        public string CustomerCode { get; set; }

        public decimal OutstandingBalance { get; set; }

        public InvestigationMetadata Investigation { get; set; }
    }

    public class GetDashboardPiutangHandler
        : IRequestHandler<GetDashboardPiutangQuery, DashboardPiutangResponse>
    {
        private readonly IDashboardPiutangDal _dal;

        public GetDashboardPiutangHandler(IDashboardPiutangDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardPiutangResponse> Handle(
            GetDashboardPiutangQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }
}
