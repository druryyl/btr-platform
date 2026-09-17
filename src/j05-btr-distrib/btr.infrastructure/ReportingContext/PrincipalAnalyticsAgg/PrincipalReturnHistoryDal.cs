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
    public class PrincipalReturnHistoryDal : IPrincipalReturnHistoryDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalReturnHistoryKpi, BTRPD_PrincipalReturnHistory";

        public const string DeleteHistorySql =
            "DELETE FROM BTRPD_PrincipalReturnHistory";

        public const string MergeHeaderSql = @"
MERGE BTRPD_PrincipalReturnHistoryKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GoodReturnKpiId = @GoodReturnKpiId,
        BrokenReturnKpiId = @BrokenReturnKpiId,
        TotalReturnKpiId = @TotalReturnKpiId,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GoodReturnKpiId, BrokenReturnKpiId, TotalReturnKpiId,
        GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GoodReturnKpiId, @BrokenReturnKpiId, @TotalReturnKpiId,
        @GeneratedAt, @LastRefreshLogId);";

        public const string InsertHistorySql = @"
INSERT INTO BTRPD_PrincipalReturnHistory (
    PrincipalReturnHistoryId, GoodReturnKpiId, BrokenReturnKpiId, TotalReturnKpiId,
    PeriodYear, PeriodMonth, SupplierId, SupplierName,
    GoodReturnAmount, BrokenReturnAmount, TotalReturnAmount, LineCount, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalReturnHistoryId, @GoodReturnKpiId, @BrokenReturnKpiId, @TotalReturnKpiId,
    @PeriodYear, @PeriodMonth, @SupplierId, @SupplierName,
    @GoodReturnAmount, @BrokenReturnAmount, @TotalReturnAmount, @LineCount, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalReturnHistoryDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalReturnHistoryResult GetHistory()
        {
            const string headerSql = @"
SELECT SnapshotKey, GoodReturnKpiId, BrokenReturnKpiId, TotalReturnKpiId,
       GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalReturnHistoryKpi
WHERE SnapshotKey = @SnapshotKey";

            const string monthSql = @"
SELECT GoodReturnKpiId, BrokenReturnKpiId, TotalReturnKpiId,
       PeriodYear, PeriodMonth, SupplierId, SupplierName,
       GoodReturnAmount, BrokenReturnAmount, TotalReturnAmount,
       LineCount, SortOrder
FROM BTRPD_PrincipalReturnHistory
ORDER BY PeriodYear, PeriodMonth, SortOrder, SupplierId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var header = conn.QueryFirstOrDefault<HeaderRow>(headerSql, new
                {
                    SnapshotKey = PrincipalReturnHistory.SnapshotKey
                });
                if (header is null)
                    return null;

                var months = conn.Query<PrincipalReturnHistoryRow>(monthSql).ToList();
                return new PrincipalReturnHistoryResult
                {
                    GoodReturnKpiId = header.GoodReturnKpiId,
                    BrokenReturnKpiId = header.BrokenReturnKpiId,
                    TotalReturnKpiId = header.TotalReturnKpiId,
                    GeneratedAt = header.GeneratedAt,
                    Months = months
                };
            }
        }

        public void ReplaceHistory(PrincipalReturnHistoryResult result, string refreshLogId)
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
                    conn.Execute(DeleteHistorySql, transaction: transaction);
                    conn.Execute(MergeHeaderSql, new
                    {
                        SnapshotKey = PrincipalReturnHistory.SnapshotKey,
                        GoodReturnKpiId = goodReturnKpiId,
                        BrokenReturnKpiId = brokenReturnKpiId,
                        TotalReturnKpiId = totalReturnKpiId,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Months ?? new List<PrincipalReturnHistoryRow>())
                    {
                        if (string.IsNullOrWhiteSpace(row?.SupplierId))
                            continue;

                        conn.Execute(InsertHistorySql, new
                        {
                            PrincipalReturnHistoryId = Ulid.NewUlid().ToString(),
                            GoodReturnKpiId = goodReturnKpiId,
                            BrokenReturnKpiId = brokenReturnKpiId,
                            TotalReturnKpiId = totalReturnKpiId,
                            row.PeriodYear,
                            row.PeriodMonth,
                            SupplierId = row.SupplierId.Trim(),
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.GoodReturnAmount,
                            row.BrokenReturnAmount,
                            TotalReturnAmount = row.GoodReturnAmount + row.BrokenReturnAmount,
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

        private sealed class HeaderRow
        {
            public string GoodReturnKpiId { get; set; }

            public string BrokenReturnKpiId { get; set; }

            public string TotalReturnKpiId { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
