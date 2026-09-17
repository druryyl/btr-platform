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
    public class PrincipalCustomerCoverageSnapshotDal : IPrincipalCustomerCoverageSnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalCustomerCoverageKpi, BTRPD_PrincipalCustomerCoverage";

        public const string DeletePrincipalSql =
            "DELETE FROM BTRPD_PrincipalCustomerCoverage WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalCustomerCoverageKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        CustomerCoverageKpiId = @CustomerCoverageKpiId,
        AsOfDate = @AsOfDate,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, CustomerCoverageKpiId,
        AsOfDate, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @CustomerCoverageKpiId,
        @AsOfDate, @GeneratedAt, @LastRefreshLogId);";

        public const string InsertPrincipalSql = @"
INSERT INTO BTRPD_PrincipalCustomerCoverage (
    PrincipalCustomerCoverageId, SnapshotKey, CustomerCoverageKpiId,
    AsOfDate, SupplierId, SupplierName,
    ActiveCustomerCount, TotalCustomerCount, CoveragePercentage, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalCustomerCoverageId, @SnapshotKey, @CustomerCoverageKpiId,
    @AsOfDate, @SupplierId, @SupplierName,
    @ActiveCustomerCount, @TotalCustomerCount, @CoveragePercentage, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalCustomerCoverageSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalCustomerCoverageResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, CustomerCoverageKpiId,
       AsOfDate, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalCustomerCoverageKpi
WHERE SnapshotKey = @SnapshotKey";

            const string principalSql = @"
SELECT CustomerCoverageKpiId,
       SupplierId, SupplierName, ActiveCustomerCount,
       TotalCustomerCount, CoveragePercentage, SortOrder
FROM BTRPD_PrincipalCustomerCoverage
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalCustomerCoverageSnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var principals = conn.Query<PrincipalCustomerCoverageRow>(principalSql, new
                {
                    SnapshotKey = PrincipalCustomerCoverageSnapshot.SnapshotKey
                }).ToList();

                return new PrincipalCustomerCoverageResult
                {
                    CustomerCoverageKpiId = kpi.CustomerCoverageKpiId,
                    AsOfDate = kpi.AsOfDate,
                    GeneratedAt = kpi.GeneratedAt,
                    Principals = principals
                };
            }
        }

        public void ReplaceCurrent(PrincipalCustomerCoverageResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var customerCoverageKpiId = PrincipalKpiCatalog.CustomerCoverageId;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeletePrincipalSql, new
                    {
                        SnapshotKey = PrincipalCustomerCoverageSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalCustomerCoverageSnapshot.SnapshotKey,
                        CustomerCoverageKpiId = customerCoverageKpiId,
                        result.AsOfDate,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Principals ?? new List<PrincipalCustomerCoverageRow>())
                    {
                        conn.Execute(InsertPrincipalSql, new
                        {
                            PrincipalCustomerCoverageId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalCustomerCoverageSnapshot.SnapshotKey,
                            CustomerCoverageKpiId = customerCoverageKpiId,
                            result.AsOfDate,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.ActiveCustomerCount,
                            row.TotalCustomerCount,
                            CoveragePercentage = row.CoveragePercentage,
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
            public string CustomerCoverageKpiId { get; set; }

            public DateTime AsOfDate { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
