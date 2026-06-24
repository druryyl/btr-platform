using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardCustomerPortfolioAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardCustomerPortfolioAgg.Queries
{
    public class GetDashboardCustomerPortfolioQuery : IRequest<DashboardCustomerPortfolioResponse>
    {
    }

    public class DashboardCustomerPortfolioResponse
    {
        public bool IsAvailable { get; set; }

        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public DashboardCustomerPortfolioKpiDto Kpi { get; set; }

        public IReadOnlyList<DashboardCustomerPortfolioLifecycleDistDto> LifecycleDistribution { get; set; }
            = new List<DashboardCustomerPortfolioLifecycleDistDto>();

        public IReadOnlyList<DashboardCustomerPortfolioTierDistDto> TierDistribution { get; set; }
            = new List<DashboardCustomerPortfolioTierDistDto>();

        public IReadOnlyList<DashboardCustomerPortfolioActionDistDto> ActionDistribution { get; set; }
            = new List<DashboardCustomerPortfolioActionDistDto>();

        public IReadOnlyList<DashboardCustomerPortfolioPriorityDto> PriorityQueue { get; set; }
            = new List<DashboardCustomerPortfolioPriorityDto>();

        public IReadOnlyList<DashboardCustomerPortfolioCustomerDto> Customers { get; set; }
            = new List<DashboardCustomerPortfolioCustomerDto>();

        public IReadOnlyList<DashboardCustomerPortfolioConcentrationDto> TopOmzet { get; set; }
            = new List<DashboardCustomerPortfolioConcentrationDto>();

        public IReadOnlyList<DashboardCustomerPortfolioConcentrationDto> TopPiutang { get; set; }
            = new List<DashboardCustomerPortfolioConcentrationDto>();

        public IReadOnlyList<DashboardCustomerPortfolioWilayahDto> WilayahBreakdown { get; set; }
            = new List<DashboardCustomerPortfolioWilayahDto>();
    }

    public class DashboardCustomerPortfolioKpiDto
    {
        public decimal PortfolioHealthScore { get; set; }

        public decimal PortfolioHealthyPercent { get; set; }

        public int TotalCustomerCount { get; set; }

        public int AttentionCustomerCount { get; set; }

        public int StrategicCustomerCount { get; set; }

        public int StrategicAtRiskCount { get; set; }

        public int CustomersAtRiskCount { get; set; }

        public decimal WorkingCapitalTiedAmount { get; set; }

        public decimal TotalMtdOmzet { get; set; }

        public decimal TotalOpenBalance { get; set; }

        public int NeverPurchasedCount { get; set; }

        public int DormantCount { get; set; }

        public int DecliningCount { get; set; }

        public string ExecutiveSummaryText { get; set; }

        public string ValueDisclaimerText { get; set; }
    }

    public class DashboardCustomerPortfolioLifecycleDistDto
    {
        public string LifecycleStage { get; set; }

        public string LifecycleLabel { get; set; }

        public int CustomerCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCustomerPortfolioTierDistDto
    {
        public string PortfolioTier { get; set; }

        public string TierLabel { get; set; }

        public int CustomerCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCustomerPortfolioActionDistDto
    {
        public string PrimaryActionKey { get; set; }

        public string PrimaryActionLabel { get; set; }

        public int CustomerCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCustomerPortfolioPriorityDto
    {
        public int SortOrder { get; set; }

        public int PortfolioPriorityScore { get; set; }

        public string CustomerKey { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string Klasifikasi { get; set; }

        public string LifecycleStage { get; set; }

        public string LifecycleLabel { get; set; }

        public string PortfolioTier { get; set; }

        public string TierLabel { get; set; }

        public string PrimaryActionKey { get; set; }

        public string PrimaryActionLabel { get; set; }

        public string ActionOwner { get; set; }

        public string ActionReasonText { get; set; }

        public string TriggeredRuleIds { get; set; }

        public decimal MtdOmzet { get; set; }

        public decimal OpenBalance { get; set; }

        public decimal? OverdueBalance { get; set; }

        public string M29Category { get; set; }

        public string SalesPersonName { get; set; }

        public decimal? SalesmanAchievementPercent { get; set; }

        public bool SalesmanHighPiutangExposure { get; set; }

        public bool IsAttention { get; set; }

        public string M30LinkRoute { get; set; }

        public string CustomerReportRoute { get; set; }

        public string DrillDownRouteM17 { get; set; }

        public string DrillDownRouteM29 { get; set; }

        public string ProfileRoute { get; set; }
    }

    public class DashboardCustomerPortfolioCustomerDto
    {
        public int SortOrder { get; set; }

        public string CustomerKey { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string Klasifikasi { get; set; }

        public string LifecycleStage { get; set; }

        public string LifecycleLabel { get; set; }

        public string PortfolioTier { get; set; }

        public string TierLabel { get; set; }

        public string PrimaryActionKey { get; set; }

        public string PrimaryActionLabel { get; set; }

        public string ActionOwner { get; set; }

        public string ActionReasonText { get; set; }

        public string TriggeredRuleIds { get; set; }

        public decimal MtdOmzet { get; set; }

        public decimal OpenBalance { get; set; }

        public decimal? OverdueBalance { get; set; }

        public int FakturCount6Mo { get; set; }

        public bool IsActiveMtd { get; set; }

        public DateTime? LastPurchaseDate { get; set; }

        public DateTime? FirstPurchaseDate { get; set; }

        public string M29Category { get; set; }

        public string M29PrimarySignalKey { get; set; }

        public string SalesPersonName { get; set; }

        public decimal? SalesmanAchievementPercent { get; set; }

        public bool SalesmanHighPiutangExposure { get; set; }

        public bool IsAttention { get; set; }

        public int PortfolioPriorityScore { get; set; }

        public string M30LinkRoute { get; set; }

        public string CustomerReportRoute { get; set; }

        public string DrillDownRouteM17 { get; set; }

        public string DrillDownRouteM29 { get; set; }

        public string ValueDisclaimer { get; set; }
    }

    public class DashboardCustomerPortfolioConcentrationDto
    {
        public int SortOrder { get; set; }

        public int Rank { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal Amount { get; set; }

        public decimal? PercentOfTotal { get; set; }
    }

    public class DashboardCustomerPortfolioWilayahDto
    {
        public int SortOrder { get; set; }

        public string WilayahName { get; set; }

        public int CustomerCount { get; set; }

        public int AttentionCustomerCount { get; set; }
    }

    public class GetDashboardCustomerPortfolioHandler
        : IRequestHandler<GetDashboardCustomerPortfolioQuery, DashboardCustomerPortfolioResponse>
    {
        private readonly IDashboardCustomerPortfolioDal _dal;

        public GetDashboardCustomerPortfolioHandler(IDashboardCustomerPortfolioDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardCustomerPortfolioResponse> Handle(
            GetDashboardCustomerPortfolioQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetCurrent());
        }
    }
}
