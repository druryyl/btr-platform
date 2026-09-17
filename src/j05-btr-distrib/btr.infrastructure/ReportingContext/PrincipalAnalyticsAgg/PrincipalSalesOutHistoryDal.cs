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
    public class PrincipalSalesOutHistoryDal : IPrincipalSalesOutHistoryDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalSalesOutHistoryKpi, BTRPD_PrincipalSalesOutHistory";

        public const string DeleteHistorySql =
            "DELETE FROM BTRPD_PrincipalSalesOutHistory";

        public const string MergeHeaderSql = @"
MERGE BTRPD_PrincipalSalesOutHistoryKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        KpiId = @KpiId,
        HistoricalLimitation = @HistoricalLimitation,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, KpiId, HistoricalLimitation, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @KpiId, @HistoricalLimitation, @GeneratedAt, @LastRefreshLogId);";

        public const string InsertHistorySql = @"
INSERT INTO BTRPD_PrincipalSalesOutHistory (
    PrincipalSalesOutHistoryId, KpiId, PeriodYear, PeriodMonth,
    SupplierId, SupplierName, SalesOutAmount, LineCount, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalSalesOutHistoryId, @KpiId, @PeriodYear, @PeriodMonth,
    @SupplierId, @SupplierName, @SalesOutAmount, @LineCount, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalSalesOutHistoryDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalSalesOutHistoryResult GetHistory()
        {
            const string headerSql = @"
SELECT SnapshotKey, KpiId, HistoricalLimitation, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalSalesOutHistoryKpi
WHERE SnapshotKey = @SnapshotKey";

            const string monthSql = @"
SELECT KpiId, PeriodYear, PeriodMonth, SupplierId, SupplierName, SalesOutAmount, LineCount, SortOrder
FROM BTRPD_PrincipalSalesOutHistory
ORDER BY PeriodYear, PeriodMonth, SortOrder, SupplierId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var header = conn.QueryFirstOrDefault<HeaderRow>(headerSql, new
                {
                    SnapshotKey = PrincipalSalesOutHistory.SnapshotKey
                });
                if (header is null)
                    return null;

                var months = conn.Query<PrincipalSalesOutHistoryRow>(monthSql).ToList();
                return new PrincipalSalesOutHistoryResult
                {
                    KpiId = header.KpiId,
                    HistoricalLimitation = header.HistoricalLimitation,
                    GeneratedAt = header.GeneratedAt,
                    Months = months
                };
            }
        }

        public void ReplaceHistory(PrincipalSalesOutHistoryResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var kpiId = PrincipalKpiCatalog.SalesOutId;
            var limitation = string.IsNullOrWhiteSpace(result.HistoricalLimitation)
                ? PrincipalSalesOutHistory.HistoricalLimitation
                : result.HistoricalLimitation;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeleteHistorySql, transaction: transaction);
                    conn.Execute(MergeHeaderSql, new
                    {
                        SnapshotKey = PrincipalSalesOutHistory.SnapshotKey,
                        KpiId = kpiId,
                        HistoricalLimitation = limitation,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Months ?? new List<PrincipalSalesOutHistoryRow>())
                    {
                        if (string.IsNullOrWhiteSpace(row?.SupplierId))
                            continue;

                        conn.Execute(InsertHistorySql, new
                        {
                            PrincipalSalesOutHistoryId = Ulid.NewUlid().ToString(),
                            KpiId = kpiId,
                            row.PeriodYear,
                            row.PeriodMonth,
                            SupplierId = row.SupplierId.Trim(),
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.SalesOutAmount,
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
            public string KpiId { get; set; }

            public string HistoricalLimitation { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
