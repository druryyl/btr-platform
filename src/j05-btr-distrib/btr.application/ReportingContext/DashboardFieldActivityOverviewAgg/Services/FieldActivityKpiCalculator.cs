using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Contracts;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Models;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Services;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Models;
using btr.domain.SalesContext.VisitPlanAgg;

namespace btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Services
{
    public static class FieldActivityKpiCalculator
    {
        public static FieldActivityKpiInputResult Compute(
            IReadOnlyList<EffectiveVisitPlanEntry> plan,
            IReadOnlyList<FieldActivityCheckInRow> checkIns,
            ISet<string> orderCustomerIds,
            IReadOnlyList<FieldActivityOrderBatchRow> orders)
        {
            plan = plan ?? Array.Empty<EffectiveVisitPlanEntry>();
            checkIns = checkIns ?? Array.Empty<FieldActivityCheckInRow>();
            orderCustomerIds = orderCustomerIds ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            orders = orders ?? Array.Empty<FieldActivityOrderBatchRow>();

            var plannedCustomerIds = plan
                .Select(x => x.CustomerId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var plannedSet = new HashSet<string>(plannedCustomerIds, StringComparer.OrdinalIgnoreCase);
            var actualSet = new HashSet<string>(
                checkIns.Select(x => x.CustomerId).Where(x => !string.IsNullOrWhiteSpace(x)),
                StringComparer.OrdinalIgnoreCase);

            var missedCount = plannedSet.Except(actualSet, StringComparer.OrdinalIgnoreCase).Count();
            var unplannedCount = actualSet.Except(plannedSet, StringComparer.OrdinalIgnoreCase).Count();

            var effectiveCallCount = checkIns
                .Select(x => x.CustomerId)
                .Where(id => !string.IsNullOrWhiteSpace(id) && orderCustomerIds.Contains(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            var plannedCount = plan.Count;
            var actualCount = checkIns.Count;

            var gpsValid = 0;
            var gpsWarning = 0;
            var gpsSuspicious = 0;

            foreach (var checkIn in checkIns)
            {
                var gpsClass = GpsValidationClassifier.Classify(
                    checkIn.CheckInLatitude,
                    checkIn.CheckInLongitude,
                    checkIn.CustomerLatitude,
                    checkIn.CustomerLongitude,
                    checkIn.Accuracy);

                switch (gpsClass)
                {
                    case GpsValidationClass.Valid:
                        gpsValid++;
                        break;
                    case GpsValidationClass.Warning:
                        gpsWarning++;
                        break;
                    case GpsValidationClass.Suspicious:
                        gpsSuspicious++;
                        break;
                }
            }

            var classifiableGps = gpsValid + gpsWarning + gpsSuspicious;
            var ordersCount = orders.Count;
            var omzetAmount = orders.Sum(x => x.TotalAmount);

            return new FieldActivityKpiInputResult
            {
                PlannedVisits = plannedCount,
                ActualVisits = actualCount,
                EffectiveCalls = effectiveCallCount,
                MissedVisits = missedCount,
                UnplannedVisits = unplannedCount,
                VisitExecutionPercent = plannedCount == 0
                    ? (double?)null
                    : Math.Round(actualCount * 100.0 / plannedCount, 1),
                EffectiveCallRate = actualCount == 0
                    ? (double?)null
                    : Math.Round(effectiveCallCount * 100.0 / actualCount, 1),
                GpsValidCount = gpsValid,
                GpsWarningCount = gpsWarning,
                GpsSuspiciousCount = gpsSuspicious,
                GpsValidPercent = classifiableGps == 0
                    ? (double?)null
                    : Math.Round(gpsValid * 100.0 / classifiableGps, 1),
                OrdersCount = ordersCount,
                OmzetAmount = omzetAmount
            };
        }
    }
}
