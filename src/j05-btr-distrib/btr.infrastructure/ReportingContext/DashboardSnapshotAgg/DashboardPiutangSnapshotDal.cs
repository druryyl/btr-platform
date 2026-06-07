using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Application;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardPiutangSnapshotDal : IDashboardPiutangSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;
        private readonly INunaCounterBL _counter;

        public DashboardPiutangSnapshotDal(
            IOptions<DatabaseOptions> opt,
            INunaCounterBL counter)
        {
            _opt = opt.Value;
            _counter = counter;
        }

        public DashboardPiutangAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, TotalPiutang, TotalCustomer, OverdueCustomer, LastRefreshLogId
FROM BTR_PortalDashboardPiutangKpi
WHERE SnapshotKey = @SnapshotKey";

            const string agingSql = @"
SELECT BucketKey, BucketLabel, SortOrder, Amount
FROM BTR_PortalDashboardPiutangAging
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string topCustomerSql = @"
SELECT Rank, CustomerName, OutstandingBalance
FROM BTR_PortalDashboardPiutangTopCustomer
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                var agingRows = conn.Query<AgingRow>(agingSql, new { SnapshotKey }).ToList();
                var topRows = conn.Query<TopCustomerRow>(topCustomerSql, new { SnapshotKey }).ToList();

                return new DashboardPiutangAggregateResult
                {
                    TotalPiutang = kpi.TotalPiutang,
                    TotalCustomer = kpi.TotalCustomer,
                    GeneratedAt = kpi.GeneratedAt,
                    OverdueCustomer = kpi.OverdueCustomer,
                    AgingBuckets = agingRows.Select(r => new DashboardPiutangAgingBucket
                    {
                        BucketKey = r.BucketKey,
                        BucketLabel = r.BucketLabel,
                        SortOrder = r.SortOrder,
                        Amount = r.Amount
                    }).ToList(),
                    TopCustomers = topRows.Select(r => new DashboardPiutangTopCustomer
                    {
                        Rank = r.Rank,
                        CustomerName = r.CustomerName,
                        OutstandingBalance = r.OutstandingBalance
                    }).ToList()
                };
            }
        }

        public void ReplaceCurrent(DashboardPiutangAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new System.ArgumentNullException(nameof(result));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();

                conn.Execute(
                    "DELETE FROM BTR_PortalDashboardPiutangAging WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                conn.Execute(
                    "DELETE FROM BTR_PortalDashboardPiutangTopCustomer WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                const string mergeKpiSql = @"
MERGE BTR_PortalDashboardPiutangKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        TotalPiutang = @TotalPiutang,
        TotalCustomer = @TotalCustomer,
        OverdueCustomer = @OverdueCustomer,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (SnapshotKey, GeneratedAt, TotalPiutang, TotalCustomer, OverdueCustomer, LastRefreshLogId)
    VALUES (@SnapshotKey, @GeneratedAt, @TotalPiutang, @TotalCustomer, @OverdueCustomer, @LastRefreshLogId);";

                conn.Execute(mergeKpiSql, new
                {
                    SnapshotKey,
                    result.GeneratedAt,
                    result.TotalPiutang,
                    result.TotalCustomer,
                    result.OverdueCustomer,
                    LastRefreshLogId = refreshLogId ?? string.Empty
                });

                const string insertAgingSql = @"
INSERT INTO BTR_PortalDashboardPiutangAging (
    PiutangAgingId, SnapshotKey, BucketKey, BucketLabel, SortOrder, Amount)
VALUES (
    @PiutangAgingId, @SnapshotKey, @BucketKey, @BucketLabel, @SortOrder, @Amount)";

                foreach (var bucket in result.AgingBuckets ?? new List<DashboardPiutangAgingBucket>())
                {
                    conn.Execute(insertAgingSql, new
                    {
                        PiutangAgingId = _counter.Generate("PDA", IDFormatEnum.PFnnn),
                        SnapshotKey,
                        bucket.BucketKey,
                        bucket.BucketLabel,
                        bucket.SortOrder,
                        bucket.Amount
                    });
                }

                const string insertTopSql = @"
INSERT INTO BTR_PortalDashboardPiutangTopCustomer (
    PiutangTopCustomerId, SnapshotKey, Rank, CustomerName, OutstandingBalance)
VALUES (
    @PiutangTopCustomerId, @SnapshotKey, @Rank, @CustomerName, @OutstandingBalance)";

                foreach (var customer in result.TopCustomers ?? new List<DashboardPiutangTopCustomer>())
                {
                    conn.Execute(insertTopSql, new
                    {
                        PiutangTopCustomerId = _counter.Generate("PDT", IDFormatEnum.PFnnn),
                        SnapshotKey,
                        customer.Rank,
                        CustomerName = customer.CustomerName ?? string.Empty,
                        customer.OutstandingBalance
                    });
                }
            }
        }

        private sealed class KpiRow
        {
            public string SnapshotKey { get; set; }
            public System.DateTime GeneratedAt { get; set; }
            public decimal TotalPiutang { get; set; }
            public int TotalCustomer { get; set; }
            public int OverdueCustomer { get; set; }
            public string LastRefreshLogId { get; set; }
        }

        private sealed class AgingRow
        {
            public string BucketKey { get; set; }
            public string BucketLabel { get; set; }
            public int SortOrder { get; set; }
            public decimal Amount { get; set; }
        }

        private sealed class TopCustomerRow
        {
            public int Rank { get; set; }
            public string CustomerName { get; set; }
            public decimal OutstandingBalance { get; set; }
        }
    }
}
