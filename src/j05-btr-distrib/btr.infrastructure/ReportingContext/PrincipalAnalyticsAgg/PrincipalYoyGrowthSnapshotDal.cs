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
    public class PrincipalYoyGrowthSnapshotDal : IPrincipalYoyGrowthSnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalYoyGrowthKpi, BTRPD_PrincipalYoyGrowth";

        public const string DeletePrincipalSql =
            "DELETE FROM BTRPD_PrincipalYoyGrowth WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalYoyGrowthKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        YoyGrowthKpiId = @YoyGrowthKpiId,
        SalesOutKpiId = @SalesOutKpiId,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        PriorYear = @PriorYear,
        PriorMonth = @PriorMonth,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, YoyGrowthKpiId, SalesOutKpiId,
        PeriodYear, PeriodMonth, PriorYear, PriorMonth, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @YoyGrowthKpiId, @SalesOutKpiId,
        @PeriodYear, @PeriodMonth, @PriorYear, @PriorMonth, @GeneratedAt, @LastRefreshLogId);";

        public const string InsertPrincipalSql = @"
INSERT INTO BTRPD_PrincipalYoyGrowth (
    PrincipalYoyGrowthId, SnapshotKey, YoyGrowthKpiId, SalesOutKpiId,
    PeriodYear, PeriodMonth, PriorYear, PriorMonth, SupplierId, SupplierName,
    CurrentSalesOutAmount, PriorSalesOutAmount, YoyGrowthPercentage, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalYoyGrowthId, @SnapshotKey, @YoyGrowthKpiId, @SalesOutKpiId,
    @PeriodYear, @PeriodMonth, @PriorYear, @PriorMonth, @SupplierId, @SupplierName,
    @CurrentSalesOutAmount, @PriorSalesOutAmount, @YoyGrowthPercentage, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalYoyGrowthSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalYoyGrowthResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, YoyGrowthKpiId, SalesOutKpiId,
       PeriodYear, PeriodMonth, PriorYear, PriorMonth, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalYoyGrowthKpi
WHERE SnapshotKey = @SnapshotKey";

            const string principalSql = @"
SELECT YoyGrowthKpiId, SalesOutKpiId,
       SupplierId, SupplierName, CurrentSalesOutAmount, PriorSalesOutAmount, YoyGrowthPercentage,
       SortOrder
FROM BTRPD_PrincipalYoyGrowth
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalYoyGrowthSnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var principals = conn.Query<PrincipalYoyGrowthRow>(principalSql, new
                {
                    SnapshotKey = PrincipalYoyGrowthSnapshot.SnapshotKey
                }).ToList();

                return new PrincipalYoyGrowthResult
                {
                    YoyGrowthKpiId = kpi.YoyGrowthKpiId,
                    SalesOutKpiId = kpi.SalesOutKpiId,
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    PriorYear = kpi.PriorYear,
                    PriorMonth = kpi.PriorMonth,
                    GeneratedAt = kpi.GeneratedAt,
                    Principals = principals
                };
            }
        }

        public void ReplaceCurrent(PrincipalYoyGrowthResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var yoyGrowthKpiId = PrincipalKpiCatalog.YoyGrowthId;
            var salesOutKpiId = PrincipalKpiCatalog.SalesOutId;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeletePrincipalSql, new
                    {
                        SnapshotKey = PrincipalYoyGrowthSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalYoyGrowthSnapshot.SnapshotKey,
                        YoyGrowthKpiId = yoyGrowthKpiId,
                        SalesOutKpiId = salesOutKpiId,
                        result.PeriodYear,
                        result.PeriodMonth,
                        result.PriorYear,
                        result.PriorMonth,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Principals ?? new List<PrincipalYoyGrowthRow>())
                    {
                        conn.Execute(InsertPrincipalSql, new
                        {
                            PrincipalYoyGrowthId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalYoyGrowthSnapshot.SnapshotKey,
                            YoyGrowthKpiId = yoyGrowthKpiId,
                            SalesOutKpiId = salesOutKpiId,
                            result.PeriodYear,
                            result.PeriodMonth,
                            result.PriorYear,
                            result.PriorMonth,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.CurrentSalesOutAmount,
                            row.PriorSalesOutAmount,
                            row.YoyGrowthPercentage,
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
            public string YoyGrowthKpiId { get; set; }

            public string SalesOutKpiId { get; set; }

            public int PeriodYear { get; set; }

            public int PeriodMonth { get; set; }

            public int PriorYear { get; set; }

            public int PriorMonth { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
