using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardCustomerRiskForecastAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardCustomerRiskForecastAgg.Queries
{
    public class GetDashboardCustomerRiskForecastQuery : IRequest<DashboardCustomerRiskForecastResponse>
    {
    }

    public class DashboardCustomerRiskForecastResponse
    {
        public bool IsAvailable { get; set; }

        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public DashboardCustomerRiskForecastKpiDto Kpi { get; set; }

        public IReadOnlyList<DashboardCustomerRiskForecastDistDto> CategoryDistribution { get; set; }
            = new List<DashboardCustomerRiskForecastDistDto>();

        public IReadOnlyList<DashboardCustomerRiskForecastWilayahDto> TopWilayah { get; set; }
            = new List<DashboardCustomerRiskForecastWilayahDto>();

        public IReadOnlyList<DashboardCustomerRiskForecastSignalMixDto> SignalMix { get; set; }
            = new List<DashboardCustomerRiskForecastSignalMixDto>();

        public IReadOnlyList<DashboardCustomerRiskForecastCustomerDto> TopCustomers { get; set; }
            = new List<DashboardCustomerRiskForecastCustomerDto>();

        public IReadOnlyList<DashboardCustomerRiskForecastAttentionDto> AttentionList { get; set; }
            = new List<DashboardCustomerRiskForecastAttentionDto>();

        public IReadOnlyList<DashboardCustomerRiskForecastRecommendationDto> Recommendations { get; set; }
            = new List<DashboardCustomerRiskForecastRecommendationDto>();
    }

    public class DashboardCustomerRiskForecastKpiDto
    {
        public int HorizonDays { get; set; }

        public int CustomersForecastedAtRisk { get; set; }

        public int HighRiskCustomerCount { get; set; }

        public int CriticalCustomerCount { get; set; }

        public decimal ElevatedRiskReceivable { get; set; }

        public decimal? ElevatedRiskReceivablePercent { get; set; }

        public decimal PortfolioHealthScore { get; set; }

        public decimal TotalPiutang { get; set; }

        public string ForecastConfidence { get; set; }

        public int PaymentDelaySignalCount { get; set; }

        public int CreditLimitSignalCount { get; set; }

        public int InactivitySignalCount { get; set; }

        public int PurchaseDeclineSignalCount { get; set; }

        public int CollectionRiskSignalCount { get; set; }

        public int HealthyCount { get; set; }

        public int WatchCount { get; set; }

        public int AttentionCount { get; set; }

        public int HighRiskCount { get; set; }

        public int CriticalCount { get; set; }

        public string ExecutiveSummaryText { get; set; }
    }

    public class DashboardCustomerRiskForecastDistDto
    {
        public string Category { get; set; }

        public string CategoryLabel { get; set; }

        public int CustomerCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCustomerRiskForecastWilayahDto
    {
        public string WilayahName { get; set; }

        public int ElevatedRiskCustomerCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCustomerRiskForecastSignalMixDto
    {
        public string SignalFamilyKey { get; set; }

        public string SignalFamilyLabel { get; set; }

        public int CustomerCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCustomerRiskForecastCustomerDto
    {
        public int SortOrder { get; set; }

        public int RiskPriorityScore { get; set; }

        public string Category { get; set; }

        public string CategoryLabel { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string SalesPersonName { get; set; }

        public decimal OpenBalance { get; set; }

        public decimal OverdueBalance { get; set; }

        public decimal DueWithinHorizon { get; set; }

        public decimal Plafond { get; set; }

        public decimal ProjectedOpenBalance { get; set; }

        public decimal MtdOmzet { get; set; }

        public decimal PriorMonthOmzet { get; set; }

        public decimal? DeclineRatio { get; set; }

        public int? DaysSinceLastFaktur { get; set; }

        public decimal? AvgPaymentLagDays { get; set; }

        public string PrimarySignalKey { get; set; }

        public string PrimarySignalLabel { get; set; }

        public string ReasonText { get; set; }

        public string RecommendationKey { get; set; }

        public string RecommendationLabel { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class DashboardCustomerRiskForecastAttentionDto
    {
        public int SortOrder { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public string Severity { get; set; }

        public decimal? Amount { get; set; }

        public string HorizonText { get; set; }

        public string RuleId { get; set; }

        public string Explanation { get; set; }

        public string ReportRoute { get; set; }
    }

    public class DashboardCustomerRiskForecastRecommendationDto
    {
        public int SortOrder { get; set; }

        public string RecommendationKey { get; set; }

        public string RecommendationLabel { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string Category { get; set; }

        public string ReasonText { get; set; }

        public string RuleId { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class GetDashboardCustomerRiskForecastHandler
        : IRequestHandler<GetDashboardCustomerRiskForecastQuery, DashboardCustomerRiskForecastResponse>
    {
        private readonly IDashboardCustomerRiskForecastDal _dal;

        public GetDashboardCustomerRiskForecastHandler(IDashboardCustomerRiskForecastDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardCustomerRiskForecastResponse> Handle(
            GetDashboardCustomerRiskForecastQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetCurrent());
        }
    }
}
