using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardFieldActivityAgg.Models
{
    public class FieldActivityResponse
    {
        public string SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string VisitDate { get; set; }
        public FieldActivityKpis Kpis { get; set; } = new FieldActivityKpis();
        public IList<FieldActivityPlannedStop> PlannedStops { get; set; } = new List<FieldActivityPlannedStop>();
        public IList<FieldActivityActualStop> ActualStops { get; set; } = new List<FieldActivityActualStop>();
        public IList<FieldActivityMissedVisit> MissedVisits { get; set; } = new List<FieldActivityMissedVisit>();
        public FieldActivityRouteGeometry RouteGeometry { get; set; } = new FieldActivityRouteGeometry();
        public FieldActivityMeta Meta { get; set; } = new FieldActivityMeta();
    }

    public class FieldActivityKpis
    {
        public int PlannedVisits { get; set; }
        public int ActualVisits { get; set; }
        public int EffectiveCalls { get; set; }
        public int MissedVisits { get; set; }
        public int UnplannedVisits { get; set; }
        public double? VisitExecutionPercent { get; set; }
        public double? EffectiveCallRate { get; set; }
        public double? CoordinateCoveragePercent { get; set; }
        public int GpsValidCount { get; set; }
        public int GpsWarningCount { get; set; }
        public int GpsSuspiciousCount { get; set; }
    }

    public class FieldActivityPlannedStop
    {
        public string CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public int NoUrut { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool HasCoordinates { get; set; }
        public string VisitStatus { get; set; }
    }

    public class FieldActivityActualStop
    {
        public string CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public int Sequence { get; set; }
        public string CheckInTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool HasCoordinates { get; set; }
        public string VisitStatus { get; set; }
        public bool IsEffectiveCall { get; set; }
        public string GpsValidation { get; set; }
        public double? DistanceMeters { get; set; }
    }

    public class FieldActivityMissedVisit
    {
        public string CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public int NoUrut { get; set; }
        public bool HasCoordinates { get; set; }
    }

    public class FieldActivityRouteGeometry
    {
        public FieldActivityGeoJsonLine Planned { get; set; } = new FieldActivityGeoJsonLine();
        public FieldActivityGeoJsonLine Actual { get; set; } = new FieldActivityGeoJsonLine();
    }

    public class FieldActivityGeoJsonLine
    {
        public string Type { get; set; } = "LineString";
        public IList<IList<double>> Coordinates { get; set; } = new List<IList<double>>();
    }

    public class FieldActivityMeta
    {
        public bool PlanDataAvailable { get; set; }
        public string VisitPlanGoLiveDate { get; set; }
        public DateTime QueriedAt { get; set; }
    }

    public enum GpsValidationClass
    {
        Valid,
        Warning,
        Suspicious,
        Invalid
    }
}
