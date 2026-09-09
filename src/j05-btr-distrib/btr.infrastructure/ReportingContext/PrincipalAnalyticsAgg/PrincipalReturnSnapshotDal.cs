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
    public class PrincipalReturnSnapshotDal : IPrincipalReturnSnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalReturnKpi, BTRPD_PrincipalReturn";

        public const string DeletePrincipalSql =
            "DELETE FROM BTRPD_PrincipalReturn WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalReturnKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GoodReturnKpiId = @GoodReturnKpiId,
        BrokenReturnKpiId = @BrokenReturnKpiId,
        TotalReturnKpiId = @TotalReturnKpiId,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GoodReturnKpiId, BrokenReturnKpiId, TotalReturnKpiId,
        PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GoodReturnKpiId, @BrokenReturnKpiId, @TotalReturnKpiId,
        @PeriodYear, @PeriodMonth, @GeneratedAt, @LastRefreshLogId);";

        public const string InsertPrincipalSql = @"
INSERT INTO BTRPD_PrincipalReturn (
    PrincipalReturnId, SnapshotKey, GoodReturnKpiId, BrokenReturnKpiId, TotalReturnKpiId,
    PeriodYear, PeriodMonth, SupplierId, SupplierName,
    GoodReturnAmount, BrokenReturnAmount, TotalReturnAmount, LineCount, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalReturnId, @SnapshotKey, @GoodReturnKpiId, @BrokenReturnKpiId, @TotalReturnKpiId,
    @PeriodYear, @PeriodMonth, @SupplierId, @SupplierName,
    @GoodReturnAmount, @BrokenReturnAmount, @TotalReturnAmount, @LineCount, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalReturnSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalReturnAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GoodReturnKpiId, BrokenReturnKpiId, TotalReturnKpiId,
       PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalReturnKpi
WHERE SnapshotKey = @SnapshotKey";

            const string principalSql = @"
SELECT GoodReturnKpiId, BrokenReturnKpiId, TotalReturnKpiId,
       SupplierId, SupplierName, GoodReturnAmount, BrokenReturnAmount, TotalReturnAmount,
       LineCount, SortOrder
FROM BTRPD_PrincipalReturn
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalReturnSnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var principals = conn.Query<PrincipalReturnRow>(principalSql, new
                {
                    SnapshotKey = PrincipalReturnSnapshot.SnapshotKey
                }).ToList();

                return new PrincipalReturnAggregateResult
                {
                    GoodReturnKpiId = kpi.GoodReturnKpiId,
                    BrokenReturnKpiId = kpi.BrokenReturnKpiId,
                    TotalReturnKpiId = kpi.TotalReturnKpiId,
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    GeneratedAt = kpi.GeneratedAt,
                    Principals = principals
                };
            }
        }

        public void ReplaceCurrent(PrincipalReturnAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var goodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId;
            var brokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId;
            var totalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeletePrincipalSql, new
                    {
                        SnapshotKey = PrincipalReturnSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalReturnSnapshot.SnapshotKey,
                        GoodReturnKpiId = goodReturnKpiId,
                        BrokenReturnKpiId = brokenReturnKpiId,
                        TotalReturnKpiId = totalReturnKpiId,
                        result.PeriodYear,
                        result.PeriodMonth,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Principals ?? new List<PrincipalReturnRow>())
                    {
                        conn.Execute(InsertPrincipalSql, new
                        {
                            PrincipalReturnId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalReturnSnapshot.SnapshotKey,
                            GoodReturnKpiId = goodReturnKpiId,
                            BrokenReturnKpiId = brokenReturnKpiId,
                            TotalReturnKpiId = totalReturnKpiId,
                            result.PeriodYear,
                            result.PeriodMonth,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.GoodReturnAmount,
                            row.BrokenReturnAmount,
                            row.TotalReturnAmount,
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
            public string GoodReturnKpiId { get; set; }

            public string BrokenReturnKpiId { get; set; }

            public string TotalReturnKpiId { get; set; }

            public int PeriodYear { get; set; }

            public int PeriodMonth { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
