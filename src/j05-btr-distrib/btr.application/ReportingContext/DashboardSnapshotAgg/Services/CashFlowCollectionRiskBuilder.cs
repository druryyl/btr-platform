using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.FinanceContext.PiutangAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class CashFlowCollectionRiskBuilder
    {
        public const string RiskLargeDueSoon = "LargeDueSoon";
        public const string RiskChronicOverdueLarge = "ChronicOverdueLarge";
        public const string RiskOverdueConcentration = "OverdueConcentration";
        public const string RiskLegacyDebtOverdue = "LegacyDebtOverdue";
        public const string RiskPlafondBreachDueSoon = "PlafondBreachDueSoon";
        public const string RiskLowRecoveryCustomer = "LowRecoveryCustomer";
        public const string RiskWilayahHotspotDue = "WilayahHotspotDue";
        public const string RiskExpectedOverdueGrowth = "ExpectedOverdueGrowth";

        private const string ReportRoute = "/reports/piutang";
        private const string EntityTypeCustomer = "Customer";
        private const string EntityTypeWilayah = "Wilayah";
        private const string AgingOver90BucketKey = "DaysOver90";
        private const decimal OverdueConcentrationThresholdPercent = 15m;
        private const decimal WilayahDueSoonThresholdPercent = 10m;
        private const int MaxRiskRows = 10;

        private sealed class RiskCandidate
        {
            public int RulePriority { get; set; }

            public DashboardCashFlowCollectionRiskRow Row { get; set; }
        }

        public static List<DashboardCashFlowCollectionRiskRow> Build(
            IEnumerable<PiutangOpenBalanceDto> openBalanceRows,
            IEnumerable<PiutangOpenBalanceWithSalesmanDto> openBalanceWithSalesmanRows,
            IEnumerable<PiutangOpenBalanceWithWilayahDto> openBalanceWithWilayahRows,
            DashboardCollectionAggregateResult collectionResult,
            IEnumerable<PenerimaanPelunasanSalesDto> pelunasanLookback30Rows,
            DateTime businessDate,
            DateTime monthEnd,
            decimal largeDueSoonFloorAmount)
        {
            if (collectionResult is null)
                throw new ArgumentNullException(nameof(collectionResult));

            var today = businessDate.Date;
            var me = monthEnd.Date;
            var openRows = (openBalanceRows ?? Enumerable.Empty<PiutangOpenBalanceDto>())
                .Where(r => r.KurangBayar > 1)
                .ToList();
            var salesmanRows = (openBalanceWithSalesmanRows ?? Enumerable.Empty<PiutangOpenBalanceWithSalesmanDto>())
                .Where(r => r.KurangBayar > 1)
                .ToList();
            var wilayahRows = (openBalanceWithWilayahRows ?? Enumerable.Empty<PiutangOpenBalanceWithWilayahDto>())
                .Where(r => r.KurangBayar > 1)
                .ToList();
            var lookback = (pelunasanLookback30Rows ?? Enumerable.Empty<PenerimaanPelunasanSalesDto>()).ToList();

            var candidates = new List<RiskCandidate>();
            var addedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var customerOverdueTotals = BuildCustomerOverdueTotals(openRows, today);
            var overdueExposure = collectionResult.OverdueExposure;
            var top10Threshold = collectionResult.TopOverdueCustomers
                ?.OrderByDescending(c => c.OverdueBalance)
                .Take(DashboardCollectionAggregator.TopRankingCount)
                .LastOrDefault()
                ?.OverdueBalance ?? 0m;

            var p75Floor = ComputeP75Floor(customerOverdueTotals.Values);
            var largeDueSoonThreshold = Math.Max(p75Floor, largeDueSoonFloorAmount);

            var legacyCustomers = GetAttentionCustomerKeys(collectionResult, DashboardCollectionAggregator.SignalLegacyDebt);
            var plafondBreachCustomers = GetAttentionCustomerKeys(collectionResult, DashboardCollectionAggregator.SignalPlafondBreachOverdue);
            var lowRecoverySalesmen = GetAttentionSalesmanIds(collectionResult, DashboardCollectionAggregator.SignalLowRecoveryVsBilling);
            var hotspotWilayah = GetAttentionWilayahKeys(collectionResult, DashboardCollectionAggregator.SignalWilayahHotspot);

            var companyDueSoonTotal = openRows
                .Where(r => r.JatuhTempo.Date > today && r.JatuhTempo.Date <= me)
                .Sum(r => r.KurangBayar);

            foreach (var row in openRows)
            {
                var dueDate = row.JatuhTempo.Date;
                var customerKey = ResolveCustomerKey(row);
                var daysUntilDue = (dueDate - today).Days;
                var bucket = ResolveAgingBucketKey(dueDate, today);
                var isOverdue = bucket != "Current";
                var customerOverdue = customerOverdueTotals.TryGetValue(customerKey, out var co) ? co : 0m;

                if (dueDate > today && dueDate <= today.AddDays(7) && row.KurangBayar >= largeDueSoonThreshold)
                {
                    TryAdd(candidates, addedKeys, 1, customerKey, RiskLargeDueSoon, "Large Invoice Due Soon",
                        EntityTypeCustomer, row.CustomerCode ?? customerKey, row.CustomerName ?? customerKey,
                        row.KurangBayar, dueDate.ToString("dd MMM yyyy", CollectionDayGrouper.LabelCulture),
                        $"Due in {daysUntilDue} days, balance {FormatAmount(row.KurangBayar)}");
                }

                if (bucket == AgingOver90BucketKey && customerOverdue >= top10Threshold && top10Threshold > 0)
                {
                    TryAdd(candidates, addedKeys, 2, customerKey, RiskChronicOverdueLarge, "Chronic Overdue — Large",
                        EntityTypeCustomer, row.CustomerCode ?? customerKey, row.CustomerName ?? customerKey,
                        row.KurangBayar, ">90 days",
                        $"Overdue balance {FormatAmount(row.KurangBayar)} in >90 day bucket");
                }

                if (isOverdue && overdueExposure > 0 &&
                    row.KurangBayar / overdueExposure * 100m >= OverdueConcentrationThresholdPercent)
                {
                    TryAdd(candidates, addedKeys, 3, customerKey, RiskOverdueConcentration, "Collection Concentration Risk",
                        EntityTypeCustomer, row.CustomerCode ?? customerKey, row.CustomerName ?? customerKey,
                        row.KurangBayar, FormatAgingLabel(bucket),
                        $"Overdue balance is {row.KurangBayar / overdueExposure * 100m:F1}% of company overdue exposure");
                }

                if (legacyCustomers.Contains(customerKey) && isOverdue)
                {
                    TryAdd(candidates, addedKeys, 4, customerKey, RiskLegacyDebtOverdue, "Legacy Debt — Overdue",
                        EntityTypeCustomer, row.CustomerCode ?? customerKey, row.CustomerName ?? customerKey,
                        row.KurangBayar, FormatAgingLabel(bucket),
                        "Legacy debt customer with overdue balance");
                }

                if (plafondBreachCustomers.Contains(customerKey) &&
                    dueDate > today && dueDate <= today.AddDays(14))
                {
                    TryAdd(candidates, addedKeys, 5, customerKey, RiskPlafondBreachDueSoon, "Plafond Breach — Due Soon",
                        EntityTypeCustomer, row.CustomerCode ?? customerKey, row.CustomerName ?? customerKey,
                        row.KurangBayar, dueDate.ToString("dd MMM yyyy", CollectionDayGrouper.LabelCulture),
                        $"Plafond breach with due date in {daysUntilDue} days");
                }
            }

            var lowRecoveryCustomerKeys = salesmanRows
                .Where(r => lowRecoverySalesmen.Contains(r.SalesPersonId ?? string.Empty))
                .GroupBy(r => ResolveCustomerKeyFromSalesmanRow(r))
                .Where(g => g.Sum(x => x.KurangBayar) > 0 &&
                            g.Any(x => ResolveAgingBucketKey(x.JatuhTempo, today) != "Current"))
                .Select(g => g.Key);

            foreach (var customerKey in lowRecoveryCustomerKeys)
            {
                var rows = salesmanRows.Where(r => ResolveCustomerKeyFromSalesmanRow(r) == customerKey).ToList();
                var overdueAmount = rows
                    .Where(r => ResolveAgingBucketKey(r.JatuhTempo, today) != "Current")
                    .Sum(r => r.KurangBayar);
                if (overdueAmount <= 0)
                    continue;

                var sample = rows.First();
                TryAdd(candidates, addedKeys, 6, customerKey, RiskLowRecoveryCustomer, "Deteriorating — Low Recovery",
                    EntityTypeCustomer, sample.CustomerCode ?? customerKey, sample.CustomerName ?? customerKey,
                    overdueAmount, "Overdue",
                    "Salesman's recovery vs billing is below billing pace");
            }

            foreach (var wilayahKey in hotspotWilayah)
            {
                var dueSoonInWilayah = wilayahRows
                    .Where(r => (r.WilayahId ?? r.WilayahName ?? string.Empty) == wilayahKey)
                    .Where(r => r.JatuhTempo.Date > today && r.JatuhTempo.Date <= me)
                    .Sum(r => r.KurangBayar);

                if (companyDueSoonTotal <= 0 ||
                    dueSoonInWilayah / companyDueSoonTotal * 100m < WilayahDueSoonThresholdPercent)
                {
                    continue;
                }

                var sample = wilayahRows.First(r =>
                    (r.WilayahId ?? r.WilayahName ?? string.Empty) == wilayahKey);
                TryAdd(candidates, addedKeys, 7, wilayahKey, RiskWilayahHotspotDue, "Wilayah Hotspot — Due Exposure",
                    EntityTypeWilayah, sample.WilayahId ?? wilayahKey, sample.WilayahName ?? wilayahKey,
                    dueSoonInWilayah, "Due this month",
                    $"Due-soon exposure is {dueSoonInWilayah / companyDueSoonTotal * 100m:F1}% of company due-soon total");
            }

            var salesmanCashById = lookback
                .GroupBy(p => p.SalesPersonId ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.BayarTunai));

            foreach (var group in salesmanRows
                         .Where(r => ResolveAgingBucketKey(r.JatuhTempo, today) != "Current")
                         .GroupBy(ResolveCustomerKeyFromSalesmanRow))
            {
                var customerKey = group.Key;
                var overdueAmount = group.Sum(r => r.KurangBayar);
                var salesmanIds = group.Select(r => r.SalesPersonId ?? string.Empty).Distinct().ToList();
                var hasRecentCash = salesmanIds.Any(id =>
                    salesmanCashById.TryGetValue(id, out var cash) && cash > 0);

                if (hasRecentCash)
                    continue;

                var sample = group.First();
                TryAdd(candidates, addedKeys, 8, customerKey, RiskExpectedOverdueGrowth, "Expected Overdue Growth",
                    EntityTypeCustomer, sample.CustomerCode ?? customerKey, sample.CustomerName ?? customerKey,
                    overdueAmount, "Overdue",
                    "No cash collection from assigned salesman in last 30 days");
            }

            return candidates
                .OrderBy(c => c.RulePriority)
                .ThenByDescending(c => c.Row.Amount)
                .Take(MaxRiskRows)
                .Select((c, index) =>
                {
                    c.Row.SortOrder = index + 1;
                    return c.Row;
                })
                .ToList();
        }

        private static void TryAdd(
            List<RiskCandidate> candidates,
            HashSet<string> addedKeys,
            int rulePriority,
            string entityKey,
            string riskKey,
            string riskLabel,
            string entityType,
            string entityId,
            string entityName,
            decimal amount,
            string dueOrAging,
            string explanation)
        {
            var dedupeKey = $"{riskKey}|{entityType}|{entityKey}";
            if (!addedKeys.Add(dedupeKey))
                return;

            candidates.Add(new RiskCandidate
            {
                RulePriority = rulePriority,
                Row = new DashboardCashFlowCollectionRiskRow
                {
                    RiskKey = riskKey,
                    RiskLabel = riskLabel,
                    EntityType = entityType,
                    EntityId = entityId ?? string.Empty,
                    EntityName = entityName ?? string.Empty,
                    Amount = amount,
                    DueOrAgingText = dueOrAging ?? string.Empty,
                    RuleExplanation = explanation ?? string.Empty,
                    ReportRoute = ReportRoute
                }
            });
        }

        private static Dictionary<string, decimal> BuildCustomerOverdueTotals(
            List<PiutangOpenBalanceDto> openRows,
            DateTime today)
        {
            var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in openRows)
            {
                if (ResolveAgingBucketKey(row.JatuhTempo, today) == "Current")
                    continue;

                var key = ResolveCustomerKey(row);
                if (!totals.TryGetValue(key, out var current))
                    current = 0m;
                totals[key] = current + row.KurangBayar;
            }

            return totals;
        }

        private static decimal ComputeP75Floor(IEnumerable<decimal> customerOverdueAmounts)
        {
            var values = customerOverdueAmounts
                .Where(v => v > 0)
                .OrderBy(v => v)
                .ToList();

            if (values.Count == 0)
                return 0m;

            var index = (int)Math.Ceiling(values.Count * 0.75) - 1;
            if (index < 0)
                index = 0;
            if (index >= values.Count)
                index = values.Count - 1;

            return values[index];
        }

        private static HashSet<string> GetAttentionCustomerKeys(
            DashboardCollectionAggregateResult collectionResult,
            string signalKey)
        {
            return new HashSet<string>(
                (collectionResult.AttentionList ?? new List<DashboardCollectionAttentionRow>())
                    .Where(a => string.Equals(a.SignalKey, signalKey, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(a.EntityType, DashboardCollectionAggregator.EntityTypeCustomer,
                                    StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.EntityId ?? a.EntityCode ?? a.EntityName ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> GetAttentionSalesmanIds(
            DashboardCollectionAggregateResult collectionResult,
            string signalKey)
        {
            return new HashSet<string>(
                (collectionResult.AttentionList ?? new List<DashboardCollectionAttentionRow>())
                    .Where(a => string.Equals(a.SignalKey, signalKey, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(a.EntityType, DashboardCollectionAggregator.EntityTypeSalesman,
                                    StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.EntityId ?? a.EntityCode ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> GetAttentionWilayahKeys(
            DashboardCollectionAggregateResult collectionResult,
            string signalKey)
        {
            return new HashSet<string>(
                (collectionResult.AttentionList ?? new List<DashboardCollectionAttentionRow>())
                    .Where(a => string.Equals(a.SignalKey, signalKey, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(a.EntityType, DashboardCollectionAggregator.EntityTypeWilayah,
                                    StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.EntityId ?? a.EntityName ?? a.WilayahName ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);
        }

        private static string ResolveCustomerKey(PiutangOpenBalanceDto row) =>
            row.CustomerId ?? row.CustomerCode ?? row.CustomerName ?? string.Empty;

        private static string ResolveCustomerKeyFromSalesmanRow(PiutangOpenBalanceWithSalesmanDto row) =>
            row.CustomerCode ?? row.CustomerName ?? string.Empty;

        private static string ResolveAgingBucketKey(DateTime jatuhTempo, DateTime today)
        {
            var daysOverdue = (today - jatuhTempo.Date).Days;

            if (daysOverdue <= 0) return "Current";
            if (daysOverdue <= 30) return "Days1To30";
            if (daysOverdue <= 60) return "Days31To60";
            if (daysOverdue <= 90) return "Days61To90";
            return AgingOver90BucketKey;
        }

        private static string FormatAgingLabel(string bucket)
        {
            switch (bucket)
            {
                case "Days1To30": return "1–30 days";
                case "Days31To60": return "31–60 days";
                case "Days61To90": return "61–90 days";
                case AgingOver90BucketKey: return ">90 days";
                default: return bucket;
            }
        }

        private static string FormatAmount(decimal amount) =>
            amount.ToString("N0", CultureInfo.GetCultureInfo("id-ID"));
    }
}
