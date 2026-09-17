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
    public class PrincipalMomGrowthSnapshotDal : IPrincipalMomGrowthSnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalMomGrowthKpi, BTRPD_PrincipalMomGrowth";

        public const string DeletePrincipalSql =
            "DELETE FROM BTRPD_PrincipalMomGrowth WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalMomGrowthKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        MomGrowthKpiId = @MomGrowthKpiId,
        SalesOutKpiId = @SalesOutKpiId,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        PriorYear = @PriorYear,
        PriorMonth = @PriorMonth,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, MomGrowthKpiId, SalesOutKpiId,
        PeriodYear, PeriodMonth, PriorYear, PriorMonth, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @MomGrowthKpiId, @SalesOutKpiId,
        @PeriodYear, @PeriodMonth, @PriorYear, @PriorMonth, @GeneratedAt, @LastRefreshLogId);";

        public const string InsertPrincipalSql = @"
INSERT INTO BTRPD_PrincipalMomGrowth (
    PrincipalMomGrowthId, SnapshotKey, MomGrowthKpiId, SalesOutKpiId,
    PeriodYear, PeriodMonth, PriorYear, PriorMonth, SupplierId, SupplierName,
    CurrentSalesOutAmount, PriorSalesOutAmount, MomGrowthPercentage, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalMomGrowthId, @SnapshotKey, @MomGrowthKpiId, @SalesOutKpiId,
    @PeriodYear, @PeriodMonth, @PriorYear, @PriorMonth, @SupplierId, @SupplierName,
    @CurrentSalesOutAmount, @PriorSalesOutAmount, @MomGrowthPercentage, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalMomGrowthSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalMomGrowthResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, MomGrowthKpiId, SalesOutKpiId,
       PeriodYear, PeriodMonth, PriorYear, PriorMonth, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalMomGrowthKpi
WHERE SnapshotKey = @SnapshotKey";

            const string principalSql = @"
SELECT MomGrowthKpiId, SalesOutKpiId,
       SupplierId, SupplierName, CurrentSalesOutAmount, PriorSalesOutAmount, MomGrowthPercentage,
       SortOrder
FROM BTRPD_PrincipalMomGrowth
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalMomGrowthSnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var principals = conn.Query<PrincipalMomGrowthRow>(principalSql, new
                {
                    SnapshotKey = PrincipalMomGrowthSnapshot.SnapshotKey
                }).ToList();

                return new PrincipalMomGrowthResult
                {
                    MomGrowthKpiId = kpi.MomGrowthKpiId,
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

        public void ReplaceCurrent(PrincipalMomGrowthResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var momGrowthKpiId = PrincipalKpiCatalog.MomGrowthId;
            var salesOutKpiId = PrincipalKpiCatalog.SalesOutId;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeletePrincipalSql, new
                    {
                        SnapshotKey = PrincipalMomGrowthSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalMomGrowthSnapshot.SnapshotKey,
                        MomGrowthKpiId = momGrowthKpiId,
                        SalesOutKpiId = salesOutKpiId,
                        result.PeriodYear,
                        result.PeriodMonth,
                        result.PriorYear,
                        result.PriorMonth,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Principals ?? new List<PrincipalMomGrowthRow>())
                    {
                        conn.Execute(InsertPrincipalSql, new
                        {
                            PrincipalMomGrowthId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalMomGrowthSnapshot.SnapshotKey,
                            MomGrowthKpiId = momGrowthKpiId,
                            SalesOutKpiId = salesOutKpiId,
                            result.PeriodYear,
                            result.PeriodMonth,
                            result.PriorYear,
                            result.PriorMonth,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.CurrentSalesOutAmount,
                            row.PriorSalesOutAmount,
                            row.MomGrowthPercentage,
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
            public string MomGrowthKpiId { get; set; }

            public string SalesOutKpiId { get; set; }

            public int PeriodYear { get; set; }

            public int PeriodMonth { get; set; }

            public int PriorYear { get; set; }

            public int PriorMonth { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
