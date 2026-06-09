using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardSalesAgg.Contracts;
using btr.application.ReportingContext.Shared;
using MediatR;

namespace btr.application.ReportingContext.DashboardSalesAgg.Queries
{
    public class GetDashboardSalesQuery : IRequest<DashboardSalesResponse>
    {
    }

    public class DashboardSalesResponse
    {
        public decimal TotalOmzet { get; set; }

        public decimal CompletedOmzet { get; set; }

        public decimal PipelineOmzet { get; set; }

        public int TotalFaktur { get; set; }

        public int TotalCustomer { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<DashboardSalesWeekTrendItem> WeeklyTrend { get; set; } =
            new List<DashboardSalesWeekTrendItem>();

        public decimal TotalTarget { get; set; }

        public decimal TotalAchievement { get; set; }

        public decimal? AchievementPercent { get; set; }

        public DashboardSalesTargetVsAchievement TargetVsAchievement { get; set; }

        public List<DashboardSalesRankingItem> TopSalesmanRanking { get; set; } =
            new List<DashboardSalesRankingItem>();
    }

    public class DashboardSalesTargetVsAchievement
    {
        public decimal TargetAmount { get; set; }

        public decimal AchievementAmount { get; set; }
    }

    public class DashboardSalesRankingItem
    {
        public int Rank { get; set; }

        public string SalesPersonName { get; set; }

        public string SalesPersonId { get; set; }

        public decimal CompletedOmzet { get; set; }

        public InvestigationMetadata Investigation { get; set; }
    }

    public class DashboardSalesWeekTrendItem
    {
        public DateTime WeekStart { get; set; }

        public DateTime WeekEnd { get; set; }

        public string WeekLabel { get; set; }

        public decimal RecognizedAmount { get; set; }
    }

    public class GetDashboardSalesHandler
        : IRequestHandler<GetDashboardSalesQuery, DashboardSalesResponse>
    {
        private readonly IDashboardSalesDal _dal;

        public GetDashboardSalesHandler(IDashboardSalesDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardSalesResponse> Handle(
            GetDashboardSalesQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }
}
