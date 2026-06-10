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
    public class DashboardPurchasingManagementSnapshotDal : IDashboardPurchasingManagementSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardPurchasingManagementSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardPurchasingManagementAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth,
       QualifiedBacklogCount, QualifiedBacklogValue, PendingPostingValue, PostedPercent,
       Top1PrincipalPercent, Top3PrincipalPercent, Top1SupplierInventoryPercent,
       CompoundDependencyCount, PrincipalInventoryNoPurchaseCount, UnknownPrincipalCount,
       PurchasingInactivityFlag, QualifiedBacklogPrincipalCount, PrincipalAtRiskExposureCount,
       LastRefreshLogId
FROM BTRPD_PurchasingManagementKpi
WHERE SnapshotKey = @SnapshotKey";

            const string attentionSql = @"
SELECT EntityType, EntityName, SignalKey, SignalLabel, ValueAmount, ValueText, ReportRoute, SortOrder
FROM BTRPD_PurchasingManagementAttention
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string topPrincipalSql = @"
SELECT Rank, PrincipalName, MtdPurchaseAmount, PercentOfPurchase, InventoryValue, PercentOfInventory,
       AtRiskValue, PercentOfAtRisk, IsCompoundDependency, IsInventoryNoPurchase, ReportRoute
FROM BTRPD_PurchasingManagementTopPrincipal
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                return new DashboardPurchasingManagementAggregateResult
                {
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    QualifiedBacklogCount = kpi.QualifiedBacklogCount,
                    QualifiedBacklogValue = kpi.QualifiedBacklogValue,
                    PendingPostingValue = kpi.PendingPostingValue,
                    PostedPercent = kpi.PostedPercent,
                    Top1PrincipalPercent = kpi.Top1PrincipalPercent,
                    Top3PrincipalPercent = kpi.Top3PrincipalPercent,
                    Top1SupplierInventoryPercent = kpi.Top1SupplierInventoryPercent,
                    CompoundDependencyCount = kpi.CompoundDependencyCount,
                    PrincipalInventoryNoPurchaseCount = kpi.PrincipalInventoryNoPurchaseCount,
                    UnknownPrincipalCount = kpi.UnknownPrincipalCount,
                    PurchasingInactivityFlag = kpi.PurchasingInactivityFlag,
                    QualifiedBacklogPrincipalCount = kpi.QualifiedBacklogPrincipalCount,
                    PrincipalAtRiskExposureCount = kpi.PrincipalAtRiskExposureCount,
                    GeneratedAt = kpi.GeneratedAt,
                    AttentionList = conn.Query<AttentionRow>(attentionSql, new { SnapshotKey })
                        .Select(r => new DashboardPurchasingManagementAttentionRow
                        {
                            EntityType = r.EntityType,
                            EntityName = r.EntityName,
                            SignalKey = r.SignalKey,
                            SignalLabel = r.SignalLabel,
                            ValueAmount = r.ValueAmount,
                            ValueText = r.ValueText,
                            ReportRoute = r.ReportRoute,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    TopPrincipal = conn.Query<TopPrincipalRow>(topPrincipalSql, new { SnapshotKey })
                        .Select(r => new DashboardPurchasingManagementTopPrincipalRow
                        {
                            Rank = r.Rank,
                            PrincipalName = r.PrincipalName,
                            MtdPurchaseAmount = r.MtdPurchaseAmount,
                            PercentOfPurchase = r.PercentOfPurchase,
                            InventoryValue = r.InventoryValue,
                            PercentOfInventory = r.PercentOfInventory,
                            AtRiskValue = r.AtRiskValue,
                            PercentOfAtRisk = r.PercentOfAtRisk,
                            IsCompoundDependency = r.IsCompoundDependency,
                            IsInventoryNoPurchase = r.IsInventoryNoPurchase,
                            ReportRoute = r.ReportRoute
                        }).ToList()
                };
            }
        }

        public void ReplaceCurrent(DashboardPurchasingManagementAggregateResult result, string refreshLogId)
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
                        ReplaceCurrentCore(conn, transaction, result, refreshLogId);
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
            DashboardPurchasingManagementAggregateResult result,
            string refreshLogId)
        {
            conn.Execute(
                "DELETE FROM BTRPD_PurchasingManagementAttention WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_PurchasingManagementTopPrincipal WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);

            const string mergeKpiSql = @"
MERGE BTRPD_PurchasingManagementKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        QualifiedBacklogCount = @QualifiedBacklogCount,
        QualifiedBacklogValue = @QualifiedBacklogValue,
        PendingPostingValue = @PendingPostingValue,
        PostedPercent = @PostedPercent,
        Top1PrincipalPercent = @Top1PrincipalPercent,
        Top3PrincipalPercent = @Top3PrincipalPercent,
        Top1SupplierInventoryPercent = @Top1SupplierInventoryPercent,
        CompoundDependencyCount = @CompoundDependencyCount,
        PrincipalInventoryNoPurchaseCount = @PrincipalInventoryNoPurchaseCount,
        UnknownPrincipalCount = @UnknownPrincipalCount,
        PurchasingInactivityFlag = @PurchasingInactivityFlag,
        QualifiedBacklogPrincipalCount = @QualifiedBacklogPrincipalCount,
        PrincipalAtRiskExposureCount = @PrincipalAtRiskExposureCount,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth,
        QualifiedBacklogCount, QualifiedBacklogValue, PendingPostingValue, PostedPercent,
        Top1PrincipalPercent, Top3PrincipalPercent, Top1SupplierInventoryPercent,
        CompoundDependencyCount, PrincipalInventoryNoPurchaseCount, UnknownPrincipalCount,
        PurchasingInactivityFlag, QualifiedBacklogPrincipalCount, PrincipalAtRiskExposureCount,
        LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @PeriodYear, @PeriodMonth,
        @QualifiedBacklogCount, @QualifiedBacklogValue, @PendingPostingValue, @PostedPercent,
        @Top1PrincipalPercent, @Top3PrincipalPercent, @Top1SupplierInventoryPercent,
        @CompoundDependencyCount, @PrincipalInventoryNoPurchaseCount, @UnknownPrincipalCount,
        @PurchasingInactivityFlag, @QualifiedBacklogPrincipalCount, @PrincipalAtRiskExposureCount,
        @LastRefreshLogId);";

            conn.Execute(mergeKpiSql, new
            {
                SnapshotKey,
                result.GeneratedAt,
                result.PeriodYear,
                result.PeriodMonth,
                result.QualifiedBacklogCount,
                result.QualifiedBacklogValue,
                result.PendingPostingValue,
                result.PostedPercent,
                result.Top1PrincipalPercent,
                result.Top3PrincipalPercent,
                result.Top1SupplierInventoryPercent,
                result.CompoundDependencyCount,
                result.PrincipalInventoryNoPurchaseCount,
                result.UnknownPrincipalCount,
                PurchasingInactivityFlag = result.PurchasingInactivityFlag,
                result.QualifiedBacklogPrincipalCount,
                result.PrincipalAtRiskExposureCount,
                LastRefreshLogId = refreshLogId ?? string.Empty
            }, transaction);

            const string insertAttentionSql = @"
INSERT INTO BTRPD_PurchasingManagementAttention (
    PurchasingManagementAttentionId, SnapshotKey, EntityType, EntityName,
    SignalKey, SignalLabel, ValueAmount, ValueText, ReportRoute, SortOrder)
VALUES (
    @PurchasingManagementAttentionId, @SnapshotKey, @EntityType, @EntityName,
    @SignalKey, @SignalLabel, @ValueAmount, @ValueText, @ReportRoute, @SortOrder)";

            foreach (var row in result.AttentionList ?? new List<DashboardPurchasingManagementAttentionRow>())
            {
                conn.Execute(insertAttentionSql, new
                {
                    PurchasingManagementAttentionId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    EntityType = row.EntityType ?? string.Empty,
                    EntityName = row.EntityName ?? string.Empty,
                    row.SignalKey,
                    SignalLabel = row.SignalLabel ?? string.Empty,
                    row.ValueAmount,
                    ValueText = row.ValueText ?? string.Empty,
                    ReportRoute = row.ReportRoute,
                    row.SortOrder
                }, transaction);
            }

            const string insertTopPrincipalSql = @"
INSERT INTO BTRPD_PurchasingManagementTopPrincipal (
    PurchasingManagementTopPrincipalId, SnapshotKey, Rank, PrincipalName, MtdPurchaseAmount,
    PercentOfPurchase, InventoryValue, PercentOfInventory, AtRiskValue, PercentOfAtRisk,
    IsCompoundDependency, IsInventoryNoPurchase, ReportRoute)
VALUES (
    @PurchasingManagementTopPrincipalId, @SnapshotKey, @Rank, @PrincipalName, @MtdPurchaseAmount,
    @PercentOfPurchase, @InventoryValue, @PercentOfInventory, @AtRiskValue, @PercentOfAtRisk,
    @IsCompoundDependency, @IsInventoryNoPurchase, @ReportRoute)";

            foreach (var row in result.TopPrincipal ?? new List<DashboardPurchasingManagementTopPrincipalRow>())
            {
                conn.Execute(insertTopPrincipalSql, new
                {
                    PurchasingManagementTopPrincipalId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    PrincipalName = row.PrincipalName ?? string.Empty,
                    row.MtdPurchaseAmount,
                    row.PercentOfPurchase,
                    row.InventoryValue,
                    row.PercentOfInventory,
                    row.AtRiskValue,
                    row.PercentOfAtRisk,
                    IsCompoundDependency = row.IsCompoundDependency,
                    IsInventoryNoPurchase = row.IsInventoryNoPurchase,
                    ReportRoute = row.ReportRoute ?? string.Empty
                }, transaction);
            }
        }

        private sealed class KpiRow
        {
            public int PeriodYear { get; set; }
            public int PeriodMonth { get; set; }
            public int QualifiedBacklogCount { get; set; }
            public decimal QualifiedBacklogValue { get; set; }
            public decimal PendingPostingValue { get; set; }
            public decimal? PostedPercent { get; set; }
            public decimal? Top1PrincipalPercent { get; set; }
            public decimal? Top3PrincipalPercent { get; set; }
            public decimal? Top1SupplierInventoryPercent { get; set; }
            public int CompoundDependencyCount { get; set; }
            public int PrincipalInventoryNoPurchaseCount { get; set; }
            public int UnknownPrincipalCount { get; set; }
            public bool PurchasingInactivityFlag { get; set; }
            public int QualifiedBacklogPrincipalCount { get; set; }
            public int PrincipalAtRiskExposureCount { get; set; }
            public System.DateTime GeneratedAt { get; set; }
        }

        private sealed class AttentionRow
        {
            public string EntityType { get; set; }
            public string EntityName { get; set; }
            public string SignalKey { get; set; }
            public string SignalLabel { get; set; }
            public decimal? ValueAmount { get; set; }
            public string ValueText { get; set; }
            public string ReportRoute { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class TopPrincipalRow
        {
            public int Rank { get; set; }
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
        }
    }
}
