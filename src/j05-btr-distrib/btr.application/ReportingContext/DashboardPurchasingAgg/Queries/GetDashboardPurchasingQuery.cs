using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardPurchasingAgg.Contracts;
using btr.application.ReportingContext.Shared;
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

        public bool IsManagementAvailable { get; set; }

        public bool IsDataFresh { get; set; }

        public DashboardPurchasingAttentionCards AttentionCards { get; set; }

        public DashboardPurchasingSummaryRow Summary { get; set; }

        public List<DashboardPurchasingAttentionItem> AttentionList { get; set; } =
            new List<DashboardPurchasingAttentionItem>();

        public List<DashboardPurchasingPrincipalExposureItem> PrincipalExposure { get; set; } =
            new List<DashboardPurchasingPrincipalExposureItem>();

        public DashboardPurchasingNavigationLinks Navigation { get; set; }
    }

    public class DashboardPurchasingAttentionCards
    {
        public DashboardPurchasingAttentionCardGroup PostingExposure { get; set; }

        public DashboardPurchasingAttentionCardGroup PrincipalDependency { get; set; }

        public DashboardPurchasingAttentionCardGroup PurchasingPace { get; set; }

        public DashboardPurchasingAttentionCardGroup InventoryCrossRisk { get; set; }
    }

    public class DashboardPurchasingAttentionCardGroup
    {
        public bool RequiresAttention { get; set; }

        public Dictionary<string, string> Metrics { get; set; } = new Dictionary<string, string>();
    }

    public class DashboardPurchasingSummaryRow
    {
        public decimal GrandTotalPurchase { get; set; }

        public int TotalInvoice { get; set; }

        public decimal? PostedPercent { get; set; }

        public decimal PendingPostingValue { get; set; }

        public int QualifiedBacklogCount { get; set; }

        public decimal QualifiedBacklogValue { get; set; }
    }

    public class DashboardPurchasingAttentionItem
    {
        public string EntityType { get; set; }

        public string EntityName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public decimal? ValueAmount { get; set; }

        public string ValueText { get; set; }

        public string ReportRoute { get; set; }

        public string ProfileRoute { get; set; }

        public InvestigationMetadata Investigation { get; set; }
    }

    public class DashboardPurchasingPrincipalExposureItem
    {
        public int Rank { get; set; }

        public string SupplierCode { get; set; }

        public string PrincipalName { get; set; }

        public decimal MtdPurchaseAmount { get; set; }

        public decimal? PercentOfPurchase { get; set; }

        public decimal? InventoryValue { get; set; }

        public decimal? PercentOfInventory { get; set; }

        public decimal? AtRiskValue { get; set; }

        public decimal? PercentOfAtRisk { get; set; }

        public bool IsCompoundDependency { get; set; }

        public bool IsInventoryNoPurchase { get; set; }

        public string ReportRoute { get; set; }

        public string ProfileRoute { get; set; }

        public InvestigationMetadata Investigation { get; set; }
    }

    public class DashboardPurchasingNavigationLinks
    {
        public string PurchasingReportRoute { get; set; }

        public string InventoryDashboardRoute { get; set; }

        public string InventoryRiskDashboardRoute { get; set; }
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

        public InvestigationMetadata Investigation { get; set; }
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
