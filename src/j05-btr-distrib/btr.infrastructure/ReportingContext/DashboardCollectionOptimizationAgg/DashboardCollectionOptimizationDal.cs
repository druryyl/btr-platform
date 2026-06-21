using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardCollectionOptimizationAgg.Contracts;
using btr.application.ReportingContext.DashboardCollectionOptimizationAgg.Queries;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardCollectionOptimizationAgg
{
    public class DashboardCollectionOptimizationDal : IDashboardCollectionOptimizationDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardCollectionOptimizationDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardCollectionOptimizationResponse GetCurrent()
        {
            const string kpiSql = @"
SELECT GeneratedAt, BusinessDate, ActionsTodayCount, ImmediateCollectionCount, ProactiveReminderCount,
       CreditReviewCount, SalesRecoveryCount, EscalateManagementCount, CollectionImpactTotal,
       ImmediateImpactTotal, OverdueExposure, DueWithin7Days, RecoveryVsBillingPercent,
       DeferNoActionCount, PlanningConfidence, ExecutiveSummaryText
FROM BTRPD_CollectionOptimizationKpi
WHERE SnapshotKey = @SnapshotKey";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                {
                    return new DashboardCollectionOptimizationResponse
                    {
                        IsAvailable = false
                    };
                }

                return new DashboardCollectionOptimizationResponse
                {
                    IsAvailable = true,
                    GeneratedAt = kpi.GeneratedAt,
                    BusinessDate = kpi.BusinessDate,
                    Kpi = new DashboardCollectionOptimizationKpiDto
                    {
                        ActionsTodayCount = kpi.ActionsTodayCount,
                        ImmediateCollectionCount = kpi.ImmediateCollectionCount,
                        ProactiveReminderCount = kpi.ProactiveReminderCount,
                        CreditReviewCount = kpi.CreditReviewCount,
                        SalesRecoveryCount = kpi.SalesRecoveryCount,
                        EscalateManagementCount = kpi.EscalateManagementCount,
                        CollectionImpactTotal = kpi.CollectionImpactTotal,
                        ImmediateImpactTotal = kpi.ImmediateImpactTotal,
                        OverdueExposure = kpi.OverdueExposure,
                        DueWithin7Days = kpi.DueWithin7Days,
                        RecoveryVsBillingPercent = kpi.RecoveryVsBillingPercent,
                        DeferNoActionCount = kpi.DeferNoActionCount,
                        PlanningConfidence = kpi.PlanningConfidence ?? string.Empty,
                        ExecutiveSummaryText = kpi.ExecutiveSummaryText ?? string.Empty
                    },
                    ActionDistribution = conn.Query<ActionDistRow>(@"
SELECT ActionCategoryKey, ActionCategoryLabel, CustomerCount, ImpactTotal, SortOrder
FROM BTRPD_CollectionOptimizationActionDist
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey })
                        .Select(r => new DashboardCollectionOptimizationActionDistDto
                        {
                            ActionCategoryKey = r.ActionCategoryKey,
                            ActionCategoryLabel = r.ActionCategoryLabel,
                            CustomerCount = r.CustomerCount,
                            ImpactTotal = r.ImpactTotal,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    Workload = conn.Query<WorkloadRow>(@"
SELECT WorkloadType, EntityKey, EntityLabel, ActionCount, ImmediateCount, ImpactTotal,
       OverdueExposure, IsHotspot, SortOrder
FROM BTRPD_CollectionOptimizationWorkload
WHERE SnapshotKey = @SnapshotKey
ORDER BY WorkloadType, SortOrder", new { SnapshotKey })
                        .Select(r => new DashboardCollectionOptimizationWorkloadDto
                        {
                            WorkloadType = r.WorkloadType,
                            EntityKey = r.EntityKey,
                            EntityLabel = r.EntityLabel,
                            ActionCount = r.ActionCount,
                            ImmediateCount = r.ImmediateCount,
                            ImpactTotal = r.ImpactTotal,
                            OverdueExposure = r.OverdueExposure,
                            IsHotspot = r.IsHotspot,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    PriorityQueue = conn.Query<PriorityRow>(@"
SELECT SortOrder, CollectionPriorityScore, CustomerCode, CustomerName, WilayahName, SalesPersonName,
       Klasifikasi, ActionCategoryKey, ActionCategoryLabel, RecommendedActionKey, RecommendedActionLabel,
       ActionOwner, OpenBalance, OverdueBalance, DueWithin7Days, CollectionImpactAmount,
       M29Category, M29RecommendationKey, M29PrimarySignalKey, MinDaysUntilDue, CreditUtilizationPercent,
       SelectionReasonText, PriorityReasonText, ActionReasonText, TriggeredRuleIds, ReportRoute, DrillDownRoute
FROM BTRPD_CollectionOptimizationPriority
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey })
                        .Select(MapPriority).ToList(),
                    SpecializedQueues = conn.Query<QueueRow>(@"
SELECT QueueKey, SortOrder, CollectionPriorityScore, CustomerCode, CustomerName, WilayahName,
       SalesPersonName, ActionCategoryKey, ActionCategoryLabel, RecommendedActionKey, RecommendedActionLabel,
       ActionOwner, OverdueBalance, DueWithin7Days, CollectionImpactAmount, M29Category, QueueReasonText,
       ReportRoute, DrillDownRoute
FROM BTRPD_CollectionOptimizationQueue
WHERE SnapshotKey = @SnapshotKey
ORDER BY QueueKey, SortOrder", new { SnapshotKey })
                        .Select(r => new DashboardCollectionOptimizationQueueDto
                        {
                            QueueKey = r.QueueKey,
                            SortOrder = r.SortOrder,
                            CollectionPriorityScore = r.CollectionPriorityScore,
                            CustomerCode = r.CustomerCode,
                            CustomerName = r.CustomerName,
                            WilayahName = r.WilayahName,
                            SalesPersonName = r.SalesPersonName,
                            ActionCategoryKey = r.ActionCategoryKey,
                            ActionCategoryLabel = r.ActionCategoryLabel,
                            RecommendedActionKey = r.RecommendedActionKey,
                            RecommendedActionLabel = r.RecommendedActionLabel,
                            ActionOwner = r.ActionOwner,
                            OverdueBalance = r.OverdueBalance,
                            DueWithin7Days = r.DueWithin7Days,
                            CollectionImpactAmount = r.CollectionImpactAmount,
                            M29Category = r.M29Category,
                            QueueReasonText = r.QueueReasonText,
                            ReportRoute = r.ReportRoute,
                            DrillDownRoute = r.DrillDownRoute
                        }).ToList(),
                    TopImpactOpportunities = conn.Query<ImpactRow>(@"
SELECT SortOrder, CustomerCode, CustomerName, WilayahName, SalesPersonName,
       ActionCategoryKey, ActionCategoryLabel, CollectionImpactAmount, OverdueBalance, DueWithin7Days,
       ReportRoute, DrillDownRoute
FROM BTRPD_CollectionOptimizationImpact
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey })
                        .Select(r => new DashboardCollectionOptimizationImpactDto
                        {
                            SortOrder = r.SortOrder,
                            CustomerCode = r.CustomerCode,
                            CustomerName = r.CustomerName,
                            WilayahName = r.WilayahName,
                            SalesPersonName = r.SalesPersonName,
                            ActionCategoryKey = r.ActionCategoryKey,
                            ActionCategoryLabel = r.ActionCategoryLabel,
                            CollectionImpactAmount = r.CollectionImpactAmount,
                            OverdueBalance = r.OverdueBalance,
                            DueWithin7Days = r.DueWithin7Days,
                            ReportRoute = r.ReportRoute,
                            DrillDownRoute = r.DrillDownRoute
                        }).ToList()
                };
            }
        }

        private static DashboardCollectionOptimizationPriorityDto MapPriority(PriorityRow r) =>
            new DashboardCollectionOptimizationPriorityDto
            {
                SortOrder = r.SortOrder,
                CollectionPriorityScore = r.CollectionPriorityScore,
                CustomerCode = r.CustomerCode,
                CustomerName = r.CustomerName,
                WilayahName = r.WilayahName,
                SalesPersonName = r.SalesPersonName,
                Klasifikasi = r.Klasifikasi,
                ActionCategoryKey = r.ActionCategoryKey,
                ActionCategoryLabel = r.ActionCategoryLabel,
                RecommendedActionKey = r.RecommendedActionKey,
                RecommendedActionLabel = r.RecommendedActionLabel,
                ActionOwner = r.ActionOwner,
                OpenBalance = r.OpenBalance,
                OverdueBalance = r.OverdueBalance,
                DueWithin7Days = r.DueWithin7Days,
                CollectionImpactAmount = r.CollectionImpactAmount,
                M29Category = r.M29Category,
                M29RecommendationKey = r.M29RecommendationKey,
                M29PrimarySignalKey = r.M29PrimarySignalKey,
                MinDaysUntilDue = r.MinDaysUntilDue,
                CreditUtilizationPercent = r.CreditUtilizationPercent,
                SelectionReasonText = r.SelectionReasonText,
                PriorityReasonText = r.PriorityReasonText,
                ActionReasonText = r.ActionReasonText,
                TriggeredRuleIds = r.TriggeredRuleIds,
                ReportRoute = r.ReportRoute,
                DrillDownRoute = r.DrillDownRoute
            };

        private sealed class KpiRow
        {
            public System.DateTime GeneratedAt { get; set; }
            public System.DateTime BusinessDate { get; set; }
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

        private sealed class ActionDistRow
        {
            public string ActionCategoryKey { get; set; }
            public string ActionCategoryLabel { get; set; }
            public int CustomerCount { get; set; }
            public decimal ImpactTotal { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class WorkloadRow
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

        private sealed class PriorityRow
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

        private sealed class QueueRow
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

        private sealed class ImpactRow
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
}
