using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public sealed class CollectionOptimizationContext
    {
        public string CustomerKey { get; set; }

        public decimal DueWithin7Days { get; set; }

        public decimal DueWithin14Days { get; set; }

        public int MinDaysUntilDue { get; set; }

        public bool HasChronicOverdue { get; set; }

        public bool HasLegacyDebtSignal { get; set; }

        public bool HasPlafondBreachOverdueSignal { get; set; }

        public bool SalesmanLowRecovery { get; set; }

        public bool IsTop10MtdOmzet { get; set; }

        public bool IsTop20MtdOmzet { get; set; }

        public decimal? CreditUtilizationPercent { get; set; }

        public string Klasifikasi { get; set; }

        public string ActionCategoryKey { get; set; }

        public string RecommendedActionKey { get; set; }

        public string ActionOwner { get; set; }

        public int CollectionPriorityScore { get; set; }

        public decimal CollectionImpactAmount { get; set; }

        public string SelectionReasonText { get; set; }

        public string PriorityReasonText { get; set; }

        public string ActionReasonText { get; set; }

        public string TriggeredRuleIds { get; set; }

        public string M29RecommendationKey { get; set; }

        public string M29PrimarySignalKey { get; set; }

        public string CatRuleId { get; set; }
    }

    public class DashboardCollectionOptimizationAggregateResult
    {
        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public int DaysElapsed { get; set; }

        public DashboardCollectionOptimizationKpiSnapshot Kpi { get; set; }
            = new DashboardCollectionOptimizationKpiSnapshot();

        public List<DashboardCollectionOptimizationActionDistRow> ActionDistribution { get; set; }
            = new List<DashboardCollectionOptimizationActionDistRow>();

        public List<DashboardCollectionOptimizationWorkloadRow> Workload { get; set; }
            = new List<DashboardCollectionOptimizationWorkloadRow>();

        public List<DashboardCollectionOptimizationPriorityRow> PriorityQueue { get; set; }
            = new List<DashboardCollectionOptimizationPriorityRow>();

        public List<DashboardCollectionOptimizationQueueRow> SpecializedQueues { get; set; }
            = new List<DashboardCollectionOptimizationQueueRow>();

        public List<DashboardCollectionOptimizationImpactRow> TopImpactOpportunities { get; set; }
            = new List<DashboardCollectionOptimizationImpactRow>();
    }

    public sealed class DashboardCollectionOptimizationKpiSnapshot
    {
        public int ActionsTodayCount { get; set; }

        public int ImmediateCollectionCount { get; set; }

        public int PriorityFollowUpCount { get; set; }

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

        public bool CollectionContextUnavailable { get; set; }
    }

    public sealed class DashboardCollectionOptimizationActionDistRow
    {
        public string ActionCategoryKey { get; set; }

        public string ActionCategoryLabel { get; set; }

        public int CustomerCount { get; set; }

        public decimal ImpactTotal { get; set; }

        public int SortOrder { get; set; }
    }

    public sealed class DashboardCollectionOptimizationWorkloadRow
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

    public sealed class DashboardCollectionOptimizationPriorityRow
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

    public sealed class DashboardCollectionOptimizationQueueRow
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

    public sealed class DashboardCollectionOptimizationImpactRow
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
}
