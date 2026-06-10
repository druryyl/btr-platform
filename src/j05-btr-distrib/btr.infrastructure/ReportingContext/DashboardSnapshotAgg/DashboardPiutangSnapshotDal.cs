using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardPiutangSnapshotDal : IDashboardPiutangSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardPiutangSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardPiutangAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, TotalPiutang, TotalCustomer, OverdueCustomer,
       OverduePiutang, AgingOver90Amount, AgingOver90Percent,
       Top10CustomerConcentrationPercent, Top20CustomerConcentrationPercent, LastRefreshLogId
FROM BTRPD_PiutangKpi
WHERE SnapshotKey = @SnapshotKey";

            const string agingSql = @"
SELECT BucketKey, BucketLabel, SortOrder, Amount
FROM BTRPD_PiutangAging
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string topRiskSql = @"
SELECT Rank, CustomerId, CustomerCode, CustomerName, TotalPiutang,
       CurrentAmount, Aging30Amount, Aging60Amount, Aging90Amount, AgingOver90Amount
FROM BTRPD_PiutangTopCustomerRisk
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                var agingRows = conn.Query<AgingRow>(agingSql, new { SnapshotKey }).ToList();
                var topRiskRows = conn.Query<TopCustomerRiskRow>(topRiskSql, new { SnapshotKey }).ToList();

                return new DashboardPiutangAggregateResult
                {
                    TotalPiutang = kpi.TotalPiutang,
                    TotalCustomer = kpi.TotalCustomer,
                    GeneratedAt = kpi.GeneratedAt,
                    OverdueCustomer = kpi.OverdueCustomer,
                    OverduePiutang = kpi.OverduePiutang,
                    AgingOver90Amount = kpi.AgingOver90Amount,
                    AgingOver90Percent = kpi.AgingOver90Percent,
                    Top10CustomerConcentrationPercent = kpi.Top10CustomerConcentrationPercent,
                    Top20CustomerConcentrationPercent = kpi.Top20CustomerConcentrationPercent,
                    AgingBuckets = agingRows.Select(r => new DashboardPiutangAgingBucket
                    {
                        BucketKey = r.BucketKey,
                        BucketLabel = r.BucketLabel,
                        SortOrder = r.SortOrder,
                        Amount = r.Amount
                    }).ToList(),
                    TopCustomerRisk = topRiskRows.Select(r => new DashboardPiutangTopCustomerRiskRow
                    {
                        Rank = r.Rank,
                        CustomerId = r.CustomerId,
                        CustomerCode = r.CustomerCode,
                        CustomerName = r.CustomerName,
                        TotalPiutang = r.TotalPiutang,
                        CurrentAmount = r.CurrentAmount,
                        Aging30Amount = r.Aging30Amount,
                        Aging60Amount = r.Aging60Amount,
                        Aging90Amount = r.Aging90Amount,
                        AgingOver90Amount = r.AgingOver90Amount
                    }).ToList()
                };
            }
        }

        public void ReplaceCurrent(DashboardPiutangAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();

                conn.Execute(
                    "DELETE FROM BTRPD_PiutangAging WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                conn.Execute(
                    "DELETE FROM BTRPD_PiutangCustomerAging WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                conn.Execute(
                    "DELETE FROM BTRPD_PiutangTopCustomerRisk WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                const string mergeKpiSql = @"
MERGE BTRPD_PiutangKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        TotalPiutang = @TotalPiutang,
        TotalCustomer = @TotalCustomer,
        OverdueCustomer = @OverdueCustomer,
        OverduePiutang = @OverduePiutang,
        AgingOver90Amount = @AgingOver90Amount,
        AgingOver90Percent = @AgingOver90Percent,
        Top10CustomerConcentrationPercent = @Top10CustomerConcentrationPercent,
        Top20CustomerConcentrationPercent = @Top20CustomerConcentrationPercent,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (SnapshotKey, GeneratedAt, TotalPiutang, TotalCustomer, OverdueCustomer,
            OverduePiutang, AgingOver90Amount, AgingOver90Percent,
            Top10CustomerConcentrationPercent, Top20CustomerConcentrationPercent, LastRefreshLogId)
    VALUES (@SnapshotKey, @GeneratedAt, @TotalPiutang, @TotalCustomer, @OverdueCustomer,
            @OverduePiutang, @AgingOver90Amount, @AgingOver90Percent,
            @Top10CustomerConcentrationPercent, @Top20CustomerConcentrationPercent, @LastRefreshLogId);";

                conn.Execute(mergeKpiSql, new
                {
                    SnapshotKey,
                    result.GeneratedAt,
                    result.TotalPiutang,
                    result.TotalCustomer,
                    result.OverdueCustomer,
                    result.OverduePiutang,
                    result.AgingOver90Amount,
                    result.AgingOver90Percent,
                    result.Top10CustomerConcentrationPercent,
                    result.Top20CustomerConcentrationPercent,
                    LastRefreshLogId = refreshLogId ?? string.Empty
                });

                const string insertAgingSql = @"
INSERT INTO BTRPD_PiutangAging (
    PiutangAgingId, SnapshotKey, BucketKey, BucketLabel, SortOrder, Amount)
VALUES (
    @PiutangAgingId, @SnapshotKey, @BucketKey, @BucketLabel, @SortOrder, @Amount)";

                foreach (var bucket in result.AgingBuckets ?? new List<DashboardPiutangAgingBucket>())
                {
                    conn.Execute(insertAgingSql, new
                    {
                        PiutangAgingId = Ulid.NewUlid().ToString(),
                        SnapshotKey,
                        bucket.BucketKey,
                        bucket.BucketLabel,
                        bucket.SortOrder,
                        bucket.Amount
                    });
                }

                const string insertCustomerAgingSql = @"
INSERT INTO BTRPD_PiutangCustomerAging (
    PiutangCustomerAgingId, SnapshotKey, CustomerId, CustomerCode, CustomerName,
    CurrentAmount, Aging30Amount, Aging60Amount, Aging90Amount, AgingOver90Amount, LastUpdate)
VALUES (
    @PiutangCustomerAgingId, @SnapshotKey, @CustomerId, @CustomerCode, @CustomerName,
    @CurrentAmount, @Aging30Amount, @Aging60Amount, @Aging90Amount, @AgingOver90Amount, @LastUpdate)";

                foreach (var customer in result.CustomerAging ?? new List<DashboardPiutangCustomerAgingRow>())
                {
                    conn.Execute(insertCustomerAgingSql, new
                    {
                        PiutangCustomerAgingId = Ulid.NewUlid().ToString(),
                        SnapshotKey,
                        customer.CustomerId,
                        CustomerCode = customer.CustomerCode ?? string.Empty,
                        CustomerName = customer.CustomerName ?? string.Empty,
                        customer.CurrentAmount,
                        customer.Aging30Amount,
                        customer.Aging60Amount,
                        customer.Aging90Amount,
                        customer.AgingOver90Amount,
                        customer.LastUpdate
                    });
                }

                const string insertTopRiskSql = @"
INSERT INTO BTRPD_PiutangTopCustomerRisk (
    PiutangTopCustomerRiskId, SnapshotKey, Rank, CustomerId, CustomerCode, CustomerName,
    TotalPiutang, CurrentAmount, Aging30Amount, Aging60Amount, Aging90Amount, AgingOver90Amount)
VALUES (
    @PiutangTopCustomerRiskId, @SnapshotKey, @Rank, @CustomerId, @CustomerCode, @CustomerName,
    @TotalPiutang, @CurrentAmount, @Aging30Amount, @Aging60Amount, @Aging90Amount, @AgingOver90Amount)";

                foreach (var customer in result.TopCustomerRisk ?? new List<DashboardPiutangTopCustomerRiskRow>())
                {
                    conn.Execute(insertTopRiskSql, new
                    {
                        PiutangTopCustomerRiskId = Ulid.NewUlid().ToString(),
                        SnapshotKey,
                        customer.Rank,
                        customer.CustomerId,
                        CustomerCode = customer.CustomerCode ?? string.Empty,
                        CustomerName = customer.CustomerName ?? string.Empty,
                        customer.TotalPiutang,
                        customer.CurrentAmount,
                        customer.Aging30Amount,
                        customer.Aging60Amount,
                        customer.Aging90Amount,
                        customer.AgingOver90Amount
                    });
                }
            }
        }

        private sealed class KpiRow
        {
            public string SnapshotKey { get; set; }
            public DateTime GeneratedAt { get; set; }
            public decimal TotalPiutang { get; set; }
            public int TotalCustomer { get; set; }
            public int OverdueCustomer { get; set; }
            public decimal OverduePiutang { get; set; }
            public decimal AgingOver90Amount { get; set; }
            public decimal? AgingOver90Percent { get; set; }
            public decimal? Top10CustomerConcentrationPercent { get; set; }
            public decimal? Top20CustomerConcentrationPercent { get; set; }
            public string LastRefreshLogId { get; set; }
        }

        private sealed class AgingRow
        {
            public string BucketKey { get; set; }
            public string BucketLabel { get; set; }
            public int SortOrder { get; set; }
            public decimal Amount { get; set; }
        }

        private sealed class TopCustomerRiskRow
        {
            public int Rank { get; set; }
            public string CustomerId { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public decimal TotalPiutang { get; set; }
            public decimal CurrentAmount { get; set; }
            public decimal Aging30Amount { get; set; }
            public decimal Aging60Amount { get; set; }
            public decimal Aging90Amount { get; set; }
            public decimal AgingOver90Amount { get; set; }
        }
    }
}
