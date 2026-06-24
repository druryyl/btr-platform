using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.CustomerAgg;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardCustomerRiskForecastAggregator
    {
        private const string ReportRoutePiutang = "/reports/piutang";
        private const string DrillDownRoute = "/dashboard/customers";

        public DashboardCustomerRiskForecastAggregateResult Aggregate(
            IEnumerable<PiutangOpenBalanceDto> piutangRows,
            IEnumerable<CustomerOmzetHistoryDto> omzetHistoryRows,
            IEnumerable<CustomerLastFakturDto> lastFakturRows,
            IEnumerable<CustomerModel> customers,
            IEnumerable<CustomerPelunasanSummaryDto> pelunasanSummaryRows,
            IEnumerable<CustomerPaymentBehaviorDto> paymentBehaviorRows,
            IEnumerable<FakturView> currentMonthFakturRows,
            DateTime businessDate,
            DateTime generatedAt,
            CustomerRiskForecastOptions options)
        {
            options = options ?? CustomerRiskForecastOptions.FromDashboardOptions(null);
            var today = businessDate.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            var daysElapsed = Math.Max(1, (today - monthStart).Days + 1);
            var horizonDays = options.HorizonDays;
            var horizonEnd = today.AddDays(horizonDays);

            var outstanding = (piutangRows ?? Enumerable.Empty<PiutangOpenBalanceDto>())
                .Where(r => r.KurangBayar > 1)
                .ToList();
            var omzetHistory = (omzetHistoryRows ?? Enumerable.Empty<CustomerOmzetHistoryDto>()).ToList();
            var lastFakturList = (lastFakturRows ?? Enumerable.Empty<CustomerLastFakturDto>()).ToList();
            var customerList = (customers ?? Enumerable.Empty<CustomerModel>()).ToList();
            var pelunasanSummary = (pelunasanSummaryRows ?? Enumerable.Empty<CustomerPelunasanSummaryDto>()).ToList();
            var paymentBehavior = (paymentBehaviorRows ?? Enumerable.Empty<CustomerPaymentBehaviorDto>()).ToList();
            var fakturList = (currentMonthFakturRows ?? Enumerable.Empty<FakturView>()).ToList();

            var masterByKey = BuildMasterLookup(customerList);
            var historicalKeys = BuildHistoricalCustomerKeys(lastFakturList);
            var activeSet = BuildActiveSet(fakturList);
            var omzetByKey = BuildOmzetByKey(fakturList);
            var omzetHistoryByKey = BuildOmzetHistoryByKey(omzetHistory);
            var paymentLagByKey = BuildPaymentLagByKey(paymentBehavior);
            var paymentRecencyByKey = BuildPaymentRecencyByKey(pelunasanSummary, today);
            var salesmanByKey = BuildSalesmanByKey(fakturList);
            var displayNames = BuildDisplayNameLookup(fakturList, outstanding, lastFakturList, customerList);
            var codeByKey = BuildCodeLookup(fakturList, outstanding, lastFakturList, customerList);
            var lastFakturByKey = BuildLastFakturByKey(lastFakturList);

            var piutangMetricsByKey = BuildPiutangMetricsByKey(outstanding, today, horizonEnd);
            var companyDueWithinHorizon = piutangMetricsByKey.Values.Sum(m => m.DueWithinHorizon);
            var totalPiutang = outstanding.Sum(r => r.KurangBayar);

            var contexts = new List<CustomerRiskForecastContext>();
            foreach (var customerKey in historicalKeys)
            {
                if (!displayNames.ContainsKey(customerKey))
                    continue;

                var master = masterByKey.TryGetValue(customerKey, out var customer) ? customer : null;
                var metrics = piutangMetricsByKey.TryGetValue(customerKey, out var pm)
                    ? pm
                    : new PiutangMetrics();

                var mtdOmzet = omzetByKey.TryGetValue(customerKey, out var mtd) ? mtd : 0m;
                var priorMonthOmzet = omzetHistoryByKey.TryGetValue(customerKey, out var history)
                    ? history.PriorMonthOmzet
                    : 0m;
                if (mtdOmzet <= 0 && history != null)
                    mtdOmzet = history.CurrentMonthOmzet;

                var projectedMonthOmzet = CustomerRiskForecastPolicy.ComputeProjectedMonthOmzet(
                    mtdOmzet,
                    daysElapsed,
                    daysInMonth);
                decimal? declineRatio = null;
                if (priorMonthOmzet >= options.PriorMonthOmzetFloorIdr && priorMonthOmzet > 0)
                    declineRatio = projectedMonthOmzet / priorMonthOmzet;

                var plafond = master?.Plafond ?? 0m;
                var projectedOpen = CustomerRiskForecastPolicy.ComputeProjectedOpenBalance(
                    metrics.OpenBalance,
                    mtdOmzet,
                    daysElapsed,
                    horizonDays);

                var context = new CustomerRiskForecastContext
                {
                    CustomerKey = customerKey,
                    CustomerCode = codeByKey.TryGetValue(customerKey, out var code) ? code : string.Empty,
                    CustomerName = displayNames[customerKey],
                    WilayahName = master?.WilayahName?.Trim() ?? string.Empty,
                    SalesPersonName = salesmanByKey.TryGetValue(customerKey, out var sp) ? sp.Name : string.Empty,
                    SalesPersonId = salesmanByKey.TryGetValue(customerKey, out var sp2) ? sp2.Id : string.Empty,
                    OpenBalance = metrics.OpenBalance,
                    OverdueBalance = metrics.OverdueBalance,
                    DueWithinHorizon = metrics.DueWithinHorizon,
                    MinDaysUntilDue = metrics.MinDaysUntilDue,
                    MtdOmzet = mtdOmzet,
                    PriorMonthOmzet = priorMonthOmzet,
                    DeclineRatio = declineRatio,
                    DaysSinceLastFaktur = lastFakturByKey.TryGetValue(customerKey, out var lastDate)
                        ? (today - lastDate.Date).Days
                        : (int?)null,
                    AvgPaymentLagDays = paymentLagByKey.TryGetValue(customerKey, out var lag) ? lag : (decimal?)null,
                    DaysSinceLastPayment = paymentRecencyByKey.TryGetValue(customerKey, out var lastPay)
                        ? (today - lastPay.Date).Days
                        : (int?)null,
                    Plafond = plafond,
                    ProjectedOpenBalance = projectedOpen,
                    IsActiveThisMonth = activeSet.Contains(customerKey),
                    IsCurrentlyPlafondBreach = plafond > 0 && metrics.OpenBalance > plafond,
                    HasChronicOverdue = metrics.HasChronicOverdue,
                    IsSuspended = master?.IsSuspend ?? false
                };

                var signalRows = CustomerRiskSignalBuilder.Build(
                    context,
                    options,
                    companyDueWithinHorizon,
                    today,
                    metrics.AgingBuckets,
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase));

                context.Signals = signalRows.Select(s => new CustomerRiskSignalContext
                {
                    SignalKey = s.SignalKey,
                    Severity = s.Severity,
                    RuleId = s.RuleId,
                    Explanation = s.Explanation
                }).ToList();

                context.Category = CustomerRiskForecastPolicy.ResolveCategory(context.Signals);

                if (context.Category == CustomerRiskForecastPolicy.CategoryHighRisk ||
                    context.Category == CustomerRiskForecastPolicy.CategoryCritical)
                {
                    context.Signals.Add(new CustomerRiskSignalContext
                    {
                        SignalKey = CustomerRiskSignalBuilder.SignalHighCollectionRisk,
                        Severity = CustomerRiskForecastPolicy.SeverityStrong,
                        RuleId = "CRF-L01",
                        Explanation = "Customer classified as high collection forecast risk."
                    });
                }

                var strongCount = context.Signals.Count(s => s.Severity == CustomerRiskForecastPolicy.SeverityStrong);
                var moderateCount = context.Signals.Count(s => s.Severity == CustomerRiskForecastPolicy.SeverityModerate);
                var weakCount = context.Signals.Count(s => s.Severity == CustomerRiskForecastPolicy.SeverityWeak);
                context.RiskPriorityScore = CustomerRiskForecastPolicy.ComputeRiskPriorityScore(
                    context.Category,
                    context.OpenBalance,
                    strongCount,
                    moderateCount,
                    weakCount,
                    context.MinDaysUntilDue);

                contexts.Add(context);
            }

            var activeCustomerCount = activeSet.Count;
            var elevatedContexts = contexts
                .Where(c => c.Category == CustomerRiskForecastPolicy.CategoryHighRisk ||
                            c.Category == CustomerRiskForecastPolicy.CategoryCritical)
                .ToList();
            var atRiskContexts = contexts
                .Where(c => c.Category != CustomerRiskForecastPolicy.CategoryHealthy)
                .ToList();

            var kpi = new DashboardCustomerRiskForecastKpiSnapshot
            {
                HorizonDays = horizonDays,
                CustomersForecastedAtRisk = atRiskContexts.Count,
                HighRiskCustomerCount = contexts.Count(c => c.Category == CustomerRiskForecastPolicy.CategoryHighRisk),
                CriticalCustomerCount = contexts.Count(c => c.Category == CustomerRiskForecastPolicy.CategoryCritical),
                ElevatedRiskReceivable = elevatedContexts.Sum(c => c.OpenBalance),
                TotalPiutang = totalPiutang,
                ForecastConfidence = CustomerRiskForecastPolicy.ResolveForecastConfidence(daysElapsed),
                HealthyCount = contexts.Count(c => c.Category == CustomerRiskForecastPolicy.CategoryHealthy),
                WatchCount = contexts.Count(c => c.Category == CustomerRiskForecastPolicy.CategoryWatch),
                AttentionCount = contexts.Count(c => c.Category == CustomerRiskForecastPolicy.CategoryAttention),
                HighRiskCount = contexts.Count(c => c.Category == CustomerRiskForecastPolicy.CategoryHighRisk),
                CriticalCount = contexts.Count(c => c.Category == CustomerRiskForecastPolicy.CategoryCritical)
            };

            kpi.ElevatedRiskReceivablePercent = totalPiutang > 0
                ? Math.Round(kpi.ElevatedRiskReceivable / totalPiutang * 100m, 1, MidpointRounding.AwayFromZero)
                : (decimal?)null;
            kpi.PortfolioHealthScore = CustomerRiskForecastPolicy.ComputePortfolioHealthScore(
                kpi.ElevatedRiskReceivable,
                totalPiutang,
                kpi.HighRiskCustomerCount + kpi.CriticalCustomerCount,
                Math.Max(activeCustomerCount, 1));

            kpi.PaymentDelaySignalCount = CountCustomersWithSignalFamily(contexts, CustomerRiskSignalBuilder.IsPaymentDelaySignal);
            kpi.CreditLimitSignalCount = CountCustomersWithSignalFamily(contexts, CustomerRiskSignalBuilder.IsCreditLimitSignal);
            kpi.InactivitySignalCount = CountCustomersWithSignalFamily(contexts, CustomerRiskSignalBuilder.IsInactivitySignal);
            kpi.PurchaseDeclineSignalCount = CountCustomersWithSignalFamily(contexts, CustomerRiskSignalBuilder.IsPurchaseDeclineSignal);
            kpi.CollectionRiskSignalCount = CountCustomersWithSignalFamily(contexts, CustomerRiskSignalBuilder.IsCollectionRiskSignal);

            var topCustomers = contexts
                .Where(c => c.Category != CustomerRiskForecastPolicy.CategoryHealthy)
                .OrderByDescending(c => c.RiskPriorityScore)
                .ThenByDescending(c => c.OpenBalance)
                .ThenBy(c => c.CustomerName, StringComparer.OrdinalIgnoreCase)
                .Take(options.MaxTopCustomers)
                .Select((c, index) => MapCustomerRow(c, index + 1))
                .ToList();

            var attentionCandidates = contexts
                .SelectMany(c => c.Signals.Select(s => new
                {
                    Context = c,
                    Signal = s
                }))
                .OrderByDescending(x => SeverityRank(x.Signal.Severity))
                .ThenByDescending(x => x.Context.OpenBalance)
                .ThenBy(x => x.Context.CustomerName, StringComparer.OrdinalIgnoreCase)
                .Take(options.MaxAttentionRows)
                .Select((x, index) => new DashboardCustomerRiskForecastAttentionRow
                {
                    SortOrder = index + 1,
                    CustomerCode = x.Context.CustomerCode,
                    CustomerName = x.Context.CustomerName,
                    SignalKey = x.Signal.SignalKey,
                    SignalLabel = CustomerRiskSignalBuilder.ResolveSignalLabel(x.Signal.SignalKey),
                    Severity = x.Signal.Severity,
                    Amount = x.Context.OpenBalance > 0 ? x.Context.OpenBalance : (decimal?)null,
                    HorizonText = $"{horizonDays} days",
                    RuleId = x.Signal.RuleId,
                    Explanation = x.Signal.Explanation,
                    ReportRoute = ReportRoutePiutang
                })
                .ToList();

            var recommendations = contexts
                .Where(c => c.Category != CustomerRiskForecastPolicy.CategoryHealthy)
                .Select(c => new
                {
                    Context = c,
                    Recommendation = CustomerRiskRecommendationBuilder.Build(c)
                })
                .OrderByDescending(x => x.Context.RiskPriorityScore)
                .ThenBy(x => x.Context.CustomerName, StringComparer.OrdinalIgnoreCase)
                .Take(options.MaxRecommendations)
                .Select((x, index) => new DashboardCustomerRiskForecastRecommendationRow
                {
                    SortOrder = index + 1,
                    RecommendationKey = x.Recommendation.RecommendationKey,
                    RecommendationLabel = x.Recommendation.RecommendationLabel,
                    CustomerCode = x.Context.CustomerCode,
                    CustomerName = x.Context.CustomerName,
                    Category = CustomerRiskForecastPolicy.ResolveCategoryLabel(x.Context.Category),
                    ReasonText = x.Recommendation.ReasonText,
                    RuleId = x.Recommendation.RuleId,
                    ReportRoute = x.Recommendation.ReportRoute,
                    DrillDownRoute = x.Recommendation.DrillDownRoute
                })
                .ToList();

            var severeDeclineCount = contexts.Count(c =>
                c.Signals.Any(s => s.SignalKey == CustomerRiskSignalBuilder.SignalSevereDecline));

            kpi.ExecutiveSummaryText = CustomerRiskExecutiveSummaryBuilder.Build(
                kpi,
                topCustomers.FirstOrDefault(),
                today,
                horizonDays,
                daysElapsed,
                severeDeclineCount);

            return new DashboardCustomerRiskForecastAggregateResult
            {
                GeneratedAt = generatedAt,
                BusinessDate = today,
                PeriodYear = today.Year,
                PeriodMonth = today.Month,
                DaysInMonth = daysInMonth,
                DaysElapsed = daysElapsed,
                Kpi = kpi,
                CategoryDistribution = BuildCategoryDistribution(contexts),
                TopWilayah = BuildTopWilayah(contexts),
                SignalMix = BuildSignalMix(kpi),
                TopCustomers = topCustomers,
                AttentionList = attentionCandidates,
                Recommendations = recommendations,
                Contexts = contexts
            };
        }

        private static DashboardCustomerRiskForecastCustomerRow MapCustomerRow(CustomerRiskForecastContext context, int sortOrder)
        {
            var primary = context.Signals
                .OrderByDescending(s => SeverityRank(s.Severity))
                .ThenBy(s => s.SignalKey, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            var recommendation = CustomerRiskRecommendationBuilder.Build(context);

            return new DashboardCustomerRiskForecastCustomerRow
            {
                SortOrder = sortOrder,
                RiskPriorityScore = context.RiskPriorityScore,
                Category = context.Category,
                CategoryLabel = CustomerRiskForecastPolicy.ResolveCategoryLabel(context.Category),
                CustomerCode = context.CustomerCode,
                CustomerName = context.CustomerName,
                WilayahName = context.WilayahName,
                SalesPersonName = context.SalesPersonName,
                OpenBalance = context.OpenBalance,
                OverdueBalance = context.OverdueBalance,
                DueWithinHorizon = context.DueWithinHorizon,
                Plafond = context.Plafond,
                ProjectedOpenBalance = context.ProjectedOpenBalance,
                MtdOmzet = context.MtdOmzet,
                PriorMonthOmzet = context.PriorMonthOmzet,
                DeclineRatio = context.DeclineRatio,
                DaysSinceLastFaktur = context.DaysSinceLastFaktur,
                AvgPaymentLagDays = context.AvgPaymentLagDays,
                PrimarySignalKey = primary?.SignalKey ?? string.Empty,
                PrimarySignalLabel = primary != null
                    ? CustomerRiskSignalBuilder.ResolveSignalLabel(primary.SignalKey)
                    : string.Empty,
                ReasonText = primary?.Explanation ?? string.Empty,
                RecommendationKey = recommendation.RecommendationKey,
                RecommendationLabel = recommendation.RecommendationLabel,
                ReportRoute = ReportRoutePiutang,
                DrillDownRoute = DrillDownRoute
            };
        }

        private static List<DashboardCustomerRiskForecastDistRow> BuildCategoryDistribution(
            List<CustomerRiskForecastContext> contexts)
        {
            var categories = new[]
            {
                (CustomerRiskForecastPolicy.CategoryHealthy, "Healthy", 1),
                (CustomerRiskForecastPolicy.CategoryWatch, "Watch", 2),
                (CustomerRiskForecastPolicy.CategoryAttention, "Attention", 3),
                (CustomerRiskForecastPolicy.CategoryHighRisk, "High Risk", 4),
                (CustomerRiskForecastPolicy.CategoryCritical, "Critical", 5)
            };

            return categories
                .Select(c => new DashboardCustomerRiskForecastDistRow
                {
                    Category = c.Item1,
                    CategoryLabel = c.Item2,
                    CustomerCount = contexts.Count(x => x.Category == c.Item1),
                    SortOrder = c.Item3
                })
                .ToList();
        }

        private static List<DashboardCustomerRiskForecastWilayahRow> BuildTopWilayah(
            List<CustomerRiskForecastContext> contexts)
        {
            return contexts
                .Where(c => c.Category == CustomerRiskForecastPolicy.CategoryHighRisk ||
                            c.Category == CustomerRiskForecastPolicy.CategoryCritical)
                .GroupBy(c => string.IsNullOrWhiteSpace(c.WilayahName) ? "Unknown" : c.WilayahName.Trim())
                .Select(g => new
                {
                    WilayahName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.WilayahName, StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .Select((x, index) => new DashboardCustomerRiskForecastWilayahRow
                {
                    WilayahName = x.WilayahName,
                    ElevatedRiskCustomerCount = x.Count,
                    SortOrder = index + 1
                })
                .ToList();
        }

        private static List<DashboardCustomerRiskForecastSignalMixRow> BuildSignalMix(
            DashboardCustomerRiskForecastKpiSnapshot kpi)
        {
            return new List<DashboardCustomerRiskForecastSignalMixRow>
            {
                Row("PaymentDelay", "Payment Delay", kpi.PaymentDelaySignalCount, 1),
                Row("CreditLimit", "Credit Limit", kpi.CreditLimitSignalCount, 2),
                Row("Inactivity", "Inactivity", kpi.InactivitySignalCount, 3),
                Row("PurchaseDecline", "Purchase Decline", kpi.PurchaseDeclineSignalCount, 4),
                Row("CollectionRisk", "Collection Risk", kpi.CollectionRiskSignalCount, 5)
            };
        }

        private static DashboardCustomerRiskForecastSignalMixRow Row(
            string key,
            string label,
            int count,
            int sortOrder) =>
            new DashboardCustomerRiskForecastSignalMixRow
            {
                SignalFamilyKey = key,
                SignalFamilyLabel = label,
                CustomerCount = count,
                SortOrder = sortOrder
            };

        private static int CountCustomersWithSignalFamily(
            List<CustomerRiskForecastContext> contexts,
            Func<string, bool> predicate) =>
            contexts.Count(c => c.Signals.Any(s => predicate(s.SignalKey)));

        private static int SeverityRank(string severity)
        {
            if (severity == CustomerRiskForecastPolicy.SeverityStrong) return 3;
            if (severity == CustomerRiskForecastPolicy.SeverityModerate) return 2;
            if (severity == CustomerRiskForecastPolicy.SeverityWeak) return 1;
            return 0;
        }

        private sealed class PiutangMetrics
        {
            public decimal OpenBalance { get; set; }

            public decimal OverdueBalance { get; set; }

            public decimal DueWithinHorizon { get; set; }

            public int MinDaysUntilDue { get; set; } = int.MaxValue;

            public bool HasChronicOverdue { get; set; }

            public HashSet<string> AgingBuckets { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, PiutangMetrics> BuildPiutangMetricsByKey(
            List<PiutangOpenBalanceDto> outstanding,
            DateTime today,
            DateTime horizonEnd)
        {
            var metrics = new Dictionary<string, PiutangMetrics>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in outstanding)
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(row.CustomerCode, row.CustomerName);
                if (key.Length == 0)
                    continue;

                if (!metrics.TryGetValue(key, out var item))
                    item = new PiutangMetrics();

                item.OpenBalance += row.KurangBayar;
                var bucket = PiutangAgingBucketResolver.ResolveBucketKey(row.JatuhTempo, today);
                item.AgingBuckets.Add(bucket);

                if (bucket != "Current")
                    item.OverdueBalance += row.KurangBayar;
                if (bucket == "DaysOver90")
                    item.HasChronicOverdue = true;

                var dueDate = row.JatuhTempo.Date;
                if (dueDate > today && dueDate <= horizonEnd)
                {
                    item.DueWithinHorizon += row.KurangBayar;
                    var daysUntil = (dueDate - today).Days;
                    if (daysUntil < item.MinDaysUntilDue)
                        item.MinDaysUntilDue = daysUntil;
                }

                metrics[key] = item;
            }

            foreach (var pair in metrics)
            {
                if (pair.Value.MinDaysUntilDue == int.MaxValue)
                    pair.Value.MinDaysUntilDue = -1;
            }

            return metrics;
        }

        private static HashSet<string> BuildHistoricalCustomerKeys(List<CustomerLastFakturDto> lastFakturList)
        {
            return lastFakturList
                .Select(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.CustomerName))
                .Where(key => key.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, CustomerModel> BuildMasterLookup(List<CustomerModel> customers)
        {
            var lookup = new Dictionary<string, CustomerModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var customer in customers)
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(customer.CustomerCode, customer.CustomerName);
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

        private static Dictionary<string, decimal> BuildOmzetByKey(List<FakturView> fakturList)
        {
            return fakturList
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.Customer))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.GrandTotal), StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, CustomerOmzetHistoryDto> BuildOmzetHistoryByKey(
            List<CustomerOmzetHistoryDto> omzetHistory)
        {
            return omzetHistory
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.CustomerName))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, decimal?> BuildPaymentLagByKey(List<CustomerPaymentBehaviorDto> paymentBehavior)
        {
            return paymentBehavior
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.CustomerName))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(g => g.Key, g => g.First().AvgPaymentLagDays, StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, DateTime> BuildPaymentRecencyByKey(
            List<CustomerPelunasanSummaryDto> pelunasanSummary,
            DateTime today)
        {
            return pelunasanSummary
                .Where(r => r.LastPaymentDate.HasValue)
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.CustomerName))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(
                    g => g.Key,
                    g => g.Max(x => x.LastPaymentDate.Value),
                    StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, (string Id, string Name)> BuildSalesmanByKey(List<FakturView> fakturList)
        {
            return fakturList
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.Customer))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var latest = g.OrderByDescending(x => x.Tgl).First();
                        return (latest.SalesPersonId ?? string.Empty, latest.SalesPersonName ?? string.Empty);
                    },
                    StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, DateTime> BuildLastFakturByKey(List<CustomerLastFakturDto> lastFakturList)
        {
            return lastFakturList
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.CustomerName))
                .Where(g => g.Key.Length > 0)
                .ToDictionary(g => g.Key, g => g.Max(x => x.LastFakturDate), StringComparer.OrdinalIgnoreCase);
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

            foreach (var c in customers) Add(c.CustomerCode, c.CustomerName);
            foreach (var r in fakturList) Add(r.CustomerCode, r.Customer);
            foreach (var r in outstanding) Add(r.CustomerCode, r.CustomerName);
            foreach (var r in lastFakturList) Add(r.CustomerCode, r.CustomerName);

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

            foreach (var c in customers) Add(c.CustomerCode, c.CustomerName);
            foreach (var r in fakturList) Add(r.CustomerCode, r.Customer);
            foreach (var r in outstanding) Add(r.CustomerCode, r.CustomerName);
            foreach (var r in lastFakturList) Add(r.CustomerCode, r.CustomerName);

            return codes;
        }
    }
}
