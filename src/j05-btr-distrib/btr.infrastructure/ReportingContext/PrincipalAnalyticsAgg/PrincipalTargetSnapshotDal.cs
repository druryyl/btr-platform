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
    public class PrincipalTargetSnapshotDal : IPrincipalTargetSnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalTargetKpi, BTRPD_PrincipalTarget";

        public const string DeletePrincipalSql =
            "DELETE FROM BTRPD_PrincipalTarget WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalTargetKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        KpiId = @KpiId,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, KpiId, PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @KpiId, @PeriodYear, @PeriodMonth, @GeneratedAt, @LastRefreshLogId);";

        public const string InsertPrincipalSql = @"
INSERT INTO BTRPD_PrincipalTarget (
    PrincipalTargetId, SnapshotKey, KpiId, PeriodYear, PeriodMonth,
    SupplierId, SupplierName, TargetAmount, SourceCount, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalTargetId, @SnapshotKey, @KpiId, @PeriodYear, @PeriodMonth,
    @SupplierId, @SupplierName, @TargetAmount, @SourceCount, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalTargetSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalTargetAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, KpiId, PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalTargetKpi
WHERE SnapshotKey = @SnapshotKey";

            const string principalSql = @"
SELECT KpiId, SupplierId, SupplierName, TargetAmount, SourceCount, SortOrder
FROM BTRPD_PrincipalTarget
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalTargetSnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var principals = conn.Query<PrincipalTargetRow>(principalSql, new
                {
                    SnapshotKey = PrincipalTargetSnapshot.SnapshotKey
                }).ToList();

                return new PrincipalTargetAggregateResult
                {
                    KpiId = kpi.KpiId,
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    GeneratedAt = kpi.GeneratedAt,
                    Principals = principals
                };
            }
        }

        public void ReplaceCurrent(PrincipalTargetAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var kpiId = PrincipalKpiCatalog.TargetId;
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeletePrincipalSql, new
                    {
                        SnapshotKey = PrincipalTargetSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalTargetSnapshot.SnapshotKey,
                        KpiId = kpiId,
                        result.PeriodYear,
                        result.PeriodMonth,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Principals ?? new List<PrincipalTargetRow>())
                    {
                        conn.Execute(InsertPrincipalSql, new
                        {
                            PrincipalTargetId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalTargetSnapshot.SnapshotKey,
                            KpiId = kpiId,
                            result.PeriodYear,
                            result.PeriodMonth,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.TargetAmount,
                            row.SourceCount,
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
            public string KpiId { get; set; }

            public int PeriodYear { get; set; }

            public int PeriodMonth { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
