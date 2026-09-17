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
    public class PrincipalPurchaseInSnapshotDal : IPrincipalPurchaseInSnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalPurchaseInKpi, BTRPD_PrincipalPurchaseIn";

        public const string DeletePrincipalSql =
            "DELETE FROM BTRPD_PrincipalPurchaseIn WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalPurchaseInKpi AS target
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
INSERT INTO BTRPD_PrincipalPurchaseIn (
    PrincipalPurchaseInId, SnapshotKey, KpiId, PeriodYear, PeriodMonth,
    SupplierId, SupplierName, PurchaseInAmount, LineCount, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalPurchaseInId, @SnapshotKey, @KpiId, @PeriodYear, @PeriodMonth,
    @SupplierId, @SupplierName, @PurchaseInAmount, @LineCount, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalPurchaseInSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalPurchaseInAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, KpiId, PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalPurchaseInKpi
WHERE SnapshotKey = @SnapshotKey";

            const string principalSql = @"
SELECT KpiId, SupplierId, SupplierName, PurchaseInAmount, LineCount, SortOrder
FROM BTRPD_PrincipalPurchaseIn
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalPurchaseInSnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var principals = conn.Query<PrincipalPurchaseInRow>(principalSql, new
                {
                    SnapshotKey = PrincipalPurchaseInSnapshot.SnapshotKey
                }).ToList();

                return new PrincipalPurchaseInAggregateResult
                {
                    KpiId = kpi.KpiId,
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    GeneratedAt = kpi.GeneratedAt,
                    Principals = principals
                };
            }
        }

        public void ReplaceCurrent(PrincipalPurchaseInAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var kpiId = PrincipalKpiCatalog.PurchaseInId;
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeletePrincipalSql, new
                    {
                        SnapshotKey = PrincipalPurchaseInSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalPurchaseInSnapshot.SnapshotKey,
                        KpiId = kpiId,
                        result.PeriodYear,
                        result.PeriodMonth,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Principals ?? new List<PrincipalPurchaseInRow>())
                    {
                        conn.Execute(InsertPrincipalSql, new
                        {
                            PrincipalPurchaseInId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalPurchaseInSnapshot.SnapshotKey,
                            KpiId = kpiId,
                            result.PeriodYear,
                            result.PeriodMonth,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.PurchaseInAmount,
                            row.LineCount,
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
