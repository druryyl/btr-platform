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
    public class DashboardSalesmanSnapshotDal : IDashboardSalesmanSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardSalesmanSnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardSalesmanAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, TotalTeamOmzet, TotalPiutang,
       ActiveSalesmanCount, BelowTargetCount, NoTargetCount, HighOverdueExposureCount,
       HighPiutangExposureCount, CustomerConcentrationCount, DormantPortfolioCount,
       TopOmzetSalesmanPercent, TopPiutangSalesmanPercent, LastRefreshLogId
FROM BTRPD_SalesmanKpi
WHERE SnapshotKey = @SnapshotKey";

            const string topOmzetSql = @"
SELECT Rank, SalesPersonId, SalesPersonCode, SalesPersonName, CompletedOmzet, PercentOfTotal
FROM BTRPD_SalesmanTopOmzet
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topAchievementSql = @"
SELECT Rank, SalesPersonId, SalesPersonCode, SalesPersonName, TargetAmount, CompletedOmzet,
       AchievementPercent, PercentOfTotal
FROM BTRPD_SalesmanTopAchievement
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string topPiutangSql = @"
SELECT Rank, SalesPersonId, SalesPersonCode, SalesPersonName, OutstandingBalance, PercentOfTotal
FROM BTRPD_SalesmanTopPiutang
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            const string attentionSql = @"
SELECT SalesPersonId, SalesPersonCode, SalesPersonName, SignalKey, SignalLabel, ValueAmount,
       ValueText, WilayahName, SortOrder
FROM BTRPD_SalesmanAttention
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string segmentationSql = @"
SELECT SegmentType, SegmentKey, SegmentLabel, SalesmanCount, ActiveCount, InactiveCount, SortOrder
FROM BTRPD_SalesmanSegmentation
WHERE SnapshotKey = @SnapshotKey
ORDER BY SegmentType, SortOrder";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                return new DashboardSalesmanAggregateResult
                {
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    TotalTeamOmzet = kpi.TotalTeamOmzet,
                    TotalPiutang = kpi.TotalPiutang,
                    ActiveSalesmanCount = kpi.ActiveSalesmanCount,
                    BelowTargetCount = kpi.BelowTargetCount,
                    NoTargetCount = kpi.NoTargetCount,
                    HighOverdueExposureCount = kpi.HighOverdueExposureCount,
                    HighPiutangExposureCount = kpi.HighPiutangExposureCount,
                    CustomerConcentrationCount = kpi.CustomerConcentrationCount,
                    DormantPortfolioCount = kpi.DormantPortfolioCount,
                    TopOmzetSalesmanPercent = kpi.TopOmzetSalesmanPercent,
                    TopPiutangSalesmanPercent = kpi.TopPiutangSalesmanPercent,
                    GeneratedAt = kpi.GeneratedAt,
                    TopOmzet = conn.Query<TopOmzetRow>(topOmzetSql, new { SnapshotKey })
                        .Select(r => new DashboardSalesmanTopOmzetRow
                        {
                            Rank = r.Rank,
                            SalesPersonId = r.SalesPersonId,
                            SalesPersonCode = r.SalesPersonCode,
                            SalesPersonName = r.SalesPersonName,
                            CompletedOmzet = r.CompletedOmzet,
                            PercentOfTotal = r.PercentOfTotal
                        }).ToList(),
                    TopAchievement = conn.Query<TopAchievementRow>(topAchievementSql, new { SnapshotKey })
                        .Select(r => new DashboardSalesmanTopAchievementRow
                        {
                            Rank = r.Rank,
                            SalesPersonId = r.SalesPersonId,
                            SalesPersonCode = r.SalesPersonCode,
                            SalesPersonName = r.SalesPersonName,
                            TargetAmount = r.TargetAmount,
                            CompletedOmzet = r.CompletedOmzet,
                            AchievementPercent = r.AchievementPercent,
                            PercentOfTotal = r.PercentOfTotal
                        }).ToList(),
                    TopPiutang = conn.Query<TopPiutangRow>(topPiutangSql, new { SnapshotKey })
                        .Select(r => new DashboardSalesmanTopPiutangRow
                        {
                            Rank = r.Rank,
                            SalesPersonId = r.SalesPersonId,
                            SalesPersonCode = r.SalesPersonCode,
                            SalesPersonName = r.SalesPersonName,
                            OutstandingBalance = r.OutstandingBalance,
                            PercentOfTotal = r.PercentOfTotal
                        }).ToList(),
                    AttentionList = conn.Query<AttentionRow>(attentionSql, new { SnapshotKey })
                        .Select(r => new DashboardSalesmanAttentionRow
                        {
                            SalesPersonId = r.SalesPersonId,
                            SalesPersonCode = r.SalesPersonCode,
                            SalesPersonName = r.SalesPersonName,
                            SignalKey = r.SignalKey,
                            SignalLabel = r.SignalLabel,
                            ValueAmount = r.ValueAmount,
                            ValueText = r.ValueText,
                            WilayahName = r.WilayahName,
                            SortOrder = r.SortOrder
                        }).ToList(),
                    Segmentation = conn.Query<SegmentationRow>(segmentationSql, new { SnapshotKey })
                        .Select(r => new DashboardSalesmanSegmentationRow
                        {
                            SegmentType = r.SegmentType,
                            SegmentKey = r.SegmentKey,
                            SegmentLabel = r.SegmentLabel,
                            SalesmanCount = r.SalesmanCount,
                            ActiveCount = r.ActiveCount,
                            InactiveCount = r.InactiveCount,
                            SortOrder = r.SortOrder
                        }).ToList()
                };
            }
        }

        public void ReplaceCurrent(DashboardSalesmanAggregateResult result, string refreshLogId)
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
            DashboardSalesmanAggregateResult result,
            string refreshLogId)
        {
            conn.Execute(
                "DELETE FROM BTRPD_SalesmanTopOmzet WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_SalesmanTopAchievement WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_SalesmanTopPiutang WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_SalesmanAttention WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);
            conn.Execute(
                "DELETE FROM BTRPD_SalesmanSegmentation WHERE SnapshotKey = @SnapshotKey",
                new { SnapshotKey },
                transaction);

            const string mergeKpiSql = @"
MERGE BTRPD_SalesmanKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        TotalTeamOmzet = @TotalTeamOmzet,
        TotalPiutang = @TotalPiutang,
        ActiveSalesmanCount = @ActiveSalesmanCount,
        BelowTargetCount = @BelowTargetCount,
        NoTargetCount = @NoTargetCount,
        HighOverdueExposureCount = @HighOverdueExposureCount,
        HighPiutangExposureCount = @HighPiutangExposureCount,
        CustomerConcentrationCount = @CustomerConcentrationCount,
        DormantPortfolioCount = @DormantPortfolioCount,
        TopOmzetSalesmanPercent = @TopOmzetSalesmanPercent,
        TopPiutangSalesmanPercent = @TopPiutangSalesmanPercent,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth, TotalTeamOmzet, TotalPiutang,
        ActiveSalesmanCount, BelowTargetCount, NoTargetCount, HighOverdueExposureCount,
        HighPiutangExposureCount, CustomerConcentrationCount, DormantPortfolioCount,
        TopOmzetSalesmanPercent, TopPiutangSalesmanPercent, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @PeriodYear, @PeriodMonth, @TotalTeamOmzet, @TotalPiutang,
        @ActiveSalesmanCount, @BelowTargetCount, @NoTargetCount, @HighOverdueExposureCount,
        @HighPiutangExposureCount, @CustomerConcentrationCount, @DormantPortfolioCount,
        @TopOmzetSalesmanPercent, @TopPiutangSalesmanPercent, @LastRefreshLogId);";

            conn.Execute(mergeKpiSql, new
            {
                SnapshotKey,
                result.GeneratedAt,
                result.PeriodYear,
                result.PeriodMonth,
                result.TotalTeamOmzet,
                result.TotalPiutang,
                result.ActiveSalesmanCount,
                result.BelowTargetCount,
                result.NoTargetCount,
                result.HighOverdueExposureCount,
                result.HighPiutangExposureCount,
                result.CustomerConcentrationCount,
                result.DormantPortfolioCount,
                result.TopOmzetSalesmanPercent,
                result.TopPiutangSalesmanPercent,
                LastRefreshLogId = refreshLogId ?? string.Empty
            }, transaction);

            const string insertTopOmzetSql = @"
INSERT INTO BTRPD_SalesmanTopOmzet (
    SalesmanTopOmzetId, SnapshotKey, Rank, SalesPersonId, SalesPersonCode, SalesPersonName,
    CompletedOmzet, PercentOfTotal)
VALUES (
    @SalesmanTopOmzetId, @SnapshotKey, @Rank, @SalesPersonId, @SalesPersonCode, @SalesPersonName,
    @CompletedOmzet, @PercentOfTotal)";

            foreach (var row in result.TopOmzet ?? new List<DashboardSalesmanTopOmzetRow>())
            {
                conn.Execute(insertTopOmzetSql, new
                {
                    SalesmanTopOmzetId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    SalesPersonId = row.SalesPersonId ?? string.Empty,
                    SalesPersonCode = row.SalesPersonCode ?? string.Empty,
                    SalesPersonName = row.SalesPersonName ?? string.Empty,
                    row.CompletedOmzet,
                    row.PercentOfTotal
                }, transaction);
            }

            const string insertTopAchievementSql = @"
INSERT INTO BTRPD_SalesmanTopAchievement (
    SalesmanTopAchievementId, SnapshotKey, Rank, SalesPersonId, SalesPersonCode, SalesPersonName,
    TargetAmount, CompletedOmzet, AchievementPercent, PercentOfTotal)
VALUES (
    @SalesmanTopAchievementId, @SnapshotKey, @Rank, @SalesPersonId, @SalesPersonCode, @SalesPersonName,
    @TargetAmount, @CompletedOmzet, @AchievementPercent, @PercentOfTotal)";

            foreach (var row in result.TopAchievement ?? new List<DashboardSalesmanTopAchievementRow>())
            {
                conn.Execute(insertTopAchievementSql, new
                {
                    SalesmanTopAchievementId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    SalesPersonId = row.SalesPersonId ?? string.Empty,
                    SalesPersonCode = row.SalesPersonCode ?? string.Empty,
                    SalesPersonName = row.SalesPersonName ?? string.Empty,
                    row.TargetAmount,
                    row.CompletedOmzet,
                    row.AchievementPercent,
                    row.PercentOfTotal
                }, transaction);
            }

            const string insertTopPiutangSql = @"
INSERT INTO BTRPD_SalesmanTopPiutang (
    SalesmanTopPiutangId, SnapshotKey, Rank, SalesPersonId, SalesPersonCode, SalesPersonName,
    OutstandingBalance, PercentOfTotal)
VALUES (
    @SalesmanTopPiutangId, @SnapshotKey, @Rank, @SalesPersonId, @SalesPersonCode, @SalesPersonName,
    @OutstandingBalance, @PercentOfTotal)";

            foreach (var row in result.TopPiutang ?? new List<DashboardSalesmanTopPiutangRow>())
            {
                conn.Execute(insertTopPiutangSql, new
                {
                    SalesmanTopPiutangId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.Rank,
                    SalesPersonId = row.SalesPersonId ?? string.Empty,
                    SalesPersonCode = row.SalesPersonCode ?? string.Empty,
                    SalesPersonName = row.SalesPersonName ?? string.Empty,
                    row.OutstandingBalance,
                    row.PercentOfTotal
                }, transaction);
            }

            const string insertAttentionSql = @"
INSERT INTO BTRPD_SalesmanAttention (
    SalesmanAttentionId, SnapshotKey, SalesPersonId, SalesPersonCode, SalesPersonName, SignalKey,
    SignalLabel, ValueAmount, ValueText, WilayahName, SortOrder)
VALUES (
    @SalesmanAttentionId, @SnapshotKey, @SalesPersonId, @SalesPersonCode, @SalesPersonName, @SignalKey,
    @SignalLabel, @ValueAmount, @ValueText, @WilayahName, @SortOrder)";

            foreach (var row in result.AttentionList ?? new List<DashboardSalesmanAttentionRow>())
            {
                conn.Execute(insertAttentionSql, new
                {
                    SalesmanAttentionId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    SalesPersonId = row.SalesPersonId ?? string.Empty,
                    SalesPersonCode = row.SalesPersonCode ?? string.Empty,
                    SalesPersonName = row.SalesPersonName ?? string.Empty,
                    row.SignalKey,
                    row.SignalLabel,
                    row.ValueAmount,
                    ValueText = row.ValueText ?? string.Empty,
                    WilayahName = row.WilayahName ?? string.Empty,
                    row.SortOrder
                }, transaction);
            }

            const string insertSegmentationSql = @"
INSERT INTO BTRPD_SalesmanSegmentation (
    SalesmanSegmentationId, SnapshotKey, SegmentType, SegmentKey, SegmentLabel,
    SalesmanCount, ActiveCount, InactiveCount, SortOrder)
VALUES (
    @SalesmanSegmentationId, @SnapshotKey, @SegmentType, @SegmentKey, @SegmentLabel,
    @SalesmanCount, @ActiveCount, @InactiveCount, @SortOrder)";

            foreach (var row in result.Segmentation ?? new List<DashboardSalesmanSegmentationRow>())
            {
                conn.Execute(insertSegmentationSql, new
                {
                    SalesmanSegmentationId = Ulid.NewUlid().ToString(),
                    SnapshotKey,
                    row.SegmentType,
                    SegmentKey = row.SegmentKey ?? string.Empty,
                    SegmentLabel = row.SegmentLabel ?? string.Empty,
                    row.SalesmanCount,
                    row.ActiveCount,
                    row.InactiveCount,
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
            public decimal TotalTeamOmzet { get; set; }
            public decimal TotalPiutang { get; set; }
            public int ActiveSalesmanCount { get; set; }
            public int BelowTargetCount { get; set; }
            public int NoTargetCount { get; set; }
            public int HighOverdueExposureCount { get; set; }
            public int HighPiutangExposureCount { get; set; }
            public int CustomerConcentrationCount { get; set; }
            public int DormantPortfolioCount { get; set; }
            public decimal? TopOmzetSalesmanPercent { get; set; }
            public decimal? TopPiutangSalesmanPercent { get; set; }
            public string LastRefreshLogId { get; set; }
        }

        private sealed class TopOmzetRow
        {
            public int Rank { get; set; }
            public string SalesPersonId { get; set; }
            public string SalesPersonCode { get; set; }
            public string SalesPersonName { get; set; }
            public decimal CompletedOmzet { get; set; }
            public decimal? PercentOfTotal { get; set; }
        }

        private sealed class TopAchievementRow
        {
            public int Rank { get; set; }
            public string SalesPersonId { get; set; }
            public string SalesPersonCode { get; set; }
            public string SalesPersonName { get; set; }
            public decimal? TargetAmount { get; set; }
            public decimal CompletedOmzet { get; set; }
            public decimal? AchievementPercent { get; set; }
            public decimal? PercentOfTotal { get; set; }
        }

        private sealed class TopPiutangRow
        {
            public int Rank { get; set; }
            public string SalesPersonId { get; set; }
            public string SalesPersonCode { get; set; }
            public string SalesPersonName { get; set; }
            public decimal OutstandingBalance { get; set; }
            public decimal? PercentOfTotal { get; set; }
        }

        private sealed class AttentionRow
        {
            public string SalesPersonId { get; set; }
            public string SalesPersonCode { get; set; }
            public string SalesPersonName { get; set; }
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
            public int SalesmanCount { get; set; }
            public int ActiveCount { get; set; }
            public int InactiveCount { get; set; }
            public int SortOrder { get; set; }
        }
    }
}
