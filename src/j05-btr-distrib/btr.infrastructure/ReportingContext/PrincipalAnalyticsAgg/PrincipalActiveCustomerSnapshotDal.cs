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
    public class PrincipalActiveCustomerSnapshotDal : IPrincipalActiveCustomerSnapshotDal
    {
        public const string WrittenTables =
            "BTRPD_PrincipalActiveCustomerKpi, BTRPD_PrincipalActiveCustomer";

        public const string DeletePrincipalSql =
            "DELETE FROM BTRPD_PrincipalActiveCustomer WHERE SnapshotKey = @SnapshotKey";

        public const string MergeKpiSql = @"
MERGE BTRPD_PrincipalActiveCustomerKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        ActiveCustomerKpiId = @ActiveCustomerKpiId,
        AsOfDate = @AsOfDate,
        GeneratedAt = @GeneratedAt,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, ActiveCustomerKpiId,
        AsOfDate, GeneratedAt, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @ActiveCustomerKpiId,
        @AsOfDate, @GeneratedAt, @LastRefreshLogId);";

        public const string InsertPrincipalSql = @"
INSERT INTO BTRPD_PrincipalActiveCustomer (
    PrincipalActiveCustomerId, SnapshotKey, ActiveCustomerKpiId,
    AsOfDate, SupplierId, SupplierName,
    ActiveCustomerCount, SortOrder,
    GeneratedAt, LastRefreshLogId)
VALUES (
    @PrincipalActiveCustomerId, @SnapshotKey, @ActiveCustomerKpiId,
    @AsOfDate, @SupplierId, @SupplierName,
    @ActiveCustomerCount, @SortOrder,
    @GeneratedAt, @LastRefreshLogId)";

        private readonly DatabaseOptions _opt;

        public PrincipalActiveCustomerSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public PrincipalActiveCustomerResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, ActiveCustomerKpiId,
       AsOfDate, GeneratedAt, LastRefreshLogId
FROM BTRPD_PrincipalActiveCustomerKpi
WHERE SnapshotKey = @SnapshotKey";

            const string principalSql = @"
SELECT ActiveCustomerKpiId,
       SupplierId, SupplierName, ActiveCustomerCount,
       SortOrder
FROM BTRPD_PrincipalActiveCustomer
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new
                {
                    SnapshotKey = PrincipalActiveCustomerSnapshot.SnapshotKey
                });
                if (kpi is null)
                    return null;

                var principals = conn.Query<PrincipalActiveCustomerRow>(principalSql, new
                {
                    SnapshotKey = PrincipalActiveCustomerSnapshot.SnapshotKey
                }).ToList();

                return new PrincipalActiveCustomerResult
                {
                    ActiveCustomerKpiId = kpi.ActiveCustomerKpiId,
                    AsOfDate = kpi.AsOfDate,
                    GeneratedAt = kpi.GeneratedAt,
                    Principals = principals
                };
            }
        }

        public void ReplaceCurrent(PrincipalActiveCustomerResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var activeCustomerKpiId = PrincipalKpiCatalog.ActiveCustomerCountId;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    conn.Execute(DeletePrincipalSql, new
                    {
                        SnapshotKey = PrincipalActiveCustomerSnapshot.SnapshotKey
                    }, transaction);

                    conn.Execute(MergeKpiSql, new
                    {
                        SnapshotKey = PrincipalActiveCustomerSnapshot.SnapshotKey,
                        ActiveCustomerKpiId = activeCustomerKpiId,
                        result.AsOfDate,
                        result.GeneratedAt,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, transaction);

                    foreach (var row in result.Principals ?? new List<PrincipalActiveCustomerRow>())
                    {
                        conn.Execute(InsertPrincipalSql, new
                        {
                            PrincipalActiveCustomerId = Ulid.NewUlid().ToString(),
                            SnapshotKey = PrincipalActiveCustomerSnapshot.SnapshotKey,
                            ActiveCustomerKpiId = activeCustomerKpiId,
                            result.AsOfDate,
                            SupplierId = row.SupplierId ?? string.Empty,
                            SupplierName = row.SupplierName ?? string.Empty,
                            row.ActiveCustomerCount,
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
            public string ActiveCustomerKpiId { get; set; }

            public DateTime AsOfDate { get; set; }

            public DateTime GeneratedAt { get; set; }
        }
    }
}
