using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.CustomerAgg;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardCustomerAggregator
    {
        public const int TopCustomerCount = 10;
        public const int DormantDaysThreshold = 90;

        public const string SignalOverdue = "Overdue";
        public const string SignalDormant = "Dormant";
        public const string SignalPlafondBreach = "PlafondBreach";
        public const string SignalSuspendedWithSales = "SuspendedWithSales";

        private const string AgingOver90BucketKey = "DaysOver90";
        private const string SegmentTypeKlasifikasi = "Klasifikasi";
        private const string SegmentTypeWilayah = "Wilayah";
        private const string SegmentTypeActivity = "Activity";
        private const string UnknownSegment = "Unknown";

        public DashboardCustomerAggregateResult Aggregate(
            IEnumerable<FakturView> fakturRows,
            IEnumerable<PiutangOpenBalanceDto> piutangRows,
            IEnumerable<CustomerLastFakturDto> lastFakturRows,
            IEnumerable<CustomerModel> customers,
            Periode periode,
            DateTime today,
            DateTime generatedAt)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var fakturList = (fakturRows ?? Enumerable.Empty<FakturView>()).ToList();
            var outstanding = (piutangRows ?? Enumerable.Empty<PiutangOpenBalanceDto>())
                .Where(r => r.KurangBayar > 1)
                .ToList();
            var lastFakturList = (lastFakturRows ?? Enumerable.Empty<CustomerLastFakturDto>()).ToList();
            var customerList = (customers ?? Enumerable.Empty<CustomerModel>()).ToList();

            var periodStart = periode.Tgl1.Date;
            var totalOmzet = fakturList.Sum(r => r.GrandTotal);
            var totalPiutang = outstanding.Sum(r => r.KurangBayar);

            var masterByKey = BuildMasterLookup(customerList);
            var activeSet = BuildActiveSet(fakturList);
            var dormantSet = BuildDormantSet(lastFakturList, activeSet, today);
            var openBalanceByKey = BuildOpenBalanceByKey(outstanding);
            var overdueBalanceByKey = BuildOverdueBalanceByKey(outstanding, today);
            var overdueCustomerKeys = overdueBalanceByKey.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var omzetByKey = BuildOmzetByKey(fakturList);
            var displayNames = BuildDisplayNameLookup(fakturList, outstanding, lastFakturList, customerList);
            var codeByKey = BuildCodeLookup(fakturList, outstanding, lastFakturList, customerList);

            var plafondBreachKeys = BuildPlafondBreachKeys(openBalanceByKey, masterByKey);
            var suspendedWithSalesKeys = BuildSuspendedWithSalesKeys(activeSet, masterByKey);

            var agingOver90Amount = outstanding
                .Where(r => ResolveAgingBucketKey(r.JatuhTempo, today) == AgingOver90BucketKey)
                .Sum(r => r.KurangBayar);

            var topOmzet = BuildTopOmzet(fakturList, totalOmzet, codeByKey, displayNames, masterByKey);
            var topPiutang = BuildTopPiutang(outstanding, totalPiutang, codeByKey, displayNames, masterByKey);

            var topOmzetPercent = topOmzet.Count > 0 && totalOmzet > 0
                ? topOmzet[0].OmzetAmount / totalOmzet * 100m
                : (decimal?)null;

            var topPiutangPercent = topPiutang.Count > 0 && totalPiutang > 0
                ? topPiutang[0].OutstandingBalance / totalPiutang * 100m
                : (decimal?)null;

            var attentionList = BuildAttentionList(
                overdueBalanceByKey,
                dormantSet,
                plafondBreachKeys,
                suspendedWithSalesKeys,
                openBalanceByKey,
                omzetByKey,
                lastFakturList,
                masterByKey,
                codeByKey,
                displayNames,
                today);

            var segmentation = BuildSegmentation(
                customerList,
                activeSet,
                dormantSet);

            return new DashboardCustomerAggregateResult
            {
                PeriodYear = periodStart.Year,
                PeriodMonth = periodStart.Month,
                TotalOmzet = totalOmzet,
                TotalPiutang = totalPiutang,
                ActiveCustomerCount = activeSet.Count,
                DormantCustomerCount = dormantSet.Count,
                OverdueCustomerCount = overdueCustomerKeys.Count,
                PlafondBreachCount = plafondBreachKeys.Count,
                SuspendedWithSalesCount = suspendedWithSalesKeys.Count,
                AgingOver90Amount = agingOver90Amount,
                TopOmzetCustomerPercent = topOmzetPercent,
                TopPiutangCustomerPercent = topPiutangPercent,
                GeneratedAt = generatedAt,
                TopOmzet = topOmzet,
                TopPiutang = topPiutang,
                AttentionList = attentionList,
                Segmentation = segmentation
            };
        }

        private static Dictionary<string, CustomerModel> BuildMasterLookup(List<CustomerModel> customers)
        {
            var lookup = new Dictionary<string, CustomerModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var customer in customers)
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(
                    customer.CustomerCode,
                    customer.CustomerName);
                if (key.Length == 0)
                    continue;

                lookup[key] = customer;
            }

            return lookup;
        }

        private static HashSet<string> BuildActiveSet(List<FakturView> fakturList)
        {
            return fakturList
                .Select(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.Customer))
                .Where(key => key.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> BuildDormantSet(
            List<CustomerLastFakturDto> lastFakturList,
            HashSet<string> activeSet,
            DateTime today)
        {
            var cutoff = today.Date.AddDays(-DormantDaysThreshold);
            var dormant = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in lastFakturList)
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(row.CustomerCode, row.CustomerName);
                if (key.Length == 0)
                    continue;

                if (activeSet.Contains(key))
                    continue;

                if (row.LastFakturDate.Date <= cutoff)
                    dormant.Add(key);
            }

            return dormant;
        }

        private static Dictionary<string, decimal> BuildOpenBalanceByKey(List<PiutangOpenBalanceDto> outstanding)
        {
            return outstanding
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.CustomerName))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.KurangBayar), StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, decimal> BuildOverdueBalanceByKey(
            List<PiutangOpenBalanceDto> outstanding,
            DateTime today)
        {
            return outstanding
                .Where(r => ResolveAgingBucketKey(r.JatuhTempo, today) != "Current")
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.CustomerName))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.KurangBayar), StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, decimal> BuildOmzetByKey(List<FakturView> fakturList)
        {
            return fakturList
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.Customer))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.GrandTotal), StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string> BuildDisplayNameLookup(
            List<FakturView> fakturList,
            List<PiutangOpenBalanceDto> outstanding,
            List<CustomerLastFakturDto> lastFakturList,
            List<CustomerModel> customers)
        {
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            void Add(string code, string name)
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(code, name);
                if (key.Length == 0)
                    return;

                if (!names.ContainsKey(key) && !string.IsNullOrWhiteSpace(name))
                    names[key] = name.Trim();
            }

            foreach (var c in customers)
                Add(c.CustomerCode, c.CustomerName);
            foreach (var r in fakturList)
                Add(r.CustomerCode, r.Customer);
            foreach (var r in outstanding)
                Add(r.CustomerCode, r.CustomerName);
            foreach (var r in lastFakturList)
                Add(r.CustomerCode, r.CustomerName);

            return names;
        }

        private static Dictionary<string, string> BuildCodeLookup(
            List<FakturView> fakturList,
            List<PiutangOpenBalanceDto> outstanding,
            List<CustomerLastFakturDto> lastFakturList,
            List<CustomerModel> customers)
        {
            var codes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            void Add(string code, string name)
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(code, name);
                if (key.Length == 0)
                    return;

                if (!codes.ContainsKey(key) && !string.IsNullOrWhiteSpace(code))
                    codes[key] = code.Trim();
            }

            foreach (var c in customers)
                Add(c.CustomerCode, c.CustomerName);
            foreach (var r in fakturList)
                Add(r.CustomerCode, r.Customer);
            foreach (var r in outstanding)
                Add(r.CustomerCode, r.CustomerName);
            foreach (var r in lastFakturList)
                Add(r.CustomerCode, r.CustomerName);

            return codes;
        }

        private static HashSet<string> BuildPlafondBreachKeys(
            Dictionary<string, decimal> openBalanceByKey,
            Dictionary<string, CustomerModel> masterByKey)
        {
            var breachKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in openBalanceByKey)
            {
                if (!masterByKey.TryGetValue(pair.Key, out var customer))
                    continue;

                if (customer.Plafond <= 0)
                    continue;

                if (pair.Value > customer.Plafond)
                    breachKeys.Add(pair.Key);
            }

            return breachKeys;
        }

        private static HashSet<string> BuildSuspendedWithSalesKeys(
            HashSet<string> activeSet,
            Dictionary<string, CustomerModel> masterByKey)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var key in activeSet)
            {
                if (!masterByKey.TryGetValue(key, out var customer))
                    continue;

                if (customer.IsSuspend)
                    keys.Add(key);
            }

            return keys;
        }

        private static string ResolveCustomerId(
            string key,
            Dictionary<string, CustomerModel> masterByKey)
        {
            if (!masterByKey.TryGetValue(key, out var customer)
                || string.IsNullOrWhiteSpace(customer.CustomerId))
            {
                return string.Empty;
            }

            return customer.CustomerId.Trim();
        }

        private static List<DashboardCustomerTopOmzetRow> BuildTopOmzet(
            List<FakturView> fakturList,
            decimal totalOmzet,
            Dictionary<string, string> codeByKey,
            Dictionary<string, string> displayNames,
            Dictionary<string, CustomerModel> masterByKey)
        {
            return fakturList
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.Customer))
                .Where(g => g.Key.Length > 0)
                .Select(g => new
                {
                    Key = g.Key,
                    CustomerName = ResolveDisplayName(g.Key, g.Select(x => x.Customer), displayNames),
                    OmzetAmount = g.Sum(r => r.GrandTotal)
                })
                .OrderByDescending(x => x.OmzetAmount)
                .ThenBy(x => x.CustomerName, StringComparer.OrdinalIgnoreCase)
                .Take(TopCustomerCount)
                .Select((x, index) => new DashboardCustomerTopOmzetRow
                {
                    Rank = index + 1,
                    CustomerId = ResolveCustomerId(x.Key, masterByKey),
                    CustomerCode = codeByKey.TryGetValue(x.Key, out var code) ? code : string.Empty,
                    CustomerName = x.CustomerName,
                    OmzetAmount = x.OmzetAmount,
                    PercentOfTotal = totalOmzet > 0 ? x.OmzetAmount / totalOmzet * 100m : (decimal?)null
                })
                .ToList();
        }

        private static List<DashboardCustomerTopPiutangRow> BuildTopPiutang(
            List<PiutangOpenBalanceDto> outstanding,
            decimal totalPiutang,
            Dictionary<string, string> codeByKey,
            Dictionary<string, string> displayNames,
            Dictionary<string, CustomerModel> masterByKey)
        {
            return outstanding
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.CustomerName))
                .Where(g => g.Key.Length > 0)
                .Select(g => new
                {
                    Key = g.Key,
                    CustomerName = ResolveDisplayName(
                        g.Key,
                        g.Select(x => x.CustomerName),
                        displayNames),
                    OutstandingBalance = g.Sum(r => r.KurangBayar)
                })
                .OrderByDescending(x => x.OutstandingBalance)
                .ThenBy(x => x.CustomerName, StringComparer.OrdinalIgnoreCase)
                .Take(TopCustomerCount)
                .Select((x, index) => new DashboardCustomerTopPiutangRow
                {
                    Rank = index + 1,
                    CustomerId = ResolveCustomerId(x.Key, masterByKey),
                    CustomerCode = codeByKey.TryGetValue(x.Key, out var code) ? code : string.Empty,
                    CustomerName = x.CustomerName,
                    OutstandingBalance = x.OutstandingBalance,
                    PercentOfTotal = totalPiutang > 0
                        ? x.OutstandingBalance / totalPiutang * 100m
                        : (decimal?)null
                })
                .ToList();
        }

        private static List<DashboardCustomerAttentionRow> BuildAttentionList(
            Dictionary<string, decimal> overdueBalanceByKey,
            HashSet<string> dormantSet,
            HashSet<string> plafondBreachKeys,
            HashSet<string> suspendedWithSalesKeys,
            Dictionary<string, decimal> openBalanceByKey,
            Dictionary<string, decimal> omzetByKey,
            List<CustomerLastFakturDto> lastFakturList,
            Dictionary<string, CustomerModel> masterByKey,
            Dictionary<string, string> codeByKey,
            Dictionary<string, string> displayNames,
            DateTime today)
        {
            var lastFakturByKey = lastFakturList
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.CustomerName))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(g => g.Key, g => g.Max(x => x.LastFakturDate), StringComparer.OrdinalIgnoreCase);

            var rows = new List<(int Priority, string CustomerName, DashboardCustomerAttentionRow Row)>();

            void AddRow(string key, string signalKey, string signalLabel, decimal? valueAmount, string valueText)
            {
                var customerName = displayNames.TryGetValue(key, out var name) ? name : key;
                var customerCode = codeByKey.TryGetValue(key, out var code) ? code : string.Empty;
                var wilayah = masterByKey.TryGetValue(key, out var customer)
                    ? customer.WilayahName?.Trim() ?? string.Empty
                    : string.Empty;

                rows.Add((
                    SignalPriority(signalKey),
                    customerName,
                    new DashboardCustomerAttentionRow
                    {
                        CustomerId = ResolveCustomerId(key, masterByKey),
                        CustomerCode = customerCode,
                        CustomerName = customerName,
                        SignalKey = signalKey,
                        SignalLabel = signalLabel,
                        ValueAmount = valueAmount,
                        ValueText = valueText,
                        WilayahName = wilayah
                    }));
            }

            foreach (var pair in overdueBalanceByKey)
                AddRow(pair.Key, SignalOverdue, "Overdue", pair.Value, null);

            foreach (var key in plafondBreachKeys)
            {
                var balance = openBalanceByKey[key];
                var plafond = masterByKey[key].Plafond;
                AddRow(key, SignalPlafondBreach, "Plafond Breach", balance - plafond, null);
            }

            foreach (var key in suspendedWithSalesKeys)
            {
                var omzet = omzetByKey.TryGetValue(key, out var amount) ? amount : 0m;
                AddRow(key, SignalSuspendedWithSales, "Suspended + Sales", omzet, null);
            }

            foreach (var key in dormantSet)
            {
                var days = lastFakturByKey.TryGetValue(key, out var lastDate)
                    ? (today.Date - lastDate.Date).Days
                    : DormantDaysThreshold;
                AddRow(key, SignalDormant, "Dormant", null, $"{days} days since last purchase");
            }

            return rows
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.CustomerName, StringComparer.OrdinalIgnoreCase)
                .Select((r, index) =>
                {
                    r.Row.SortOrder = index + 1;
                    return r.Row;
                })
                .ToList();
        }

        private static List<DashboardCustomerSegmentationRow> BuildSegmentation(
            List<CustomerModel> customers,
            HashSet<string> activeSet,
            HashSet<string> dormantSet)
        {
            var klasifikasiGroups = new Dictionary<string, (int Total, int Active, int Dormant)>(
                StringComparer.OrdinalIgnoreCase);
            var wilayahGroups = new Dictionary<string, (int Total, int Active, int Dormant)>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var customer in customers)
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(
                    customer.CustomerCode,
                    customer.CustomerName);
                if (key.Length == 0)
                    continue;

                var isActive = activeSet.Contains(key);
                var isDormant = dormantSet.Contains(key);

                var klasifikasi = NormalizeSegmentLabel(customer.KlasifikasiName);
                var wilayah = NormalizeSegmentLabel(customer.WilayahName);

                AddSegmentCount(klasifikasiGroups, klasifikasi, isActive, isDormant);
                AddSegmentCount(wilayahGroups, wilayah, isActive, isDormant);
            }

            var rows = new List<DashboardCustomerSegmentationRow>();

            rows.AddRange(ToSegmentRows(klasifikasiGroups, SegmentTypeKlasifikasi));
            rows.AddRange(ToSegmentRows(wilayahGroups, SegmentTypeWilayah));

            rows.Add(new DashboardCustomerSegmentationRow
            {
                SegmentType = SegmentTypeActivity,
                SegmentKey = "Active",
                SegmentLabel = "Active",
                CustomerCount = activeSet.Count,
                ActiveCount = activeSet.Count,
                DormantCount = 0,
                SortOrder = 1
            });

            rows.Add(new DashboardCustomerSegmentationRow
            {
                SegmentType = SegmentTypeActivity,
                SegmentKey = "Dormant",
                SegmentLabel = "Dormant",
                CustomerCount = dormantSet.Count,
                ActiveCount = 0,
                DormantCount = dormantSet.Count,
                SortOrder = 2
            });

            return rows;
        }

        private static void AddSegmentCount(
            Dictionary<string, (int Total, int Active, int Dormant)> groups,
            string segmentLabel,
            bool isActive,
            bool isDormant)
        {
            if (!groups.TryGetValue(segmentLabel, out var counts))
                counts = (0, 0, 0);

            counts.Total++;
            if (isActive)
                counts.Active++;
            if (isDormant)
                counts.Dormant++;

            groups[segmentLabel] = counts;
        }

        private static IEnumerable<DashboardCustomerSegmentationRow> ToSegmentRows(
            Dictionary<string, (int Total, int Active, int Dormant)> groups,
            string segmentType)
        {
            return groups
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select((g, index) => new DashboardCustomerSegmentationRow
                {
                    SegmentType = segmentType,
                    SegmentKey = g.Key,
                    SegmentLabel = g.Key,
                    CustomerCount = g.Value.Total,
                    ActiveCount = g.Value.Active,
                    DormantCount = g.Value.Dormant,
                    SortOrder = index + 1
                });
        }

        private static string NormalizeSegmentLabel(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? UnknownSegment : value.Trim();
        }

        private static string ResolveDisplayName(
            string key,
            IEnumerable<string> candidates,
            Dictionary<string, string> displayNames)
        {
            if (displayNames.TryGetValue(key, out var name))
                return name;

            return candidates
                .Select(n => n?.Trim())
                .FirstOrDefault(n => !string.IsNullOrEmpty(n))
                ?? key;
        }

        private static int SignalPriority(string signalKey)
        {
            switch (signalKey)
            {
                case SignalOverdue:
                    return 0;
                case SignalPlafondBreach:
                    return 1;
                case SignalSuspendedWithSales:
                    return 2;
                case SignalDormant:
                    return 3;
                default:
                    return 99;
            }
        }

        private static string ResolveAgingBucketKey(DateTime jatuhTempo, DateTime today)
        {
            var daysOverdue = (today - jatuhTempo.Date).Days;

            if (daysOverdue <= 0) return "Current";
            if (daysOverdue <= 30) return "Days1To30";
            if (daysOverdue <= 60) return "Days31To60";
            if (daysOverdue <= 90) return "Days61To90";
            return AgingOver90BucketKey;
        }
    }
}
