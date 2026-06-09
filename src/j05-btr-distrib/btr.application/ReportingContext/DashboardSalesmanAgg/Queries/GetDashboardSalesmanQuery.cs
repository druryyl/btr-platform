using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardSalesmanAgg.Contracts;
using btr.application.ReportingContext.Shared;
using MediatR;

namespace btr.application.ReportingContext.DashboardSalesmanAgg.Queries
{
    public class GetDashboardSalesmanQuery : IRequest<DashboardSalesmanResponse>
    {
    }

    public class DashboardSalesmanResponse
    {
        public bool IsAvailable { get; set; }

        public bool IsDataFresh { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DashboardSalesmanAttentionCards AttentionCards { get; set; }

        public IList<DashboardSalesmanAttentionItem> AttentionList { get; set; }
            = new List<DashboardSalesmanAttentionItem>();

        public DashboardSalesmanPerformanceRankings PerformanceRankings { get; set; }

        public DashboardSalesmanExposureRankings ExposureRankings { get; set; }

        public DashboardSalesmanSegmentationSummary Segmentation { get; set; }

        public DashboardSalesmanNavigationLinks Navigation { get; set; }
    }

    public class DashboardSalesmanAttentionCards
    {
        public int BelowTargetCount { get; set; }

        public int NoTargetCount { get; set; }

        public int HighOverdueExposureCount { get; set; }

        public int HighPiutangExposureCount { get; set; }

        public int CustomerConcentrationCount { get; set; }

        public int DormantPortfolioCount { get; set; }

        public decimal? TopOmzetSalesmanPercent { get; set; }

        public decimal? TopPiutangSalesmanPercent { get; set; }

        public bool PerformanceRequiresAttention { get; set; }

        public bool CollectionRequiresAttention { get; set; }

        public bool PortfolioRequiresAttention { get; set; }
    }

    public class DashboardSalesmanAttentionItem
    {
        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public decimal? ValueAmount { get; set; }

        public string ValueText { get; set; }

        public string WilayahName { get; set; }

        public string ReportRoute { get; set; }

        public bool RequiresAttention { get; set; }

        public InvestigationMetadata Investigation { get; set; }
    }

    public class DashboardSalesmanRankingRow
    {
        public int Rank { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public decimal Amount { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public decimal? AchievementPercent { get; set; }

        public decimal? TargetAmount { get; set; }

        public string ReportRoute { get; set; }

        public InvestigationMetadata Investigation { get; set; }
    }

    public class DashboardSalesmanPerformanceRankings
    {
        public IList<DashboardSalesmanRankingRow> TopOmzet { get; set; }
            = new List<DashboardSalesmanRankingRow>();

        public IList<DashboardSalesmanRankingRow> TopAchievement { get; set; }
            = new List<DashboardSalesmanRankingRow>();
    }

    public class DashboardSalesmanExposureRankings
    {
        public IList<DashboardSalesmanRankingRow> TopPiutang { get; set; }
            = new List<DashboardSalesmanRankingRow>();
    }

    public class DashboardSalesmanSegmentRow
    {
        public string SegmentType { get; set; }

        public string SegmentLabel { get; set; }

        public int SalesmanCount { get; set; }

        public int ActiveCount { get; set; }

        public int InactiveCount { get; set; }
    }

    public class DashboardSalesmanSegmentationSummary
    {
        public IList<DashboardSalesmanSegmentRow> ByWilayah { get; set; }
            = new List<DashboardSalesmanSegmentRow>();

        public IList<DashboardSalesmanSegmentRow> BySegment { get; set; }
            = new List<DashboardSalesmanSegmentRow>();

        public DashboardSalesmanSegmentRow ActiveSummary { get; set; }

        public DashboardSalesmanSegmentRow InactiveSummary { get; set; }
    }

    public class DashboardSalesmanNavigationLinks
    {
        public string SalesDashboardRoute { get; set; }

        public string PiutangDashboardRoute { get; set; }

        public string SalesReportRoute { get; set; }

        public string PiutangReportRoute { get; set; }
    }

    public class GetDashboardSalesmanHandler
        : IRequestHandler<GetDashboardSalesmanQuery, DashboardSalesmanResponse>
    {
        private readonly IDashboardSalesmanDal _dal;

        public GetDashboardSalesmanHandler(IDashboardSalesmanDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardSalesmanResponse> Handle(
            GetDashboardSalesmanQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }
}
