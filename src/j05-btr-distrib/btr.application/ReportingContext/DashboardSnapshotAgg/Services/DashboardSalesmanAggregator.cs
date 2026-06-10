using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.SalesPersonPrincipalTargetAgg;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardSalesmanAggregator
    {
        public const int TopSalesmanCount = 10;
        public const int DormantDaysThreshold = 90;
        public const decimal DefaultExposureTopPercent = 20m;

        public const string SignalBelowTarget = "BelowTarget";
        public const string SignalMissingTargetSetup = "MissingTargetSetup";
        public const string SignalHighOverdueExposure = "HighOverdueExposure";
        public const string SignalHighPiutangExposure = "HighPiutangExposure";
        public const string SignalCustomerConcentration = "CustomerConcentration";
        public const string SignalDormantCustomerPortfolio = "DormantCustomerPortfolio";

        private const string AgingOver90BucketKey = "DaysOver90";
        private const string SegmentTypeWilayah = "Wilayah";
        private const string SegmentTypeActivity = "Activity";
        private const string SegmentTypeSegment = "Segment";
        private const string UnknownSegment = "Unknown";

        public DashboardSalesmanAggregateResult Aggregate(
            IEnumerable<FakturView> fakturRows,
            IEnumerable<PiutangOpenBalanceWithSalesmanDto> piutangRows,
            IEnumerable<CustomerLastFakturWithSalesmanDto> lastFakturRows,
            IEnumerable<SalesPersonModel> salespeople,
            IReadOnlyDictionary<string, decimal?> targetsByRep,
            Periode periode,
            DateTime today,
            DateTime generatedAt,
            decimal exposureTopPercent = DefaultExposureTopPercent,
            IEnumerable<SalesPersonPrincipalTargetModel> principalTargets = null,
            IEnumerable<FakturPrincipalOmzetDto> principalOmzet = null)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var fakturList = (fakturRows ?? Enumerable.Empty<FakturView>()).ToList();
            var outstanding = (piutangRows ?? Enumerable.Empty<PiutangOpenBalanceWithSalesmanDto>())
                .Where(r => r.KurangBayar > 1)
                .ToList();
            var lastFakturList = (lastFakturRows ?? Enumerable.Empty<CustomerLastFakturWithSalesmanDto>()).ToList();
            var salesPersonList = (salespeople ?? Enumerable.Empty<SalesPersonModel>()).ToList();
            var targets = targetsByRep ?? new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

            var lookup = DashboardSalesmanKeyResolver.BuildLookup(salesPersonList);
            var periodStart = periode.Tgl1.Date;
            var totalTeamOmzet = fakturList.Sum(r => r.GrandTotal);
            var totalPiutang = outstanding.Sum(r => r.KurangBayar);

            var repStates = BuildRepStates(salesPersonList, lookup);
            ApplyFakturMetrics(fakturList, repStates, lookup);
            ApplyPiutangMetrics(outstanding, repStates, lookup, today);
            ApplyTargetsAndAchievement(repStates, targets);
            ApplyDormantMetrics(lastFakturList, fakturList, repStates, lookup, today);

            var activeSet = repStates.Values
                .Where(r => r.IsActive)
                .Select(r => r.SalesPersonId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var topOmzet = BuildTopOmzet(repStates.Values, totalTeamOmzet);
            var topAchievement = BuildTopAchievement(repStates.Values, totalTeamOmzet);
            var topPiutang = BuildTopPiutang(repStates.Values, totalPiutang);

            var topOmzetPercent = topOmzet.Count > 0 && totalTeamOmzet > 0
                ? topOmzet[0].CompletedOmzet / totalTeamOmzet * 100m
                : (decimal?)null;

            var topPiutangPercent = topPiutang.Count > 0 && totalPiutang > 0
                ? topPiutang[0].OutstandingBalance / totalPiutang * 100m
                : (decimal?)null;

            var highPiutangSet = ResolveTopPercentByMetric(
                repStates.Values,
                r => r.OpenBalance,
                exposureTopPercent);
            var highOverdueSet = ResolveTopPercentByMetric(
                repStates.Values,
                r => r.OverdueBalance,
                exposureTopPercent);

            var attentionList = BuildAttentionList(
                repStates.Values,
                totalPiutang,
                highPiutangSet,
                highOverdueSet);
            var segmentation = BuildSegmentation(salesPersonList, activeSet);

            var belowTargetCount = repStates.Values.Count(r =>
                r.HasTarget && IsBelowTargetBand(r.AchievementBand));
            var missingTargetSetupCount = repStates.Values.Count(r =>
                r.HasMonthActivity && !r.HasTarget);
            var highOverdueCount = highOverdueSet.Count;
            var highPiutangCount = highPiutangSet.Count;
            var concentrationCount = repStates.Values.Count(r =>
                r.OmzetAmount > 0 && r.TopCustomerPercent.HasValue);
            var dormantPortfolioCount = repStates.Values.Count(r => r.DormantCustomerCount > 0);

            var principalAchievement = BuildPrincipalAchievement(
                repStates,
                principalTargets,
                principalOmzet);

            var repHistory = BuildRepHistory(
                repStates.Values,
                periodStart.Year,
                periodStart.Month);

            return new DashboardSalesmanAggregateResult
            {
                PeriodYear = periodStart.Year,
                PeriodMonth = periodStart.Month,
                TotalTeamOmzet = totalTeamOmzet,
                TotalPiutang = totalPiutang,
                ActiveSalesmanCount = activeSet.Count,
                BelowTargetCount = belowTargetCount,
                MissingTargetSetupCount = missingTargetSetupCount,
                HighOverdueExposureCount = highOverdueCount,
                HighPiutangExposureCount = highPiutangCount,
                CustomerConcentrationCount = concentrationCount,
                DormantPortfolioCount = dormantPortfolioCount,
                TopOmzetSalesmanPercent = topOmzetPercent,
                TopPiutangSalesmanPercent = topPiutangPercent,
                GeneratedAt = generatedAt,
                TopOmzet = topOmzet,
                TopAchievement = topAchievement,
                TopPiutang = topPiutang,
                AttentionList = attentionList,
                Segmentation = segmentation,
                PrincipalAchievement = principalAchievement,
                RepHistory = repHistory
            };
        }

        private static HashSet<string> ResolveTopPercentByMetric(
            IEnumerable<RepState> reps,
            Func<RepState, decimal> metricSelector,
            decimal topPercent)
        {
            var eligible = reps
                .Select(r => new { r.SalesPersonId, Value = metricSelector(r) })
                .Where(x => x.Value > 0)
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.SalesPersonId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (eligible.Count == 0)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var take = Math.Max(1, (int)Math.Ceiling(eligible.Count * topPercent / 100m));
            return eligible.Take(take).Select(x => x.SalesPersonId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static List<DashboardSalesmanPrincipalAchievementRow> BuildPrincipalAchievement(
            Dictionary<string, RepState> repStates,
            IEnumerable<SalesPersonPrincipalTargetModel> principalTargets,
            IEnumerable<FakturPrincipalOmzetDto> principalOmzet)
        {
            var targetMap = new Dictionary<string, (string SalesPersonId, string SupplierId, decimal Target, string SupplierName)>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var row in principalTargets ?? Enumerable.Empty<SalesPersonPrincipalTargetModel>())
            {
                if (string.IsNullOrWhiteSpace(row.SalesPersonId) || string.IsNullOrWhiteSpace(row.SupplierId))
                    continue;

                var salesPersonId = row.SalesPersonId.Trim();
                var supplierId = row.SupplierId.Trim();
                targetMap[PrincipalKey(salesPersonId, supplierId)] =
                    (salesPersonId, supplierId, row.TargetAmount, row.SupplierName?.Trim() ?? string.Empty);
            }

            var omzetMap = new Dictionary<string, (string SalesPersonId, string SupplierId, decimal Omzet, string SupplierName)>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var row in principalOmzet ?? Enumerable.Empty<FakturPrincipalOmzetDto>())
            {
                if (string.IsNullOrWhiteSpace(row.SalesPersonId) || string.IsNullOrWhiteSpace(row.SupplierId))
                    continue;

                var salesPersonId = row.SalesPersonId.Trim();
                var supplierId = row.SupplierId.Trim();
                omzetMap[PrincipalKey(salesPersonId, supplierId)] =
                    (salesPersonId, supplierId, row.CompletedOmzet, row.SupplierName?.Trim() ?? string.Empty);
            }

            var keys = targetMap.Keys
                .Union(omzetMap.Keys, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var rows = new List<DashboardSalesmanPrincipalAchievementRow>();

            foreach (var key in keys)
            {
                targetMap.TryGetValue(key, out var targetEntry);
                omzetMap.TryGetValue(key, out var omzetEntry);

                var salesPersonId = targetEntry.SalesPersonId ?? omzetEntry.SalesPersonId;
                var supplierId = targetEntry.SupplierId ?? omzetEntry.SupplierId;
                var targetAmount = targetEntry.Target > 0 ? (decimal?)targetEntry.Target : null;
                var completedOmzet = omzetEntry.Omzet;

                if (!targetAmount.HasValue && completedOmzet <= 0)
                    continue;

                repStates.TryGetValue(salesPersonId, out var rep);

                var supplierName = !string.IsNullOrWhiteSpace(omzetEntry.SupplierName)
                    ? omzetEntry.SupplierName
                    : targetEntry.SupplierName;

                var achievementPercent = SalesOmzetChartAchievementPolicy.ComputePercent(
                    completedOmzet,
                    targetAmount);

                rows.Add(new DashboardSalesmanPrincipalAchievementRow
                {
                    SalesPersonId = salesPersonId,
                    SalesPersonCode = rep?.SalesPersonCode ?? string.Empty,
                    SalesPersonName = rep?.SalesPersonName ?? string.Empty,
                    SupplierId = supplierId,
                    SupplierName = supplierName ?? string.Empty,
                    TargetAmount = targetAmount,
                    CompletedOmzet = completedOmzet,
                    AchievementPercent = achievementPercent
                });
            }

            return rows
                .OrderByDescending(r => r.AchievementPercent ?? -1m)
                .ThenByDescending(r => r.CompletedOmzet)
                .ThenBy(r => r.SupplierName, StringComparer.OrdinalIgnoreCase)
                .Select((r, index) =>
                {
                    r.SortOrder = index + 1;
                    return r;
                })
                .ToList();
        }

        private static List<DashboardSalesmanRepHistoryRow> BuildRepHistory(
            IEnumerable<RepState> reps,
            int periodYear,
            int periodMonth)
        {
            return reps
                .Select(r => new DashboardSalesmanRepHistoryRow
                {
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth,
                    SalesPersonId = r.SalesPersonId,
                    SalesPersonCode = r.SalesPersonCode,
                    SalesPersonName = r.SalesPersonName,
                    TargetAmount = r.TargetAmount,
                    CompletedOmzet = r.OmzetAmount,
                    AchievementPercent = r.AchievementPercent,
                    AchievementBand = r.AchievementBand,
                    OpenBalance = r.OpenBalance,
                    IsActive = r.IsActive
                })
                .ToList();
        }

        private static Dictionary<string, RepState> BuildRepStates(
            List<SalesPersonModel> salespeople,
            DashboardSalesmanKeyResolver.SalesmanLookup lookup)
        {
            var states = new Dictionary<string, RepState>(StringComparer.OrdinalIgnoreCase);

            foreach (var person in salespeople)
            {
                if (string.IsNullOrWhiteSpace(person.SalesPersonId))
                    continue;

                var id = person.SalesPersonId.Trim();
                states[id] = CreateRepState(id, person);
            }

            foreach (var id in lookup.MasterById.Keys)
            {
                if (!states.ContainsKey(id))
                    states[id] = CreateRepState(id, lookup.MasterById[id]);
            }

            return states;
        }

        private static RepState CreateRepState(string id, SalesPersonModel person)
        {
            return new RepState
            {
                SalesPersonId = id,
                SalesPersonCode = person?.SalesPersonCode?.Trim() ?? string.Empty,
                SalesPersonName = person?.SalesPersonName?.Trim() ?? string.Empty,
                WilayahName = person?.WilayahName?.Trim() ?? string.Empty,
                SegmentName = person?.SegmentName?.Trim() ?? string.Empty
            };
        }

        private static RepState GetOrCreateRep(
            Dictionary<string, RepState> states,
            string salesPersonId,
            string salesPersonName,
            DashboardSalesmanKeyResolver.SalesmanLookup lookup)
        {
            var id = DashboardSalesmanKeyResolver.ResolveId(salesPersonId, salesPersonName, lookup);
            if (id.Length == 0)
                return null;

            if (!states.TryGetValue(id, out var state))
            {
                lookup.MasterById.TryGetValue(id, out var master);
                state = CreateRepState(id, master);
                if (string.IsNullOrWhiteSpace(state.SalesPersonName) && !string.IsNullOrWhiteSpace(salesPersonName))
                    state.SalesPersonName = salesPersonName.Trim();
                states[id] = state;
            }
            else if (string.IsNullOrWhiteSpace(state.SalesPersonName) && !string.IsNullOrWhiteSpace(salesPersonName))
            {
                state.SalesPersonName = salesPersonName.Trim();
            }

            return state;
        }

        private static void ApplyFakturMetrics(
            List<FakturView> fakturList,
            Dictionary<string, RepState> states,
            DashboardSalesmanKeyResolver.SalesmanLookup lookup)
        {
            foreach (var row in fakturList)
            {
                var rep = GetOrCreateRep(states, row.SalesPersonId, row.SalesPersonName, lookup);
                if (rep is null)
                    continue;

                rep.IsActive = true;
                rep.OmzetAmount += row.GrandTotal;

                var customerKey = !string.IsNullOrWhiteSpace(row.CustomerCode)
                    ? row.CustomerCode.Trim()
                    : row.Customer?.Trim() ?? string.Empty;

                if (customerKey.Length > 0)
                    rep.CustomerKeys.Add(customerKey);

                if (!string.IsNullOrWhiteSpace(row.SalesPersonCode))
                    rep.SalesPersonCode = row.SalesPersonCode.Trim();

                var customerOmzetKey = customerKey.Length > 0 ? customerKey : row.Customer?.Trim() ?? string.Empty;
                if (customerOmzetKey.Length == 0)
                    continue;

                if (!rep.CustomerOmzet.TryGetValue(customerOmzetKey, out var current))
                    current = 0m;

                rep.CustomerOmzet[customerOmzetKey] = current + row.GrandTotal;
            }

            foreach (var rep in states.Values)
            {
                rep.CustomerCount = rep.CustomerKeys.Count;
                if (rep.CustomerOmzet.Count == 0 || rep.OmzetAmount <= 0)
                    continue;

                var topCustomerOmzet = rep.CustomerOmzet.Values.Max();
                rep.TopCustomerPercent = topCustomerOmzet / rep.OmzetAmount * 100m;
            }
        }

        private static void ApplyPiutangMetrics(
            List<PiutangOpenBalanceWithSalesmanDto> outstanding,
            Dictionary<string, RepState> states,
            DashboardSalesmanKeyResolver.SalesmanLookup lookup,
            DateTime today)
        {
            foreach (var row in outstanding)
            {
                var rep = GetOrCreateRep(states, row.SalesPersonId, row.SalesPersonName, lookup);
                if (rep is null)
                    continue;

                rep.OpenBalance += row.KurangBayar;

                if (ResolveAgingBucketKey(row.JatuhTempo, today) != "Current")
                {
                    rep.OverdueBalance += row.KurangBayar;

                    var customerKey = !string.IsNullOrWhiteSpace(row.CustomerCode)
                        ? row.CustomerCode.Trim()
                        : row.CustomerName?.Trim() ?? string.Empty;

                    if (customerKey.Length > 0)
                        rep.OverdueCustomerKeys.Add(customerKey);
                }
            }

            foreach (var rep in states.Values)
                rep.OverdueCustomerCount = rep.OverdueCustomerKeys.Count;
        }

        private static void ApplyTargetsAndAchievement(
            Dictionary<string, RepState> states,
            IReadOnlyDictionary<string, decimal?> targets)
        {
            foreach (var pair in states)
            {
                var rep = pair.Value;
                if (targets.TryGetValue(pair.Key, out var target) && target.HasValue && target.Value > 0)
                {
                    rep.TargetAmount = target;
                    rep.HasTarget = true;
                }

                rep.AchievementPercent = SalesOmzetChartAchievementPolicy.ComputePercent(
                    rep.OmzetAmount,
                    rep.TargetAmount);

                rep.AchievementBand = ExecutiveSalesAchievementBandResolver.Resolve(rep.AchievementPercent);
                rep.HasMonthActivity = rep.OmzetAmount > 0 || rep.CustomerCount > 0;
            }
        }

        private static void ApplyDormantMetrics(
            List<CustomerLastFakturWithSalesmanDto> lastFakturList,
            List<FakturView> fakturList,
            Dictionary<string, RepState> states,
            DashboardSalesmanKeyResolver.SalesmanLookup lookup,
            DateTime today)
        {
            var activeCustomers = fakturList
                .Select(r => !string.IsNullOrWhiteSpace(r.CustomerCode)
                    ? r.CustomerCode.Trim()
                    : r.Customer?.Trim() ?? string.Empty)
                .Where(k => k.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var cutoff = today.Date.AddDays(-DormantDaysThreshold);

            foreach (var row in lastFakturList)
            {
                var customerKey = !string.IsNullOrWhiteSpace(row.CustomerCode)
                    ? row.CustomerCode.Trim()
                    : row.CustomerName?.Trim() ?? string.Empty;

                if (customerKey.Length == 0)
                    continue;

                if (activeCustomers.Contains(customerKey))
                    continue;

                if (row.LastFakturDate.Date > cutoff)
                    continue;

                var rep = GetOrCreateRep(states, row.SalesPersonId, row.SalesPersonName, lookup);
                if (rep is null)
                    continue;

                rep.DormantCustomerCount++;
            }
        }

        private static List<DashboardSalesmanTopOmzetRow> BuildTopOmzet(
            IEnumerable<RepState> reps,
            decimal totalTeamOmzet)
        {
            return reps
                .Where(r => r.OmzetAmount > 0)
                .OrderByDescending(r => r.OmzetAmount)
                .ThenBy(r => r.SalesPersonName, StringComparer.OrdinalIgnoreCase)
                .Take(TopSalesmanCount)
                .Select((r, index) => new DashboardSalesmanTopOmzetRow
                {
                    Rank = index + 1,
                    SalesPersonId = r.SalesPersonId,
                    SalesPersonCode = r.SalesPersonCode,
                    SalesPersonName = r.SalesPersonName,
                    CompletedOmzet = r.OmzetAmount,
                    PercentOfTotal = totalTeamOmzet > 0
                        ? r.OmzetAmount / totalTeamOmzet * 100m
                        : (decimal?)null,
                    IsActive = r.IsActive
                })
                .ToList();
        }

        private static List<DashboardSalesmanTopAchievementRow> BuildTopAchievement(
            IEnumerable<RepState> reps,
            decimal totalTeamOmzet)
        {
            return reps
                .Where(r => r.HasTarget && r.AchievementPercent.HasValue && r.OmzetAmount > 0)
                .OrderByDescending(r => r.AchievementPercent)
                .ThenByDescending(r => r.OmzetAmount)
                .ThenBy(r => r.SalesPersonName, StringComparer.OrdinalIgnoreCase)
                .Take(TopSalesmanCount)
                .Select((r, index) => new DashboardSalesmanTopAchievementRow
                {
                    Rank = index + 1,
                    SalesPersonId = r.SalesPersonId,
                    SalesPersonCode = r.SalesPersonCode,
                    SalesPersonName = r.SalesPersonName,
                    TargetAmount = r.TargetAmount,
                    CompletedOmzet = r.OmzetAmount,
                    AchievementPercent = r.AchievementPercent,
                    PercentOfTotal = totalTeamOmzet > 0
                        ? r.OmzetAmount / totalTeamOmzet * 100m
                        : (decimal?)null,
                    IsActive = r.IsActive
                })
                .ToList();
        }

        private static List<DashboardSalesmanTopPiutangRow> BuildTopPiutang(
            IEnumerable<RepState> reps,
            decimal totalPiutang)
        {
            return reps
                .Where(r => r.OpenBalance > 0)
                .OrderByDescending(r => r.OpenBalance)
                .ThenBy(r => r.SalesPersonName, StringComparer.OrdinalIgnoreCase)
                .Take(TopSalesmanCount)
                .Select((r, index) => new DashboardSalesmanTopPiutangRow
                {
                    Rank = index + 1,
                    SalesPersonId = r.SalesPersonId,
                    SalesPersonCode = r.SalesPersonCode,
                    SalesPersonName = r.SalesPersonName,
                    OutstandingBalance = r.OpenBalance,
                    PercentOfTotal = totalPiutang > 0
                        ? r.OpenBalance / totalPiutang * 100m
                        : (decimal?)null,
                    IsActive = r.IsActive
                })
                .ToList();
        }

        private static List<DashboardSalesmanAttentionRow> BuildAttentionList(
            IEnumerable<RepState> reps,
            decimal totalPiutang,
            HashSet<string> highPiutangSet,
            HashSet<string> highOverdueSet)
        {
            var rows = new List<(int Priority, string Name, DashboardSalesmanAttentionRow Row)>();

            foreach (var rep in reps)
            {
                if (rep.HasTarget && IsBelowTargetBand(rep.AchievementBand))
                {
                    rows.Add((SignalPriority(SignalBelowTarget), rep.SalesPersonName, CreateAttentionRow(
                        rep,
                        SignalBelowTarget,
                        "Below Target",
                        rep.OmzetAmount,
                        $"{rep.AchievementPercent:N1}% (Target Rp {rep.TargetAmount:N0})")));
                }

                if (rep.HasMonthActivity && !rep.HasTarget)
                {
                    rows.Add((SignalPriority(SignalMissingTargetSetup), rep.SalesPersonName, CreateAttentionRow(
                        rep,
                        SignalMissingTargetSetup,
                        "Missing Target Setup",
                        rep.OmzetAmount,
                        "No target configured")));
                }

                if (highOverdueSet.Contains(rep.SalesPersonId))
                {
                    rows.Add((SignalPriority(SignalHighOverdueExposure), rep.SalesPersonName, CreateAttentionRow(
                        rep,
                        SignalHighOverdueExposure,
                        "High Overdue Exposure",
                        rep.OverdueBalance,
                        $"{rep.OverdueCustomerCount} overdue customers, Rp {rep.OverdueBalance:N0} overdue")));
                }

                if (highPiutangSet.Contains(rep.SalesPersonId))
                {
                    var share = totalPiutang > 0 ? rep.OpenBalance / totalPiutang * 100m : 0m;
                    rows.Add((SignalPriority(SignalHighPiutangExposure), rep.SalesPersonName, CreateAttentionRow(
                        rep,
                        SignalHighPiutangExposure,
                        "High Piutang Exposure",
                        rep.OpenBalance,
                        $"Rp {rep.OpenBalance:N0} open piutang ({share:N1}% of company)")));
                }

                if (rep.OmzetAmount > 0 && rep.TopCustomerPercent.HasValue)
                {
                    rows.Add((SignalPriority(SignalCustomerConcentration), rep.SalesPersonName, CreateAttentionRow(
                        rep,
                        SignalCustomerConcentration,
                        "Customer Concentration",
                        rep.CustomerOmzet.Values.DefaultIfEmpty(0m).Max(),
                        $"{rep.TopCustomerPercent:N1}% top customer")));
                }

                if (rep.DormantCustomerCount > 0)
                {
                    rows.Add((SignalPriority(SignalDormantCustomerPortfolio), rep.SalesPersonName, CreateAttentionRow(
                        rep,
                        SignalDormantCustomerPortfolio,
                        "Dormant Customer Portfolio",
                        null,
                        $"{rep.DormantCustomerCount} dormant customers on book")));
                }
            }

            return rows
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .Select((r, index) =>
                {
                    r.Row.SortOrder = index + 1;
                    return r.Row;
                })
                .ToList();
        }

        private static DashboardSalesmanAttentionRow CreateAttentionRow(
            RepState rep,
            string signalKey,
            string signalLabel,
            decimal? valueAmount,
            string valueText)
        {
            return new DashboardSalesmanAttentionRow
            {
                SalesPersonId = rep.SalesPersonId,
                SalesPersonCode = rep.SalesPersonCode,
                SalesPersonName = rep.SalesPersonName,
                SignalKey = signalKey,
                SignalLabel = signalLabel,
                ValueAmount = valueAmount,
                ValueText = valueText,
                WilayahName = rep.WilayahName,
                IsActive = rep.IsActive
            };
        }

        private static List<DashboardSalesmanSegmentationRow> BuildSegmentation(
            List<SalesPersonModel> salespeople,
            HashSet<string> activeSet)
        {
            var wilayahGroups = new Dictionary<string, (int Total, int Active, int Inactive)>(
                StringComparer.OrdinalIgnoreCase);
            var segmentGroups = new Dictionary<string, (int Total, int Active, int Inactive)>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var person in salespeople)
            {
                if (string.IsNullOrWhiteSpace(person.SalesPersonId))
                    continue;

                var id = person.SalesPersonId.Trim();
                var isActive = activeSet.Contains(id);
                var wilayah = NormalizeSegmentLabel(person.WilayahName);
                var segment = NormalizeSegmentLabel(person.SegmentName);

                AddSegmentCount(wilayahGroups, wilayah, isActive);
                if (!string.Equals(segment, UnknownSegment, StringComparison.OrdinalIgnoreCase))
                    AddSegmentCount(segmentGroups, segment, isActive);
            }

            var rows = new List<DashboardSalesmanSegmentationRow>();
            rows.AddRange(ToSegmentRows(wilayahGroups, SegmentTypeWilayah));

            if (segmentGroups.Count > 0)
                rows.AddRange(ToSegmentRows(segmentGroups, SegmentTypeSegment));

            var activeCount = activeSet.Count;
            var inactiveCount = Math.Max(0, salespeople.Count(p => !string.IsNullOrWhiteSpace(p.SalesPersonId)) - activeCount);

            rows.Add(new DashboardSalesmanSegmentationRow
            {
                SegmentType = SegmentTypeActivity,
                SegmentKey = "Active",
                SegmentLabel = "Active",
                SalesmanCount = activeCount,
                ActiveCount = activeCount,
                InactiveCount = 0,
                SortOrder = 1
            });

            rows.Add(new DashboardSalesmanSegmentationRow
            {
                SegmentType = SegmentTypeActivity,
                SegmentKey = "Inactive",
                SegmentLabel = "Inactive",
                SalesmanCount = inactiveCount,
                ActiveCount = 0,
                InactiveCount = inactiveCount,
                SortOrder = 2
            });

            return rows;
        }

        private static void AddSegmentCount(
            Dictionary<string, (int Total, int Active, int Inactive)> groups,
            string segmentLabel,
            bool isActive)
        {
            if (!groups.TryGetValue(segmentLabel, out var counts))
                counts = (0, 0, 0);

            counts.Total++;
            if (isActive)
                counts.Active++;
            else
                counts.Inactive++;

            groups[segmentLabel] = counts;
        }

        private static IEnumerable<DashboardSalesmanSegmentationRow> ToSegmentRows(
            Dictionary<string, (int Total, int Active, int Inactive)> groups,
            string segmentType)
        {
            return groups
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select((g, index) => new DashboardSalesmanSegmentationRow
                {
                    SegmentType = segmentType,
                    SegmentKey = g.Key,
                    SegmentLabel = g.Key,
                    SalesmanCount = g.Value.Total,
                    ActiveCount = g.Value.Active,
                    InactiveCount = g.Value.Inactive,
                    SortOrder = index + 1
                });
        }

        private static string NormalizeSegmentLabel(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? UnknownSegment : value.Trim();
        }

        private static bool IsBelowTargetBand(string band)
        {
            return string.Equals(band, ExecutiveSalesAchievementBandResolver.Warning, StringComparison.OrdinalIgnoreCase)
                || string.Equals(band, ExecutiveSalesAchievementBandResolver.Critical, StringComparison.OrdinalIgnoreCase);
        }

        private static int SignalPriority(string signalKey)
        {
            switch (signalKey)
            {
                case SignalBelowTarget:
                    return 0;
                case SignalMissingTargetSetup:
                    return 1;
                case SignalHighOverdueExposure:
                    return 2;
                case SignalHighPiutangExposure:
                    return 3;
                case SignalCustomerConcentration:
                    return 4;
                case SignalDormantCustomerPortfolio:
                    return 5;
                default:
                    return 99;
            }
        }

        private static string PrincipalKey(string salesPersonId, string supplierId)
        {
            return $"{salesPersonId}|{supplierId}";
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

        private sealed class RepState
        {
            public string SalesPersonId { get; set; }

            public string SalesPersonCode { get; set; }

            public string SalesPersonName { get; set; }

            public string WilayahName { get; set; }

            public string SegmentName { get; set; }

            public bool IsActive { get; set; }

            public decimal OmzetAmount { get; set; }

            public int CustomerCount { get; set; }

            public HashSet<string> CustomerKeys { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, decimal> CustomerOmzet { get; } =
                new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            public decimal? TopCustomerPercent { get; set; }

            public decimal? TargetAmount { get; set; }

            public bool HasTarget { get; set; }

            public decimal? AchievementPercent { get; set; }

            public string AchievementBand { get; set; }

            public bool HasMonthActivity { get; set; }

            public decimal OpenBalance { get; set; }

            public decimal OverdueBalance { get; set; }

            public HashSet<string> OverdueCustomerKeys { get; } =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public int OverdueCustomerCount { get; set; }

            public int DormantCustomerCount { get; set; }
        }
    }
}
