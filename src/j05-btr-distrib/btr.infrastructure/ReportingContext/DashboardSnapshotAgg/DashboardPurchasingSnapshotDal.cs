using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardPurchasingAgg.Queries;
using btr.application.ReportingContext.Shared;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Application;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardPurchasingSnapshotDal : IDashboardPurchasingSnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;
        private readonly INunaCounterBL _counter;

        public DashboardPurchasingSnapshotDal(
            IOptions<DatabaseOptions> opt,
            INunaCounterBL counter)
        {
            _opt = opt.Value;
            _counter = counter;
        }

        public DashboardPurchasingAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth,
       GrandTotalPurchase, TotalInvoice, PendingPostingInvoiceCount, LastRefreshLogId
FROM BTR_PortalDashboardPurchasingKpi
WHERE SnapshotKey = @SnapshotKey";

            const string weekTrendSql = @"
SELECT WeekStart, WeekEnd, WeekLabel, PurchaseAmount
FROM BTR_PortalDashboardPurchasingWeekTrend
WHERE SnapshotKey = @SnapshotKey
ORDER BY WeekStart";

            const string postingStatusSql = @"
SELECT StatusKey, StatusLabel, SortOrder, PurchaseAmount
FROM BTR_PortalDashboardPurchasingPostingStatus
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder";

            const string topPrincipalSql = @"
SELECT Rank, PrincipalName, PurchaseAmount
FROM BTR_PortalDashboardPurchasingTopPrincipal
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                var weekRows = conn.Query<WeekTrendRow>(weekTrendSql, new { SnapshotKey }).ToList();
                var postingRows = conn.Query<PostingStatusRow>(postingStatusSql, new { SnapshotKey }).ToList();
                var topRows = conn.Query<TopPrincipalRow>(topPrincipalSql, new { SnapshotKey }).ToList();

                return new DashboardPurchasingAggregateResult
                {
                    PeriodYear = kpi.PeriodYear,
                    PeriodMonth = kpi.PeriodMonth,
                    GrandTotalPurchase = kpi.GrandTotalPurchase,
                    TotalInvoice = kpi.TotalInvoice,
                    PendingPostingInvoiceCount = kpi.PendingPostingInvoiceCount,
                    GeneratedAt = kpi.GeneratedAt,
                    WeekTrend = weekRows.Select(r => new DashboardPurchasingWeekTrendRow
                    {
                        WeekStart = r.WeekStart,
                        WeekEnd = r.WeekEnd,
                        WeekLabel = r.WeekLabel,
                        PurchaseAmount = r.PurchaseAmount
                    }).ToList(),
                    PostingStatus = postingRows.Select(r => new DashboardPurchasingPostingStatusRow
                    {
                        StatusKey = r.StatusKey,
                        StatusLabel = r.StatusLabel,
                        SortOrder = r.SortOrder,
                        PurchaseAmount = r.PurchaseAmount
                    }).ToList(),
                    TopPrincipal = topRows.Select(r => new DashboardPurchasingTopPrincipalRow
                    {
                        Rank = r.Rank,
                        PrincipalName = r.PrincipalName,
                        PurchaseAmount = r.PurchaseAmount
                    }).ToList()
                };
            }
        }

        public void ReplaceCurrent(DashboardPurchasingAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new System.ArgumentNullException(nameof(result));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();

                conn.Execute(
                    "DELETE FROM BTR_PortalDashboardPurchasingWeekTrend WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                conn.Execute(
                    "DELETE FROM BTR_PortalDashboardPurchasingPostingStatus WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                conn.Execute(
                    "DELETE FROM BTR_PortalDashboardPurchasingTopPrincipal WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                const string mergeKpiSql = @"
MERGE BTR_PortalDashboardPurchasingKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        PeriodYear = @PeriodYear,
        PeriodMonth = @PeriodMonth,
        GrandTotalPurchase = @GrandTotalPurchase,
        TotalInvoice = @TotalInvoice,
        PendingPostingInvoiceCount = @PendingPostingInvoiceCount,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (
        SnapshotKey, GeneratedAt, PeriodYear, PeriodMonth,
        GrandTotalPurchase, TotalInvoice, PendingPostingInvoiceCount, LastRefreshLogId)
    VALUES (
        @SnapshotKey, @GeneratedAt, @PeriodYear, @PeriodMonth,
        @GrandTotalPurchase, @TotalInvoice, @PendingPostingInvoiceCount, @LastRefreshLogId);";

                conn.Execute(mergeKpiSql, new
                {
                    SnapshotKey,
                    result.GeneratedAt,
                    result.PeriodYear,
                    result.PeriodMonth,
                    result.GrandTotalPurchase,
                    result.TotalInvoice,
                    result.PendingPostingInvoiceCount,
                    LastRefreshLogId = refreshLogId ?? string.Empty
                });

                const string insertWeekTrendSql = @"
INSERT INTO BTR_PortalDashboardPurchasingWeekTrend (
    PurchasingWeekTrendId, SnapshotKey, WeekStart, WeekEnd, WeekLabel, PurchaseAmount)
VALUES (
    @PurchasingWeekTrendId, @SnapshotKey, @WeekStart, @WeekEnd, @WeekLabel, @PurchaseAmount)";

                foreach (var row in result.WeekTrend ?? new List<DashboardPurchasingWeekTrendRow>())
                {
                    conn.Execute(insertWeekTrendSql, new
                    {
                        PurchasingWeekTrendId = _counter.Generate("PDP", IDFormatEnum.PFnnn),
                        SnapshotKey,
                        row.WeekStart,
                        row.WeekEnd,
                        WeekLabel = row.WeekLabel ?? string.Empty,
                        row.PurchaseAmount
                    });
                }

                const string insertPostingStatusSql = @"
INSERT INTO BTR_PortalDashboardPurchasingPostingStatus (
    PurchasingPostingStatusId, SnapshotKey, StatusKey, StatusLabel, SortOrder, PurchaseAmount)
VALUES (
    @PurchasingPostingStatusId, @SnapshotKey, @StatusKey, @StatusLabel, @SortOrder, @PurchaseAmount)";

                foreach (var row in result.PostingStatus ?? new List<DashboardPurchasingPostingStatusRow>())
                {
                    conn.Execute(insertPostingStatusSql, new
                    {
                        PurchasingPostingStatusId = _counter.Generate("PDG", IDFormatEnum.PFnnn),
                        SnapshotKey,
                        StatusKey = row.StatusKey ?? string.Empty,
                        StatusLabel = row.StatusLabel ?? string.Empty,
                        row.SortOrder,
                        row.PurchaseAmount
                    });
                }

                const string insertTopPrincipalSql = @"
INSERT INTO BTR_PortalDashboardPurchasingTopPrincipal (
    PurchasingTopPrincipalId, SnapshotKey, Rank, PrincipalName, PurchaseAmount)
VALUES (
    @PurchasingTopPrincipalId, @SnapshotKey, @Rank, @PrincipalName, @PurchaseAmount)";

                foreach (var row in result.TopPrincipal ?? new List<DashboardPurchasingTopPrincipalRow>())
                {
                    conn.Execute(insertTopPrincipalSql, new
                    {
                        PurchasingTopPrincipalId = _counter.Generate("PDT", IDFormatEnum.PFnnn),
                        SnapshotKey,
                        row.Rank,
                        PrincipalName = row.PrincipalName ?? string.Empty,
                        row.PurchaseAmount
                    });
                }
            }
        }

        public static DashboardPurchasingResponse MapToResponse(DashboardPurchasingAggregateResult snapshot)
        {
            return new DashboardPurchasingResponse
            {
                GrandTotalPurchase = snapshot.GrandTotalPurchase,
                TotalInvoice = snapshot.TotalInvoice,
                PendingPostingInvoiceCount = snapshot.PendingPostingInvoiceCount,
                GeneratedAt = snapshot.GeneratedAt,
                WeeklyTrend = (snapshot.WeekTrend ?? new List<DashboardPurchasingWeekTrendRow>())
                    .Select(w => new DashboardPurchasingWeekTrendItem
                    {
                        WeekStart = w.WeekStart,
                        WeekEnd = w.WeekEnd,
                        WeekLabel = w.WeekLabel,
                        PurchaseAmount = w.PurchaseAmount
                    })
                    .ToList(),
                PostingStatusBreakdown = (snapshot.PostingStatus ?? new List<DashboardPurchasingPostingStatusRow>())
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new DashboardPurchasingPostingStatusItem
                    {
                        StatusKey = s.StatusKey,
                        StatusLabel = s.StatusLabel,
                        SortOrder = s.SortOrder,
                        PurchaseAmount = s.PurchaseAmount
                    })
                    .ToList(),
                TopPrincipalRanking = (snapshot.TopPrincipal ?? new List<DashboardPurchasingTopPrincipalRow>())
                    .OrderBy(r => r.Rank)
                    .Select(r => new DashboardPurchasingRankingItem
                    {
                        Rank = r.Rank,
                        PrincipalName = r.PrincipalName,
                        PurchaseAmount = r.PurchaseAmount,
                        Investigation = InvestigationMetadataBuilder.Build(
                            InvestigationRegistry.SignalRankingTopPrincipal,
                            InvestigationMetadataBuilder.EntityTypePrincipal,
                            null,
                            r.PrincipalName)
                    })
                    .ToList()
            };
        }

        private sealed class KpiRow
        {
            public string SnapshotKey { get; set; }
            public System.DateTime GeneratedAt { get; set; }
            public int PeriodYear { get; set; }
            public int PeriodMonth { get; set; }
            public decimal GrandTotalPurchase { get; set; }
            public int TotalInvoice { get; set; }
            public int PendingPostingInvoiceCount { get; set; }
            public string LastRefreshLogId { get; set; }
        }

        private sealed class WeekTrendRow
        {
            public System.DateTime WeekStart { get; set; }
            public System.DateTime WeekEnd { get; set; }
            public string WeekLabel { get; set; }
            public decimal PurchaseAmount { get; set; }
        }

        private sealed class PostingStatusRow
        {
            public string StatusKey { get; set; }
            public string StatusLabel { get; set; }
            public int SortOrder { get; set; }
            public decimal PurchaseAmount { get; set; }
        }

        private sealed class TopPrincipalRow
        {
            public int Rank { get; set; }
            public string PrincipalName { get; set; }
            public decimal PurchaseAmount { get; set; }
        }
    }
}
