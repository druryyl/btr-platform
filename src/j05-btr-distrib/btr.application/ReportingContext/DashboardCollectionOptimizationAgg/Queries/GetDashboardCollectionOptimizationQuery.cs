using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardCollectionOptimizationAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardCollectionOptimizationAgg.Queries
{
    public class GetDashboardCollectionOptimizationQuery : IRequest<DashboardCollectionOptimizationResponse>
    {
    }

    public class DashboardCollectionOptimizationResponse
    {
        public bool IsAvailable { get; set; }

        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public DashboardCollectionOptimizationKpiDto Kpi { get; set; }

        public IReadOnlyList<DashboardCollectionOptimizationActionDistDto> ActionDistribution { get; set; }
            = new List<DashboardCollectionOptimizationActionDistDto>();

        public IReadOnlyList<DashboardCollectionOptimizationWorkloadDto> Workload { get; set; }
            = new List<DashboardCollectionOptimizationWorkloadDto>();

        public IReadOnlyList<DashboardCollectionOptimizationPriorityDto> PriorityQueue { get; set; }
            = new List<DashboardCollectionOptimizationPriorityDto>();

        public IReadOnlyList<DashboardCollectionOptimizationQueueDto> SpecializedQueues { get; set; }
            = new List<DashboardCollectionOptimizationQueueDto>();

        public IReadOnlyList<DashboardCollectionOptimizationImpactDto> TopImpactOpportunities { get; set; }
            = new List<DashboardCollectionOptimizationImpactDto>();
    }

    public class DashboardCollectionOptimizationKpiDto
    {
        public int ActionsTodayCount { get; set; }

        public int ImmediateCollectionCount { get; set; }

        public int ProactiveReminderCount { get; set; }

        public int CreditReviewCount { get; set; }

        public int SalesRecoveryCount { get; set; }

        public int EscalateManagementCount { get; set; }

        public decimal CollectionImpactTotal { get; set; }

        public decimal ImmediateImpactTotal { get; set; }

        public decimal OverdueExposure { get; set; }

        public decimal DueWithin7Days { get; set; }

        public decimal? RecoveryVsBillingPercent { get; set; }

        public int DeferNoActionCount { get; set; }

        public string PlanningConfidence { get; set; }

        public string ExecutiveSummaryText { get; set; }
    }

    public class DashboardCollectionOptimizationActionDistDto
    {
        public string ActionCategoryKey { get; set; }

        public string ActionCategoryLabel { get; set; }

        public int CustomerCount { get; set; }

        public decimal ImpactTotal { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCollectionOptimizationWorkloadDto
    {
        public string WorkloadType { get; set; }

        public string EntityKey { get; set; }

        public string EntityLabel { get; set; }

        public int ActionCount { get; set; }

        public int ImmediateCount { get; set; }

        public decimal ImpactTotal { get; set; }

        public decimal OverdueExposure { get; set; }

        public bool IsHotspot { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCollectionOptimizationPriorityDto
    {
        public int SortOrder { get; set; }

        public int CollectionPriorityScore { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string SalesPersonName { get; set; }

        public string Klasifikasi { get; set; }

        public string ActionCategoryKey { get; set; }

        public string ActionCategoryLabel { get; set; }

        public string RecommendedActionKey { get; set; }

        public string RecommendedActionLabel { get; set; }

        public string ActionOwner { get; set; }

        public decimal OpenBalance { get; set; }

        public decimal OverdueBalance { get; set; }

        public decimal DueWithin7Days { get; set; }

        public decimal CollectionImpactAmount { get; set; }

        public string M29Category { get; set; }

        public string M29RecommendationKey { get; set; }

        public string M29PrimarySignalKey { get; set; }

        public int? MinDaysUntilDue { get; set; }

        public decimal? CreditUtilizationPercent { get; set; }

        public string SelectionReasonText { get; set; }

        public string PriorityReasonText { get; set; }

        public string ActionReasonText { get; set; }

        public string TriggeredRuleIds { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class DashboardCollectionOptimizationQueueDto
    {
        public string QueueKey { get; set; }

        public int SortOrder { get; set; }

        public int CollectionPriorityScore { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string SalesPersonName { get; set; }

        public string ActionCategoryKey { get; set; }

        public string ActionCategoryLabel { get; set; }

        public string RecommendedActionKey { get; set; }

        public string RecommendedActionLabel { get; set; }

        public string ActionOwner { get; set; }

        public decimal OverdueBalance { get; set; }

        public decimal DueWithin7Days { get; set; }

        public decimal CollectionImpactAmount { get; set; }

        public string M29Category { get; set; }

        public string QueueReasonText { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class DashboardCollectionOptimizationImpactDto
    {
        public int SortOrder { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string SalesPersonName { get; set; }

        public string ActionCategoryKey { get; set; }

        public string ActionCategoryLabel { get; set; }

        public decimal CollectionImpactAmount { get; set; }

        public decimal OverdueBalance { get; set; }

        public decimal DueWithin7Days { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class GetDashboardCollectionOptimizationHandler
        : IRequestHandler<GetDashboardCollectionOptimizationQuery, DashboardCollectionOptimizationResponse>
    {
        private readonly IDashboardCollectionOptimizationDal _dal;

        public GetDashboardCollectionOptimizationHandler(IDashboardCollectionOptimizationDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardCollectionOptimizationResponse> Handle(
            GetDashboardCollectionOptimizationQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetCurrent());
        }
    }
}
