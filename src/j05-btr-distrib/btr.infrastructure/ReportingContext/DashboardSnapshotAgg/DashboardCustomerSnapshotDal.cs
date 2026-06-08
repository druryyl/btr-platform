using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Application;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardCustomerSnapshotDal : IDashboardCustomerSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;
        private readonly INunaCounterBL _counter;

        public DashboardCustomerSnapshotDal(
            IOptions<DatabaseOptions> opt,
            INunaCounterBL counter)
        {
            _opt = opt.Value;
            _counter = counter;
        }

        public DashboardCustomerAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, TotalOmzet, TotalPiutang,
       ActiveCustomerCount, DormantCustomerCount, OverdueCustomerCount, PlafondBreachCount,
       SuspendedWithSalesCount, AgingOver90Amount, TopOmzetCustomerPercent, TopPiutangCustomerPercent,
       LastRefreshLogId
FROM BTR_PortalDashboardCustomerKpi
WHERE SnapshotKey = @SnapshotKey";

            const string topOmzetSql = @"
SELECT Rank, CustomerCode, CustomerName, OmzetAmount, PercentOfTotal
FROM BTR_PortalDashboardCustomerTopOmzet
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topPiutangSql = @"
SELECT Rank, CustomerCode, CustomerName, OutstandingBalance, PercentOfTotal
FROM BTR_PortalDashboardCustomerTopPiutang
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string attentionSql = @"
SELECT CustomerCode, CustomerName, SignalKey, SignalLabel, ValueAmount, ValueText, WilayahName, SortOrder
FROM BTR_PortalDashboardCustomerAttention
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string segmentationSql = @"
SELECT SegmentType, SegmentKey, SegmentLabel, CustomerCount, ActiveCount, DormantCount, SortOrder
FROM BTR_PortalDashboardCustomerSegmentation
WHERE SnapshotKey = @SnapshotKey
ORDER BY SegmentType, SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                var topOmzet = conn.Query<TopOmzetRow>(topOmzetSql, new { SnapshotKey }).ToList();
                var topPiutang = conn.Query<TopPiutangRow>(topPiutangSql, new { SnapshotKey }).ToList();
                var attention = conn.Query<AttentionRow>(attentionSql, new { SnapshotKey }).ToList();
                var segmentation = conn.Query<SegmentationRow>(segmentationSql, new { SnapshotKey }).ToList();

                return new DashboardCustomerAggregateResult
                {
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    TotalOmzet = kpi.TotalOmzet,
                    TotalPiutang = kpi.TotalPiutang,
                    ActiveCustomerCount = kpi.ActiveCustomerCount,
                    DormantCustomerCount = kpi.DormantCustomerCount,
                    OverdueCustomerCount = kpi.OverdueCustomerCount,
                    PlafondBreachCount = kpi.PlafondBreachCount,
                    SuspendedWithSalesCount = kpi.SuspendedWithSalesCount,
                    AgingOver90Amount = kpi.AgingOver90Amount,
                    TopOmzetCustomerPercent = kpi.TopOmzetCustomerPercent,
                    TopPiutangCustomerPercent = kpi.TopPiutangCustomerPercent,
                    GeneratedAt = kpi.GeneratedAt,
                    TopOmzet = topOmzet.Select(r => new DashboardCustomerTopOmzetRow
                    {
                        Rank = r.Rank,
                        CustomerCode = r.CustomerCode,
                        CustomerName = r.CustomerName,
                        OmzetAmount = r.OmzetAmount,
                        PercentOfTotal = r.PercentOfTotal
                    }).ToList(),
                    TopPiutang = topPiutang.Select(r => new DashboardCustomerTopPiutangRow
                    {
                        Rank = r.Rank,
                        CustomerCode = r.CustomerCode,
                        CustomerName = r.CustomerName,
                        OutstandingBalance = r.OutstandingBalance,
                        PercentOfTotal = r.PercentOfTotal
                    }).ToList(),
                    AttentionList = attention.Select(r => new DashboardCustomerAttentionRow
                    {
                        CustomerCode = r.CustomerCode,
                        CustomerName = r.CustomerName,
                        SignalKey = r.SignalKey,
                        SignalLabel = r.SignalLabel,
                        ValueAmount = r.ValueAmount,
                        ValueText = r.ValueText,
                        WilayahName = r.WilayahName,
                        SortOrder = r.SortOrder
                    }).ToList(),
                    Segmentation = segmentation.Select(r => new DashboardCustomerSegmentationRow
                    {
                        SegmentType = r.SegmentType,
                        SegmentKey = r.SegmentKey,
                        SegmentLabel = r.SegmentLabel,
                        CustomerCount = r.CustomerCount,
                        ActiveCount = r.ActiveCount,
                        DormantCount = r.DormantCount,
                        SortOrder = r.SortOrder
                    }).ToList()
                };
            }
        }

        public void ReplaceCurrent(DashboardCustomerAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new System.ArgumentNullException(nameof(result));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        ReplaceCurrentCore(conn, transaction, result, refreshLogId);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void ReplaceCurrentCore(
            SqlConnection conn,
            SqlTransaction transaction,
            DashboardCustomerAggregateResult result,
            string refreshLogId)
        {
                conn.Execute(
                    "DELETE FROM BTR_PortalDashboardCustomerTopOmzet WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey },
                    transaction);
                conn.Execute(
                    "DELETE FROM BTR_PortalDashboardCustomerTopPiutang WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey },
                    transaction);
                conn.Execute(
                    "DELETE FROM BTR_PortalDashboardCustomerAttention WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey },
                    transaction);
                conn.Execute(
                    "DELETE FROM BTR_PortalDashboardCustomerSegmentation WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey },
                    transaction);

                const string mergeKpiSql = @"
MERGE BTR_PortalDashboardCustomerKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        TotalOmzet = @TotalOmzet,
        TotalPiutang = @TotalPiutang,
        ActiveCustomerCount = @ActiveCustomerCount,
        DormantCustomerCount = @DormantCustomerCount,
        OverdueCustomerCount = @OverdueCustomerCount,
        PlafondBreachCount = @PlafondBreachCount,
        SuspendedWithSalesCount = @SuspendedWithSalesCount,
        AgingOver90Amount = @AgingOver90Amount,
        TopOmzetCustomerPercent = @TopOmzetCustomerPercent,
        TopPiutangCustomerPercent = @TopPiutangCustomerPercent,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, TotalOmzet, TotalPiutang,
        ActiveCustomerCount, DormantCustomerCount, OverdueCustomerCount, PlafondBreachCount,
        SuspendedWithSalesCount, AgingOver90Amount, TopOmzetCustomerPercent, TopPiutangCustomerPercent,
        LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @PeriodYear, @PeriodMonth, @TotalOmzet, @TotalPiutang,
        @ActiveCustomerCount, @DormantCustomerCount, @OverdueCustomerCount, @PlafondBreachCount,
        @SuspendedWithSalesCount, @AgingOver90Amount, @TopOmzetCustomerPercent, @TopPiutangCustomerPercent,
        @LastRefreshLogId);";

                conn.Execute(mergeKpiSql, new
                {
                    SnapshotKey,
                    result.GeneratedAt,
                    result.PeriodYear,
                    result.PeriodMonth,
                    result.TotalOmzet,
                    result.TotalPiutang,
                    result.ActiveCustomerCount,
                    result.DormantCustomerCount,
                    result.OverdueCustomerCount,
                    result.PlafondBreachCount,
                    result.SuspendedWithSalesCount,
                    result.AgingOver90Amount,
                    result.TopOmzetCustomerPercent,
                    result.TopPiutangCustomerPercent,
                    LastRefreshLogId = refreshLogId ?? string.Empty
                }, transaction);

                const string insertTopOmzetSql = @"
INSERT INTO BTR_PortalDashboardCustomerTopOmzet (
    CustomerTopOmzetId, SnapshotKey, Rank, CustomerCode, CustomerName, OmzetAmount, PercentOfTotal)
VALUES (
    @CustomerTopOmzetId, @SnapshotKey, @Rank, @CustomerCode, @CustomerName, @OmzetAmount, @PercentOfTotal)";

                foreach (var row in result.TopOmzet ?? new List<DashboardCustomerTopOmzetRow>())
                {
                    conn.Execute(insertTopOmzetSql, new
                    {
                        CustomerTopOmzetId = _counter.Generate("PDC", IDFormatEnum.PFnnn),
                        SnapshotKey,
                        row.Rank,
                        CustomerCode = row.CustomerCode ?? string.Empty,
                        CustomerName = row.CustomerName ?? string.Empty,
                        row.OmzetAmount,
                        row.PercentOfTotal
                    }, transaction);
                }

                const string insertTopPiutangSql = @"
INSERT INTO BTR_PortalDashboardCustomerTopPiutang (
    CustomerTopPiutangId, SnapshotKey, Rank, CustomerCode, CustomerName, OutstandingBalance, PercentOfTotal)
VALUES (
    @CustomerTopPiutangId, @SnapshotKey, @Rank, @CustomerCode, @CustomerName, @OutstandingBalance, @PercentOfTotal)";

                foreach (var row in result.TopPiutang ?? new List<DashboardCustomerTopPiutangRow>())
                {
                    conn.Execute(insertTopPiutangSql, new
                    {
                        CustomerTopPiutangId = _counter.Generate("PDC", IDFormatEnum.PFnnn),
                        SnapshotKey,
                        row.Rank,
                        CustomerCode = row.CustomerCode ?? string.Empty,
                        CustomerName = row.CustomerName ?? string.Empty,
                        row.OutstandingBalance,
                        row.PercentOfTotal
                    }, transaction);
                }

                const string insertAttentionSql = @"
INSERT INTO BTR_PortalDashboardCustomerAttention (
    CustomerAttentionId, SnapshotKey, CustomerCode, CustomerName, SignalKey, SignalLabel,
    ValueAmount, ValueText, WilayahName, SortOrder)
VALUES (
    @CustomerAttentionId, @SnapshotKey, @CustomerCode, @CustomerName, @SignalKey, @SignalLabel,
    @ValueAmount, @ValueText, @WilayahName, @SortOrder)";

                foreach (var row in result.AttentionList ?? new List<DashboardCustomerAttentionRow>())
                {
                    conn.Execute(insertAttentionSql, new
                    {
                        CustomerAttentionId = _counter.Generate("PDC", IDFormatEnum.PFnnn),
                        SnapshotKey,
                        CustomerCode = row.CustomerCode ?? string.Empty,
                        CustomerName = row.CustomerName ?? string.Empty,
                        row.SignalKey,
                        row.SignalLabel,
                        row.ValueAmount,
                        ValueText = row.ValueText ?? string.Empty,
                        WilayahName = row.WilayahName ?? string.Empty,
                        row.SortOrder
                    }, transaction);
                }

                const string insertSegmentationSql = @"
INSERT INTO BTR_PortalDashboardCustomerSegmentation (
    CustomerSegmentationId, SnapshotKey, SegmentType, SegmentKey, SegmentLabel,
    CustomerCount, ActiveCount, DormantCount, SortOrder)
VALUES (
    @CustomerSegmentationId, @SnapshotKey, @SegmentType, @SegmentKey, @SegmentLabel,
    @CustomerCount, @ActiveCount, @DormantCount, @SortOrder)";

                foreach (var row in result.Segmentation ?? new List<DashboardCustomerSegmentationRow>())
                {
                    conn.Execute(insertSegmentationSql, new
                    {
                        CustomerSegmentationId = _counter.Generate("PDC", IDFormatEnum.PFnnn),
                        SnapshotKey,
                        row.SegmentType,
                        SegmentKey = row.SegmentKey ?? string.Empty,
                        SegmentLabel = row.SegmentLabel ?? string.Empty,
                        row.CustomerCount,
                        row.ActiveCount,
                        row.DormantCount,
                        row.SortOrder
                    }, transaction);
                }
        }

        private sealed class KpiRow
        {
            public string SnapshotKey { get; set; }
            public System.DateTime GeneratedAt { get; set; }
            public int PeriodYear { get; set; }
            public int PeriodMonth { get; set; }
            public decimal TotalOmzet { get; set; }
            public decimal TotalPiutang { get; set; }
            public int ActiveCustomerCount { get; set; }
            public int DormantCustomerCount { get; set; }
            public int OverdueCustomerCount { get; set; }
            public int PlafondBreachCount { get; set; }
            public int SuspendedWithSalesCount { get; set; }
            public decimal AgingOver90Amount { get; set; }
            public decimal? TopOmzetCustomerPercent { get; set; }
            public decimal? TopPiutangCustomerPercent { get; set; }
            public string LastRefreshLogId { get; set; }
        }

        private sealed class TopOmzetRow
        {
            public int Rank { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public decimal OmzetAmount { get; set; }
            public decimal? PercentOfTotal { get; set; }
        }

        private sealed class TopPiutangRow
        {
            public int Rank { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public decimal OutstandingBalance { get; set; }
            public decimal? PercentOfTotal { get; set; }
        }

        private sealed class AttentionRow
        {
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public string SignalKey { get; set; }
            public string SignalLabel { get; set; }
            public decimal? ValueAmount { get; set; }
            public string ValueText { get; set; }
            public string WilayahName { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class SegmentationRow
        {
            public string SegmentType { get; set; }
            public string SegmentKey { get; set; }
            public string SegmentLabel { get; set; }
            public int CustomerCount { get; set; }
            public int ActiveCount { get; set; }
            public int DormantCount { get; set; }
            public int SortOrder { get; set; }
        }
    }
}
