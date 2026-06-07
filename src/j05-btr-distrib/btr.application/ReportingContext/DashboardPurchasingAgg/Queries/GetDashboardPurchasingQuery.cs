using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardPurchasingAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardPurchasingAgg.Queries
{
    public class GetDashboardPurchasingQuery : IRequest<DashboardPurchasingResponse>
    {
    }

    public class DashboardPurchasingResponse
    {
        public decimal GrandTotalPurchase { get; set; }

        public int TotalInvoice { get; set; }

        public int PendingPostingInvoiceCount { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<DashboardPurchasingWeekTrendItem> WeeklyTrend { get; set; } =
            new List<DashboardPurchasingWeekTrendItem>();

        public List<DashboardPurchasingPostingStatusItem> PostingStatusBreakdown { get; set; } =
            new List<DashboardPurchasingPostingStatusItem>();

        public List<DashboardPurchasingRankingItem> TopPrincipalRanking { get; set; } =
            new List<DashboardPurchasingRankingItem>();
    }

    public class DashboardPurchasingWeekTrendItem
    {
        public DateTime WeekStart { get; set; }

        public DateTime WeekEnd { get; set; }

        public string WeekLabel { get; set; }

        public decimal PurchaseAmount { get; set; }
    }

    public class DashboardPurchasingPostingStatusItem
    {
        public string StatusKey { get; set; }

        public string StatusLabel { get; set; }

        public int SortOrder { get; set; }

        public decimal PurchaseAmount { get; set; }
    }

    public class DashboardPurchasingRankingItem
    {
        public int Rank { get; set; }

        public string PrincipalName { get; set; }

        public decimal PurchaseAmount { get; set; }
    }

    public class GetDashboardPurchasingHandler
        : IRequestHandler<GetDashboardPurchasingQuery, DashboardPurchasingResponse>
    {
        private readonly IDashboardPurchasingDal _dal;

        public GetDashboardPurchasingHandler(IDashboardPurchasingDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardPurchasingResponse> Handle(
            GetDashboardPurchasingQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }
}
