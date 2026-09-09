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
    public class PrincipalSalesmanContributionSnapshotDal : IPrincipalSalesmanContributionSnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalContributionKpi, BTRPD_PrincipalContribution, BTRPD_PrincipalContributionException";

        public const string DeleteContributionSql =
            "DELETE FROM BTRPD_PrincipalContribution WHERE SnapshotKey = @SnapshotKey";

        public const string DeleteExceptionSql =
            "DELETE FROM BTRPD_PrincipalContributionException WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalContributionKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        SourceSalesOutKpiId = @SourceSalesOutKpiId,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, SourceSalesOutKpiId,
        PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @SourceSalesOutKpiId,
        @PeriodYear, @PeriodMonth, @GeneratedAt, @LastRefreshLogId);";

        public const string InsertContributionSql = @"
INSERT INTO BTRPD_PrincipalContribution (
    PrincipalContributionId, SnapshotKey, SourceSalesOutKpiId,
    PeriodYear, PeriodMonth, SupplierId, SupplierName,
    SalesPersonId, SalesPersonCode, SalesPersonName,
    ContributionAmount, LineCount, HasTargetResponsibility, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalContributionId, @SnapshotKey, @SourceSalesOutKpiId,
    @PeriodYear, @PeriodMonth, @SupplierId, @SupplierName,
    @SalesPersonId, @SalesPersonCode, @SalesPersonName,
    @ContributionAmount, @LineCount, @HasTargetResponsibility, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        public const string InsertExceptionSql = @"
INSERT INTO BTRPD_PrincipalContributionException (
    PrincipalContributionExceptionId, SnapshotKey,
    PeriodYear, PeriodMonth, SupplierId, SupplierName,
    SalesPersonId, SalesPersonCode, SalesPersonName,
    ContributionAmount, LineCount, TargetYear, TargetMonth, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalContributionExceptionId, @SnapshotKey,
    @PeriodYear, @PeriodMonth, @SupplierId, @SupplierName,
    @SalesPersonId, @SalesPersonCode, @SalesPersonName,
    @ContributionAmount, @LineCount, @TargetYear, @TargetMonth, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalSalesmanContributionSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalSalesmanContributionResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, SourceSalesOutKpiId,
       PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalContributionKpi
WHERE SnapshotKey = @SnapshotKey";

            const string contributionSql = @"
SELECT SourceSalesOutKpiId,
       SupplierId, SupplierName, SalesPersonId, SalesPersonCode, SalesPersonName,
       ContributionAmount, LineCount, HasTargetResponsibility, SortOrder
FROM BTRPD_PrincipalContribution
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string exceptionSql = @"
SELECT SupplierId, SupplierName, SalesPersonId, SalesPersonCode, SalesPersonName,
       ContributionAmount, LineCount, TargetYear, TargetMonth, SortOrder
FROM BTRPD_PrincipalContributionException
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalSalesmanContributionSnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var contributions = conn.Query<PrincipalSalesmanContributionRow>(contributionSql, new
                {
                    SnapshotKey = PrincipalSalesmanContributionSnapshot.SnapshotKey
                }).ToList();

                var exceptions = conn.Query<PrincipalSalesmanContributionExceptionRow>(exceptionSql, new
                {
                    SnapshotKey = PrincipalSalesmanContributionSnapshot.SnapshotKey
                }).ToList();

                return new PrincipalSalesmanContributionResult
                {
                    SourceSalesOutKpiId = kpi.SourceSalesOutKpiId,
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    GeneratedAt = kpi.GeneratedAt,
                    Contributions = contributions,
                    Exceptions = exceptions
                };
            }
        }

        public void ReplaceCurrent(PrincipalSalesmanContributionResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var sourceSalesOutKpiId = PrincipalKpiCatalog.SalesOutId;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeleteContributionSql, new
                    {
                        SnapshotKey = PrincipalSalesmanContributionSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(DeleteExceptionSql, new
                    {
                        SnapshotKey = PrincipalSalesmanContributionSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalSalesmanContributionSnapshot.SnapshotKey,
                        SourceSalesOutKpiId = sourceSalesOutKpiId,
                        result.PeriodYear,
                        result.PeriodMonth,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Contributions ?? new List<PrincipalSalesmanContributionRow>())
                    {
                        conn.Execute(InsertContributionSql, new
                        {
                            PrincipalContributionId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalSalesmanContributionSnapshot.SnapshotKey,
                            SourceSalesOutKpiId = sourceSalesOutKpiId,
                            result.PeriodYear,
                            result.PeriodMonth,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            SalesPersonId = row.SalesPersonId ?? string.Empty,
                            SalesPersonCode = row.SalesPersonCode ?? string.Empty,
                            SalesPersonName = row.SalesPersonName ?? string.Empty,
                            row.ContributionAmount,
                            row.LineCount,
                            row.HasTargetResponsibility,
                            row.SortOrder,
                            result.GeneratedAt,
                            LastRefreshLogId = refreshLogId ?? string.Empty
                        }, transaction);
                    }

                    foreach (var row in result.Exceptions ?? new List<PrincipalSalesmanContributionExceptionRow>())
                    {
                        conn.Execute(InsertExceptionSql, new
                        {
                            PrincipalContributionExceptionId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalSalesmanContributionSnapshot.SnapshotKey,
                            result.PeriodYear,
                            result.PeriodMonth,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            SalesPersonId = row.SalesPersonId ?? string.Empty,
                            SalesPersonCode = row.SalesPersonCode ?? string.Empty,
                            SalesPersonName = row.SalesPersonName ?? string.Empty,
                            row.ContributionAmount,
                            row.LineCount,
                            row.TargetYear,
                            row.TargetMonth,
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
            public string SourceSalesOutKpiId { get; set; }

            public int PeriodYear { get; set; }

            public int PeriodMonth { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
