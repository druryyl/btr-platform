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
    public class PrincipalSalesOutSnapshotDal : IPrincipalSalesOutSnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalSalesOutKpi, BTRPD_PrincipalSalesOut, BTRPD_PrincipalSalesOutDataQuality";

        public const string DeletePrincipalSql =
            "DELETE FROM BTRPD_PrincipalSalesOut WHERE SnapshotKey = @SnapshotKey";

        public const string DeleteDataQualitySql =
            "DELETE FROM BTRPD_PrincipalSalesOutDataQuality WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalSalesOutKpi AS target
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
INSERT INTO BTRPD_PrincipalSalesOut (
    PrincipalSalesOutId, SnapshotKey, KpiId, PeriodYear, PeriodMonth,
    SupplierId, SupplierName, SalesOutAmount, LineCount, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalSalesOutId, @SnapshotKey, @KpiId, @PeriodYear, @PeriodMonth,
    @SupplierId, @SupplierName, @SalesOutAmount, @LineCount, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        public const string InsertDataQualitySql = @"
INSERT INTO BTRPD_PrincipalSalesOutDataQuality (
    PrincipalSalesOutDataQualityId, SnapshotKey, PeriodYear, PeriodMonth,
    ExceptionCode, Amount, LineCount, GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalSalesOutDataQualityId, @SnapshotKey, @PeriodYear, @PeriodMonth,
    @ExceptionCode, @Amount, @LineCount, @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalSalesOutSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalSalesOutAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, KpiId, PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalSalesOutKpi
WHERE SnapshotKey = @SnapshotKey";

            const string principalSql = @"
SELECT KpiId, SupplierId, SupplierName, SalesOutAmount, LineCount, SortOrder
FROM BTRPD_PrincipalSalesOut
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string dataQualitySql = @"
SELECT ExceptionCode, Amount, LineCount
FROM BTRPD_PrincipalSalesOutDataQuality
WHERE SnapshotKey = @SnapshotKey
ORDER BY ExceptionCode";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalSalesOutSnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var principals = conn.Query<PrincipalSalesOutRow>(principalSql, new
                {
                    SnapshotKey = PrincipalSalesOutSnapshot.SnapshotKey
                }).ToList();
                var dataQuality = conn.Query<PrincipalSalesOutDataQualityRow>(dataQualitySql, new
                {
                    SnapshotKey = PrincipalSalesOutSnapshot.SnapshotKey
                }).ToList();

                return new PrincipalSalesOutAggregateResult
                {
                    KpiId = kpi.KpiId,
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    GeneratedAt = kpi.GeneratedAt,
                    Principals = principals,
                    DataQuality = dataQuality
                };
            }
        }

        public void ReplaceCurrent(PrincipalSalesOutAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var kpiId = PrincipalKpiCatalog.SalesOutId;
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeletePrincipalSql, new
                    {
                        SnapshotKey = PrincipalSalesOutSnapshot.SnapshotKey
                    }, transaction);
                    conn.Execute(DeleteDataQualitySql, new
                    {
                        SnapshotKey = PrincipalSalesOutSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalSalesOutSnapshot.SnapshotKey,
                        KpiId = kpiId,
                        result.PeriodYear,
                        result.PeriodMonth,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Principals ?? new List<PrincipalSalesOutRow>())
                    {
                        conn.Execute(InsertPrincipalSql, new
                        {
                            PrincipalSalesOutId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalSalesOutSnapshot.SnapshotKey,
                            KpiId = kpiId,
                            result.PeriodYear,
                            result.PeriodMonth,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.SalesOutAmount,
                            row.LineCount,
                            row.SortOrder,
                            result.GeneratedAt,
                            LastRefreshLogId = refreshLogId ?? string.Empty
                        }, transaction);
                    }

                    foreach (var row in result.DataQuality ?? new List<PrincipalSalesOutDataQualityRow>())
                    {
                        conn.Execute(InsertDataQualitySql, new
                        {
                            PrincipalSalesOutDataQualityId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalSalesOutSnapshot.SnapshotKey,
                            result.PeriodYear,
                            result.PeriodMonth,
                            ExceptionCode = row.ExceptionCode ?? string.Empty,
                            row.Amount,
                            row.LineCount,
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
