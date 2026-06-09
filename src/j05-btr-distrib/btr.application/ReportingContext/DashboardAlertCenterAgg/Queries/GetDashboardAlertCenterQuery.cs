using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardAlertCenterAgg.Contracts;
using btr.application.ReportingContext.Shared;
using MediatR;

namespace btr.application.ReportingContext.DashboardAlertCenterAgg.Queries
{
    public class GetDashboardAlertCenterQuery : IRequest<DashboardAlertCenterResponse>
    {
    }

    public class DashboardAlertCenterResponse
    {
        public bool IsAvailable { get; set; }

        public bool IsDataFresh { get; set; }

        public string OverallHealthStatus { get; set; }

        public bool HasUnavailableDomain { get; set; }

        public DateTime? LastRefreshed { get; set; }

        public IList<DashboardAlertCenterPlatformAlert> PlatformAlerts { get; set; }
            = new List<DashboardAlertCenterPlatformAlert>();

        public IList<DashboardAlertCenterCategorySummary> CategorySummaries { get; set; }
            = new List<DashboardAlertCenterCategorySummary>();

        public IList<DashboardAlertCenterCategoryGroup> AlertGroups { get; set; }
            = new List<DashboardAlertCenterCategoryGroup>();

        public DashboardAlertCenterInventoryRiskSummary InventoryRiskSummary { get; set; }
            = new DashboardAlertCenterInventoryRiskSummary();

        public IList<DashboardAlertCenterConcentrationItem> Concentrations { get; set; }
            = new List<DashboardAlertCenterConcentrationItem>();

        public DashboardAlertCenterNavigationLinks Navigation { get; set; }
            = new DashboardAlertCenterNavigationLinks();
    }

    public class DashboardAlertCenterPlatformAlert
    {
        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public string ValueText { get; set; }

        public string DashboardRoute { get; set; }
    }

    public class DashboardAlertCenterCategorySummary
    {
        public string Category { get; set; }

        public int TotalCount { get; set; }

        public int DisplayedCount { get; set; }

        public bool HasMore { get; set; }
    }

    public class DashboardAlertCenterCategoryGroup
    {
        public string Category { get; set; }

        public IList<DashboardAlertCenterAlertRow> Alerts { get; set; }
            = new List<DashboardAlertCenterAlertRow>();
    }

    public class DashboardAlertCenterAlertRow
    {
        public string Category { get; set; }

        public string EntityType { get; set; }

        public string EntityCode { get; set; }

        public string EntityName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public decimal? ValueAmount { get; set; }

        public string ValueText { get; set; }

        public string AchievementBand { get; set; }

        public string DashboardRoute { get; set; }

        public string ReportRoute { get; set; }

        public string EntityFilterQuery { get; set; }

        public int SortOrder { get; set; }

        public InvestigationMetadata Investigation { get; set; }
    }

    public class DashboardAlertCenterInventoryRiskSummary
    {
        public bool IsAvailable { get; set; }

        public int DeadStockItemCount { get; set; }

        public decimal DeadStockValue { get; set; }

        public int SlowMovingItemCount { get; set; }

        public decimal SlowMovingValue { get; set; }

        public int NeverSoldItemCount { get; set; }

        public decimal NeverSoldValue { get; set; }

        public decimal? AtRiskInventoryPercent { get; set; }

        public string DashboardRoute { get; set; }
    }

    public class DashboardAlertCenterConcentrationItem
    {
        public string Label { get; set; }

        public string ValueText { get; set; }

        public decimal? ValuePercent { get; set; }

        public string DashboardRoute { get; set; }

        public string SourceDomain { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardAlertCenterNavigationLinks
    {
        public string ExecutiveDashboardRoute { get; set; }

        public string SalesDashboardRoute { get; set; }

        public string PiutangDashboardRoute { get; set; }

        public string CustomerDashboardRoute { get; set; }

        public string SalesmanDashboardRoute { get; set; }

        public string CollectionDashboardRoute { get; set; }

        public string InventoryDashboardRoute { get; set; }

        public string InventoryRiskDashboardRoute { get; set; }

        public string PurchasingDashboardRoute { get; set; }

        public string LocationDashboardRoute { get; set; }
    }

    public class GetDashboardAlertCenterHandler
        : IRequestHandler<GetDashboardAlertCenterQuery, DashboardAlertCenterResponse>
    {
        private readonly IDashboardAlertCenterDal _dal;

        public GetDashboardAlertCenterHandler(IDashboardAlertCenterDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardAlertCenterResponse> Handle(
            GetDashboardAlertCenterQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetAlerts());
        }
    }
}
