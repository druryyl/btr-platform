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

        public decimal OverduePiutang { get; set; }

        public decimal AgingOver90Amount { get; set; }

        public decimal? AgingOver90Percent { get; set; }

        public decimal? Top10CustomerConcentrationPercent { get; set; }

        public decimal? Top20CustomerConcentrationPercent { get; set; }

        public List<DashboardPiutangAgingBucket> AgingBuckets { get; set; }
            = new List<DashboardPiutangAgingBucket>();

        public List<DashboardPiutangTopCustomerRiskRow> TopCustomerRisk { get; set; }
            = new List<DashboardPiutangTopCustomerRiskRow>();
    }

    public class DashboardPiutangAgingBucket
    {
        public string BucketKey { get; set; }

        public string BucketLabel { get; set; }

        public decimal Amount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardPiutangTopCustomerRiskRow
    {
        public int Rank { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal TotalPiutang { get; set; }

        public decimal CurrentAmount { get; set; }

        public decimal Aging30Amount { get; set; }

        public decimal Aging60Amount { get; set; }

        public decimal Aging90Amount { get; set; }

        public decimal AgingOver90Amount { get; set; }

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
