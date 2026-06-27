using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Models;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardFieldActivityOverviewAgg
{
    public class DashboardFieldActivitySnapshotDal : IDashboardFieldActivitySnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardFieldActivitySnapshotDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardFieldActivityAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, ActivityDate, ActiveSalesmenCount, PlannedVisits, ActualVisits,
       VisitExecutionPercent, EffectiveCalls, EffectiveCallRate, MissedVisits, UnplannedVisits,
       GpsValidRate, TotalOrders, TotalOmzet, LastRefreshLogId
FROM BTRPD_FieldActivityKpi
WHERE SnapshotKey = @SnapshotKey";

            const string salesmanSql = @"
SELECT SalesPersonId, SalesPersonCode, SalesPersonName, WilayahName, HasEmail, Rank,
       PlannedVisits, ActualVisits, VisitExecutionPercent, EffectiveCalls, EffectiveCallRate,
       MissedVisits, UnplannedVisits, GpsValidPercent, GpsValidCount, GpsWarningCount,
       GpsSuspiciousCount, OrdersCount, OmzetAmount, StatusCode
FROM BTRPD_FieldActivitySalesman
WHERE SnapshotKey = @SnapshotKey
ORDER BY Rank, SalesPersonName";

            const string trendSql = @"
SELECT TrendDate, VisitExecutionPercent, EffectiveCallRate, OrdersCount, OmzetAmount
FROM BTRPD_FieldActivityTrend
WHERE SnapshotKey = @SnapshotKey
ORDER BY TrendDate";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi == null)
                    return null;

                var salesmen = conn.Query<SalesmanRow>(salesmanSql, new { SnapshotKey }).ToList();
                var trends = conn.Query<TrendRow>(trendSql, new { SnapshotKey }).ToList();

                return new DashboardFieldActivityAggregateResult
                {
                    ActivityDate = kpi.ActivityDate,
                    GeneratedAt = kpi.GeneratedAt,
                    TeamKpis = new FieldActivityTeamKpis
                    {
                        ActiveSalesmenCount = kpi.ActiveSalesmenCount,
                        PlannedVisits = kpi.PlannedVisits,
                        ActualVisits = kpi.ActualVisits,
                        VisitExecutionPercent = kpi.VisitExecutionPercent,
                        EffectiveCalls = kpi.EffectiveCalls,
                        EffectiveCallRate = kpi.EffectiveCallRate,
                        MissedVisits = kpi.MissedVisits,
                        UnplannedVisits = kpi.UnplannedVisits,
                        GpsValidRate = kpi.GpsValidRate,
                        TotalOrders = kpi.TotalOrders,
                        TotalOmzet = kpi.TotalOmzet
                    },
                    Salesmen = salesmen.Select(MapSalesman).ToList(),
                    TrendPoints = trends.Select(r => new FieldActivityTrendPoint
                    {
                        TrendDate = r.TrendDate.ToString("yyyy-MM-dd"),
                        VisitExecutionPercent = r.VisitExecutionPercent,
                        EffectiveCallRate = r.EffectiveCallRate,
                        OrdersCount = r.OrdersCount,
                        OmzetAmount = r.OmzetAmount
                    }).ToList(),
                    WilayahBreakdown = BuildWilayahFromSalesmen(salesmen.Select(MapSalesman).ToList()),
                    Meta = new FieldActivityOverviewMeta()
                };
            }
        }

        public void ReplaceCurrent(DashboardFieldActivityAggregateResult aggregate, string refreshLogId)
        {
            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    conn.Execute(
                        "DELETE FROM BTRPD_FieldActivityTrend WHERE SnapshotKey = @SnapshotKey",
                        new { SnapshotKey }, trans);
                    conn.Execute(
                        "DELETE FROM BTRPD_FieldActivitySalesman WHERE SnapshotKey = @SnapshotKey",
                        new { SnapshotKey }, trans);
                    conn.Execute(
                        "DELETE FROM BTRPD_FieldActivityKpi WHERE SnapshotKey = @SnapshotKey",
                        new { SnapshotKey }, trans);

                    var team = aggregate.TeamKpis ?? new FieldActivityTeamKpis();
                    conn.Execute(@"
INSERT INTO BTRPD_FieldActivityKpi (
    SnapshotKey, GeneratedAt, ActivityDate, ActiveSalesmenCount, PlannedVisits, ActualVisits,
    VisitExecutionPercent, EffectiveCalls, EffectiveCallRate, MissedVisits, UnplannedVisits,
    GpsValidRate, TotalOrders, TotalOmzet, LastRefreshLogId)
VALUES (
    @SnapshotKey, @GeneratedAt, @ActivityDate, @ActiveSalesmenCount, @PlannedVisits, @ActualVisits,
    @VisitExecutionPercent, @EffectiveCalls, @EffectiveCallRate, @MissedVisits, @UnplannedVisits,
    @GpsValidRate, @TotalOrders, @TotalOmzet, @LastRefreshLogId)",
                        new
                        {
                            SnapshotKey,
                            aggregate.GeneratedAt,
                            aggregate.ActivityDate,
                            team.ActiveSalesmenCount,
                            team.PlannedVisits,
                            team.ActualVisits,
                            team.VisitExecutionPercent,
                            team.EffectiveCalls,
                            team.EffectiveCallRate,
                            team.MissedVisits,
                            team.UnplannedVisits,
                            team.GpsValidRate,
                            team.TotalOrders,
                            team.TotalOmzet,
                            LastRefreshLogId = refreshLogId ?? string.Empty
                        }, trans);

                    var salesmen = aggregate.Salesmen ?? new List<FieldActivitySalesmanRow>();
                    foreach (var row in salesmen)
                    {
                        conn.Execute(@"
INSERT INTO BTRPD_FieldActivitySalesman (
    SnapshotKey, SalesPersonId, SalesPersonCode, SalesPersonName, WilayahName, HasEmail, Rank,
    PlannedVisits, ActualVisits, VisitExecutionPercent, EffectiveCalls, EffectiveCallRate,
    MissedVisits, UnplannedVisits, GpsValidPercent, GpsValidCount, GpsWarningCount,
    GpsSuspiciousCount, OrdersCount, OmzetAmount, StatusCode)
VALUES (
    @SnapshotKey, @SalesPersonId, @SalesPersonCode, @SalesPersonName, @WilayahName, @HasEmail, @Rank,
    @PlannedVisits, @ActualVisits, @VisitExecutionPercent, @EffectiveCalls, @EffectiveCallRate,
    @MissedVisits, @UnplannedVisits, @GpsValidPercent, @GpsValidCount, @GpsWarningCount,
    @GpsSuspiciousCount, @OrdersCount, @OmzetAmount, @StatusCode)",
                            new
                            {
                                SnapshotKey,
                                row.SalesPersonId,
                                SalesPersonCode = row.SalesPersonCode ?? string.Empty,
                                SalesPersonName = row.SalesPersonName ?? string.Empty,
                                WilayahName = row.WilayahName ?? string.Empty,
                                row.HasEmail,
                                row.Rank,
                                row.PlannedVisits,
                                row.ActualVisits,
                                row.VisitExecutionPercent,
                                row.EffectiveCalls,
                                row.EffectiveCallRate,
                                row.MissedVisits,
                                row.UnplannedVisits,
                                row.GpsValidPercent,
                                row.GpsValidCount,
                                row.GpsWarningCount,
                                row.GpsSuspiciousCount,
                                row.OrdersCount,
                                row.OmzetAmount,
                                StatusCode = row.StatusCode ?? string.Empty
                            }, trans);
                    }

                    var trends = aggregate.TrendPoints ?? new List<FieldActivityTrendPoint>();
                    foreach (var trend in trends)
                    {
                        var trendDate = DateTime.ParseExact(
                            trend.TrendDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

                        conn.Execute(@"
INSERT INTO BTRPD_FieldActivityTrend (
    SnapshotKey, TrendDate, VisitExecutionPercent, EffectiveCallRate, OrdersCount, OmzetAmount)
VALUES (
    @SnapshotKey, @TrendDate, @VisitExecutionPercent, @EffectiveCallRate, @OrdersCount, @OmzetAmount)",
                            new
                            {
                                SnapshotKey,
                                TrendDate = trendDate,
                                trend.VisitExecutionPercent,
                                trend.EffectiveCallRate,
                                trend.OrdersCount,
                                trend.OmzetAmount
                            }, trans);
                    }

                    trans.Commit();
                }
            }
        }

        private static FieldActivitySalesmanRow MapSalesman(SalesmanRow row)
        {
            return new FieldActivitySalesmanRow
            {
                SalesPersonId = row.SalesPersonId,
                SalesPersonCode = row.SalesPersonCode,
                SalesPersonName = row.SalesPersonName,
                WilayahName = row.WilayahName,
                HasEmail = row.HasEmail,
                Rank = row.Rank,
                PlannedVisits = row.PlannedVisits,
                ActualVisits = row.ActualVisits,
                VisitExecutionPercent = row.VisitExecutionPercent,
                EffectiveCalls = row.EffectiveCalls,
                EffectiveCallRate = row.EffectiveCallRate,
                MissedVisits = row.MissedVisits,
                UnplannedVisits = row.UnplannedVisits,
                GpsValidPercent = row.GpsValidPercent,
                GpsValidCount = row.GpsValidCount,
                GpsWarningCount = row.GpsWarningCount,
                GpsSuspiciousCount = row.GpsSuspiciousCount,
                OrdersCount = row.OrdersCount,
                OmzetAmount = row.OmzetAmount,
                StatusCode = row.StatusCode
            };
        }

        private static IList<FieldActivityWilayahBreakdownRow> BuildWilayahFromSalesmen(
            IList<FieldActivitySalesmanRow> salesmen)
        {
            return salesmen
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

        private sealed class KpiRow
        {
            public DateTime GeneratedAt { get; set; }
            public DateTime ActivityDate { get; set; }
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

        private sealed class SalesmanRow
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

        private sealed class TrendRow
        {
            public DateTime TrendDate { get; set; }
            public double? VisitExecutionPercent { get; set; }
            public double? EffectiveCallRate { get; set; }
            public int OrdersCount { get; set; }
            public decimal OmzetAmount { get; set; }
        }
    }
}
