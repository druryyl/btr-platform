using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardPiutangAggregator
    {
        private static readonly (string Key, string Label, int SortOrder)[] AgingBucketDefinitions =
        {
            ("Current", "Current (Not Yet Due)", 1),
            ("Days1To30", "1–30 Days", 2),
            ("Days31To60", "31–60 Days", 3),
            ("Days61To90", "61–90 Days", 4),
            ("DaysOver90", "> 90 Days", 5),
        };

        public DashboardPiutangAggregateResult Aggregate(
            IEnumerable<PiutangOpenBalanceDto> rows,
            DateTime today,
            DateTime generatedAt)
        {
            var outstanding = (rows ?? Enumerable.Empty<PiutangOpenBalanceDto>())
                .Where(r => r.KurangBayar > 1)
                .ToList();

            var totalPiutang = outstanding.Sum(r => r.KurangBayar);
            var totalCustomer = outstanding
                .Select(ResolveCustomerKey)
                .Where(key => key.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            var bucketTotals = outstanding
                .GroupBy(r => ResolveAgingBucketKey(r.JatuhTempo, today))
                .ToDictionary(g => g.Key, g => g.Sum(r => r.KurangBayar));

            var agingBuckets = BuildAgingBuckets(bucketTotals);

            var overdueCustomerCount = outstanding
                .Where(r => ResolveAgingBucketKey(r.JatuhTempo, today) != "Current")
                .GroupBy(r => ResolveCustomerKey(r))
                .Where(g => g.Key.Length > 0)
                .Count(g => g.Sum(r => r.KurangBayar) > 0);

            var topCustomers = outstanding
                .GroupBy(r => ResolveCustomerKey(r))
                .Where(g => g.Key.Length > 0)
                .Select(g => new
                {
                    CustomerName = ResolveCustomerDisplayName(g),
                    OutstandingBalance = g.Sum(r => r.KurangBayar)
                })
                .OrderByDescending(x => x.OutstandingBalance)
                .ThenBy(x => x.CustomerName, StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .Select((x, index) => new DashboardPiutangTopCustomer
                {
                    Rank = index + 1,
                    CustomerName = x.CustomerName,
                    OutstandingBalance = x.OutstandingBalance
                })
                .ToList();

            return new DashboardPiutangAggregateResult
            {
                TotalPiutang = totalPiutang,
                TotalCustomer = totalCustomer,
                GeneratedAt = generatedAt,
                OverdueCustomer = overdueCustomerCount,
                AgingBuckets = agingBuckets,
                TopCustomers = topCustomers
            };
        }

        private static List<DashboardPiutangAgingBucket> BuildAgingBuckets(
            Dictionary<string, decimal> bucketTotals)
        {
            return AgingBucketDefinitions
                .Select(def => new DashboardPiutangAgingBucket
                {
                    BucketKey = def.Key,
                    BucketLabel = def.Label,
                    Amount = bucketTotals.TryGetValue(def.Key, out var amount) ? amount : 0m,
                    SortOrder = def.SortOrder
                })
                .ToList();
        }

        private static string ResolveAgingBucketKey(DateTime jatuhTempo, DateTime today)
        {
            var daysOverdue = (today - jatuhTempo.Date).Days;

            if (daysOverdue <= 0) return "Current";
            if (daysOverdue <= 30) return "Days1To30";
            if (daysOverdue <= 60) return "Days31To60";
            if (daysOverdue <= 90) return "Days61To90";
            return "DaysOver90";
        }

        private static string ResolveCustomerKey(PiutangOpenBalanceDto row)
        {
            if (row is null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(row.CustomerCode))
                return row.CustomerCode.Trim();

            return row.CustomerName?.Trim() ?? string.Empty;
        }

        private static string ResolveCustomerDisplayName(
            IGrouping<string, PiutangOpenBalanceDto> group)
        {
            return group
                .Select(r => r.CustomerName?.Trim())
                .FirstOrDefault(n => !string.IsNullOrEmpty(n))
                ?? group.Key;
        }
    }
}
