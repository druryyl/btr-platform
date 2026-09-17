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
    public class PrincipalReturnPercentageSnapshotDal : IPrincipalReturnPercentageSnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalReturnPercentageKpi, BTRPD_PrincipalReturnPercentage";

        public const string DeletePrincipalSql =
            "DELETE FROM BTRPD_PrincipalReturnPercentage WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalReturnPercentageKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        ReturnPercentageKpiId = @ReturnPercentageKpiId,
        SalesOutKpiId = @SalesOutKpiId,
        TotalReturnKpiId = @TotalReturnKpiId,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, ReturnPercentageKpiId, SalesOutKpiId, TotalReturnKpiId,
        PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @ReturnPercentageKpiId, @SalesOutKpiId, @TotalReturnKpiId,
        @PeriodYear, @PeriodMonth, @GeneratedAt, @LastRefreshLogId);";

        public const string InsertPrincipalSql = @"
INSERT INTO BTRPD_PrincipalReturnPercentage (
    PrincipalReturnPercentageId, SnapshotKey, ReturnPercentageKpiId, SalesOutKpiId, TotalReturnKpiId,
    PeriodYear, PeriodMonth, SupplierId, SupplierName,
    TotalReturnAmount, SalesOutAmount, ReturnPercentage, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalReturnPercentageId, @SnapshotKey, @ReturnPercentageKpiId, @SalesOutKpiId, @TotalReturnKpiId,
    @PeriodYear, @PeriodMonth, @SupplierId, @SupplierName,
    @TotalReturnAmount, @SalesOutAmount, @ReturnPercentage, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalReturnPercentageSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalReturnPercentageResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, ReturnPercentageKpiId, SalesOutKpiId, TotalReturnKpiId,
       PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalReturnPercentageKpi
WHERE SnapshotKey = @SnapshotKey";

            const string principalSql = @"
SELECT ReturnPercentageKpiId, SalesOutKpiId, TotalReturnKpiId,
       SupplierId, SupplierName, TotalReturnAmount, SalesOutAmount, ReturnPercentage,
       SortOrder
FROM BTRPD_PrincipalReturnPercentage
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalReturnPercentageSnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var principals = conn.Query<PrincipalReturnPercentageRow>(principalSql, new
                {
                    SnapshotKey = PrincipalReturnPercentageSnapshot.SnapshotKey
                }).ToList();

                return new PrincipalReturnPercentageResult
                {
                    ReturnPercentageKpiId = kpi.ReturnPercentageKpiId,
                    SalesOutKpiId = kpi.SalesOutKpiId,
                    TotalReturnKpiId = kpi.TotalReturnKpiId,
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    GeneratedAt = kpi.GeneratedAt,
                    Principals = principals
                };
            }
        }

        public void ReplaceCurrent(PrincipalReturnPercentageResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var returnPercentageKpiId = PrincipalKpiCatalog.ReturnPercentageId;
            var salesOutKpiId = PrincipalKpiCatalog.SalesOutId;
            var totalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeletePrincipalSql, new
                    {
                        SnapshotKey = PrincipalReturnPercentageSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalReturnPercentageSnapshot.SnapshotKey,
                        ReturnPercentageKpiId = returnPercentageKpiId,
                        SalesOutKpiId = salesOutKpiId,
                        TotalReturnKpiId = totalReturnKpiId,
                        result.PeriodYear,
                        result.PeriodMonth,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Principals ?? new List<PrincipalReturnPercentageRow>())
                    {
                        conn.Execute(InsertPrincipalSql, new
                        {
                            PrincipalReturnPercentageId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalReturnPercentageSnapshot.SnapshotKey,
                            ReturnPercentageKpiId = returnPercentageKpiId,
                            SalesOutKpiId = salesOutKpiId,
                            TotalReturnKpiId = totalReturnKpiId,
                            result.PeriodYear,
                            result.PeriodMonth,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.TotalReturnAmount,
                            row.SalesOutAmount,
                            row.ReturnPercentage,
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
            public string ReturnPercentageKpiId { get; set; }

            public string SalesOutKpiId { get; set; }

            public string TotalReturnKpiId { get; set; }

            public int PeriodYear { get; set; }

            public int PeriodMonth { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
