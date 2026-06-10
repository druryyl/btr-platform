using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardPiutangAggregator
    {
        public const int TopCustomerRiskCount = 20;
        public const int TopConcentration10 = 10;
        public const int TopConcentration20 = 20;

        private const string AgingOver90BucketKey = "DaysOver90";
        private const string CurrentBucketKey = "Current";

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

            var bucketTotals = new Dictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var def in PiutangAgingBucketResolver.BucketDefinitions)
                bucketTotals[def.Key] = 0m;

            var customerAccumulators = new Dictionary<string, CustomerAgingAccumulator>(StringComparer.Ordinal);
            var skippedCustomerIdRowCount = 0;

            foreach (var row in outstanding)
            {
                var bucketKey = PiutangAgingBucketResolver.ResolveBucketKey(row.JatuhTempo, today);
                bucketTotals[bucketKey] += row.KurangBayar;

                var customerId = row.CustomerId?.Trim() ?? string.Empty;
                if (customerId.Length == 0)
                {
                    skippedCustomerIdRowCount++;
                    continue;
                }

                if (!customerAccumulators.TryGetValue(customerId, out var accumulator))
                {
                    accumulator = new CustomerAgingAccumulator
                    {
                        CustomerId = customerId,
                        CustomerCode = row.CustomerCode?.Trim() ?? string.Empty,
                        CustomerName = row.CustomerName?.Trim() ?? string.Empty
                    };
                    customerAccumulators[customerId] = accumulator;
                }

                accumulator.AddToBucket(bucketKey, row.KurangBayar);
                accumulator.MergeDisplayFields(row);
            }

            var agingBuckets = BuildAgingBuckets(bucketTotals);

            var overdueCustomerCount = outstanding
                .Where(r => PiutangAgingBucketResolver.ResolveBucketKey(r.JatuhTempo, today) != CurrentBucketKey)
                .GroupBy(r => ResolveCustomerKey(r))
                .Where(g => g.Key.Length > 0)
                .Count(g => g.Sum(r => r.KurangBayar) > 0);

            var currentAmount = bucketTotals.TryGetValue(CurrentBucketKey, out var current) ? current : 0m;
            var overduePiutang = totalPiutang - currentAmount;
            var agingOver90Amount = bucketTotals.TryGetValue(AgingOver90BucketKey, out var over90) ? over90 : 0m;
            decimal? agingOver90Percent = totalPiutang > 0
                ? agingOver90Amount / totalPiutang * 100m
                : (decimal?)null;

            var customerAging = customerAccumulators.Values
                .Select(a => a.ToRow(generatedAt))
                .ToList();

            var rankedCustomers = customerAging
                .OrderByDescending(c => c.TotalPiutang)
                .ThenBy(c => c.CustomerName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var topCustomerRisk = rankedCustomers
                .Take(TopCustomerRiskCount)
                .Select((c, index) => new DashboardPiutangTopCustomerRiskRow
                {
                    Rank = index + 1,
                    CustomerId = c.CustomerId,
                    CustomerCode = c.CustomerCode,
                    CustomerName = c.CustomerName,
                    TotalPiutang = c.TotalPiutang,
                    CurrentAmount = c.CurrentAmount,
                    Aging30Amount = c.Aging30Amount,
                    Aging60Amount = c.Aging60Amount,
                    Aging90Amount = c.Aging90Amount,
                    AgingOver90Amount = c.AgingOver90Amount
                })
                .ToList();

            decimal? top10Percent = null;
            decimal? top20Percent = null;
            if (totalPiutang > 0)
            {
                var top10Sum = rankedCustomers.Take(TopConcentration10).Sum(c => c.TotalPiutang);
                var top20Sum = rankedCustomers.Take(TopConcentration20).Sum(c => c.TotalPiutang);
                top10Percent = top10Sum / totalPiutang * 100m;
                top20Percent = top20Sum / totalPiutang * 100m;
            }

            return new DashboardPiutangAggregateResult
            {
                TotalPiutang = totalPiutang,
                TotalCustomer = totalCustomer,
                GeneratedAt = generatedAt,
                OverdueCustomer = overdueCustomerCount,
                OverduePiutang = overduePiutang,
                AgingOver90Amount = agingOver90Amount,
                AgingOver90Percent = agingOver90Percent,
                Top10CustomerConcentrationPercent = top10Percent,
                Top20CustomerConcentrationPercent = top20Percent,
                SkippedCustomerIdRowCount = skippedCustomerIdRowCount,
                AgingBuckets = agingBuckets,
                CustomerAging = customerAging,
                TopCustomerRisk = topCustomerRisk
            };
        }

        private static List<DashboardPiutangAgingBucket> BuildAgingBuckets(
            Dictionary<string, decimal> bucketTotals)
        {
            return PiutangAgingBucketResolver.BucketDefinitions
                .Select(def => new DashboardPiutangAgingBucket
                {
                    BucketKey = def.Key,
                    BucketLabel = def.Label,
                    Amount = bucketTotals.TryGetValue(def.Key, out var amount) ? amount : 0m,
                    SortOrder = def.SortOrder
                })
                .ToList();
        }

        private static string ResolveCustomerKey(PiutangOpenBalanceDto row)
        {
            if (row is null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(row.CustomerCode))
                return row.CustomerCode.Trim();

            return row.CustomerName?.Trim() ?? string.Empty;
        }

        private sealed class CustomerAgingAccumulator
        {
            public string CustomerId { get; set; }

            public string CustomerCode { get; set; }

            public string CustomerName { get; set; }

            public decimal CurrentAmount { get; private set; }

            public decimal Aging30Amount { get; private set; }

            public decimal Aging60Amount { get; private set; }

            public decimal Aging90Amount { get; private set; }

            public decimal AgingOver90Amount { get; private set; }

            public void AddToBucket(string bucketKey, decimal amount)
            {
                switch (bucketKey)
                {
                    case CurrentBucketKey:
                        CurrentAmount += amount;
                        break;
                    case "Days1To30":
                        Aging30Amount += amount;
                        break;
                    case "Days31To60":
                        Aging60Amount += amount;
                        break;
                    case "Days61To90":
                        Aging90Amount += amount;
                        break;
                    case AgingOver90BucketKey:
                        AgingOver90Amount += amount;
                        break;
                }
            }

            public void MergeDisplayFields(PiutangOpenBalanceDto row)
            {
                if (string.IsNullOrWhiteSpace(CustomerCode) && !string.IsNullOrWhiteSpace(row.CustomerCode))
                    CustomerCode = row.CustomerCode.Trim();

                if (string.IsNullOrWhiteSpace(CustomerName) && !string.IsNullOrWhiteSpace(row.CustomerName))
                    CustomerName = row.CustomerName.Trim();
            }

            public DashboardPiutangCustomerAgingRow ToRow(DateTime lastUpdate)
            {
                return new DashboardPiutangCustomerAgingRow
                {
                    CustomerId = CustomerId,
                    CustomerCode = CustomerCode,
                    CustomerName = CustomerName,
                    CurrentAmount = CurrentAmount,
                    Aging30Amount = Aging30Amount,
                    Aging60Amount = Aging60Amount,
                    Aging90Amount = Aging90Amount,
                    AgingOver90Amount = AgingOver90Amount,
                    LastUpdate = lastUpdate
                };
            }
        }
    }
}
