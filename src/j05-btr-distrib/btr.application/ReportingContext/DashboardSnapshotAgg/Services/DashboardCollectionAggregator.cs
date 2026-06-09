using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.CustomerAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardCollectionAggregator
    {
        public const int TopRankingCount = 10;
        public const int DormantDaysThreshold = 90;
        public const decimal WilayahHotspotThresholdPercent = 15m;

        public const string SignalChronicOverdue = "ChronicOverdue";
        public const string SignalLegacyDebt = "LegacyDebt";
        public const string SignalPlafondBreachOverdue = "PlafondBreachOverdue";
        public const string SignalOverdue = "Overdue";
        public const string SignalHighOverdueWorkload = "HighOverdueWorkload";
        public const string SignalLowRecoveryVsBilling = "LowRecoveryVsBilling";
        public const string SignalWilayahHotspot = "WilayahHotspot";

        public const string EntityTypeCustomer = "Customer";
        public const string EntityTypeSalesman = "Salesman";
        public const string EntityTypeWilayah = "Wilayah";

        private const string AgingOver90BucketKey = "DaysOver90";
        private const string PiutangReportRoute = "/reports/piutang";

        private static readonly (string Key, string Label, int SortOrder)[] OverdueAgingBucketDefinitions =
        {
            ("Days1To30", "1–30 Days", 1),
            ("Days31To60", "31–60 Days", 2),
            ("Days61To90", "61–90 Days", 3),
            ("DaysOver90", "> 90 Days", 4),
        };

        public DashboardCollectionAggregateResult Aggregate(
            IEnumerable<PiutangOpenBalanceDto> openBalanceRows,
            IEnumerable<PiutangOpenBalanceWithSalesmanDto> openBalanceWithSalesmanRows,
            IEnumerable<PiutangOpenBalanceWithWilayahDto> openBalanceWithWilayahRows,
            IEnumerable<PenerimaanPelunasanSalesDto> pelunasanRows,
            IEnumerable<FakturView> fakturRows,
            IEnumerable<CustomerLastFakturDto> lastFakturRows,
            IEnumerable<CustomerModel> customers,
            IEnumerable<SalesPersonModel> salespeople,
            Periode periode,
            DateTime today,
            DateTime generatedAt)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var outstanding = (openBalanceRows ?? Enumerable.Empty<PiutangOpenBalanceDto>())
                .Where(r => r.KurangBayar > 1)
                .ToList();
            var outstandingWithSalesman = (openBalanceWithSalesmanRows ?? Enumerable.Empty<PiutangOpenBalanceWithSalesmanDto>())
                .Where(r => r.KurangBayar > 1)
                .ToList();
            var outstandingWithWilayah = (openBalanceWithWilayahRows ?? Enumerable.Empty<PiutangOpenBalanceWithWilayahDto>())
                .Where(r => r.KurangBayar > 1)
                .ToList();
            var pelunasanList = (pelunasanRows ?? Enumerable.Empty<PenerimaanPelunasanSalesDto>()).ToList();
            var fakturList = (fakturRows ?? Enumerable.Empty<FakturView>()).ToList();
            var lastFakturList = (lastFakturRows ?? Enumerable.Empty<CustomerLastFakturDto>()).ToList();
            var customerList = (customers ?? Enumerable.Empty<CustomerModel>()).ToList();
            var salesPersonList = (salespeople ?? Enumerable.Empty<SalesPersonModel>()).ToList();

            var periodStart = periode.Tgl1.Date;
            var salesmanLookup = DashboardSalesmanKeyResolver.BuildLookup(salesPersonList);
            var masterByKey = BuildMasterLookup(customerList);

            var bucketTotals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var customerOverdue = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var customerOver90 = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var salesmanOverdue = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var wilayahOverdue = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var wilayahNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var customerCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var customerNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var customerWilayah = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            decimal overdueExposure = 0m;
            decimal agingOver90Exposure = 0m;

            foreach (var row in outstanding)
            {
                var key = DashboardCollectionKeyResolver.ResolveCustomerKey(row.CustomerCode, row.CustomerName);
                if (key.Length > 0)
                {
                    AddCustomerMetadata(key, row.CustomerCode, row.CustomerName, customerCodes, customerNames);
                }

                var bucket = ResolveAgingBucketKey(row.JatuhTempo, today);
                if (bucket == "Current")
                    continue;

                overdueExposure += row.KurangBayar;
                AddBucketAmount(bucketTotals, bucket, row.KurangBayar);

                if (key.Length == 0)
                    continue;

                AddAmount(customerOverdue, key, row.KurangBayar);
                if (bucket == AgingOver90BucketKey)
                    AddAmount(customerOver90, key, row.KurangBayar);
            }

            foreach (var row in outstandingWithSalesman)
            {
                var bucket = ResolveAgingBucketKey(row.JatuhTempo, today);
                if (bucket == "Current")
                    continue;

                var salesId = DashboardCollectionKeyResolver.ResolveSalesPersonId(
                    row.SalesPersonId,
                    row.SalesPersonName);
                if (salesId.Length == 0)
                    continue;

                AddAmount(salesmanOverdue, salesId, row.KurangBayar);
            }

            foreach (var row in outstandingWithWilayah)
            {
                var bucket = ResolveAgingBucketKey(row.JatuhTempo, today);
                if (bucket == "Current")
                    continue;

                var wilayahKey = DashboardCollectionKeyResolver.ResolveWilayahKey(row.WilayahId);
                var wilayahName = DashboardCollectionKeyResolver.ResolveWilayahDisplayName(row.WilayahId, row.WilayahName);
                wilayahNames[wilayahKey] = wilayahName;
                AddAmount(wilayahOverdue, wilayahKey, row.KurangBayar);

                var customerKey = DashboardCollectionKeyResolver.ResolveCustomerKey(row.CustomerCode, row.CustomerName);
                if (customerKey.Length > 0 && !customerWilayah.ContainsKey(customerKey))
                    customerWilayah[customerKey] = wilayahName;
            }

            foreach (var customer in customerList)
            {
                var key = DashboardCollectionKeyResolver.ResolveCustomerKey(customer.CustomerCode, customer.CustomerName);
                if (key.Length == 0)
                    continue;

                AddCustomerMetadata(key, customer.CustomerCode, customer.CustomerName, customerCodes, customerNames);
                if (!customerWilayah.ContainsKey(key) && !string.IsNullOrWhiteSpace(customer.WilayahName))
                    customerWilayah[key] = customer.WilayahName.Trim();
            }

            agingOver90Exposure = bucketTotals.TryGetValue(AgingOver90BucketKey, out var over90)
                ? over90
                : 0m;

            var openBalanceByKey = outstanding
                .GroupBy(r => DashboardCollectionKeyResolver.ResolveCustomerKey(r.CustomerCode, r.CustomerName))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.KurangBayar), StringComparer.OrdinalIgnoreCase);

            var activeSet = fakturList
                .Select(r => DashboardCollectionKeyResolver.ResolveCustomerKey(r.CustomerCode, r.Customer))
                .Where(k => k.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var dormantSet = BuildDormantSet(lastFakturList, activeSet, today);
            var legacyDebtKeys = dormantSet
                .Where(k => openBalanceByKey.TryGetValue(k, out var bal) && bal > 1)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var plafondBreachOverdueKeys = BuildPlafondBreachOverdueKeys(
                customerOverdue,
                openBalanceByKey,
                masterByKey);

            var chronicOverdueKeys = customerOver90.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var topCustomerOverdue = customerOverdue
                .OrderByDescending(p => p.Value)
                .ThenBy(p => ResolveCustomerName(p.Key, customerNames), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            var overdueConcentrationPercent = topCustomerOverdue.Key != null &&
                overdueExposure > 0 &&
                topCustomerOverdue.Value > 0
                    ? topCustomerOverdue.Value / overdueExposure * 100m
                    : (decimal?)null;

            var cashCollectedMtd = pelunasanList.Sum(r => r.BayarTunai);
            var monthCollections = pelunasanList.Sum(r => r.TotalBayar);
            var paymentMixGiro = pelunasanList.Sum(r => r.BayarGiro);
            var paymentMixAdjustment = pelunasanList.Sum(r => r.Retur + r.Potongan + r.MateraiAdmin);
            var settlementTotal = cashCollectedMtd + paymentMixGiro + paymentMixAdjustment;
            var monthFakturOmzet = fakturList.Sum(r => r.GrandTotal);

            decimal? recoveryVsBillingPercent = monthFakturOmzet > 0
                ? monthCollections / monthFakturOmzet * 100m
                : (decimal?)null;

            decimal? paymentMixCashPercent = settlementTotal > 0
                ? cashCollectedMtd / settlementTotal * 100m
                : (decimal?)null;
            decimal? paymentMixGiroPercent = settlementTotal > 0
                ? paymentMixGiro / settlementTotal * 100m
                : (decimal?)null;
            decimal? paymentMixAdjustmentPercent = settlementTotal > 0
                ? paymentMixAdjustment / settlementTotal * 100m
                : (decimal?)null;

            var repCollections = pelunasanList
                .GroupBy(r => DashboardCollectionKeyResolver.ResolveSalesPersonId(r.SalesPersonId, r.SalesName))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.TotalBayar),
                    StringComparer.OrdinalIgnoreCase);

            var repOmzet = fakturList
                .GroupBy(r => DashboardCollectionKeyResolver.ResolveSalesPersonId(r.SalesPersonId, r.SalesPersonName))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.GrandTotal),
                    StringComparer.OrdinalIgnoreCase);

            var agingRiskSummary = BuildAgingRiskSummary(bucketTotals);
            var topOverdueCustomers = BuildTopOverdueCustomers(customerOverdue, overdueExposure, customerCodes, customerNames);
            var topOverdueSalesmen = BuildTopOverdueSalesmen(salesmanOverdue, overdueExposure, salesPersonList, salesmanLookup);
            var topOverdueWilayah = BuildTopOverdueWilayah(wilayahOverdue, overdueExposure, wilayahNames);

            var wilayahHotspotKeys = wilayahOverdue
                .Where(p => overdueExposure > 0 && p.Value / overdueExposure * 100m >= WilayahHotspotThresholdPercent)
                .Select(p => p.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var lowRecoveryRepIds = repOmzet
                .Where(p => p.Value > 0)
                .Where(p =>
                {
                    repCollections.TryGetValue(p.Key, out var collections);
                    return collections < p.Value;
                })
                .Select(p => p.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var highOverdueRepIds = salesmanOverdue
                .Where(p => p.Value > 0)
                .Select(p => p.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var attentionList = BuildAttentionList(
                customerOverdue,
                customerOver90,
                chronicOverdueKeys,
                plafondBreachOverdueKeys,
                legacyDebtKeys,
                highOverdueRepIds,
                lowRecoveryRepIds,
                repOmzet,
                repCollections,
                salesmanOverdue,
                wilayahHotspotKeys,
                wilayahOverdue,
                overdueExposure,
                customerCodes,
                customerNames,
                customerWilayah,
                salesPersonList,
                salesmanLookup,
                wilayahNames);

            return new DashboardCollectionAggregateResult
            {
                PeriodYear = periodStart.Year,
                PeriodMonth = periodStart.Month,
                OverdueExposure = overdueExposure,
                AgingOver90Exposure = agingOver90Exposure,
                OverdueConcentrationPercent = overdueConcentrationPercent,
                CashCollectedMtd = cashCollectedMtd,
                MonthCollections = monthCollections,
                MonthFakturOmzet = monthFakturOmzet,
                RecoveryVsBillingPercent = recoveryVsBillingPercent,
                PaymentMixCashAmount = cashCollectedMtd,
                PaymentMixGiroAmount = paymentMixGiro,
                PaymentMixAdjustmentAmount = paymentMixAdjustment,
                PaymentMixCashPercent = paymentMixCashPercent,
                PaymentMixGiroPercent = paymentMixGiroPercent,
                PaymentMixAdjustmentPercent = paymentMixAdjustmentPercent,
                LegacyDebtCount = legacyDebtKeys.Count,
                ChronicOverdueCount = chronicOverdueKeys.Count,
                WilayahHotspotCount = wilayahHotspotKeys.Count,
                LowRecoveryVsBillingCount = lowRecoveryRepIds.Count,
                GeneratedAt = generatedAt,
                AgingRiskSummary = agingRiskSummary,
                AttentionList = attentionList,
                TopOverdueCustomers = topOverdueCustomers,
                TopOverdueSalesmen = topOverdueSalesmen,
                TopOverdueWilayah = topOverdueWilayah
            };
        }

        private static Dictionary<string, CustomerModel> BuildMasterLookup(List<CustomerModel> customers)
        {
            var lookup = new Dictionary<string, CustomerModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var customer in customers)
            {
                var key = DashboardCollectionKeyResolver.ResolveCustomerKey(customer.CustomerCode, customer.CustomerName);
                if (key.Length > 0)
                    lookup[key] = customer;
            }

            return lookup;
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
                var key = DashboardCollectionKeyResolver.ResolveCustomerKey(row.CustomerCode, row.CustomerName);
                if (key.Length == 0 || activeSet.Contains(key))
                    continue;

                if (row.LastFakturDate.Date <= cutoff)
                    dormant.Add(key);
            }

            return dormant;
        }

        private static HashSet<string> BuildPlafondBreachOverdueKeys(
            Dictionary<string, decimal> customerOverdue,
            Dictionary<string, decimal> openBalanceByKey,
            Dictionary<string, CustomerModel> masterByKey)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in customerOverdue)
            {
                if (pair.Value <= 0)
                    continue;

                if (!masterByKey.TryGetValue(pair.Key, out var customer))
                    continue;

                if (customer.Plafond <= 0)
                    continue;

                if (openBalanceByKey.TryGetValue(pair.Key, out var balance) && balance > customer.Plafond)
                    keys.Add(pair.Key);
            }

            return keys;
        }

        private static List<DashboardCollectionAgingRow> BuildAgingRiskSummary(
            Dictionary<string, decimal> bucketTotals)
        {
            return OverdueAgingBucketDefinitions
                .Select(def => new DashboardCollectionAgingRow
                {
                    BucketKey = def.Key,
                    BucketLabel = def.Label,
                    Amount = bucketTotals.TryGetValue(def.Key, out var amount) ? amount : 0m,
                    SortOrder = def.SortOrder
                })
                .ToList();
        }

        private static List<DashboardCollectionTopOverdueCustomerRow> BuildTopOverdueCustomers(
            Dictionary<string, decimal> customerOverdue,
            decimal totalOverdue,
            Dictionary<string, string> customerCodes,
            Dictionary<string, string> customerNames)
        {
            return customerOverdue
                .Where(p => p.Value > 0)
                .OrderByDescending(p => p.Value)
                .ThenBy(p => ResolveCustomerName(p.Key, customerNames), StringComparer.OrdinalIgnoreCase)
                .Take(TopRankingCount)
                .Select((p, index) => new DashboardCollectionTopOverdueCustomerRow
                {
                    Rank = index + 1,
                    CustomerCode = customerCodes.TryGetValue(p.Key, out var code) ? code : string.Empty,
                    CustomerName = ResolveCustomerName(p.Key, customerNames),
                    OverdueBalance = p.Value,
                    PercentOfTotal = totalOverdue > 0 ? p.Value / totalOverdue * 100m : (decimal?)null
                })
                .ToList();
        }

        private static List<DashboardCollectionTopOverdueSalesmanRow> BuildTopOverdueSalesmen(
            Dictionary<string, decimal> salesmanOverdue,
            decimal totalOverdue,
            List<SalesPersonModel> salespeople,
            DashboardSalesmanKeyResolver.SalesmanLookup lookup)
        {
            var codeById = salespeople
                .Where(p => !string.IsNullOrWhiteSpace(p.SalesPersonId))
                .ToDictionary(
                    p => p.SalesPersonId.Trim(),
                    p => p.SalesPersonCode?.Trim() ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            var nameById = salespeople
                .Where(p => !string.IsNullOrWhiteSpace(p.SalesPersonId))
                .ToDictionary(
                    p => p.SalesPersonId.Trim(),
                    p => p.SalesPersonName?.Trim() ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            return salesmanOverdue
                .Where(p => p.Value > 0)
                .OrderByDescending(p => p.Value)
                .ThenBy(p => ResolveSalesPersonName(p.Key, nameById, lookup), StringComparer.OrdinalIgnoreCase)
                .Take(TopRankingCount)
                .Select((p, index) => new DashboardCollectionTopOverdueSalesmanRow
                {
                    Rank = index + 1,
                    SalesPersonId = p.Key,
                    SalesPersonCode = codeById.TryGetValue(p.Key, out var code) ? code : string.Empty,
                    SalesPersonName = ResolveSalesPersonName(p.Key, nameById, lookup),
                    OverdueBalance = p.Value,
                    PercentOfTotal = totalOverdue > 0 ? p.Value / totalOverdue * 100m : (decimal?)null
                })
                .ToList();
        }

        private static List<DashboardCollectionTopOverdueWilayahRow> BuildTopOverdueWilayah(
            Dictionary<string, decimal> wilayahOverdue,
            decimal totalOverdue,
            Dictionary<string, string> wilayahNames)
        {
            return wilayahOverdue
                .Where(p => p.Value > 0)
                .OrderByDescending(p => p.Value)
                .ThenBy(p => ResolveWilayahName(p.Key, wilayahNames), StringComparer.OrdinalIgnoreCase)
                .Take(TopRankingCount)
                .Select((p, index) => new DashboardCollectionTopOverdueWilayahRow
                {
                    Rank = index + 1,
                    WilayahId = p.Key,
                    WilayahName = ResolveWilayahName(p.Key, wilayahNames),
                    OverdueBalance = p.Value,
                    PercentOfTotal = totalOverdue > 0 ? p.Value / totalOverdue * 100m : (decimal?)null
                })
                .ToList();
        }

        private static List<DashboardCollectionAttentionRow> BuildAttentionList(
            Dictionary<string, decimal> customerOverdue,
            Dictionary<string, decimal> customerOver90,
            HashSet<string> chronicOverdueKeys,
            HashSet<string> plafondBreachOverdueKeys,
            HashSet<string> legacyDebtKeys,
            HashSet<string> highOverdueRepIds,
            HashSet<string> lowRecoveryRepIds,
            Dictionary<string, decimal> repOmzet,
            Dictionary<string, decimal> repCollections,
            Dictionary<string, decimal> salesmanOverdue,
            HashSet<string> wilayahHotspotKeys,
            Dictionary<string, decimal> wilayahOverdue,
            decimal totalOverdue,
            Dictionary<string, string> customerCodes,
            Dictionary<string, string> customerNames,
            Dictionary<string, string> customerWilayah,
            List<SalesPersonModel> salespeople,
            DashboardSalesmanKeyResolver.SalesmanLookup lookup,
            Dictionary<string, string> wilayahNames)
        {
            var rows = new List<(int SignalPriority, int EntityTypePriority, string EntityName, decimal ValueAmount, DashboardCollectionAttentionRow Row)>();

            void AddCustomer(string key, string signalKey, string signalLabel, decimal? valueAmount, string valueText)
            {
                var name = ResolveCustomerName(key, customerNames);
                var code = customerCodes.TryGetValue(key, out var c) ? c : string.Empty;
                var wilayah = customerWilayah.TryGetValue(key, out var w) ? w : string.Empty;

                rows.Add((
                    CustomerSignalPriority(signalKey),
                    0,
                    name,
                    valueAmount ?? 0m,
                    new DashboardCollectionAttentionRow
                    {
                        EntityType = EntityTypeCustomer,
                        EntityId = key,
                        EntityCode = code,
                        EntityName = name,
                        SignalKey = signalKey,
                        SignalLabel = signalLabel,
                        ValueAmount = valueAmount,
                        ValueText = valueText,
                        WilayahName = wilayah,
                        ReportRoute = PiutangReportRoute
                    }));
            }

            foreach (var key in chronicOverdueKeys)
            {
                customerOver90.TryGetValue(key, out var amount);
                AddCustomer(key, SignalChronicOverdue, "Chronic Overdue", amount, null);
            }

            foreach (var key in plafondBreachOverdueKeys)
            {
                if (chronicOverdueKeys.Contains(key))
                    continue;

                customerOverdue.TryGetValue(key, out var amount);
                AddCustomer(key, SignalPlafondBreachOverdue, "Plafond Breach + Overdue", amount, null);
            }

            foreach (var key in legacyDebtKeys)
            {
                if (chronicOverdueKeys.Contains(key) || plafondBreachOverdueKeys.Contains(key))
                    continue;

                customerOverdue.TryGetValue(key, out var amount);
                AddCustomer(key, SignalLegacyDebt, "Legacy Debt", amount, null);
            }

            foreach (var pair in customerOverdue)
            {
                if (chronicOverdueKeys.Contains(pair.Key) ||
                    plafondBreachOverdueKeys.Contains(pair.Key) ||
                    legacyDebtKeys.Contains(pair.Key))
                {
                    continue;
                }

                AddCustomer(pair.Key, SignalOverdue, "Overdue", pair.Value, null);
            }

            var codeById = salespeople
                .Where(p => !string.IsNullOrWhiteSpace(p.SalesPersonId))
                .ToDictionary(
                    p => p.SalesPersonId.Trim(),
                    p => p.SalesPersonCode?.Trim() ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            var nameById = salespeople
                .Where(p => !string.IsNullOrWhiteSpace(p.SalesPersonId))
                .ToDictionary(
                    p => p.SalesPersonId.Trim(),
                    p => p.SalesPersonName?.Trim() ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            foreach (var repId in lowRecoveryRepIds)
            {
                repOmzet.TryGetValue(repId, out var omzet);
                repCollections.TryGetValue(repId, out var collections);
                var recoveryPct = omzet > 0 ? collections / omzet * 100m : 0m;
                var name = ResolveSalesPersonName(repId, nameById, lookup);

                rows.Add((
                    AttentionSignalPriority(SignalLowRecoveryVsBilling),
                    1,
                    name,
                    omzet,
                    new DashboardCollectionAttentionRow
                    {
                        EntityType = EntityTypeSalesman,
                        EntityId = repId,
                        EntityCode = codeById.TryGetValue(repId, out var code) ? code : string.Empty,
                        EntityName = name,
                        SignalKey = SignalLowRecoveryVsBilling,
                        SignalLabel = "Low Recovery vs Billing",
                        ValueAmount = omzet,
                        ValueText = $"Recovery {recoveryPct:N0}% vs billing",
                        WilayahName = lookup.MasterById.TryGetValue(repId, out var master)
                            ? master.WilayahName?.Trim() ?? string.Empty
                            : string.Empty,
                        ReportRoute = PiutangReportRoute
                    }));
            }

            foreach (var repId in highOverdueRepIds)
            {
                if (lowRecoveryRepIds.Contains(repId))
                    continue;

                var name = ResolveSalesPersonName(repId, nameById, lookup);
                salesmanOverdue.TryGetValue(repId, out var overdue);

                rows.Add((
                    AttentionSignalPriority(SignalHighOverdueWorkload),
                    1,
                    name,
                    overdue,
                    new DashboardCollectionAttentionRow
                    {
                        EntityType = EntityTypeSalesman,
                        EntityId = repId,
                        EntityCode = codeById.TryGetValue(repId, out var code) ? code : string.Empty,
                        EntityName = name,
                        SignalKey = SignalHighOverdueWorkload,
                        SignalLabel = "High Overdue Workload",
                        ValueAmount = overdue,
                        ValueText = null,
                        WilayahName = lookup.MasterById.TryGetValue(repId, out var master)
                            ? master.WilayahName?.Trim() ?? string.Empty
                            : string.Empty,
                        ReportRoute = PiutangReportRoute
                    }));
            }

            foreach (var wilayahKey in wilayahHotspotKeys)
            {
                wilayahOverdue.TryGetValue(wilayahKey, out var amount);
                var share = totalOverdue > 0 ? amount / totalOverdue * 100m : 0m;
                var name = ResolveWilayahName(wilayahKey, wilayahNames);

                rows.Add((
                    AttentionSignalPriority(SignalWilayahHotspot),
                    2,
                    name,
                    amount,
                    new DashboardCollectionAttentionRow
                    {
                        EntityType = EntityTypeWilayah,
                        EntityId = wilayahKey,
                        EntityCode = string.Empty,
                        EntityName = name,
                        SignalKey = SignalWilayahHotspot,
                        SignalLabel = "Wilayah Hotspot",
                        ValueAmount = amount,
                        ValueText = $"{share:N0}% of company overdue",
                        WilayahName = name,
                        ReportRoute = null
                    }));
            }

            return rows
                .OrderBy(r => r.SignalPriority)
                .ThenBy(r => r.EntityTypePriority)
                .ThenByDescending(r => r.ValueAmount)
                .ThenBy(r => r.EntityName, StringComparer.OrdinalIgnoreCase)
                .Select((r, index) =>
                {
                    r.Row.SortOrder = index + 1;
                    return r.Row;
                })
                .ToList();
        }

        private static void AddCustomerMetadata(
            string key,
            string customerCode,
            string customerName,
            Dictionary<string, string> customerCodes,
            Dictionary<string, string> customerNames)
        {
            if (!customerCodes.ContainsKey(key) && !string.IsNullOrWhiteSpace(customerCode))
                customerCodes[key] = customerCode.Trim();

            if (!customerNames.ContainsKey(key) && !string.IsNullOrWhiteSpace(customerName))
                customerNames[key] = customerName.Trim();
        }

        private static void AddAmount(Dictionary<string, decimal> map, string key, decimal amount)
        {
            if (!map.TryGetValue(key, out var current))
                current = 0m;

            map[key] = current + amount;
        }

        private static void AddBucketAmount(Dictionary<string, decimal> bucketTotals, string bucket, decimal amount)
        {
            if (!bucketTotals.TryGetValue(bucket, out var current))
                current = 0m;

            bucketTotals[bucket] = current + amount;
        }

        private static string ResolveCustomerName(string key, Dictionary<string, string> customerNames)
        {
            return customerNames.TryGetValue(key, out var name) ? name : key;
        }

        private static string ResolveSalesPersonName(
            string id,
            Dictionary<string, string> nameById,
            DashboardSalesmanKeyResolver.SalesmanLookup lookup)
        {
            if (nameById.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name))
                return name;

            if (lookup.MasterById.TryGetValue(id, out var master))
                return master.SalesPersonName?.Trim() ?? id;

            return id;
        }

        private static string ResolveWilayahName(string key, Dictionary<string, string> wilayahNames)
        {
            return wilayahNames.TryGetValue(key, out var name) ? name : key;
        }

        private static int CustomerSignalPriority(string signalKey)
        {
            return AttentionSignalPriority(signalKey);
        }

        private static int AttentionSignalPriority(string signalKey)
        {
            switch (signalKey)
            {
                case SignalLowRecoveryVsBilling:
                    return 0;
                case SignalWilayahHotspot:
                    return 1;
                case SignalChronicOverdue:
                    return 2;
                case SignalPlafondBreachOverdue:
                    return 3;
                case SignalLegacyDebt:
                    return 4;
                case SignalHighOverdueWorkload:
                    return 5;
                case SignalOverdue:
                    return 6;
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
