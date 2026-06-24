using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.CustomerAgg;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardCustomerPortfolioAggregator
    {
        public const string ConcentrationOmzet = "Omzet";
        public const string ConcentrationPiutang = "Piutang";

        public DashboardCustomerPortfolioAggregateResult Aggregate(
            DashboardCustomerAggregateResult customerAggregate,
            IReadOnlyList<CustomerRiskForecastContext> forecastContexts,
            DashboardCustomerRiskForecastAggregateResult forecastAggregate,
            IReadOnlyDictionary<string, CollectionOptimizationContext> optimizationByKey,
            DashboardSalesmanAggregateResult salesmanSnapshot,
            IEnumerable<CustomerModel> customers,
            IEnumerable<CustomerLastFakturWithSalesmanDto> lastFakturWithSalesman,
            IEnumerable<CustomerFirstFakturDto> firstFakturRows,
            IEnumerable<CustomerPurchaseFrequencyDto> frequencyRows,
            IEnumerable<FakturView> currentMonthFakturRows,
            IEnumerable<PiutangOpenBalanceDto> piutangRows,
            DateTime businessDate,
            DateTime generatedAt,
            CustomerPortfolioOptions options)
        {
            options = options ?? CustomerPortfolioOptions.FromDashboardOptions(null);
            var today = businessDate.Date;
            var customerList = (customers ?? Enumerable.Empty<CustomerModel>()).ToList();
            var fakturList = (currentMonthFakturRows ?? Enumerable.Empty<FakturView>()).ToList();
            var outstanding = (piutangRows ?? Enumerable.Empty<PiutangOpenBalanceDto>())
                .Where(r => r.KurangBayar > 1)
                .ToList();

            var forecastByKey = (forecastContexts ?? Array.Empty<CustomerRiskForecastContext>())
                .Where(c => c != null && !string.IsNullOrWhiteSpace(c.CustomerKey))
                .GroupBy(c => c.CustomerKey, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var optimizationLookup = optimizationByKey ??
                new Dictionary<string, CollectionOptimizationContext>(StringComparer.OrdinalIgnoreCase);

            var m17AttentionKeys = BuildM17AttentionKeys(customerAggregate);
            var plafondBreachKeys = BuildPlafondBreachKeys(customerAggregate);
            var activeSet = BuildActiveSet(fakturList);
            var dormantSet = BuildDormantSet(
                (lastFakturWithSalesman ?? Enumerable.Empty<CustomerLastFakturWithSalesmanDto>()).ToList(),
                activeSet,
                today);

            var firstFakturByKey = BuildFirstFakturLookup(firstFakturRows);
            var lastFakturByKey = BuildLastFakturLookup(lastFakturWithSalesman);
            var frequencyByKey = BuildFrequencyLookup(frequencyRows);
            var omzetByKey = BuildOmzetByKey(fakturList);
            var openBalanceByKey = BuildOpenBalanceByKey(outstanding);
            var overdueBalanceByKey = BuildOverdueBalanceByKey(outstanding, today);

            var omzetRankByKey = BuildRankMap(omzetByKey);
            var piutangRankByKey = BuildRankMap(openBalanceByKey);

            var salesmanAchievement = BuildSalesmanAchievementLookup(salesmanSnapshot);
            var highPiutangSalesmen = BuildHighPiutangSalesmen(salesmanSnapshot);

            var universe = BuildCustomerUniverse(customerList);
            var processed = new List<PortfolioCustomerContext>();

            foreach (var entry in universe)
            {
                var key = entry.Key;
                forecastByKey.TryGetValue(key, out var forecast);
                optimizationLookup.TryGetValue(key, out var optimization);

                DateTime? firstPurchaseDate = null;
                if (firstFakturByKey.TryGetValue(key, out var firstPurchaseDateValue))
                    firstPurchaseDate = firstPurchaseDateValue;

                lastFakturByKey.TryGetValue(key, out var lastFaktur);
                frequencyByKey.TryGetValue(key, out var fakturCount6Mo);

                var hasPurchaseHistory = firstPurchaseDate.HasValue || lastFaktur != null;
                var mtdOmzet = omzetByKey.TryGetValue(key, out var omzet) ? omzet : forecast?.MtdOmzet ?? 0m;
                var openBalance = openBalanceByKey.TryGetValue(key, out var balance) ? balance : forecast?.OpenBalance ?? 0m;
                var overdueBalance = overdueBalanceByKey.TryGetValue(key, out var overdue)
                    ? (decimal?)overdue
                    : forecast?.OverdueBalance;
                var priorMonthOmzet = forecast?.PriorMonthOmzet ?? 0m;
                var isActiveMtd = activeSet.Contains(key);
                var isDormant = dormantSet.Contains(key);
                var salesPersonName = lastFaktur?.SalesPersonName?.Trim()
                    ?? forecast?.SalesPersonName?.Trim()
                    ?? string.Empty;

                var lifecycleStage = CustomerPortfolioLifecycleResolver.Resolve(
                    hasPurchaseHistory,
                    firstPurchaseDate,
                    lastFaktur?.LastFakturDate,
                    forecast?.DaysSinceLastFaktur,
                    isActiveMtd,
                    isDormant,
                    forecast,
                    mtdOmzet,
                    priorMonthOmzet,
                    today,
                    options);

                var portfolioTier = CustomerPortfolioTierResolver.Resolve(
                    omzetRankByKey.TryGetValue(key, out var omzetRank) ? omzetRank : (int?)null,
                    piutangRankByKey.TryGetValue(key, out var piutangRank) ? piutangRank : (int?)null,
                    mtdOmzet,
                    openBalance,
                    fakturCount6Mo,
                    forecast?.Category ?? CustomerRiskForecastPolicy.CategoryHealthy,
                    options);

                var m29RecommendationKey = forecast != null
                    ? CustomerRiskRecommendationBuilder.Build(forecast).RecommendationKey
                    : string.Empty;

                var action = CustomerPortfolioActionBuilder.ResolvePrimaryAction(
                    lifecycleStage,
                    portfolioTier,
                    forecast?.Category ?? CustomerRiskForecastPolicy.CategoryHealthy,
                    forecast,
                    optimization,
                    plafondBreachKeys.Contains(key),
                    m29RecommendationKey);

                var m29Category = forecast?.Category ?? CustomerRiskForecastPolicy.CategoryHealthy;
                var m29PrimarySignal = forecast?.Signals?
                    .OrderByDescending(s => SeverityRank(s.Severity))
                    .FirstOrDefault();

                var ctx = new PortfolioCustomerContext
                {
                    CustomerKey = key,
                    CustomerCode = entry.CustomerCode,
                    CustomerName = entry.CustomerName,
                    WilayahName = entry.WilayahName,
                    Klasifikasi = entry.Klasifikasi,
                    LifecycleStage = lifecycleStage,
                    PortfolioTier = portfolioTier,
                    PrimaryActionKey = action.PrimaryActionKey,
                    ActionOwner = action.ActionOwner,
                    ActionReasonText = action.ActionReasonText,
                    TriggeredRuleIds = action.TriggeredRuleIds,
                    MtdOmzet = mtdOmzet,
                    OpenBalance = openBalance,
                    OverdueBalance = overdueBalance,
                    LastPurchaseDate = lastFaktur?.LastFakturDate,
                    FirstPurchaseDate = firstPurchaseDate,
                    FakturCount6Mo = fakturCount6Mo,
                    IsActiveMtd = isActiveMtd,
                    M29Category = m29Category,
                    M29PrimarySignalKey = m29PrimarySignal?.SignalKey ?? string.Empty,
                    SalesPersonName = salesPersonName,
                    SalesmanAchievementPercent = ResolveSalesmanAchievement(salesPersonName, salesmanAchievement),
                    SalesmanHighPiutangExposure = highPiutangSalesmen.Contains(NormalizeKey(salesPersonName)),
                    M30LinkRoute = action.M30LinkRoute ?? string.Empty,
                    CustomerReportRoute = CustomerPortfolioOptimizationPolicy.BuildCustomerReportRoute(entry.CustomerCode),
                    ValueDisclaimer = CustomerPortfolioOptimizationPolicy.ValueDisclaimerText
                };

                ctx.IsAttention = CustomerPortfolioOptimizationPolicy.QualifiesForAttention(ctx, m17AttentionKeys);
                ctx.PortfolioPriorityScore = CustomerPortfolioOptimizationPolicy.ComputePortfolioPriorityScore(ctx);

                processed.Add(ctx);
            }

            var forecastKpi = forecastAggregate?.Kpi ?? new DashboardCustomerRiskForecastKpiSnapshot();
            var totalCustomers = processed.Count;
            var attentionCustomers = processed.Where(c => c.IsAttention).ToList();
            var healthyPercent = totalCustomers > 0
                ? Math.Round((decimal)forecastKpi.HealthyCount / totalCustomers * 100m, 1)
                : 0m;

            var kpi = new DashboardCustomerPortfolioKpiSnapshot
            {
                PortfolioHealthScore = forecastKpi.PortfolioHealthScore,
                PortfolioHealthyPercent = healthyPercent,
                TotalCustomerCount = totalCustomers,
                AttentionCustomerCount = attentionCustomers.Count,
                StrategicCustomerCount = processed.Count(c =>
                    string.Equals(c.PortfolioTier, CustomerPortfolioOptimizationPolicy.TierStrategic, StringComparison.OrdinalIgnoreCase)),
                StrategicAtRiskCount = processed.Count(c =>
                    string.Equals(c.PortfolioTier, CustomerPortfolioOptimizationPolicy.TierStrategic, StringComparison.OrdinalIgnoreCase) &&
                    CustomerPortfolioOptimizationPolicy.IsAtOrAboveCategory(c.M29Category, CustomerRiskForecastPolicy.CategoryWatch)),
                CustomersAtRiskCount = processed.Count(c =>
                    CustomerPortfolioOptimizationPolicy.CompareCategory(c.M29Category, CustomerRiskForecastPolicy.CategoryHealthy) > 0),
                WorkingCapitalTiedAmount = attentionCustomers.Sum(c => c.OpenBalance),
                TotalMtdOmzet = processed.Sum(c => c.MtdOmzet),
                TotalOpenBalance = processed.Sum(c => c.OpenBalance),
                NeverPurchasedCount = processed.Count(c =>
                    string.Equals(c.LifecycleStage, CustomerPortfolioOptimizationPolicy.LifecycleNeverPurchased, StringComparison.OrdinalIgnoreCase)),
                DormantCount = processed.Count(c =>
                    string.Equals(c.LifecycleStage, CustomerPortfolioOptimizationPolicy.LifecycleDormant, StringComparison.OrdinalIgnoreCase)),
                DecliningCount = processed.Count(c =>
                    string.Equals(c.LifecycleStage, CustomerPortfolioOptimizationPolicy.LifecycleDeclining, StringComparison.OrdinalIgnoreCase)),
                ValueDisclaimerText = CustomerPortfolioOptimizationPolicy.ValueDisclaimerText
            };

            var priorityQueue = attentionCustomers
                .OrderByDescending(c => c.PortfolioPriorityScore)
                .ThenByDescending(c => c.OpenBalance)
                .ThenByDescending(c => c.MtdOmzet)
                .ThenBy(c => c.CustomerName, StringComparer.OrdinalIgnoreCase)
                .Take(options.MaxPriorityRows)
                .Select((c, i) => ToPriorityRow(c, i + 1))
                .ToList();

            kpi.ExecutiveSummaryText = CustomerPortfolioExecutiveSummaryBuilder.Build(kpi, priorityQueue.FirstOrDefault(), today);

            var customerRows = processed
                .OrderByDescending(c => c.IsAttention)
                .ThenByDescending(c => c.PortfolioPriorityScore)
                .ThenByDescending(c => c.OpenBalance)
                .ThenByDescending(c => c.MtdOmzet)
                .ThenBy(c => c.CustomerName, StringComparer.OrdinalIgnoreCase)
                .Select((c, i) => ToCustomerRow(c, i + 1))
                .ToList();

            return new DashboardCustomerPortfolioAggregateResult
            {
                GeneratedAt = generatedAt,
                BusinessDate = today,
                Kpi = kpi,
                LifecycleDistribution = BuildDistribution(
                    processed,
                    c => c.LifecycleStage,
                    CustomerPortfolioOptimizationPolicy.ResolveLifecycleLabel),
                TierDistribution = BuildDistribution(
                    processed,
                    c => c.PortfolioTier,
                    CustomerPortfolioOptimizationPolicy.ResolveTierLabel),
                ActionDistribution = BuildDistribution(
                    processed,
                    c => c.PrimaryActionKey,
                    CustomerPortfolioOptimizationPolicy.ResolveActionLabel),
                PriorityQueue = priorityQueue,
                Customers = customerRows,
                Concentration = BuildConcentration(customerAggregate),
                WilayahBreakdown = BuildWilayahBreakdown(processed, options.MaxWilayahRows)
            };
        }

        private static List<DashboardCustomerPortfolioConcentrationRow> BuildConcentration(
            DashboardCustomerAggregateResult customerAggregate)
        {
            var rows = new List<DashboardCustomerPortfolioConcentrationRow>();

            foreach (var row in customerAggregate?.TopOmzet ?? new List<DashboardCustomerTopOmzetRow>())
            {
                rows.Add(new DashboardCustomerPortfolioConcentrationRow
                {
                    ConcentrationType = ConcentrationOmzet,
                    SortOrder = row.Rank,
                    Rank = row.Rank,
                    CustomerCode = row.CustomerCode ?? string.Empty,
                    CustomerName = row.CustomerName ?? string.Empty,
                    Amount = row.OmzetAmount,
                    PercentOfTotal = row.PercentOfTotal
                });
            }

            foreach (var row in customerAggregate?.TopPiutang ?? new List<DashboardCustomerTopPiutangRow>())
            {
                rows.Add(new DashboardCustomerPortfolioConcentrationRow
                {
                    ConcentrationType = ConcentrationPiutang,
                    SortOrder = row.Rank,
                    Rank = row.Rank,
                    CustomerCode = row.CustomerCode ?? string.Empty,
                    CustomerName = row.CustomerName ?? string.Empty,
                    Amount = row.OutstandingBalance,
                    PercentOfTotal = row.PercentOfTotal
                });
            }

            return rows;
        }

        private static List<DashboardCustomerPortfolioWilayahRow> BuildWilayahBreakdown(
            List<PortfolioCustomerContext> processed,
            int maxRows)
        {
            return processed
                .GroupBy(c => NormalizeKey(string.IsNullOrWhiteSpace(c.WilayahName) ? "Unknown" : c.WilayahName.Trim()),
                    StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    WilayahName = g.First().WilayahName?.Trim() ?? "Unknown",
                    CustomerCount = g.Count(),
                    AttentionCustomerCount = g.Count(c => c.IsAttention)
                })
                .OrderByDescending(x => x.CustomerCount)
                .ThenBy(x => x.WilayahName, StringComparer.OrdinalIgnoreCase)
                .Take(maxRows)
                .Select((x, i) => new DashboardCustomerPortfolioWilayahRow
                {
                    SortOrder = i + 1,
                    WilayahName = x.WilayahName,
                    CustomerCount = x.CustomerCount,
                    AttentionCustomerCount = x.AttentionCustomerCount
                })
                .ToList();
        }

        private static List<DashboardCustomerPortfolioDistRow> BuildDistribution(
            List<PortfolioCustomerContext> processed,
            Func<PortfolioCustomerContext, string> keySelector,
            Func<string, string> labelResolver)
        {
            return processed
                .GroupBy(keySelector, StringComparer.OrdinalIgnoreCase)
                .Select((g, i) => new DashboardCustomerPortfolioDistRow
                {
                    Key = g.Key ?? string.Empty,
                    Label = labelResolver(g.Key),
                    CustomerCount = g.Count(),
                    SortOrder = i + 1
                })
                .OrderByDescending(r => r.CustomerCount)
                .Select((r, i) =>
                {
                    r.SortOrder = i + 1;
                    return r;
                })
                .ToList();
        }

        private static DashboardCustomerPortfolioPriorityRow ToPriorityRow(PortfolioCustomerContext ctx, int sortOrder) =>
            new DashboardCustomerPortfolioPriorityRow
            {
                SortOrder = sortOrder,
                PortfolioPriorityScore = ctx.PortfolioPriorityScore,
                CustomerKey = ctx.CustomerKey,
                CustomerCode = ctx.CustomerCode ?? string.Empty,
                CustomerName = ctx.CustomerName ?? string.Empty,
                WilayahName = ctx.WilayahName ?? string.Empty,
                Klasifikasi = ctx.Klasifikasi ?? string.Empty,
                LifecycleStage = ctx.LifecycleStage ?? string.Empty,
                LifecycleLabel = CustomerPortfolioOptimizationPolicy.ResolveLifecycleLabel(ctx.LifecycleStage),
                PortfolioTier = ctx.PortfolioTier ?? string.Empty,
                TierLabel = CustomerPortfolioOptimizationPolicy.ResolveTierLabel(ctx.PortfolioTier),
                PrimaryActionKey = ctx.PrimaryActionKey ?? string.Empty,
                PrimaryActionLabel = CustomerPortfolioOptimizationPolicy.ResolveActionLabel(ctx.PrimaryActionKey),
                ActionOwner = ctx.ActionOwner ?? string.Empty,
                ActionReasonText = ctx.ActionReasonText ?? string.Empty,
                TriggeredRuleIds = ctx.TriggeredRuleIds ?? string.Empty,
                MtdOmzet = ctx.MtdOmzet,
                OpenBalance = ctx.OpenBalance,
                OverdueBalance = ctx.OverdueBalance,
                M29Category = ctx.M29Category ?? string.Empty,
                SalesPersonName = ctx.SalesPersonName ?? string.Empty,
                SalesmanAchievementPercent = ctx.SalesmanAchievementPercent,
                SalesmanHighPiutangExposure = ctx.SalesmanHighPiutangExposure,
                IsAttention = ctx.IsAttention,
                M30LinkRoute = ctx.M30LinkRoute ?? string.Empty,
                CustomerReportRoute = ctx.CustomerReportRoute ?? string.Empty,
                DrillDownRouteM17 = CustomerPortfolioOptimizationPolicy.CustomerAnalyticsRoute,
                DrillDownRouteM29 = CustomerPortfolioOptimizationPolicy.CustomerRiskForecastRoute
            };

        private static DashboardCustomerPortfolioCustomerRow ToCustomerRow(PortfolioCustomerContext ctx, int sortOrder) =>
            new DashboardCustomerPortfolioCustomerRow
            {
                SortOrder = sortOrder,
                CustomerKey = ctx.CustomerKey,
                CustomerCode = ctx.CustomerCode ?? string.Empty,
                CustomerName = ctx.CustomerName ?? string.Empty,
                WilayahName = ctx.WilayahName ?? string.Empty,
                Klasifikasi = ctx.Klasifikasi ?? string.Empty,
                LifecycleStage = ctx.LifecycleStage ?? string.Empty,
                LifecycleLabel = CustomerPortfolioOptimizationPolicy.ResolveLifecycleLabel(ctx.LifecycleStage),
                PortfolioTier = ctx.PortfolioTier ?? string.Empty,
                TierLabel = CustomerPortfolioOptimizationPolicy.ResolveTierLabel(ctx.PortfolioTier),
                PrimaryActionKey = ctx.PrimaryActionKey ?? string.Empty,
                PrimaryActionLabel = CustomerPortfolioOptimizationPolicy.ResolveActionLabel(ctx.PrimaryActionKey),
                ActionOwner = ctx.ActionOwner ?? string.Empty,
                ActionReasonText = ctx.ActionReasonText ?? string.Empty,
                TriggeredRuleIds = ctx.TriggeredRuleIds ?? string.Empty,
                MtdOmzet = ctx.MtdOmzet,
                OpenBalance = ctx.OpenBalance,
                OverdueBalance = ctx.OverdueBalance,
                FakturCount6Mo = ctx.FakturCount6Mo,
                IsActiveMtd = ctx.IsActiveMtd,
                LastPurchaseDate = ctx.LastPurchaseDate,
                FirstPurchaseDate = ctx.FirstPurchaseDate,
                M29Category = ctx.M29Category ?? string.Empty,
                M29PrimarySignalKey = ctx.M29PrimarySignalKey ?? string.Empty,
                SalesPersonName = ctx.SalesPersonName ?? string.Empty,
                SalesmanAchievementPercent = ctx.SalesmanAchievementPercent,
                SalesmanHighPiutangExposure = ctx.SalesmanHighPiutangExposure,
                IsAttention = ctx.IsAttention,
                PortfolioPriorityScore = ctx.PortfolioPriorityScore,
                M30LinkRoute = ctx.M30LinkRoute ?? string.Empty,
                CustomerReportRoute = ctx.CustomerReportRoute ?? string.Empty,
                DrillDownRouteM17 = CustomerPortfolioOptimizationPolicy.CustomerAnalyticsRoute,
                DrillDownRouteM29 = CustomerPortfolioOptimizationPolicy.CustomerRiskForecastRoute,
                ValueDisclaimer = ctx.ValueDisclaimer ?? string.Empty
            };

        private sealed class CustomerUniverseEntry
        {
            public string Key { get; set; }

            public string CustomerCode { get; set; }

            public string CustomerName { get; set; }

            public string WilayahName { get; set; }

            public string Klasifikasi { get; set; }
        }

        private static List<CustomerUniverseEntry> BuildCustomerUniverse(List<CustomerModel> customers)
        {
            var universe = new List<CustomerUniverseEntry>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var customer in customers)
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(customer.CustomerCode, customer.CustomerName);
                if (key.Length == 0 || !seen.Add(key))
                    continue;

                universe.Add(new CustomerUniverseEntry
                {
                    Key = key,
                    CustomerCode = customer.CustomerCode?.Trim() ?? string.Empty,
                    CustomerName = customer.CustomerName?.Trim() ?? string.Empty,
                    WilayahName = customer.WilayahName?.Trim() ?? string.Empty,
                    Klasifikasi = customer.KlasifikasiName?.Trim() ?? string.Empty
                });
            }

            return universe;
        }

        private static HashSet<string> BuildM17AttentionKeys(DashboardCustomerAggregateResult customerAggregate)
        {
            if (customerAggregate?.AttentionList is null)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return customerAggregate.AttentionList
                .Select(a => DashboardCustomerKeyResolver.ResolveCodeFirst(a.CustomerCode, a.CustomerName))
                .Where(k => k.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> BuildPlafondBreachKeys(DashboardCustomerAggregateResult customerAggregate)
        {
            if (customerAggregate?.AttentionList is null)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return customerAggregate.AttentionList
                .Where(a => string.Equals(a.SignalKey, DashboardCustomerAggregator.SignalPlafondBreach, StringComparison.OrdinalIgnoreCase))
                .Select(a => DashboardCustomerKeyResolver.ResolveCodeFirst(a.CustomerCode, a.CustomerName))
                .Where(k => k.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> BuildActiveSet(List<FakturView> fakturList) =>
            fakturList
                .Select(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.Customer))
                .Where(key => key.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        private static HashSet<string> BuildDormantSet(
            List<CustomerLastFakturWithSalesmanDto> lastFakturList,
            HashSet<string> activeSet,
            DateTime today)
        {
            var cutoff = today.Date.AddDays(-DashboardCustomerAggregator.DormantDaysThreshold);
            var dormant = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in lastFakturList)
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(row.CustomerCode, row.CustomerName);
                if (key.Length == 0 || activeSet.Contains(key))
                    continue;

                if (row.LastFakturDate.Date <= cutoff)
                    dormant.Add(key);
            }

            return dormant;
        }

        private static Dictionary<string, DateTime> BuildFirstFakturLookup(IEnumerable<CustomerFirstFakturDto> rows)
        {
            var lookup = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows ?? Enumerable.Empty<CustomerFirstFakturDto>())
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(row.CustomerCode, row.CustomerName);
                if (key.Length == 0)
                    continue;

                lookup[key] = row.FirstFakturDate;
            }

            return lookup;
        }

        private static Dictionary<string, CustomerLastFakturWithSalesmanDto> BuildLastFakturLookup(
            IEnumerable<CustomerLastFakturWithSalesmanDto> rows)
        {
            var lookup = new Dictionary<string, CustomerLastFakturWithSalesmanDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows ?? Enumerable.Empty<CustomerLastFakturWithSalesmanDto>())
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(row.CustomerCode, row.CustomerName);
                if (key.Length == 0)
                    continue;

                lookup[key] = row;
            }

            return lookup;
        }

        private static Dictionary<string, int> BuildFrequencyLookup(IEnumerable<CustomerPurchaseFrequencyDto> rows)
        {
            var lookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows ?? Enumerable.Empty<CustomerPurchaseFrequencyDto>())
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(row.CustomerCode, row.CustomerName);
                if (key.Length == 0)
                    continue;

                lookup[key] = row.FakturCount;
            }

            return lookup;
        }

        private static Dictionary<string, decimal> BuildOmzetByKey(List<FakturView> fakturList) =>
            fakturList
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.Customer), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Key.Length > 0)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Total), StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, decimal> BuildOpenBalanceByKey(List<PiutangOpenBalanceDto> outstanding) =>
            outstanding
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.CustomerName), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Key.Length > 0)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.KurangBayar), StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, decimal> BuildOverdueBalanceByKey(
            List<PiutangOpenBalanceDto> outstanding,
            DateTime today) =>
            outstanding
                .Where(r => r.JatuhTempo.Date <= today.Date)
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.CustomerName), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Key.Length > 0)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.KurangBayar), StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, int> BuildRankMap(Dictionary<string, decimal> amountsByKey)
        {
            return amountsByKey
                .OrderByDescending(x => x.Value)
                .Select((x, i) => new { x.Key, Rank = i + 1 })
                .ToDictionary(x => x.Key, x => x.Rank, StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, decimal?> BuildSalesmanAchievementLookup(DashboardSalesmanAggregateResult snapshot)
        {
            if (snapshot?.TopAchievement is null)
                return new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

            return snapshot.TopAchievement
                .Where(r => !string.IsNullOrWhiteSpace(r.SalesPersonName))
                .GroupBy(r => NormalizeKey(r.SalesPersonName), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().AchievementPercent, StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> BuildHighPiutangSalesmen(DashboardSalesmanAggregateResult snapshot)
        {
            if (snapshot?.AttentionList is null)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return snapshot.AttentionList
                .Where(a => string.Equals(a.SignalKey, DashboardSalesmanAggregator.SignalHighPiutangExposure, StringComparison.OrdinalIgnoreCase))
                .Select(a => NormalizeKey(a.SalesPersonName))
                .Where(k => k.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static decimal? ResolveSalesmanAchievement(
            string salesPersonName,
            Dictionary<string, decimal?> lookup)
        {
            if (string.IsNullOrWhiteSpace(salesPersonName))
                return null;

            return lookup.TryGetValue(NormalizeKey(salesPersonName), out var achievement)
                ? achievement
                : null;
        }

        private static string NormalizeKey(string value) =>
            (value ?? string.Empty).Trim().ToUpperInvariant();

        private static int SeverityRank(string severity)
        {
            if (severity == CustomerRiskForecastPolicy.SeverityStrong) return 3;
            if (severity == CustomerRiskForecastPolicy.SeverityModerate) return 2;
            if (severity == CustomerRiskForecastPolicy.SeverityWeak) return 1;
            return 0;
        }
    }
}
