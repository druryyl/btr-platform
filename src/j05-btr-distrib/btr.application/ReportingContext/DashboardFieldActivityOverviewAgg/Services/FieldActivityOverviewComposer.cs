using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Services;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Models;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.SalesContext.VisitPlanAgg.Services;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.VisitPlanAgg;

namespace btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Services
{
    public class FieldActivityOverviewComposer
    {
        private readonly ISalesPersonDal _salesPersonDal;
        private readonly IFieldActivityBatchCheckInDal _batchCheckInDal;
        private readonly IFieldActivityBatchOrderDal _batchOrderDal;
        private readonly IFieldActivityBatchVisitPlanDal _batchVisitPlanDal;
        private readonly IEffectiveVisitPlanResolver _visitPlanResolver;
        private readonly FieldActivityOptions _options;

        public FieldActivityOverviewComposer(
            ISalesPersonDal salesPersonDal,
            IFieldActivityBatchCheckInDal batchCheckInDal,
            IFieldActivityBatchOrderDal batchOrderDal,
            IFieldActivityBatchVisitPlanDal batchVisitPlanDal,
            IEffectiveVisitPlanResolver visitPlanResolver,
            FieldActivityOptions options)
        {
            _salesPersonDal = salesPersonDal;
            _batchCheckInDal = batchCheckInDal;
            _batchOrderDal = batchOrderDal;
            _batchVisitPlanDal = batchVisitPlanDal;
            _visitPlanResolver = visitPlanResolver;
            _options = options ?? new FieldActivityOptions();
        }

        public FieldActivityOverviewResponse Compose(DateTime visitDate, DateTime? queriedAt = null)
        {
            var visitDay = visitDate.Date;
            var aggregate = BuildAggregate(visitDay, queriedAt ?? DateTime.Now, includeTrends: true);
            return MapToResponse(aggregate, "LiveBatch", queriedAt);
        }

        public DashboardFieldActivityAggregateResult BuildAggregate(
            DateTime visitDay,
            DateTime generatedAt,
            bool includeTrends)
        {
            var planDataAvailable = visitDay >= _options.VisitPlanGoLiveDate.Date;
            var salesmen = (_salesPersonDal.ListData() ?? Enumerable.Empty<SalesPersonModel>()).ToList();

            var salesmanRows = BuildSalesmanRowsForDate(visitDay, planDataAvailable, salesmen);
            AssignRanks(salesmanRows);
            var teamKpis = BuildTeamKpis(salesmanRows);
            var wilayahBreakdown = BuildWilayahBreakdown(salesmanRows);

            var trendPoints = includeTrends
                ? BuildTrendPoints(visitDay, planDataAvailable, salesmen)
                : new List<FieldActivityTrendPoint>();

            return new DashboardFieldActivityAggregateResult
            {
                ActivityDate = visitDay,
                GeneratedAt = generatedAt,
                TeamKpis = teamKpis,
                Salesmen = salesmanRows,
                TrendPoints = trendPoints,
                WilayahBreakdown = wilayahBreakdown,
                Meta = new FieldActivityOverviewMeta
                {
                    PlanDataAvailable = planDataAvailable,
                    VisitPlanGoLiveDate = _options.VisitPlanGoLiveDate.ToString(
                        "yyyy-MM-dd", CultureInfo.InvariantCulture)
                }
            };
        }

        public FieldActivityOverviewResponse MapToResponse(
            DashboardFieldActivityAggregateResult aggregate,
            string dataSource,
            DateTime? queriedAt)
        {
            var visitDay = aggregate.ActivityDate;
            var salesmanRows = aggregate.Salesmen?.ToList() ?? new List<FieldActivitySalesmanRow>();
            AssignRanks(salesmanRows);

            var trendPoints = aggregate.TrendPoints?.ToList() ?? new List<FieldActivityTrendPoint>();
            var last7Start = visitDay.AddDays(-6);
            var last30Start = visitDay.AddDays(-29);

            return new FieldActivityOverviewResponse
            {
                VisitDate = visitDay.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                DataSource = dataSource,
                GeneratedAt = string.Equals(dataSource, "Snapshot", StringComparison.OrdinalIgnoreCase)
                    ? aggregate.GeneratedAt
                    : (DateTime?)null,
                QueriedAt = string.Equals(dataSource, "LiveBatch", StringComparison.OrdinalIgnoreCase)
                    ? queriedAt ?? aggregate.GeneratedAt
                    : (DateTime?)null,
                TeamKpis = aggregate.TeamKpis ?? new FieldActivityTeamKpis(),
                Salesmen = salesmanRows,
                Rankings = BuildRankings(salesmanRows),
                Trends = new FieldActivityTrendSection
                {
                    Last7Days = trendPoints
                        .Where(x => ParseTrendDate(x.TrendDate) >= last7Start)
                        .OrderBy(x => x.TrendDate)
                        .ToList(),
                    Last30Days = trendPoints
                        .Where(x => ParseTrendDate(x.TrendDate) >= last30Start)
                        .OrderBy(x => x.TrendDate)
                        .ToList()
                },
                WilayahBreakdown = aggregate.WilayahBreakdown?.ToList()
                    ?? new List<FieldActivityWilayahBreakdownRow>(),
                Meta = aggregate.Meta ?? new FieldActivityOverviewMeta()
            };
        }

        private IList<FieldActivitySalesmanRow> BuildSalesmanRowsForDate(
            DateTime visitDay,
            bool planDataAvailable,
            IList<SalesPersonModel> salesmen)
        {
            var checkIns = _batchCheckInDal.ListByDate(visitDay) ?? Array.Empty<FieldActivityBatchCheckInRow>();
            var orders = _batchOrderDal.ListByDate(visitDay) ?? Array.Empty<FieldActivityOrderBatchRow>();
            var visitPlanBatch = _batchVisitPlanDal.ListByDate(visitDay);

            var checkInsByEmail = checkIns
                .Where(x => !string.IsNullOrWhiteSpace(x.UserEmail))
                .GroupBy(x => x.UserEmail, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<FieldActivityBatchCheckInRow>)g.ToList(),
                    StringComparer.OrdinalIgnoreCase);

            var ordersByEmail = orders
                .Where(x => !string.IsNullOrWhiteSpace(x.UserEmail))
                .GroupBy(x => x.UserEmail, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            var plansBySalesPersonId = BuildEffectivePlansBySalesPerson(
                visitPlanBatch, planDataAvailable);

            var rows = new List<FieldActivitySalesmanRow>();

            foreach (var salesman in salesmen)
            {
                var hasEmail = !string.IsNullOrWhiteSpace(salesman.Email);
                IReadOnlyList<EffectiveVisitPlanEntry> plan = Array.Empty<EffectiveVisitPlanEntry>();
                IReadOnlyList<FieldActivityBatchCheckInRow> salesmanCheckIns = Array.Empty<FieldActivityBatchCheckInRow>();
                IReadOnlyList<FieldActivityOrderBatchRow> salesmanOrders = Array.Empty<FieldActivityOrderBatchRow>();

                if (hasEmail)
                {
                    checkInsByEmail.TryGetValue(salesman.Email, out salesmanCheckIns);
                    salesmanCheckIns = salesmanCheckIns ?? Array.Empty<FieldActivityBatchCheckInRow>();
                    ordersByEmail.TryGetValue(salesman.Email, out var orderList);
                    salesmanOrders = orderList ?? new List<FieldActivityOrderBatchRow>();
                }

                if (planDataAvailable && plansBySalesPersonId.TryGetValue(salesman.SalesPersonId, out var effectivePlan))
                    plan = effectivePlan;

                var orderCustomerIds = new HashSet<string>(
                    salesmanOrders.Select(x => x.CustomerId).Where(x => !string.IsNullOrWhiteSpace(x)),
                    StringComparer.OrdinalIgnoreCase);

                var kpis = hasEmail
                    ? FieldActivityKpiCalculator.Compute(plan, salesmanCheckIns, orderCustomerIds, salesmanOrders)
                    : new FieldActivityKpiInputResult();

                var status = FieldActivityStatusPolicy.Resolve(kpis, hasEmail);

                rows.Add(new FieldActivitySalesmanRow
                {
                    SalesPersonId = salesman.SalesPersonId,
                    SalesPersonCode = salesman.SalesPersonCode,
                    SalesPersonName = salesman.SalesPersonName,
                    WilayahName = salesman.WilayahName,
                    HasEmail = hasEmail,
                    PlannedVisits = kpis.PlannedVisits,
                    ActualVisits = kpis.ActualVisits,
                    VisitExecutionPercent = kpis.VisitExecutionPercent,
                    EffectiveCalls = kpis.EffectiveCalls,
                    EffectiveCallRate = kpis.EffectiveCallRate,
                    MissedVisits = kpis.MissedVisits,
                    UnplannedVisits = kpis.UnplannedVisits,
                    GpsValidPercent = kpis.GpsValidPercent,
                    GpsValidCount = kpis.GpsValidCount,
                    GpsWarningCount = kpis.GpsWarningCount,
                    GpsSuspiciousCount = kpis.GpsSuspiciousCount,
                    OrdersCount = kpis.OrdersCount,
                    OmzetAmount = kpis.OmzetAmount,
                    StatusCode = FieldActivityStatusPolicy.ToStatusCode(status)
                });
            }

            return rows;
        }

        private Dictionary<string, IReadOnlyList<EffectiveVisitPlanEntry>> BuildEffectivePlansBySalesPerson(
            FieldActivityVisitPlanBatchResult visitPlanBatch,
            bool planDataAvailable)
        {
            if (!planDataAvailable || visitPlanBatch == null)
                return new Dictionary<string, IReadOnlyList<EffectiveVisitPlanEntry>>(StringComparer.OrdinalIgnoreCase);

            var basePlans = visitPlanBatch.BasePlans ?? Array.Empty<VisitPlanModel>();
            var exceptions = visitPlanBatch.Exceptions ?? Array.Empty<VisitPlanExceptionModel>();

            var baseBySalesPerson = basePlans
                .GroupBy(x => x.SalesPersonId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.AsEnumerable(), StringComparer.OrdinalIgnoreCase);

            var exceptionsBySalesPerson = exceptions
                .GroupBy(x => x.SalesPersonId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.AsEnumerable(), StringComparer.OrdinalIgnoreCase);

            var salesPersonIds = baseBySalesPerson.Keys
                .Union(exceptionsBySalesPerson.Keys, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var result = new Dictionary<string, IReadOnlyList<EffectiveVisitPlanEntry>>(StringComparer.OrdinalIgnoreCase);

            foreach (var salesPersonId in salesPersonIds)
            {
                baseBySalesPerson.TryGetValue(salesPersonId, out var basePlan);
                exceptionsBySalesPerson.TryGetValue(salesPersonId, out var exceptionPlan);

                var resolved = _visitPlanResolver.Resolve(
                        basePlan ?? Enumerable.Empty<VisitPlanModel>(),
                        exceptionPlan ?? Enumerable.Empty<VisitPlanExceptionModel>())
                    ?.ToList() ?? new List<EffectiveVisitPlanEntry>();

                result[salesPersonId] = resolved;
            }

            return result;
        }

        private static FieldActivityTeamKpis BuildTeamKpis(IList<FieldActivitySalesmanRow> rows)
        {
            var fieldRows = rows.Where(x => x.HasEmail).ToList();

            var activeCount = fieldRows.Count(x => x.PlannedVisits > 0 || x.ActualVisits > 0);
            var planned = fieldRows.Sum(x => x.PlannedVisits);
            var actual = fieldRows.Sum(x => x.ActualVisits);
            var effective = fieldRows.Sum(x => x.EffectiveCalls);
            var missed = fieldRows.Sum(x => x.MissedVisits);
            var unplanned = fieldRows.Sum(x => x.UnplannedVisits);
            var gpsValid = fieldRows.Sum(x => x.GpsValidCount);
            var gpsClassifiable = fieldRows.Sum(x => x.GpsValidCount + x.GpsWarningCount + x.GpsSuspiciousCount);
            var totalOrders = fieldRows.Sum(x => x.OrdersCount);
            var totalOmzet = fieldRows.Sum(x => x.OmzetAmount);

            return new FieldActivityTeamKpis
            {
                ActiveSalesmenCount = activeCount,
                PlannedVisits = planned,
                ActualVisits = actual,
                VisitExecutionPercent = planned == 0
                    ? (double?)null
                    : Math.Round(actual * 100.0 / planned, 1),
                EffectiveCalls = effective,
                EffectiveCallRate = actual == 0
                    ? (double?)null
                    : Math.Round(effective * 100.0 / actual, 1),
                MissedVisits = missed,
                UnplannedVisits = unplanned,
                GpsValidRate = gpsClassifiable == 0
                    ? (double?)null
                    : Math.Round(gpsValid * 100.0 / gpsClassifiable, 1),
                TotalOrders = totalOrders,
                TotalOmzet = totalOmzet
            };
        }

        private FieldActivityTeamKpis BuildTeamKpisForDate(
            DateTime visitDay,
            bool planDataAvailable,
            IList<SalesPersonModel> salesmen)
        {
            var rows = BuildSalesmanRowsForDate(visitDay, planDataAvailable, salesmen);
            return BuildTeamKpis(rows);
        }

        private IList<FieldActivityTrendPoint> BuildTrendPoints(
            DateTime visitDay,
            bool planDataAvailable,
            IList<SalesPersonModel> salesmen)
        {
            var points = new List<FieldActivityTrendPoint>();

            for (var day = visitDay.AddDays(-29); day <= visitDay; day = day.AddDays(1))
            {
                var dayPlanAvailable = day >= _options.VisitPlanGoLiveDate.Date;
                var team = BuildTeamKpisForDate(day, dayPlanAvailable, salesmen);

                points.Add(new FieldActivityTrendPoint
                {
                    TrendDate = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    VisitExecutionPercent = team.VisitExecutionPercent,
                    EffectiveCallRate = team.EffectiveCallRate,
                    OrdersCount = team.TotalOrders,
                    OmzetAmount = team.TotalOmzet
                });
            }

            return points;
        }

        private static IList<FieldActivityWilayahBreakdownRow> BuildWilayahBreakdown(
            IList<FieldActivitySalesmanRow> rows)
        {
            return rows
                .Where(x => x.HasEmail && x.ActualVisits > 0)
                .GroupBy(x => string.IsNullOrWhiteSpace(x.WilayahName) ? "(Unknown)" : x.WilayahName)
                .Select(g => new FieldActivityWilayahBreakdownRow
                {
                    WilayahName = g.Key,
                    ActualVisits = g.Sum(x => x.ActualVisits)
                })
                .OrderByDescending(x => x.ActualVisits)
                .ThenBy(x => x.WilayahName)
                .ToList();
        }

        private static void AssignRanks(IList<FieldActivitySalesmanRow> rows)
        {
            var ranked = rows
                .OrderByDescending(x => x.VisitExecutionPercent ?? -1)
                .ThenBy(x => x.SalesPersonName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (var i = 0; i < ranked.Count; i++)
                ranked[i].Rank = i + 1;
        }

        private static FieldActivityRankingSection BuildRankings(IList<FieldActivitySalesmanRow> rows)
        {
            return new FieldActivityRankingSection
            {
                TopVisitExecution = BuildRankingEntries(
                    rows.Where(x => x.HasEmail && x.PlannedVisits >= 1),
                    x => x.VisitExecutionPercent ?? 0,
                    x => x.VisitExecutionPercent,
                    "Visit Execution %",
                    descending: true),
                BottomVisitExecution = BuildRankingEntries(
                    rows.Where(x => x.HasEmail && x.PlannedVisits >= 1),
                    x => x.VisitExecutionPercent ?? 0,
                    x => x.VisitExecutionPercent,
                    "Visit Execution %",
                    descending: false),
                TopEffectiveCallRate = BuildRankingEntries(
                    rows.Where(x => x.HasEmail && x.ActualVisits >= 1),
                    x => x.EffectiveCallRate ?? 0,
                    x => x.EffectiveCallRate,
                    "Effective Call Rate",
                    descending: true),
                BottomEffectiveCallRate = BuildRankingEntries(
                    rows.Where(x => x.HasEmail && x.ActualVisits >= 1),
                    x => x.EffectiveCallRate ?? 0,
                    x => x.EffectiveCallRate,
                    "Effective Call Rate",
                    descending: false),
                TopOmzet = BuildRankingEntries(
                    rows.Where(x => x.HasEmail),
                    x => (double)x.OmzetAmount,
                    x => (double?)x.OmzetAmount,
                    "Order Value",
                    descending: true),
                TopOrders = BuildRankingEntries(
                    rows.Where(x => x.HasEmail),
                    x => x.OrdersCount,
                    x => (double?)x.OrdersCount,
                    "Orders",
                    descending: true),
                MostMissedVisits = BuildRankingEntries(
                    rows.Where(x => x.HasEmail),
                    x => x.MissedVisits,
                    x => (double?)x.MissedVisits,
                    "Missed Visits",
                    descending: true),
                MostUnplannedVisits = BuildRankingEntries(
                    rows.Where(x => x.HasEmail),
                    x => x.UnplannedVisits,
                    x => (double?)x.UnplannedVisits,
                    "Unplanned Visits",
                    descending: true)
            };
        }

        private static IList<FieldActivityRankingEntry> BuildRankingEntries(
            IEnumerable<FieldActivitySalesmanRow> source,
            Func<FieldActivitySalesmanRow, double> sortKey,
            Func<FieldActivitySalesmanRow, double?> valueSelector,
            string label,
            bool descending)
        {
            var ordered = descending
                ? source.OrderByDescending(sortKey).ThenBy(x => x.SalesPersonName, StringComparer.OrdinalIgnoreCase)
                : source.OrderBy(sortKey).ThenBy(x => x.SalesPersonName, StringComparer.OrdinalIgnoreCase);

            return ordered
                .Take(10)
                .Select((row, index) => new FieldActivityRankingEntry
                {
                    Rank = index + 1,
                    SalesPersonId = row.SalesPersonId,
                    SalesPersonCode = row.SalesPersonCode,
                    SalesPersonName = row.SalesPersonName,
                    PrimaryValue = valueSelector(row),
                    PrimaryLabel = label
                })
                .ToList();
        }

        private static DateTime ParseTrendDate(string trendDate)
        {
            return DateTime.ParseExact(trendDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
