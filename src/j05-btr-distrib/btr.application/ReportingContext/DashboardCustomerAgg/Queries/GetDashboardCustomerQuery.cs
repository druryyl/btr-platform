using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardCustomerAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardCustomerAgg.Queries
{
    public class GetDashboardCustomerQuery : IRequest<DashboardCustomerResponse>
    {
    }

    public class DashboardCustomerResponse
    {
        public bool IsAvailable { get; set; }

        public bool IsDataFresh { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DashboardCustomerAttentionCards AttentionCards { get; set; }

        public IList<DashboardCustomerAttentionItem> AttentionList { get; set; }
            = new List<DashboardCustomerAttentionItem>();

        public DashboardCustomerRankings Rankings { get; set; }

        public DashboardCustomerSegmentationSummary Segmentation { get; set; }

        public DashboardCustomerNavigationLinks Navigation { get; set; }
    }

    public class DashboardCustomerAttentionCards
    {
        public int OverdueCustomerCount { get; set; }

        public decimal AgingOver90Amount { get; set; }

        public bool CollectionRequiresAttention { get; set; }

        public decimal? TopOmzetCustomerPercent { get; set; }

        public decimal? TopPiutangCustomerPercent { get; set; }

        public int ActiveCustomerCount { get; set; }

        public int DormantCustomerCount { get; set; }

        public bool InactivityRequiresAttention { get; set; }

        public int PlafondBreachCount { get; set; }

        public int SuspendedWithSalesCount { get; set; }

        public bool CreditRequiresAttention { get; set; }
    }

    public class DashboardCustomerAttentionItem
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public decimal? ValueAmount { get; set; }

        public string ValueText { get; set; }

        public string WilayahName { get; set; }

        public string ReportRoute { get; set; }

        public bool RequiresAttention { get; set; }
    }

    public class DashboardCustomerRankingRow
    {
        public int Rank { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal Amount { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public string ReportRoute { get; set; }
    }

    public class DashboardCustomerRankings
    {
        public IList<DashboardCustomerRankingRow> TopOmzet { get; set; }
            = new List<DashboardCustomerRankingRow>();

        public IList<DashboardCustomerRankingRow> TopPiutang { get; set; }
            = new List<DashboardCustomerRankingRow>();
    }

    public class DashboardCustomerSegmentRow
    {
        public string SegmentType { get; set; }

        public string SegmentLabel { get; set; }

        public int CustomerCount { get; set; }

        public int ActiveCount { get; set; }

        public int DormantCount { get; set; }
    }

    public class DashboardCustomerSegmentationSummary
    {
        public IList<DashboardCustomerSegmentRow> ByKlasifikasi { get; set; }
            = new List<DashboardCustomerSegmentRow>();

        public IList<DashboardCustomerSegmentRow> ByWilayah { get; set; }
            = new List<DashboardCustomerSegmentRow>();

        public DashboardCustomerSegmentRow ActiveSummary { get; set; }

        public DashboardCustomerSegmentRow DormantSummary { get; set; }
    }

    public class DashboardCustomerNavigationLinks
    {
        public string SalesDashboardRoute { get; set; }

        public string PiutangDashboardRoute { get; set; }

        public string SalesReportRoute { get; set; }

        public string PiutangReportRoute { get; set; }
    }

    public class GetDashboardCustomerHandler
        : IRequestHandler<GetDashboardCustomerQuery, DashboardCustomerResponse>
    {
        private readonly IDashboardCustomerDal _dal;

        public GetDashboardCustomerHandler(IDashboardCustomerDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardCustomerResponse> Handle(
            GetDashboardCustomerQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }
}
