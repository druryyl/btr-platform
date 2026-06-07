using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardPurchasingInvoiceAggregator
    {
        public const int TopPrincipalCount = 10;

        private const string UnknownPrincipal = "Unknown";

        private static readonly (string Key, string Label, int SortOrder)[] PostingStatusDefinitions =
        {
            ("BELUM", "BELUM", 1),
            ("SUDAH", "SUDAH", 2),
        };

        public DashboardPurchasingAggregateResult Aggregate(
            IEnumerable<InvoiceView> rows,
            Periode periode,
            DateTime generatedAt)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var list = (rows ?? Enumerable.Empty<InvoiceView>()).ToList();
            var periodStart = periode.Tgl1.Date;

            return new DashboardPurchasingAggregateResult
            {
                PeriodYear = periodStart.Year,
                PeriodMonth = periodStart.Month,
                GrandTotalPurchase = list.Sum(r => r.GrandTotal),
                TotalInvoice = list.Count,
                PendingPostingInvoiceCount = list.Count(r =>
                    string.Equals(r.PostingStok, "BELUM", StringComparison.OrdinalIgnoreCase)),
                GeneratedAt = generatedAt,
                WeekTrend = BuildWeekTrend(list, periode),
                PostingStatus = BuildPostingStatus(list),
                TopPrincipal = BuildTopPrincipal(list)
            };
        }

        private static List<DashboardPurchasingWeekTrendRow> BuildWeekTrend(
            List<InvoiceView> rows,
            Periode periode)
        {
            var buckets = SalesOmzetChartWeekGrouper.BuildBuckets(periode);
            var totals = buckets.ToDictionary(
                b => b.WeekStart,
                b => new DashboardPurchasingWeekTrendRow
                {
                    WeekStart = b.WeekStart,
                    WeekEnd = b.WeekEnd,
                    WeekLabel = b.WeekLabel,
                    PurchaseAmount = 0m
                });

            foreach (var row in rows)
            {
                var invoiceDate = row.Tgl.Date;
                var bucket = SalesOmzetChartWeekGrouper.FindBucket(buckets, invoiceDate);
                if (bucket is null)
                    continue;

                totals[bucket.WeekStart].PurchaseAmount += row.GrandTotal;
            }

            return totals.Values.ToList();
        }

        private static List<DashboardPurchasingPostingStatusRow> BuildPostingStatus(
            List<InvoiceView> rows)
        {
            var statusTotals = rows
                .Where(r => IsKnownPostingStatus(r.PostingStok))
                .GroupBy(r => r.PostingStok.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key.ToUpperInvariant(), g => g.Sum(r => r.GrandTotal));

            return PostingStatusDefinitions
                .Select(def =>
                {
                    var key = def.Key.ToUpperInvariant();
                    statusTotals.TryGetValue(key, out var amount);
                    return new DashboardPurchasingPostingStatusRow
                    {
                        StatusKey = def.Key,
                        StatusLabel = def.Label,
                        SortOrder = def.SortOrder,
                        PurchaseAmount = amount
                    };
                })
                .ToList();
        }

        private static bool IsKnownPostingStatus(string postingStok)
        {
            if (string.IsNullOrWhiteSpace(postingStok))
                return false;

            var normalized = postingStok.Trim();
            return string.Equals(normalized, "BELUM", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(normalized, "SUDAH", StringComparison.OrdinalIgnoreCase);
        }

        private static List<DashboardPurchasingTopPrincipalRow> BuildTopPrincipal(
            List<InvoiceView> rows)
        {
            return rows
                .GroupBy(r => ResolvePrincipalName(r.SupplierName), StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    PrincipalName = g.Key,
                    PurchaseAmount = g.Sum(r => r.GrandTotal)
                })
                .OrderByDescending(x => x.PurchaseAmount)
                .ThenBy(x => x.PrincipalName, StringComparer.OrdinalIgnoreCase)
                .Take(TopPrincipalCount)
                .Select((x, index) => new DashboardPurchasingTopPrincipalRow
                {
                    Rank = index + 1,
                    PrincipalName = x.PrincipalName,
                    PurchaseAmount = x.PurchaseAmount
                })
                .ToList();
        }

        private static string ResolvePrincipalName(string supplierName)
        {
            return string.IsNullOrWhiteSpace(supplierName)
                ? UnknownPrincipal
                : supplierName.Trim();
        }
    }
}
