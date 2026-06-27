using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Models
{
    public class FieldActivityOverviewResponse
    {
        public string VisitDate { get; set; }
        public string DataSource { get; set; }
        public DateTime? GeneratedAt { get; set; }
        public DateTime? QueriedAt { get; set; }
        public FieldActivityTeamKpis TeamKpis { get; set; } = new FieldActivityTeamKpis();
        public IList<FieldActivitySalesmanRow> Salesmen { get; set; } = new List<FieldActivitySalesmanRow>();
        public FieldActivityRankingSection Rankings { get; set; } = new FieldActivityRankingSection();
        public FieldActivityTrendSection Trends { get; set; } = new FieldActivityTrendSection();
        public IList<FieldActivityWilayahBreakdownRow> WilayahBreakdown { get; set; }
            = new List<FieldActivityWilayahBreakdownRow>();
        public FieldActivityOverviewMeta Meta { get; set; } = new FieldActivityOverviewMeta();
    }

    public class FieldActivityTeamKpis
    {
        public int ActiveSalesmenCount { get; set; }
        public int PlannedVisits { get; set; }
        public int ActualVisits { get; set; }
        public double? VisitExecutionPercent { get; set; }
        public int EffectiveCalls { get; set; }
        public double? EffectiveCallRate { get; set; }
        public int MissedVisits { get; set; }
        public int UnplannedVisits { get; set; }
        public double? GpsValidRate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalOmzet { get; set; }
    }

    public class FieldActivitySalesmanRow
    {
        public string SalesPersonId { get; set; }
        public string SalesPersonCode { get; set; }
        public string SalesPersonName { get; set; }
        public string WilayahName { get; set; }
        public bool HasEmail { get; set; }
        public int Rank { get; set; }
        public int PlannedVisits { get; set; }
        public int ActualVisits { get; set; }
        public double? VisitExecutionPercent { get; set; }
        public int EffectiveCalls { get; set; }
        public double? EffectiveCallRate { get; set; }
        public int MissedVisits { get; set; }
        public int UnplannedVisits { get; set; }
        public double? GpsValidPercent { get; set; }
        public int GpsValidCount { get; set; }
        public int GpsWarningCount { get; set; }
        public int GpsSuspiciousCount { get; set; }
        public int OrdersCount { get; set; }
        public decimal OmzetAmount { get; set; }
        public string StatusCode { get; set; }
    }

    public class FieldActivityRankingSection
    {
        public IList<FieldActivityRankingEntry> TopVisitExecution { get; set; } = new List<FieldActivityRankingEntry>();
        public IList<FieldActivityRankingEntry> BottomVisitExecution { get; set; } = new List<FieldActivityRankingEntry>();
        public IList<FieldActivityRankingEntry> TopEffectiveCallRate { get; set; } = new List<FieldActivityRankingEntry>();
        public IList<FieldActivityRankingEntry> BottomEffectiveCallRate { get; set; } = new List<FieldActivityRankingEntry>();
        public IList<FieldActivityRankingEntry> TopOmzet { get; set; } = new List<FieldActivityRankingEntry>();
        public IList<FieldActivityRankingEntry> TopOrders { get; set; } = new List<FieldActivityRankingEntry>();
        public IList<FieldActivityRankingEntry> MostMissedVisits { get; set; } = new List<FieldActivityRankingEntry>();
        public IList<FieldActivityRankingEntry> MostUnplannedVisits { get; set; } = new List<FieldActivityRankingEntry>();
    }

    public class FieldActivityRankingEntry
    {
        public int Rank { get; set; }
        public string SalesPersonId { get; set; }
        public string SalesPersonCode { get; set; }
        public string SalesPersonName { get; set; }
        public double? PrimaryValue { get; set; }
        public string PrimaryLabel { get; set; }
    }

    public class FieldActivityTrendSection
    {
        public IList<FieldActivityTrendPoint> Last7Days { get; set; } = new List<FieldActivityTrendPoint>();
        public IList<FieldActivityTrendPoint> Last30Days { get; set; } = new List<FieldActivityTrendPoint>();
    }

    public class FieldActivityTrendPoint
    {
        public string TrendDate { get; set; }
        public double? VisitExecutionPercent { get; set; }
        public double? EffectiveCallRate { get; set; }
        public int OrdersCount { get; set; }
        public decimal OmzetAmount { get; set; }
    }

    public class FieldActivityWilayahBreakdownRow
    {
        public string WilayahName { get; set; }
        public int ActualVisits { get; set; }
    }

    public class FieldActivityOverviewMeta
    {
        public bool PlanDataAvailable { get; set; }
        public string VisitPlanGoLiveDate { get; set; }
    }

    public class FieldActivityKpiInputResult
    {
        public int PlannedVisits { get; set; }
        public int ActualVisits { get; set; }
        public int EffectiveCalls { get; set; }
        public int MissedVisits { get; set; }
        public int UnplannedVisits { get; set; }
        public double? VisitExecutionPercent { get; set; }
        public double? EffectiveCallRate { get; set; }
        public int GpsValidCount { get; set; }
        public int GpsWarningCount { get; set; }
        public int GpsSuspiciousCount { get; set; }
        public int OrdersCount { get; set; }
        public decimal OmzetAmount { get; set; }
        public double? GpsValidPercent { get; set; }
    }

    public enum FieldActivitySalesmanStatus
    {
        OnTrack,
        NeedsAttention,
        Critical,
        NoPlan,
        NoFieldData
    }

    public class DashboardFieldActivityAggregateResult
    {
        public DateTime ActivityDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public FieldActivityTeamKpis TeamKpis { get; set; } = new FieldActivityTeamKpis();
        public IList<FieldActivitySalesmanRow> Salesmen { get; set; } = new List<FieldActivitySalesmanRow>();
        public IList<FieldActivityTrendPoint> TrendPoints { get; set; } = new List<FieldActivityTrendPoint>();
        public IList<FieldActivityWilayahBreakdownRow> WilayahBreakdown { get; set; }
            = new List<FieldActivityWilayahBreakdownRow>();
        public FieldActivityOverviewMeta Meta { get; set; } = new FieldActivityOverviewMeta();
    }
}
