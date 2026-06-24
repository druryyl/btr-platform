using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.CustomerAgg;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardCollectionOptimizationAggregator
    {
        public const string QueueProactiveReminder = "ProactiveReminder";
        public const string QueueCreditReview = "CreditReview";
        public const string QueueSalesRecovery = "SalesRecovery";
        public const string QueueEscalateManagement = "EscalateManagement";

        public const string WorkloadSalesman = "Salesman";
        public const string WorkloadWilayah = "Wilayah";
        public const string WorkloadKlasifikasi = "Klasifikasi";

        public DashboardCollectionOptimizationAggregateResult Aggregate(
            IReadOnlyList<CustomerRiskForecastContext> forecastContexts,
            DashboardCustomerRiskForecastAggregateResult forecastAggregate,
            DashboardCollectionAggregateResult collectionSnapshot,
            IEnumerable<PiutangOpenBalanceDto> piutangRows,
            IEnumerable<FakturView> currentMonthFakturRows,
            IEnumerable<CustomerModel> customers,
            DateTime businessDate,
            DateTime generatedAt,
            CollectionOptimizationOptions options)
        {
            options = options ?? CollectionOptimizationOptions.FromDashboardOptions(null);
            var contexts = (forecastContexts ?? Array.Empty<CustomerRiskForecastContext>()).ToList();
            var today = businessDate.Date;
            var daysElapsed = forecastAggregate?.DaysElapsed ?? Math.Max(1, today.Day);

            var dueAmounts = BuildDueAmountsByKey(piutangRows, today);
            var top10Keys = BuildTopOmzetKeys(currentMonthFakturRows, 10);
            var top20Keys = BuildTopOmzetKeys(currentMonthFakturRows, 20);
            var klasifikasiByKey = BuildKlasifikasiLookup(customers);

            var m20LegacyKeys = BuildCustomerAttentionKeys(collectionSnapshot, DashboardCollectionAggregator.SignalLegacyDebt);
            var m20ChronicKeys = BuildCustomerAttentionKeys(collectionSnapshot, DashboardCollectionAggregator.SignalChronicOverdue);
            var m20PlafondKeys = BuildCustomerAttentionKeys(collectionSnapshot, DashboardCollectionAggregator.SignalPlafondBreachOverdue);
            var lowRecoverySalesmen = BuildLowRecoverySalesmen(collectionSnapshot);
            var hotspotWilayah = BuildHotspotWilayah(collectionSnapshot);

            var processed = new List<(CustomerRiskForecastContext Forecast, CollectionOptimizationContext Opt)>();

            foreach (var forecast in contexts)
            {
                if (forecast is null)
                    continue;

                var due = dueAmounts.TryGetValue(forecast.CustomerKey, out var d)
                    ? d
                    : new DueAmountMetrics();

                var enrichment = new CollectionOptimizationEnrichment
                {
                    DueWithin7Days = due.DueWithin7Days,
                    DueWithin14Days = due.DueWithin14Days,
                    HasChronicOverdue = forecast.HasChronicOverdue || m20ChronicKeys.Contains(forecast.CustomerKey),
                    HasLegacyDebtSignal = m20LegacyKeys.Contains(forecast.CustomerKey),
                    HasPlafondBreachOverdueSignal = m20PlafondKeys.Contains(forecast.CustomerKey),
                    SalesmanLowRecovery = lowRecoverySalesmen.Contains(NormalizeKey(forecast.SalesPersonName)),
                    IsTop10MtdOmzet = top10Keys.Contains(forecast.CustomerKey),
                    IsTop20MtdOmzet = top20Keys.Contains(forecast.CustomerKey),
                    Klasifikasi = klasifikasiByKey.TryGetValue(forecast.CustomerKey, out var klas) ? klas : string.Empty
                };

                var m29Recommendation = CustomerRiskRecommendationBuilder.Build(forecast);
                var actionCategory = CollectionOptimizationPolicy.ResolveActionCategory(
                    forecast,
                    enrichment,
                    m29Recommendation.RecommendationKey,
                    options.SalesRecoveryOverdueFloorIdr);

                if (enrichment.IsTop10MtdOmzet &&
                    forecast.Category == CustomerRiskForecastPolicy.CategoryWatch &&
                    actionCategory == CollectionOptimizationPolicy.ActionNoActionToday &&
                    forecast.Signals != null &&
                    forecast.Signals.Count > 0)
                {
                    actionCategory = CollectionOptimizationPolicy.ActionRelationshipMonitor;
                }

                var impactAmount = CollectionOptimizationPolicy.ComputeCollectionImpactAmount(
                    forecast.OverdueBalance,
                    due.DueWithin7Days);

                var priorityScore = CollectionOptimizationPolicy.ComputeCollectionPriorityScore(
                    actionCategory,
                    forecast,
                    enrichment,
                    impactAmount);

                var recommendedActionKey = CollectionOptimizationPolicy.ResolveRecommendedActionKey(actionCategory);
                var catRuleId = CollectionOptimizationPolicy.ResolveCatRuleId(actionCategory);
                var primarySignal = forecast.Signals?
                    .OrderByDescending(s => SeverityRank(s.Severity))
                    .FirstOrDefault();

                var opt = new CollectionOptimizationContext
                {
                    CustomerKey = forecast.CustomerKey,
                    DueWithin7Days = due.DueWithin7Days,
                    DueWithin14Days = due.DueWithin14Days,
                    MinDaysUntilDue = forecast.MinDaysUntilDue,
                    HasChronicOverdue = enrichment.HasChronicOverdue,
                    HasLegacyDebtSignal = enrichment.HasLegacyDebtSignal,
                    HasPlafondBreachOverdueSignal = enrichment.HasPlafondBreachOverdueSignal,
                    SalesmanLowRecovery = enrichment.SalesmanLowRecovery,
                    IsTop10MtdOmzet = enrichment.IsTop10MtdOmzet,
                    IsTop20MtdOmzet = enrichment.IsTop20MtdOmzet,
                    CreditUtilizationPercent = forecast.Plafond > 0
                        ? Math.Round(forecast.OpenBalance / forecast.Plafond * 100m, 1)
                        : (decimal?)null,
                    Klasifikasi = enrichment.Klasifikasi,
                    ActionCategoryKey = actionCategory,
                    RecommendedActionKey = recommendedActionKey,
                    ActionOwner = CollectionOptimizationPolicy.ResolveActionOwner(actionCategory),
                    CollectionPriorityScore = priorityScore,
                    CollectionImpactAmount = impactAmount,
                    M29RecommendationKey = m29Recommendation.RecommendationKey,
                    M29PrimarySignalKey = primarySignal?.SignalKey ?? string.Empty,
                    CatRuleId = catRuleId,
                    SelectionReasonText = CollectionOptimizationActionBuilder.BuildSelectionReasonText(
                        forecast,
                        null,
                        primarySignal != null ? CustomerRiskSignalBuilder.ResolveSignalLabel(primarySignal.SignalKey) : string.Empty),
                    PriorityReasonText = CollectionOptimizationActionBuilder.BuildPriorityReasonText(
                        actionCategory,
                        impactAmount,
                        forecast.MinDaysUntilDue),
                    ActionReasonText = CollectionOptimizationActionBuilder.BuildActionReasonText(
                        actionCategory,
                        forecast,
                        catRuleId),
                    TriggeredRuleIds = CollectionOptimizationActionBuilder.BuildTriggeredRuleIds(
                        forecast,
                        catRuleId,
                        m29Recommendation.RuleId)
                };

                processed.Add((forecast, opt));
            }

            var actionable = processed
                .Where(x => CollectionOptimizationPolicy.IsActionable(x.Opt.ActionCategoryKey))
                .OrderByDescending(x => x.Opt.CollectionPriorityScore)
                .ThenByDescending(x => x.Opt.CollectionImpactAmount)
                .ThenBy(x => x.Forecast.CustomerName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var priorityQueue = actionable
                .Take(options.MaxPriorityRows)
                .Select((x, i) => CollectionOptimizationActionBuilder.ToPriorityRow(x.Forecast, x.Opt, i + 1))
                .ToList();

            var specializedQueues = BuildSpecializedQueues(actionable, options.MaxQueueRows);
            var impactRows = actionable
                .Where(x => x.Opt.CollectionImpactAmount > 0)
                .OrderByDescending(x => x.Opt.CollectionImpactAmount)
                .Take(options.MaxImpactRows)
                .Select((x, i) => CollectionOptimizationActionBuilder.ToImpactRow(x.Forecast, x.Opt, i + 1))
                .ToList();

            var actionDistribution = processed
                .GroupBy(x => x.Opt.ActionCategoryKey, StringComparer.OrdinalIgnoreCase)
                .Select((g, i) => new DashboardCollectionOptimizationActionDistRow
                {
                    ActionCategoryKey = g.Key,
                    ActionCategoryLabel = CollectionOptimizationPolicy.ResolveActionCategoryLabel(g.Key),
                    CustomerCount = g.Count(),
                    ImpactTotal = g.Sum(x => x.Opt.CollectionImpactAmount),
                    SortOrder = i + 1
                })
                .OrderByDescending(r => r.CustomerCount)
                .Select((r, i) =>
                {
                    r.SortOrder = i + 1;
                    return r;
                })
                .ToList();

            var workload = BuildWorkload(actionable, hotspotWilayah, options.MaxWorkloadRows);

            var overdueExposure = processed.Sum(x => x.Forecast.OverdueBalance);
            var dueWithin7Total = processed.Sum(x => x.Opt.DueWithin7Days);
            var collectionUnavailable = collectionSnapshot is null;

            var kpi = new DashboardCollectionOptimizationKpiSnapshot
            {
                ActionsTodayCount = actionable.Count,
                ImmediateCollectionCount = CountByCategory(processed, CollectionOptimizationPolicy.ActionImmediateCollection),
                PriorityFollowUpCount = CountByCategory(processed, CollectionOptimizationPolicy.ActionPriorityFollowUp),
                ProactiveReminderCount = CountByCategory(processed, CollectionOptimizationPolicy.ActionProactiveReminder),
                CreditReviewCount = CountByCategory(processed, CollectionOptimizationPolicy.ActionCreditReview),
                SalesRecoveryCount = CountByCategory(processed, CollectionOptimizationPolicy.ActionSalesRecoveryVisit),
                EscalateManagementCount = CountByCategory(processed, CollectionOptimizationPolicy.ActionEscalateManagement),
                CollectionImpactTotal = actionable.Sum(x => x.Opt.CollectionImpactAmount),
                ImmediateImpactTotal = processed
                    .Where(x => CollectionOptimizationPolicy.IsImmediateOrPriority(x.Opt.ActionCategoryKey))
                    .Sum(x => x.Opt.CollectionImpactAmount),
                OverdueExposure = collectionSnapshot?.OverdueExposure ?? overdueExposure,
                DueWithin7Days = dueWithin7Total,
                RecoveryVsBillingPercent = collectionSnapshot?.RecoveryVsBillingPercent,
                DeferNoActionCount = CountByCategory(processed, CollectionOptimizationPolicy.ActionDeferCollection) +
                    CountByCategory(processed, CollectionOptimizationPolicy.ActionNoActionToday),
                PlanningConfidence = CollectionOptimizationPolicy.ResolvePlanningConfidence(daysElapsed),
                CollectionContextUnavailable = collectionUnavailable
            };

            kpi.ExecutiveSummaryText = CollectionOptimizationExecutiveSummaryBuilder.Build(
                kpi,
                priorityQueue.FirstOrDefault(),
                today,
                daysElapsed,
                collectionUnavailable);

            return new DashboardCollectionOptimizationAggregateResult
            {
                GeneratedAt = generatedAt,
                BusinessDate = today,
                DaysElapsed = daysElapsed,
                Kpi = kpi,
                ActionDistribution = actionDistribution,
                Workload = workload,
                PriorityQueue = priorityQueue,
                SpecializedQueues = specializedQueues,
                TopImpactOpportunities = impactRows,
                ContextsByKey = processed.ToDictionary(
                    x => x.Opt.CustomerKey,
                    x => x.Opt,
                    StringComparer.OrdinalIgnoreCase)
            };
        }

        private static List<DashboardCollectionOptimizationQueueRow> BuildSpecializedQueues(
            List<(CustomerRiskForecastContext Forecast, CollectionOptimizationContext Opt)> actionable,
            int maxPerQueue)
        {
            var rows = new List<DashboardCollectionOptimizationQueueRow>();

            void AddQueue(string queueKey, string actionCategory)
            {
                var queueItems = actionable
                    .Where(x => string.Equals(x.Opt.ActionCategoryKey, actionCategory, StringComparison.OrdinalIgnoreCase))
                    .Take(maxPerQueue)
                    .Select((x, i) => CollectionOptimizationActionBuilder.ToQueueRow(queueKey, x.Forecast, x.Opt, i + 1));
                rows.AddRange(queueItems);
            }

            AddQueue(QueueProactiveReminder, CollectionOptimizationPolicy.ActionProactiveReminder);
            AddQueue(QueueCreditReview, CollectionOptimizationPolicy.ActionCreditReview);
            AddQueue(QueueSalesRecovery, CollectionOptimizationPolicy.ActionSalesRecoveryVisit);
            AddQueue(QueueEscalateManagement, CollectionOptimizationPolicy.ActionEscalateManagement);

            return rows;
        }

        private static List<DashboardCollectionOptimizationWorkloadRow> BuildWorkload(
            List<(CustomerRiskForecastContext Forecast, CollectionOptimizationContext Opt)> actionable,
            HashSet<string> hotspotWilayah,
            int maxRows)
        {
            var rows = new List<DashboardCollectionOptimizationWorkloadRow>();

            rows.AddRange(BuildWorkloadGroup(
                actionable,
                WorkloadSalesman,
                x => NormalizeKey(x.Forecast.SalesPersonName),
                x => x.Forecast.SalesPersonName,
                _ => false,
                maxRows));

            rows.AddRange(BuildWorkloadGroup(
                actionable,
                WorkloadWilayah,
                x => NormalizeKey(x.Forecast.WilayahName),
                x => x.Forecast.WilayahName,
                x => hotspotWilayah.Contains(NormalizeKey(x.Forecast.WilayahName)),
                maxRows));

            rows.AddRange(BuildWorkloadGroup(
                actionable,
                WorkloadKlasifikasi,
                x => NormalizeKey(x.Opt.Klasifikasi),
                x => string.IsNullOrWhiteSpace(x.Opt.Klasifikasi) ? "Unclassified" : x.Opt.Klasifikasi,
                _ => false,
                maxRows));

            return rows;
        }

        private static IEnumerable<DashboardCollectionOptimizationWorkloadRow> BuildWorkloadGroup(
            List<(CustomerRiskForecastContext Forecast, CollectionOptimizationContext Opt)> actionable,
            string workloadType,
            Func<(CustomerRiskForecastContext Forecast, CollectionOptimizationContext Opt), string> keySelector,
            Func<(CustomerRiskForecastContext Forecast, CollectionOptimizationContext Opt), string> labelSelector,
            Func<(CustomerRiskForecastContext Forecast, CollectionOptimizationContext Opt), bool> isHotspot,
            int maxRows)
        {
            return actionable
                .GroupBy(keySelector, StringComparer.OrdinalIgnoreCase)
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .Select(g => new
                {
                    Key = g.Key,
                    Label = labelSelector(g.First()),
                    ActionCount = g.Count(),
                    ImmediateCount = g.Count(x => string.Equals(
                        x.Opt.ActionCategoryKey,
                        CollectionOptimizationPolicy.ActionImmediateCollection,
                        StringComparison.OrdinalIgnoreCase)),
                    ImpactTotal = g.Sum(x => x.Opt.CollectionImpactAmount),
                    OverdueExposure = g.Sum(x => x.Forecast.OverdueBalance),
                    IsHotspot = g.Any(isHotspot)
                })
                .OrderByDescending(x => x.ActionCount)
                .ThenByDescending(x => x.ImpactTotal)
                .Take(maxRows)
                .Select((x, i) => new DashboardCollectionOptimizationWorkloadRow
                {
                    WorkloadType = workloadType,
                    EntityKey = x.Key,
                    EntityLabel = x.Label,
                    ActionCount = x.ActionCount,
                    ImmediateCount = x.ImmediateCount,
                    ImpactTotal = x.ImpactTotal,
                    OverdueExposure = x.OverdueExposure,
                    IsHotspot = x.IsHotspot,
                    SortOrder = i + 1
                });
        }

        private static int CountByCategory(
            List<(CustomerRiskForecastContext Forecast, CollectionOptimizationContext Opt)> processed,
            string actionCategory) =>
            processed.Count(x => string.Equals(x.Opt.ActionCategoryKey, actionCategory, StringComparison.OrdinalIgnoreCase));

        private static HashSet<string> BuildTopOmzetKeys(IEnumerable<FakturView> fakturRows, int take)
        {
            return (fakturRows ?? Enumerable.Empty<FakturView>())
                .GroupBy(r => DashboardCustomerKeyResolver.ResolveCodeFirst(r.CustomerCode, r.Customer), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Key.Length > 0)
                .Select(g => new { Key = g.Key, Omzet = g.Sum(x => x.Total) })
                .OrderByDescending(x => x.Omzet)
                .Take(take)
                .Select(x => x.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string> BuildKlasifikasiLookup(IEnumerable<CustomerModel> customers)
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var customer in customers ?? Enumerable.Empty<CustomerModel>())
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(customer.CustomerCode, customer.CustomerName);
                if (key.Length == 0)
                    continue;
                lookup[key] = customer.KlasifikasiName?.Trim() ?? string.Empty;
            }

            return lookup;
        }

        private static HashSet<string> BuildCustomerAttentionKeys(
            DashboardCollectionAggregateResult collectionSnapshot,
            string signalKey)
        {
            if (collectionSnapshot?.AttentionList is null)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return collectionSnapshot.AttentionList
                .Where(a => string.Equals(a.EntityType, DashboardCollectionAggregator.EntityTypeCustomer, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(a.SignalKey, signalKey, StringComparison.OrdinalIgnoreCase))
                .Select(a => DashboardCustomerKeyResolver.ResolveCodeFirst(a.EntityCode ?? a.EntityId, a.EntityName))
                .Where(k => k.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> BuildLowRecoverySalesmen(DashboardCollectionAggregateResult collectionSnapshot)
        {
            if (collectionSnapshot?.AttentionList is null)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return collectionSnapshot.AttentionList
                .Where(a => string.Equals(a.EntityType, DashboardCollectionAggregator.EntityTypeSalesman, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(a.SignalKey, DashboardCollectionAggregator.SignalLowRecoveryVsBilling, StringComparison.OrdinalIgnoreCase))
                .Select(a => NormalizeKey(a.EntityName))
                .Where(k => k.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> BuildHotspotWilayah(DashboardCollectionAggregateResult collectionSnapshot)
        {
            if (collectionSnapshot?.AttentionList is null)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return collectionSnapshot.AttentionList
                .Where(a => string.Equals(a.EntityType, DashboardCollectionAggregator.EntityTypeWilayah, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(a.SignalKey, DashboardCollectionAggregator.SignalWilayahHotspot, StringComparison.OrdinalIgnoreCase))
                .Select(a => NormalizeKey(a.EntityName))
                .Where(k => k.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, DueAmountMetrics> BuildDueAmountsByKey(
            IEnumerable<PiutangOpenBalanceDto> piutangRows,
            DateTime today)
        {
            var metrics = new Dictionary<string, DueAmountMetrics>(StringComparer.OrdinalIgnoreCase);
            var end7 = today.AddDays(7);
            var end14 = today.AddDays(14);

            foreach (var row in piutangRows ?? Enumerable.Empty<PiutangOpenBalanceDto>())
            {
                var key = DashboardCustomerKeyResolver.ResolveCodeFirst(row.CustomerCode, row.CustomerName);
                if (key.Length == 0)
                    continue;

                if (!metrics.TryGetValue(key, out var item))
                    item = new DueAmountMetrics();

                var dueDate = row.JatuhTempo.Date;
                if (dueDate > today && dueDate <= end7)
                    item.DueWithin7Days += row.KurangBayar;
                if (dueDate > today && dueDate <= end14)
                    item.DueWithin14Days += row.KurangBayar;

                metrics[key] = item;
            }

            return metrics;
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

        private sealed class DueAmountMetrics
        {
            public decimal DueWithin7Days { get; set; }

            public decimal DueWithin14Days { get; set; }
        }
    }
}
