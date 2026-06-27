using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Contracts;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Models;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Services;
using btr.application.SalesContext.SalesPersonAgg.Contracts;
using btr.application.SalesContext.VisitPlanAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.VisitPlanAgg;

namespace btr.application.ReportingContext.DashboardFieldActivityAgg.Services
{
    public class FieldActivityComposer
    {
        private readonly ISalesPersonDal _salesPersonDal;
        private readonly IEffectiveVisitPlanDal _effectiveVisitPlanDal;
        private readonly IFieldActivityCheckInDal _checkInDal;
        private readonly IFieldActivityOrderDal _orderDal;
        private readonly ICustomerCoordinateDal _customerCoordinateDal;
        private readonly FieldActivityOptions _options;

        public FieldActivityComposer(
            ISalesPersonDal salesPersonDal,
            IEffectiveVisitPlanDal effectiveVisitPlanDal,
            IFieldActivityCheckInDal checkInDal,
            IFieldActivityOrderDal orderDal,
            ICustomerCoordinateDal customerCoordinateDal,
            FieldActivityOptions options)
        {
            _salesPersonDal = salesPersonDal;
            _effectiveVisitPlanDal = effectiveVisitPlanDal;
            _checkInDal = checkInDal;
            _orderDal = orderDal;
            _customerCoordinateDal = customerCoordinateDal;
            _options = options ?? new FieldActivityOptions();
        }

        public FieldActivityResponse Compose(string salesPersonId, DateTime visitDate)
        {
            if (string.IsNullOrWhiteSpace(salesPersonId))
                throw new ArgumentException("salesPersonId is required.");

            var visitDay = visitDate.Date;
            var salesman = _salesPersonDal.GetData(new SalesPersonModel(salesPersonId));
            if (salesman == null)
                throw new KeyNotFoundException($"Salesperson '{salesPersonId}' was not found.");

            if (string.IsNullOrWhiteSpace(salesman.Email))
                throw new ArgumentException("Selected salesperson has no Email configured; field activity data cannot be loaded.");

            var planDataAvailable = visitDay >= _options.VisitPlanGoLiveDate.Date;
            var effectivePlan = planDataAvailable
                ? (_effectiveVisitPlanDal.ListEffectivePlan(salesPersonId, visitDay)
                    ?? Enumerable.Empty<EffectiveVisitPlanEntry>()).ToList()
                : new List<EffectiveVisitPlanEntry>();

            var plannedCustomerIds = effectivePlan
                .Select(x => x.CustomerId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var customerCoordinates = _customerCoordinateDal.ListByCustomerIds(plannedCustomerIds)
                ?? new Dictionary<string, CustomerCoordinateRow>();

            var checkIns = _checkInDal.ListBySalesPersonDate(salesman.Email, visitDay)
                ?? Array.Empty<FieldActivityCheckInRow>();

            var orderCustomerIds = _orderDal.ListCustomerIdsWithOrder(salesman.Email, visitDay)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var plannedSet = new HashSet<string>(
                plannedCustomerIds,
                StringComparer.OrdinalIgnoreCase);

            var actualSet = new HashSet<string>(
                checkIns.Select(x => x.CustomerId).Where(x => !string.IsNullOrWhiteSpace(x)),
                StringComparer.OrdinalIgnoreCase);

            var missedSet = plannedSet.Except(actualSet, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var unplannedSet = actualSet.Except(plannedSet, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var visitedPlannedSet = plannedSet.Intersect(actualSet, StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var plannedStops = BuildPlannedStops(effectivePlan, customerCoordinates, visitedPlannedSet, missedSet);
            var actualStops = BuildActualStops(checkIns, plannedSet, unplannedSet, orderCustomerIds);
            var missedVisits = BuildMissedVisits(effectivePlan, customerCoordinates, missedSet);

            var plannedWithCoords = plannedStops.Count(x => x.HasCoordinates);
            var plannedCount = plannedStops.Count;

            var kpiInput = FieldActivityKpiCalculator.Compute(
                effectivePlan,
                checkIns,
                orderCustomerIds,
                BuildOrderRowsFromCustomerIds(orderCustomerIds));

            var kpis = new FieldActivityKpis
            {
                PlannedVisits = kpiInput.PlannedVisits,
                ActualVisits = kpiInput.ActualVisits,
                EffectiveCalls = kpiInput.EffectiveCalls,
                MissedVisits = kpiInput.MissedVisits,
                UnplannedVisits = kpiInput.UnplannedVisits,
                VisitExecutionPercent = kpiInput.VisitExecutionPercent,
                EffectiveCallRate = kpiInput.EffectiveCallRate,
                CoordinateCoveragePercent = plannedCount == 0
                    ? (double?)null
                    : Math.Round(plannedWithCoords * 100.0 / plannedCount, 1),
                GpsValidCount = kpiInput.GpsValidCount,
                GpsWarningCount = kpiInput.GpsWarningCount,
                GpsSuspiciousCount = kpiInput.GpsSuspiciousCount
            };

            return new FieldActivityResponse
            {
                SalesPersonId = salesman.SalesPersonId,
                SalesPersonName = salesman.SalesPersonName,
                VisitDate = visitDay.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Kpis = kpis,
                PlannedStops = plannedStops,
                ActualStops = actualStops,
                MissedVisits = missedVisits,
                RouteGeometry = BuildRouteGeometry(plannedStops, actualStops),
                Meta = new FieldActivityMeta
                {
                    PlanDataAvailable = planDataAvailable,
                    VisitPlanGoLiveDate = _options.VisitPlanGoLiveDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    QueriedAt = DateTime.Now
                }
            };
        }

        private static IList<FieldActivityPlannedStop> BuildPlannedStops(
            IList<EffectiveVisitPlanEntry> effectivePlan,
            IReadOnlyDictionary<string, CustomerCoordinateRow> customerCoordinates,
            ISet<string> visitedPlannedSet,
            ISet<string> missedSet)
        {
            return effectivePlan
                .OrderBy(x => x.NoUrut)
                .ThenBy(x => x.CustomerId, StringComparer.OrdinalIgnoreCase)
                .Select(entry =>
                {
                    customerCoordinates.TryGetValue(entry.CustomerId, out var coords);
                    var latitude = coords?.Latitude ?? 0;
                    var longitude = coords?.Longitude ?? 0;
                    var hasCoordinates = GpsValidationClassifier.HasNonZeroCoordinates(latitude, longitude);
                    var visitStatus = missedSet.Contains(entry.CustomerId)
                        ? "Missed"
                        : visitedPlannedSet.Contains(entry.CustomerId)
                            ? "Visited"
                            : "Missed";

                    return new FieldActivityPlannedStop
                    {
                        CustomerId = entry.CustomerId,
                        CustomerCode = entry.CustomerCode,
                        CustomerName = entry.CustomerName,
                        NoUrut = entry.NoUrut,
                        Latitude = latitude,
                        Longitude = longitude,
                        HasCoordinates = hasCoordinates,
                        VisitStatus = visitStatus
                    };
                })
                .ToList();
        }

        private static IList<FieldActivityActualStop> BuildActualStops(
            IReadOnlyList<FieldActivityCheckInRow> checkIns,
            ISet<string> plannedSet,
            ISet<string> unplannedSet,
            ISet<string> orderCustomerIds)
        {
            var stops = new List<FieldActivityActualStop>();
            var sequence = 1;

            foreach (var checkIn in checkIns)
            {
                var isUnplanned = unplannedSet.Contains(checkIn.CustomerId);
                var gpsClass = GpsValidationClassifier.Classify(
                    checkIn.CheckInLatitude,
                    checkIn.CheckInLongitude,
                    checkIn.CustomerLatitude,
                    checkIn.CustomerLongitude,
                    checkIn.Accuracy);

                stops.Add(new FieldActivityActualStop
                {
                    CustomerId = checkIn.CustomerId,
                    CustomerCode = checkIn.CustomerCode,
                    CustomerName = checkIn.CustomerName,
                    Sequence = sequence++,
                    CheckInTime = checkIn.CheckInTime,
                    Latitude = checkIn.CheckInLatitude,
                    Longitude = checkIn.CheckInLongitude,
                    HasCoordinates = GpsValidationClassifier.HasNonZeroCoordinates(
                        checkIn.CheckInLatitude,
                        checkIn.CheckInLongitude),
                    VisitStatus = isUnplanned ? "Unplanned" : "Visited",
                    IsEffectiveCall = orderCustomerIds.Contains(checkIn.CustomerId),
                    GpsValidation = gpsClass.ToString(),
                    DistanceMeters = GpsValidationClassifier.DistanceMeters(
                        checkIn.CheckInLatitude,
                        checkIn.CheckInLongitude,
                        checkIn.CustomerLatitude,
                        checkIn.CustomerLongitude)
                });
            }

            return stops;
        }

        private static IList<FieldActivityMissedVisit> BuildMissedVisits(
            IList<EffectiveVisitPlanEntry> effectivePlan,
            IReadOnlyDictionary<string, CustomerCoordinateRow> customerCoordinates,
            ISet<string> missedSet)
        {
            return effectivePlan
                .Where(x => missedSet.Contains(x.CustomerId))
                .OrderBy(x => x.NoUrut)
                .ThenBy(x => x.CustomerId, StringComparer.OrdinalIgnoreCase)
                .Select(entry =>
                {
                    customerCoordinates.TryGetValue(entry.CustomerId, out var coords);
                    var latitude = coords?.Latitude ?? 0;
                    var longitude = coords?.Longitude ?? 0;

                    return new FieldActivityMissedVisit
                    {
                        CustomerId = entry.CustomerId,
                        CustomerCode = entry.CustomerCode,
                        CustomerName = entry.CustomerName,
                        NoUrut = entry.NoUrut,
                        HasCoordinates = GpsValidationClassifier.HasNonZeroCoordinates(latitude, longitude)
                    };
                })
                .ToList();
        }

        private static IReadOnlyList<FieldActivityOrderBatchRow> BuildOrderRowsFromCustomerIds(
            ISet<string> orderCustomerIds)
        {
            return orderCustomerIds
                .Select(id => new FieldActivityOrderBatchRow
                {
                    CustomerId = id,
                    TotalAmount = 0
                })
                .ToList();
        }

        private static FieldActivityRouteGeometry BuildRouteGeometry(
            IList<FieldActivityPlannedStop> plannedStops,
            IList<FieldActivityActualStop> actualStops)
        {
            return new FieldActivityRouteGeometry
            {
                Planned = new FieldActivityGeoJsonLine
                {
                    Type = "LineString",
                    Coordinates = plannedStops
                        .Where(x => x.HasCoordinates)
                        .Select(x => (IList<double>)new List<double> { x.Longitude, x.Latitude })
                        .ToList()
                },
                Actual = new FieldActivityGeoJsonLine
                {
                    Type = "LineString",
                    Coordinates = actualStops
                        .Where(x => x.HasCoordinates)
                        .Select(x => (IList<double>)new List<double> { x.Longitude, x.Latitude })
                        .ToList()
                }
            };
        }
    }
}
