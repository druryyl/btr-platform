using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardSalesFakturAggregator
    {
        public const int TopSalesmanCount = 10;

        public DashboardSalesAggregateResult Aggregate(
            IEnumerable<FakturView> rows,
            Periode periode,
            decimal totalTarget,
            DateTime generatedAt)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var list = (rows ?? Enumerable.Empty<FakturView>()).ToList();
            var totalOmzet = list.Sum(r => r.GrandTotal);
            var periodStart = periode.Tgl1.Date;

            return new DashboardSalesAggregateResult
            {
                PeriodYear = periodStart.Year,
                PeriodMonth = periodStart.Month,
                TotalOmzet = totalOmzet,
                CompletedOmzet = totalOmzet,
                PipelineOmzet = 0m,
                TotalFaktur = list.Count,
                TotalCustomer = list
                    .Select(ResolveCustomerKey)
                    .Where(key => key.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                GeneratedAt = generatedAt,
                TotalTarget = totalTarget,
                TotalAchievement = totalOmzet,
                AchievementPercent = SalesOmzetChartAchievementPolicy.ComputePercent(
                    totalOmzet,
                    totalTarget),
                WeekTrend = BuildWeekTrend(list, periode),
                TopSalesman = BuildTopSalesman(list)
            };
        }

        private static List<DashboardSalesWeekTrendRow> BuildWeekTrend(
            List<FakturView> rows,
            Periode periode)
        {
            var buckets = SalesOmzetChartWeekGrouper.BuildBuckets(periode);
            var totals = buckets.ToDictionary(
                b => b.WeekStart,
                b => new DashboardSalesWeekTrendRow
                {
                    WeekStart = b.WeekStart,
                    WeekEnd = b.WeekEnd,
                    WeekLabel = b.WeekLabel,
                    RecognizedAmount = 0m
                });

            foreach (var row in rows)
            {
                var fakturDate = row.Tgl.Date;
                var bucket = SalesOmzetChartWeekGrouper.FindBucket(buckets, fakturDate);
                if (bucket is null)
                    continue;

                totals[bucket.WeekStart].RecognizedAmount += row.GrandTotal;
            }

            return totals.Values.ToList();
        }

        private static List<DashboardSalesTopSalesmanRow> BuildTopSalesman(List<FakturView> rows)
        {
            return rows
                .GroupBy(r => r.SalesPersonName?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Key.Length > 0)
                .Select(g => new
                {
                    SalesPersonName = g.Key,
                    CompletedOmzet = g.Sum(r => r.GrandTotal)
                })
                .OrderByDescending(x => x.CompletedOmzet)
                .ThenBy(x => x.SalesPersonName, StringComparer.OrdinalIgnoreCase)
                .Take(TopSalesmanCount)
                .Select((x, index) => new DashboardSalesTopSalesmanRow
                {
                    Rank = index + 1,
                    SalesPersonName = x.SalesPersonName,
                    CompletedOmzet = x.CompletedOmzet
                })
                .ToList();
        }

        private static string ResolveCustomerKey(FakturView row)
        {
            if (row is null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(row.CustomerCode))
                return row.CustomerCode.Trim();

            return row.Customer?.Trim() ?? string.Empty;
        }
    }
}
