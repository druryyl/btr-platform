using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Application;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg
{
    public class PrincipalAchievementSnapshotDal : IPrincipalAchievementSnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalAchievementKpi, BTRPD_PrincipalAchievement";

        public const string DeletePrincipalSql =
            "DELETE FROM BTRPD_PrincipalAchievement WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalAchievementKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        AchievementAmountKpiId = @AchievementAmountKpiId,
        AchievementPercentageKpiId = @AchievementPercentageKpiId,
        SalesOutKpiId = @SalesOutKpiId,
        TargetKpiId = @TargetKpiId,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, AchievementAmountKpiId, AchievementPercentageKpiId, SalesOutKpiId, TargetKpiId,
        PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @AchievementAmountKpiId, @AchievementPercentageKpiId, @SalesOutKpiId, @TargetKpiId,
        @PeriodYear, @PeriodMonth, @GeneratedAt, @LastRefreshLogId);";

        public const string InsertPrincipalSql = @"
INSERT INTO BTRPD_PrincipalAchievement (
    PrincipalAchievementId, SnapshotKey, AchievementAmountKpiId, AchievementPercentageKpiId,
    SalesOutKpiId, TargetKpiId,
    PeriodYear, PeriodMonth, SupplierId, SupplierName,
    SalesOutAmount, TargetAmount, AchievementAmount, AchievementPercentage, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalAchievementId, @SnapshotKey, @AchievementAmountKpiId, @AchievementPercentageKpiId,
    @SalesOutKpiId, @TargetKpiId,
    @PeriodYear, @PeriodMonth, @SupplierId, @SupplierName,
    @SalesOutAmount, @TargetAmount, @AchievementAmount, @AchievementPercentage, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalAchievementSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalAchievementResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, AchievementAmountKpiId, AchievementPercentageKpiId, SalesOutKpiId, TargetKpiId,
       PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalAchievementKpi
WHERE SnapshotKey = @SnapshotKey";

            const string principalSql = @"
SELECT AchievementAmountKpiId, AchievementPercentageKpiId, SalesOutKpiId, TargetKpiId,
       SupplierId, SupplierName, SalesOutAmount, TargetAmount, AchievementAmount, AchievementPercentage,
       SortOrder
FROM BTRPD_PrincipalAchievement
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalAchievementSnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var principals = conn.Query<PrincipalAchievementRow>(principalSql, new
                {
                    SnapshotKey = PrincipalAchievementSnapshot.SnapshotKey
                }).ToList();

                return new PrincipalAchievementResult
                {
                    AchievementAmountKpiId = kpi.AchievementAmountKpiId,
                    AchievementPercentageKpiId = kpi.AchievementPercentageKpiId,
                    SalesOutKpiId = kpi.SalesOutKpiId,
                    TargetKpiId = kpi.TargetKpiId,
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    GeneratedAt = kpi.GeneratedAt,
                    Principals = principals
                };
            }
        }

        public void ReplaceCurrent(PrincipalAchievementResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var achievementAmountKpiId = PrincipalKpiCatalog.AchievementAmountId;
            var achievementPercentageKpiId = PrincipalKpiCatalog.AchievementPercentageId;
            var salesOutKpiId = PrincipalKpiCatalog.SalesOutId;
            var targetKpiId = PrincipalKpiCatalog.TargetId;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeletePrincipalSql, new
                    {
                        SnapshotKey = PrincipalAchievementSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalAchievementSnapshot.SnapshotKey,
                        AchievementAmountKpiId = achievementAmountKpiId,
                        AchievementPercentageKpiId = achievementPercentageKpiId,
                        SalesOutKpiId = salesOutKpiId,
                        TargetKpiId = targetKpiId,
                        result.PeriodYear,
                        result.PeriodMonth,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Principals ?? new List<PrincipalAchievementRow>())
                    {
                        conn.Execute(InsertPrincipalSql, new
                        {
                            PrincipalAchievementId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalAchievementSnapshot.SnapshotKey,
                            AchievementAmountKpiId = achievementAmountKpiId,
                            AchievementPercentageKpiId = achievementPercentageKpiId,
                            SalesOutKpiId = salesOutKpiId,
                            TargetKpiId = targetKpiId,
                            result.PeriodYear,
                            result.PeriodMonth,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.SalesOutAmount,
                            row.TargetAmount,
                            row.AchievementAmount,
                            row.AchievementPercentage,
                            row.SortOrder,
                            result.GeneratedAt,
                            LastRefreshLogId = refreshLogId ?? string.Empty
                        }, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

        private sealed class KpiRow
        {
            public string AchievementAmountKpiId { get; set; }

            public string AchievementPercentageKpiId { get; set; }

            public string SalesOutKpiId { get; set; }

            public string TargetKpiId { get; set; }

            public int PeriodYear { get; set; }

            public int PeriodMonth { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
