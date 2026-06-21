using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardCustomerSnapshotDal : IDashboardCustomerSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardCustomerSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardCustomerAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, TotalOmzet, TotalPiutang,
       ActiveCustomerCount, DormantCustomerCount, OverdueCustomerCount, PlafondBreachCount,
       SuspendedWithSalesCount, AgingOver90Amount, TopOmzetCustomerPercent, TopPiutangCustomerPercent,
       LastRefreshLogId
FROM BTRPD_CustomerKpi
WHERE SnapshotKey = @SnapshotKey";

            const string topOmzetSql = @"
SELECT Rank, CustomerCode, CustomerName, OmzetAmount, PercentOfTotal
FROM BTRPD_CustomerTopOmzet
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topPiutangSql = @"
SELECT Rank, CustomerCode, CustomerName, OutstandingBalance, PercentOfTotal
FROM BTRPD_CustomerTopPiutang
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string attentionSql = @"
SELECT CustomerCode, CustomerName, SignalKey, SignalLabel, ValueAmount, ValueText, WilayahName, SortOrder
FROM BTRPD_CustomerAttention
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string segmentationSql = @"
SELECT SegmentType, SegmentKey, SegmentLabel, CustomerCount, ActiveCount, DormantCount, SortOrder
FROM BTRPD_CustomerSegmentation
WHERE SnapshotKey = @SnapshotKey
ORDER BY SegmentType, SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                var topOmzet = conn.Query<TopOmzetRow>(topOmzetSql, new { SnapshotKey }).ToList();
                var topPiutang = conn.Query<TopPiutangRow>(topPiutangSql, new { SnapshotKey }).ToList();
                var attention = conn.Query<AttentionRow>(attentionSql, new { SnapshotKey }).ToList();
                var segmentation = conn.Query<SegmentationRow>(segmentationSql, new { SnapshotKey }).ToList();

                return new DashboardCustomerAggregateResult
                {
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    TotalOmzet = kpi.TotalOmzet,
                    TotalPiutang = kpi.TotalPiutang,
                    ActiveCustomerCount = kpi.ActiveCustomerCount,
                    DormantCustomerCount = kpi.DormantCustomerCount,
                    OverdueCustomerCount = kpi.OverdueCustomerCount,
                    PlafondBreachCount = kpi.PlafondBreachCount,
                    SuspendedWithSalesCount = kpi.SuspendedWithSalesCount,
                    AgingOver90Amount = kpi.AgingOver90Amount,
                    TopOmzetCustomerPercent = kpi.TopOmzetCustomerPercent,
                    TopPiutangCustomerPercent = kpi.TopPiutangCustomerPercent,
                    GeneratedAt = kpi.GeneratedAt,
                    TopOmzet = topOmzet.Select(r => new DashboardCustomerTopOmzetRow
                    {
                        Rank = r.Rank,
                        CustomerCode = r.CustomerCode,
                        CustomerName = r.CustomerName,
                        OmzetAmount = r.OmzetAmount,
                        PercentOfTotal = r.PercentOfTotal
                    }).ToList(),
                    TopPiutang = topPiutang.Select(r => new DashboardCustomerTopPiutangRow
                    {
                        Rank = r.Rank,
                        CustomerCode = r.CustomerCode,
                        CustomerName = r.CustomerName,
                        OutstandingBalance = r.OutstandingBalance,
                        PercentOfTotal = r.PercentOfTotal
                    }).ToList(),
                    AttentionList = attention.Select(r => new DashboardCustomerAttentionRow
                    {
                        CustomerCode = r.CustomerCode,
                        CustomerName = r.CustomerName,
                        SignalKey = r.SignalKey,
                        SignalLabel = r.SignalLabel,
                        ValueAmount = r.ValueAmount,
                        ValueText = r.ValueText,
                        WilayahName = r.WilayahName,
                        SortOrder = r.SortOrder
                    }).ToList(),
                    Segmentation = segmentation.Select(r => new DashboardCustomerSegmentationRow
                    {
                        SegmentType = r.SegmentType,
                        SegmentKey = r.SegmentKey,
                        SegmentLabel = r.SegmentLabel,
                        CustomerCount = r.CustomerCount,
                        ActiveCount = r.ActiveCount,
                        DormantCount = r.DormantCount,
                        SortOrder = r.SortOrder
                    }).ToList()
                };
            }
        }

        public void ReplaceCurrent(DashboardCustomerAggregateResult result, string refreshLogId)
        {
            ReplaceCurrent(result, null, refreshLogId);
        }

        public void ReplaceCurrent(
            DashboardCustomerAggregateResult result,
            DashboardCustomerRiskForecastAggregateResult forecast,
            string refreshLogId)
        {
            ReplaceCurrent(result, forecast, null, refreshLogId);
        }

        public void ReplaceCurrent(
            DashboardCustomerAggregateResult result,
            DashboardCustomerRiskForecastAggregateResult forecast,
            DashboardCollectionOptimizationAggregateResult optimization,
            string refreshLogId)
        {
            if (result is null)
                throw new System.ArgumentNullException(nameof(result));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        ReplaceCurrentCore(conn, transaction, result, forecast, optimization, refreshLogId);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void ReplaceCurrentCore(
            SqlConnection conn,
            SqlTransaction transaction,
            DashboardCustomerAggregateResult result,
            DashboardCustomerRiskForecastAggregateResult forecast,
            DashboardCollectionOptimizationAggregateResult optimization,
            string refreshLogId)
        {
                conn.Execute(
                    "DELETE FROM BTRPD_CustomerTopOmzet WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey },
                    transaction);
                conn.Execute(
                    "DELETE FROM BTRPD_CustomerTopPiutang WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey },
                    transaction);
                conn.Execute(
                    "DELETE FROM BTRPD_CustomerAttention WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey },
                    transaction);
                conn.Execute(
                    "DELETE FROM BTRPD_CustomerSegmentation WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey },
                    transaction);

                const string mergeKpiSql = @"
MERGE BTRPD_CustomerKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        TotalOmzet = @TotalOmzet,
        TotalPiutang = @TotalPiutang,
        ActiveCustomerCount = @ActiveCustomerCount,
        DormantCustomerCount = @DormantCustomerCount,
        OverdueCustomerCount = @OverdueCustomerCount,
        PlafondBreachCount = @PlafondBreachCount,
        SuspendedWithSalesCount = @SuspendedWithSalesCount,
        AgingOver90Amount = @AgingOver90Amount,
        TopOmzetCustomerPercent = @TopOmzetCustomerPercent,
        TopPiutangCustomerPercent = @TopPiutangCustomerPercent,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, TotalOmzet, TotalPiutang,
        ActiveCustomerCount, DormantCustomerCount, OverdueCustomerCount, PlafondBreachCount,
        SuspendedWithSalesCount, AgingOver90Amount, TopOmzetCustomerPercent, TopPiutangCustomerPercent,
        LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @PeriodYear, @PeriodMonth, @TotalOmzet, @TotalPiutang,
        @ActiveCustomerCount, @DormantCustomerCount, @OverdueCustomerCount, @PlafondBreachCount,
        @SuspendedWithSalesCount, @AgingOver90Amount, @TopOmzetCustomerPercent, @TopPiutangCustomerPercent,
        @LastRefreshLogId);";

                conn.Execute(mergeKpiSql, new
                {
                    SnapshotKey,
                    result.GeneratedAt,
                    result.PeriodYear,
                    result.PeriodMonth,
                    result.TotalOmzet,
                    result.TotalPiutang,
                    result.ActiveCustomerCount,
                    result.DormantCustomerCount,
                    result.OverdueCustomerCount,
                    result.PlafondBreachCount,
                    result.SuspendedWithSalesCount,
                    result.AgingOver90Amount,
                    result.TopOmzetCustomerPercent,
                    result.TopPiutangCustomerPercent,
                    LastRefreshLogId = refreshLogId ?? string.Empty
                }, transaction);

                const string insertTopOmzetSql = @"
INSERT INTO BTRPD_CustomerTopOmzet (
    CustomerTopOmzetId, SnapshotKey, Rank, CustomerCode, CustomerName, OmzetAmount, PercentOfTotal)
VALUES (
    @CustomerTopOmzetId, @SnapshotKey, @Rank, @CustomerCode, @CustomerName, @OmzetAmount, @PercentOfTotal)";

                foreach (var row in result.TopOmzet ?? new List<DashboardCustomerTopOmzetRow>())
                {
                    conn.Execute(insertTopOmzetSql, new
                    {
                        CustomerTopOmzetId = Ulid.NewUlid().ToString(),
                        SnapshotKey,
                        row.Rank,
                        CustomerCode = row.CustomerCode ?? string.Empty,
                        CustomerName = row.CustomerName ?? string.Empty,
                        row.OmzetAmount,
                        row.PercentOfTotal
                    }, transaction);
                }

                const string insertTopPiutangSql = @"
INSERT INTO BTRPD_CustomerTopPiutang (
    CustomerTopPiutangId, SnapshotKey, Rank, CustomerCode, CustomerName, OutstandingBalance, PercentOfTotal)
VALUES (
    @CustomerTopPiutangId, @SnapshotKey, @Rank, @CustomerCode, @CustomerName, @OutstandingBalance, @PercentOfTotal)";

                foreach (var row in result.TopPiutang ?? new List<DashboardCustomerTopPiutangRow>())
                {
                    conn.Execute(insertTopPiutangSql, new
                    {
                        CustomerTopPiutangId = Ulid.NewUlid().ToString(),
                        SnapshotKey,
                        row.Rank,
                        CustomerCode = row.CustomerCode ?? string.Empty,
                        CustomerName = row.CustomerName ?? string.Empty,
                        row.OutstandingBalance,
                        row.PercentOfTotal
                    }, transaction);
                }

                const string insertAttentionSql = @"
INSERT INTO BTRPD_CustomerAttention (
    CustomerAttentionId, SnapshotKey, CustomerCode, CustomerName, SignalKey, SignalLabel,
    ValueAmount, ValueText, WilayahName, SortOrder)
VALUES (
    @CustomerAttentionId, @SnapshotKey, @CustomerCode, @CustomerName, @SignalKey, @SignalLabel,
    @ValueAmount, @ValueText, @WilayahName, @SortOrder)";

                foreach (var row in result.AttentionList ?? new List<DashboardCustomerAttentionRow>())
                {
                    conn.Execute(insertAttentionSql, new
                    {
                        CustomerAttentionId = Ulid.NewUlid().ToString(),
                        SnapshotKey,
                        CustomerCode = row.CustomerCode ?? string.Empty,
                        CustomerName = row.CustomerName ?? string.Empty,
                        row.SignalKey,
                        row.SignalLabel,
                        row.ValueAmount,
                        ValueText = row.ValueText ?? string.Empty,
                        WilayahName = row.WilayahName ?? string.Empty,
                        row.SortOrder
                    }, transaction);
                }

                const string insertSegmentationSql = @"
INSERT INTO BTRPD_CustomerSegmentation (
    CustomerSegmentationId, SnapshotKey, SegmentType, SegmentKey, SegmentLabel,
    CustomerCount, ActiveCount, DormantCount, SortOrder)
VALUES (
    @CustomerSegmentationId, @SnapshotKey, @SegmentType, @SegmentKey, @SegmentLabel,
    @CustomerCount, @ActiveCount, @DormantCount, @SortOrder)";

                foreach (var row in result.Segmentation ?? new List<DashboardCustomerSegmentationRow>())
                {
                    conn.Execute(insertSegmentationSql, new
                    {
                        CustomerSegmentationId = Ulid.NewUlid().ToString(),
                        SnapshotKey,
                        row.SegmentType,
                        SegmentKey = row.SegmentKey ?? string.Empty,
                        SegmentLabel = row.SegmentLabel ?? string.Empty,
                        row.CustomerCount,
                        row.ActiveCount,
                        row.DormantCount,
                        row.SortOrder
                    }, transaction);
                }

            if (forecast != null)
                ReplaceForecastCore(conn, transaction, forecast, refreshLogId);

            if (optimization != null)
                ReplaceOptimizationCore(conn, transaction, optimization, refreshLogId);
        }

        private void ReplaceOptimizationCore(
            SqlConnection conn,
            SqlTransaction transaction,
            DashboardCollectionOptimizationAggregateResult optimization,
            string refreshLogId)
        {
            conn.Execute("DELETE FROM BTRPD_CollectionOptimizationActionDist WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }, transaction);
            conn.Execute("DELETE FROM BTRPD_CollectionOptimizationWorkload WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }, transaction);
            conn.Execute("DELETE FROM BTRPD_CollectionOptimizationPriority WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }, transaction);
            conn.Execute("DELETE FROM BTRPD_CollectionOptimizationQueue WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }, transaction);
            conn.Execute("DELETE FROM BTRPD_CollectionOptimizationImpact WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }, transaction);

            var kpi = optimization.Kpi ?? new DashboardCollectionOptimizationKpiSnapshot();

            const string mergeKpiSql = @"
MERGE BTRPD_CollectionOptimizationKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        BusinessDate = @BusinessDate,
        ActionsTodayCount = @ActionsTodayCount,
        ImmediateCollectionCount = @ImmediateCollectionCount,
        ProactiveReminderCount = @ProactiveReminderCount,
        CreditReviewCount = @CreditReviewCount,
        SalesRecoveryCount = @SalesRecoveryCount,
        EscalateManagementCount = @EscalateManagementCount,
        CollectionImpactTotal = @CollectionImpactTotal,
        ImmediateImpactTotal = @ImmediateImpactTotal,
        OverdueExposure = @OverdueExposure,
        DueWithin7Days = @DueWithin7Days,
        RecoveryVsBillingPercent = @RecoveryVsBillingPercent,
        DeferNoActionCount = @DeferNoActionCount,
        PlanningConfidence = @PlanningConfidence,
        ExecutiveSummaryText = @ExecutiveSummaryText,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (SnapshotKey, GeneratedAt, BusinessDate, ActionsTodayCount, ImmediateCollectionCount,
            ProactiveReminderCount, CreditReviewCount, SalesRecoveryCount, EscalateManagementCount,
            CollectionImpactTotal, ImmediateImpactTotal, OverdueExposure, DueWithin7Days,
            RecoveryVsBillingPercent, DeferNoActionCount, PlanningConfidence, ExecutiveSummaryText, LastRefreshLogId)
    VALUES (@SnapshotKey, @GeneratedAt, @BusinessDate, @ActionsTodayCount, @ImmediateCollectionCount,
            @ProactiveReminderCount, @CreditReviewCount, @SalesRecoveryCount, @EscalateManagementCount,
            @CollectionImpactTotal, @ImmediateImpactTotal, @OverdueExposure, @DueWithin7Days,
            @RecoveryVsBillingPercent, @DeferNoActionCount, @PlanningConfidence, @ExecutiveSummaryText, @LastRefreshLogId);";

            conn.Execute(mergeKpiSql, new
            {
                SnapshotKey,
                optimization.GeneratedAt,
                optimization.BusinessDate,
                kpi.ActionsTodayCount,
                kpi.ImmediateCollectionCount,
                kpi.ProactiveReminderCount,
                kpi.CreditReviewCount,
                kpi.SalesRecoveryCount,
                kpi.EscalateManagementCount,
                kpi.CollectionImpactTotal,
                kpi.ImmediateImpactTotal,
                kpi.OverdueExposure,
                kpi.DueWithin7Days,
                kpi.RecoveryVsBillingPercent,
                kpi.DeferNoActionCount,
                PlanningConfidence = kpi.PlanningConfidence ?? string.Empty,
                ExecutiveSummaryText = kpi.ExecutiveSummaryText ?? string.Empty,
                LastRefreshLogId = refreshLogId ?? string.Empty
            }, transaction);

            const string insertDistSql = @"
INSERT INTO BTRPD_CollectionOptimizationActionDist (
    CollectionOptimizationActionDistId, SnapshotKey, ActionCategoryKey, ActionCategoryLabel,
    CustomerCount, ImpactTotal, SortOrder)
VALUES (
    @CollectionOptimizationActionDistId, @SnapshotKey, @ActionCategoryKey, @ActionCategoryLabel,
    @CustomerCount, @ImpactTotal, @SortOrder)";

            foreach (var row in optimization.ActionDistribution ?? new List<DashboardCollectionOptimizationActionDistRow>())
            {
                conn.Execute(insertDistSql, new
                {
                    CollectionOptimizationActionDistId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    ActionCategoryKey = row.ActionCategoryKey ?? string.Empty,
                    ActionCategoryLabel = row.ActionCategoryLabel ?? string.Empty,
                    row.CustomerCount,
                    row.ImpactTotal,
                    row.SortOrder
                }, transaction);
            }

            const string insertWorkloadSql = @"
INSERT INTO BTRPD_CollectionOptimizationWorkload (
    CollectionOptimizationWorkloadId, SnapshotKey, WorkloadType, EntityKey, EntityLabel,
    ActionCount, ImmediateCount, ImpactTotal, OverdueExposure, IsHotspot, SortOrder)
VALUES (
    @CollectionOptimizationWorkloadId, @SnapshotKey, @WorkloadType, @EntityKey, @EntityLabel,
    @ActionCount, @ImmediateCount, @ImpactTotal, @OverdueExposure, @IsHotspot, @SortOrder)";

            foreach (var row in optimization.Workload ?? new List<DashboardCollectionOptimizationWorkloadRow>())
            {
                conn.Execute(insertWorkloadSql, new
                {
                    CollectionOptimizationWorkloadId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    WorkloadType = row.WorkloadType ?? string.Empty,
                    EntityKey = row.EntityKey ?? string.Empty,
                    EntityLabel = row.EntityLabel ?? string.Empty,
                    row.ActionCount,
                    row.ImmediateCount,
                    row.ImpactTotal,
                    row.OverdueExposure,
                    row.IsHotspot,
                    row.SortOrder
                }, transaction);
            }

            const string insertPrioritySql = @"
INSERT INTO BTRPD_CollectionOptimizationPriority (
    CollectionOptimizationPriorityId, SnapshotKey, SortOrder, CollectionPriorityScore,
    CustomerCode, CustomerName, WilayahName, SalesPersonName, Klasifikasi,
    ActionCategoryKey, ActionCategoryLabel, RecommendedActionKey, RecommendedActionLabel, ActionOwner,
    OpenBalance, OverdueBalance, DueWithin7Days, CollectionImpactAmount,
    M29Category, M29RecommendationKey, M29PrimarySignalKey, MinDaysUntilDue, CreditUtilizationPercent,
    SelectionReasonText, PriorityReasonText, ActionReasonText, TriggeredRuleIds, ReportRoute, DrillDownRoute)
VALUES (
    @CollectionOptimizationPriorityId, @SnapshotKey, @SortOrder, @CollectionPriorityScore,
    @CustomerCode, @CustomerName, @WilayahName, @SalesPersonName, @Klasifikasi,
    @ActionCategoryKey, @ActionCategoryLabel, @RecommendedActionKey, @RecommendedActionLabel, @ActionOwner,
    @OpenBalance, @OverdueBalance, @DueWithin7Days, @CollectionImpactAmount,
    @M29Category, @M29RecommendationKey, @M29PrimarySignalKey, @MinDaysUntilDue, @CreditUtilizationPercent,
    @SelectionReasonText, @PriorityReasonText, @ActionReasonText, @TriggeredRuleIds, @ReportRoute, @DrillDownRoute)";

            foreach (var row in optimization.PriorityQueue ?? new List<DashboardCollectionOptimizationPriorityRow>())
            {
                conn.Execute(insertPrioritySql, new
                {
                    CollectionOptimizationPriorityId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.SortOrder,
                    row.CollectionPriorityScore,
                    CustomerCode = row.CustomerCode ?? string.Empty,
                    CustomerName = row.CustomerName ?? string.Empty,
                    WilayahName = row.WilayahName ?? string.Empty,
                    SalesPersonName = row.SalesPersonName ?? string.Empty,
                    Klasifikasi = row.Klasifikasi ?? string.Empty,
                    ActionCategoryKey = row.ActionCategoryKey ?? string.Empty,
                    ActionCategoryLabel = row.ActionCategoryLabel ?? string.Empty,
                    RecommendedActionKey = row.RecommendedActionKey ?? string.Empty,
                    RecommendedActionLabel = row.RecommendedActionLabel ?? string.Empty,
                    ActionOwner = row.ActionOwner ?? string.Empty,
                    row.OpenBalance,
                    row.OverdueBalance,
                    row.DueWithin7Days,
                    row.CollectionImpactAmount,
                    M29Category = row.M29Category ?? string.Empty,
                    M29RecommendationKey = row.M29RecommendationKey ?? string.Empty,
                    M29PrimarySignalKey = row.M29PrimarySignalKey ?? string.Empty,
                    row.MinDaysUntilDue,
                    row.CreditUtilizationPercent,
                    SelectionReasonText = row.SelectionReasonText ?? string.Empty,
                    PriorityReasonText = row.PriorityReasonText ?? string.Empty,
                    ActionReasonText = row.ActionReasonText ?? string.Empty,
                    TriggeredRuleIds = row.TriggeredRuleIds ?? string.Empty,
                    ReportRoute = row.ReportRoute ?? string.Empty,
                    DrillDownRoute = row.DrillDownRoute ?? string.Empty
                }, transaction);
            }

            const string insertQueueSql = @"
INSERT INTO BTRPD_CollectionOptimizationQueue (
    CollectionOptimizationQueueId, SnapshotKey, QueueKey, SortOrder, CollectionPriorityScore,
    CustomerCode, CustomerName, WilayahName, SalesPersonName,
    ActionCategoryKey, ActionCategoryLabel, RecommendedActionKey, RecommendedActionLabel, ActionOwner,
    OverdueBalance, DueWithin7Days, CollectionImpactAmount, M29Category, QueueReasonText, ReportRoute, DrillDownRoute)
VALUES (
    @CollectionOptimizationQueueId, @SnapshotKey, @QueueKey, @SortOrder, @CollectionPriorityScore,
    @CustomerCode, @CustomerName, @WilayahName, @SalesPersonName,
    @ActionCategoryKey, @ActionCategoryLabel, @RecommendedActionKey, @RecommendedActionLabel, @ActionOwner,
    @OverdueBalance, @DueWithin7Days, @CollectionImpactAmount, @M29Category, @QueueReasonText, @ReportRoute, @DrillDownRoute)";

            foreach (var row in optimization.SpecializedQueues ?? new List<DashboardCollectionOptimizationQueueRow>())
            {
                conn.Execute(insertQueueSql, new
                {
                    CollectionOptimizationQueueId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    QueueKey = row.QueueKey ?? string.Empty,
                    row.SortOrder,
                    row.CollectionPriorityScore,
                    CustomerCode = row.CustomerCode ?? string.Empty,
                    CustomerName = row.CustomerName ?? string.Empty,
                    WilayahName = row.WilayahName ?? string.Empty,
                    SalesPersonName = row.SalesPersonName ?? string.Empty,
                    ActionCategoryKey = row.ActionCategoryKey ?? string.Empty,
                    ActionCategoryLabel = row.ActionCategoryLabel ?? string.Empty,
                    RecommendedActionKey = row.RecommendedActionKey ?? string.Empty,
                    RecommendedActionLabel = row.RecommendedActionLabel ?? string.Empty,
                    ActionOwner = row.ActionOwner ?? string.Empty,
                    row.OverdueBalance,
                    row.DueWithin7Days,
                    row.CollectionImpactAmount,
                    M29Category = row.M29Category ?? string.Empty,
                    QueueReasonText = row.QueueReasonText ?? string.Empty,
                    ReportRoute = row.ReportRoute ?? string.Empty,
                    DrillDownRoute = row.DrillDownRoute ?? string.Empty
                }, transaction);
            }

            const string insertImpactSql = @"
INSERT INTO BTRPD_CollectionOptimizationImpact (
    CollectionOptimizationImpactId, SnapshotKey, SortOrder, CustomerCode, CustomerName, WilayahName, SalesPersonName,
    ActionCategoryKey, ActionCategoryLabel, CollectionImpactAmount, OverdueBalance, DueWithin7Days, ReportRoute, DrillDownRoute)
VALUES (
    @CollectionOptimizationImpactId, @SnapshotKey, @SortOrder, @CustomerCode, @CustomerName, @WilayahName, @SalesPersonName,
    @ActionCategoryKey, @ActionCategoryLabel, @CollectionImpactAmount, @OverdueBalance, @DueWithin7Days, @ReportRoute, @DrillDownRoute)";

            foreach (var row in optimization.TopImpactOpportunities ?? new List<DashboardCollectionOptimizationImpactRow>())
            {
                conn.Execute(insertImpactSql, new
                {
                    CollectionOptimizationImpactId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.SortOrder,
                    CustomerCode = row.CustomerCode ?? string.Empty,
                    CustomerName = row.CustomerName ?? string.Empty,
                    WilayahName = row.WilayahName ?? string.Empty,
                    SalesPersonName = row.SalesPersonName ?? string.Empty,
                    ActionCategoryKey = row.ActionCategoryKey ?? string.Empty,
                    ActionCategoryLabel = row.ActionCategoryLabel ?? string.Empty,
                    row.CollectionImpactAmount,
                    row.OverdueBalance,
                    row.DueWithin7Days,
                    ReportRoute = row.ReportRoute ?? string.Empty,
                    DrillDownRoute = row.DrillDownRoute ?? string.Empty
                }, transaction);
            }
        }

        private void ReplaceForecastCore(
            SqlConnection conn,
            SqlTransaction transaction,
            DashboardCustomerRiskForecastAggregateResult forecast,
            string refreshLogId)
        {
            conn.Execute("DELETE FROM BTRPD_CustomerRiskForecastDist WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }, transaction);
            conn.Execute("DELETE FROM BTRPD_CustomerRiskForecastWilayah WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }, transaction);
            conn.Execute("DELETE FROM BTRPD_CustomerRiskForecastSignalMix WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }, transaction);
            conn.Execute("DELETE FROM BTRPD_CustomerRiskForecastCustomer WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }, transaction);
            conn.Execute("DELETE FROM BTRPD_CustomerRiskForecastAttention WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }, transaction);
            conn.Execute("DELETE FROM BTRPD_CustomerRiskForecastRecommendation WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }, transaction);

            var kpi = forecast.Kpi ?? new DashboardCustomerRiskForecastKpiSnapshot();

            const string mergeKpiSql = @"
MERGE BTRPD_CustomerRiskForecastKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        BusinessDate = @BusinessDate,
        HorizonDays = @HorizonDays,
        CustomersForecastedAtRisk = @CustomersForecastedAtRisk,
        HighRiskCustomerCount = @HighRiskCustomerCount,
        CriticalCustomerCount = @CriticalCustomerCount,
        ElevatedRiskReceivable = @ElevatedRiskReceivable,
        ElevatedRiskReceivablePercent = @ElevatedRiskReceivablePercent,
        PortfolioHealthScore = @PortfolioHealthScore,
        TotalPiutang = @TotalPiutang,
        ForecastConfidence = @ForecastConfidence,
        PaymentDelaySignalCount = @PaymentDelaySignalCount,
        CreditLimitSignalCount = @CreditLimitSignalCount,
        InactivitySignalCount = @InactivitySignalCount,
        PurchaseDeclineSignalCount = @PurchaseDeclineSignalCount,
        CollectionRiskSignalCount = @CollectionRiskSignalCount,
        HealthyCount = @HealthyCount,
        WatchCount = @WatchCount,
        AttentionCount = @AttentionCount,
        HighRiskCount = @HighRiskCount,
        CriticalCount = @CriticalCount,
        ExecutiveSummaryText = @ExecutiveSummaryText,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, BusinessDate, HorizonDays,
        CustomersForecastedAtRisk, HighRiskCustomerCount, CriticalCustomerCount,
        ElevatedRiskReceivable, ElevatedRiskReceivablePercent, PortfolioHealthScore,
        TotalPiutang, ForecastConfidence, PaymentDelaySignalCount, CreditLimitSignalCount,
        InactivitySignalCount, PurchaseDeclineSignalCount, CollectionRiskSignalCount,
        HealthyCount, WatchCount, AttentionCount, HighRiskCount, CriticalCount,
        ExecutiveSummaryText, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @BusinessDate, @HorizonDays,
        @CustomersForecastedAtRisk, @HighRiskCustomerCount, @CriticalCustomerCount,
        @ElevatedRiskReceivable, @ElevatedRiskReceivablePercent, @PortfolioHealthScore,
        @TotalPiutang, @ForecastConfidence, @PaymentDelaySignalCount, @CreditLimitSignalCount,
        @InactivitySignalCount, @PurchaseDeclineSignalCount, @CollectionRiskSignalCount,
        @HealthyCount, @WatchCount, @AttentionCount, @HighRiskCount, @CriticalCount,
        @ExecutiveSummaryText, @LastRefreshLogId);";

            conn.Execute(mergeKpiSql, new
            {
                SnapshotKey,
                forecast.GeneratedAt,
                forecast.BusinessDate,
                kpi.HorizonDays,
                kpi.CustomersForecastedAtRisk,
                kpi.HighRiskCustomerCount,
                kpi.CriticalCustomerCount,
                kpi.ElevatedRiskReceivable,
                kpi.ElevatedRiskReceivablePercent,
                kpi.PortfolioHealthScore,
                kpi.TotalPiutang,
                ForecastConfidence = kpi.ForecastConfidence ?? string.Empty,
                kpi.PaymentDelaySignalCount,
                kpi.CreditLimitSignalCount,
                kpi.InactivitySignalCount,
                kpi.PurchaseDeclineSignalCount,
                kpi.CollectionRiskSignalCount,
                kpi.HealthyCount,
                kpi.WatchCount,
                kpi.AttentionCount,
                kpi.HighRiskCount,
                kpi.CriticalCount,
                ExecutiveSummaryText = kpi.ExecutiveSummaryText ?? string.Empty,
                LastRefreshLogId = refreshLogId ?? string.Empty
            }, transaction);

            const string insertDistSql = @"
INSERT INTO BTRPD_CustomerRiskForecastDist (
    CustomerRiskForecastDistId, SnapshotKey, Category, CategoryLabel, CustomerCount, SortOrder)
VALUES (
    @CustomerRiskForecastDistId, @SnapshotKey, @Category, @CategoryLabel, @CustomerCount, @SortOrder)";

            foreach (var row in forecast.CategoryDistribution ?? new List<DashboardCustomerRiskForecastDistRow>())
            {
                conn.Execute(insertDistSql, new
                {
                    CustomerRiskForecastDistId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    Category = row.Category ?? string.Empty,
                    CategoryLabel = row.CategoryLabel ?? string.Empty,
                    row.CustomerCount,
                    row.SortOrder
                }, transaction);
            }

            const string insertWilayahSql = @"
INSERT INTO BTRPD_CustomerRiskForecastWilayah (
    CustomerRiskForecastWilayahId, SnapshotKey, WilayahName, ElevatedRiskCustomerCount, SortOrder)
VALUES (
    @CustomerRiskForecastWilayahId, @SnapshotKey, @WilayahName, @ElevatedRiskCustomerCount, @SortOrder)";

            foreach (var row in forecast.TopWilayah ?? new List<DashboardCustomerRiskForecastWilayahRow>())
            {
                conn.Execute(insertWilayahSql, new
                {
                    CustomerRiskForecastWilayahId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    WilayahName = row.WilayahName ?? string.Empty,
                    row.ElevatedRiskCustomerCount,
                    row.SortOrder
                }, transaction);
            }

            const string insertSignalMixSql = @"
INSERT INTO BTRPD_CustomerRiskForecastSignalMix (
    CustomerRiskForecastSignalMixId, SnapshotKey, SignalFamilyKey, SignalFamilyLabel, CustomerCount, SortOrder)
VALUES (
    @CustomerRiskForecastSignalMixId, @SnapshotKey, @SignalFamilyKey, @SignalFamilyLabel, @CustomerCount, @SortOrder)";

            foreach (var row in forecast.SignalMix ?? new List<DashboardCustomerRiskForecastSignalMixRow>())
            {
                conn.Execute(insertSignalMixSql, new
                {
                    CustomerRiskForecastSignalMixId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    SignalFamilyKey = row.SignalFamilyKey ?? string.Empty,
                    SignalFamilyLabel = row.SignalFamilyLabel ?? string.Empty,
                    row.CustomerCount,
                    row.SortOrder
                }, transaction);
            }

            const string insertCustomerSql = @"
INSERT INTO BTRPD_CustomerRiskForecastCustomer (
    CustomerRiskForecastCustomerId, SnapshotKey, SortOrder, RiskPriorityScore, Category, CategoryLabel,
    CustomerCode, CustomerName, WilayahName, SalesPersonName, OpenBalance, OverdueBalance, DueWithinHorizon,
    Plafond, ProjectedOpenBalance, MtdOmzet, PriorMonthOmzet, DeclineRatio, DaysSinceLastFaktur,
    AvgPaymentLagDays, PrimarySignalKey, PrimarySignalLabel, ReasonText, RecommendationKey,
    RecommendationLabel, ReportRoute, DrillDownRoute)
VALUES (
    @CustomerRiskForecastCustomerId, @SnapshotKey, @SortOrder, @RiskPriorityScore, @Category, @CategoryLabel,
    @CustomerCode, @CustomerName, @WilayahName, @SalesPersonName, @OpenBalance, @OverdueBalance, @DueWithinHorizon,
    @Plafond, @ProjectedOpenBalance, @MtdOmzet, @PriorMonthOmzet, @DeclineRatio, @DaysSinceLastFaktur,
    @AvgPaymentLagDays, @PrimarySignalKey, @PrimarySignalLabel, @ReasonText, @RecommendationKey,
    @RecommendationLabel, @ReportRoute, @DrillDownRoute)";

            foreach (var row in forecast.TopCustomers ?? new List<DashboardCustomerRiskForecastCustomerRow>())
            {
                conn.Execute(insertCustomerSql, new
                {
                    CustomerRiskForecastCustomerId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.SortOrder,
                    row.RiskPriorityScore,
                    Category = row.Category ?? string.Empty,
                    CategoryLabel = row.CategoryLabel ?? string.Empty,
                    CustomerCode = row.CustomerCode ?? string.Empty,
                    CustomerName = row.CustomerName ?? string.Empty,
                    WilayahName = row.WilayahName ?? string.Empty,
                    SalesPersonName = row.SalesPersonName ?? string.Empty,
                    row.OpenBalance,
                    row.OverdueBalance,
                    row.DueWithinHorizon,
                    row.Plafond,
                    row.ProjectedOpenBalance,
                    row.MtdOmzet,
                    row.PriorMonthOmzet,
                    row.DeclineRatio,
                    row.DaysSinceLastFaktur,
                    row.AvgPaymentLagDays,
                    PrimarySignalKey = row.PrimarySignalKey ?? string.Empty,
                    PrimarySignalLabel = row.PrimarySignalLabel ?? string.Empty,
                    ReasonText = row.ReasonText ?? string.Empty,
                    RecommendationKey = row.RecommendationKey ?? string.Empty,
                    RecommendationLabel = row.RecommendationLabel ?? string.Empty,
                    ReportRoute = row.ReportRoute ?? string.Empty,
                    DrillDownRoute = row.DrillDownRoute ?? string.Empty
                }, transaction);
            }

            const string insertAttentionSql = @"
INSERT INTO BTRPD_CustomerRiskForecastAttention (
    CustomerRiskForecastAttentionId, SnapshotKey, SortOrder, CustomerCode, CustomerName,
    SignalKey, SignalLabel, Severity, Amount, HorizonText, RuleId, Explanation, ReportRoute)
VALUES (
    @CustomerRiskForecastAttentionId, @SnapshotKey, @SortOrder, @CustomerCode, @CustomerName,
    @SignalKey, @SignalLabel, @Severity, @Amount, @HorizonText, @RuleId, @Explanation, @ReportRoute)";

            foreach (var row in forecast.AttentionList ?? new List<DashboardCustomerRiskForecastAttentionRow>())
            {
                conn.Execute(insertAttentionSql, new
                {
                    CustomerRiskForecastAttentionId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.SortOrder,
                    CustomerCode = row.CustomerCode ?? string.Empty,
                    CustomerName = row.CustomerName ?? string.Empty,
                    SignalKey = row.SignalKey ?? string.Empty,
                    SignalLabel = row.SignalLabel ?? string.Empty,
                    Severity = row.Severity ?? string.Empty,
                    row.Amount,
                    HorizonText = row.HorizonText ?? string.Empty,
                    RuleId = row.RuleId ?? string.Empty,
                    Explanation = row.Explanation ?? string.Empty,
                    ReportRoute = row.ReportRoute ?? string.Empty
                }, transaction);
            }

            const string insertRecommendationSql = @"
INSERT INTO BTRPD_CustomerRiskForecastRecommendation (
    CustomerRiskForecastRecommendationId, SnapshotKey, SortOrder, RecommendationKey, RecommendationLabel,
    CustomerCode, CustomerName, Category, ReasonText, RuleId, ReportRoute, DrillDownRoute)
VALUES (
    @CustomerRiskForecastRecommendationId, @SnapshotKey, @SortOrder, @RecommendationKey, @RecommendationLabel,
    @CustomerCode, @CustomerName, @Category, @ReasonText, @RuleId, @ReportRoute, @DrillDownRoute)";

            foreach (var row in forecast.Recommendations ?? new List<DashboardCustomerRiskForecastRecommendationRow>())
            {
                conn.Execute(insertRecommendationSql, new
                {
                    CustomerRiskForecastRecommendationId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.SortOrder,
                    RecommendationKey = row.RecommendationKey ?? string.Empty,
                    RecommendationLabel = row.RecommendationLabel ?? string.Empty,
                    CustomerCode = row.CustomerCode ?? string.Empty,
                    CustomerName = row.CustomerName ?? string.Empty,
                    Category = row.Category ?? string.Empty,
                    ReasonText = row.ReasonText ?? string.Empty,
                    RuleId = row.RuleId ?? string.Empty,
                    ReportRoute = row.ReportRoute ?? string.Empty,
                    DrillDownRoute = row.DrillDownRoute ?? string.Empty
                }, transaction);
            }
        }

        private sealed class KpiRow
        {
            public string SnapshotKey { get; set; }
            public System.DateTime GeneratedAt { get; set; }
            public int PeriodYear { get; set; }
            public int PeriodMonth { get; set; }
            public decimal TotalOmzet { get; set; }
            public decimal TotalPiutang { get; set; }
            public int ActiveCustomerCount { get; set; }
            public int DormantCustomerCount { get; set; }
            public int OverdueCustomerCount { get; set; }
            public int PlafondBreachCount { get; set; }
            public int SuspendedWithSalesCount { get; set; }
            public decimal AgingOver90Amount { get; set; }
            public decimal? TopOmzetCustomerPercent { get; set; }
            public decimal? TopPiutangCustomerPercent { get; set; }
            public string LastRefreshLogId { get; set; }
        }

        private sealed class TopOmzetRow
        {
            public int Rank { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public decimal OmzetAmount { get; set; }
            public decimal? PercentOfTotal { get; set; }
        }

        private sealed class TopPiutangRow
        {
            public int Rank { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public decimal OutstandingBalance { get; set; }
            public decimal? PercentOfTotal { get; set; }
        }

        private sealed class AttentionRow
        {
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public string SignalKey { get; set; }
            public string SignalLabel { get; set; }
            public decimal? ValueAmount { get; set; }
            public string ValueText { get; set; }
            public string WilayahName { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class SegmentationRow
        {
            public string SegmentType { get; set; }
            public string SegmentKey { get; set; }
            public string SegmentLabel { get; set; }
            public int CustomerCount { get; set; }
            public int ActiveCount { get; set; }
            public int DormantCount { get; set; }
            public int SortOrder { get; set; }
        }
    }
}
