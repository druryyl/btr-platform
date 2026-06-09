using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardInventoryAgg.Queries;
using btr.application.ReportingContext.Shared;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.infrastructure.Helpers;
using btr.nuna.Application;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardInventorySnapshotDal : IDashboardInventorySnapshotDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;
        private readonly INunaCounterBL _counter;

        public DashboardInventorySnapshotDal(
            IOptions<DatabaseOptions> opt,
            INunaCounterBL counter)
        {
            _opt = opt.Value;
            _counter = counter;
        }

        public DashboardInventoryAggregateResult GetCurrent()
        {
            const string kpiSql = @"
SELECT SnapshotKey, GeneratedAt, TotalInventoryValue, TotalItem, LastRefreshLogId
FROM BTR_PortalDashboardInventoryKpi
WHERE SnapshotKey = @SnapshotKey";

            const string breakdownSql = @"
SELECT DimensionType, Name, InventoryValue, IsTop10, Top10Rank
FROM BTR_PortalDashboardInventoryBreakdown
WHERE SnapshotKey = @SnapshotKey
ORDER BY DimensionType, InventoryValue DESC, Name";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<KpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                    return null;

                var breakdownRows = conn.Query<BreakdownRow>(breakdownSql, new { SnapshotKey }).ToList();

                return new DashboardInventoryAggregateResult
                {
                    TotalInventoryValue = kpi.TotalInventoryValue,
                    TotalItem = kpi.TotalItem,
                    GeneratedAt = kpi.GeneratedAt,
                    Breakdown = breakdownRows.Select(r => new DashboardInventoryBreakdownRow
                    {
                        DimensionType = r.DimensionType,
                        Name = r.Name,
                        InventoryValue = r.InventoryValue,
                        IsTop10 = r.IsTop10,
                        Top10Rank = r.Top10Rank
                    }).ToList()
                };
            }
        }

        public void ReplaceCurrent(DashboardInventoryAggregateResult result, string refreshLogId)
        {
            if (result is null)
                throw new System.ArgumentNullException(nameof(result));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();

                conn.Execute(
                    "DELETE FROM BTR_PortalDashboardInventoryBreakdown WHERE SnapshotKey = @SnapshotKey",
                    new { SnapshotKey });

                const string mergeKpiSql = @"
MERGE BTR_PortalDashboardInventoryKpi AS target
USING (SELECT @SnapshotKey AS SnapshotKey) AS source
ON target.SnapshotKey = source.SnapshotKey
WHEN MATCHED THEN
    UPDATE SET
        GeneratedAt = @GeneratedAt,
        TotalInventoryValue = @TotalInventoryValue,
        TotalItem = @TotalItem,
        LastRefreshLogId = @LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (SnapshotKey, GeneratedAt, TotalInventoryValue, TotalItem, LastRefreshLogId)
    VALUES (@SnapshotKey, @GeneratedAt, @TotalInventoryValue, @TotalItem, @LastRefreshLogId);";

                conn.Execute(mergeKpiSql, new
                {
                    SnapshotKey,
                    result.GeneratedAt,
                    result.TotalInventoryValue,
                    result.TotalItem,
                    LastRefreshLogId = refreshLogId ?? string.Empty
                });

                const string insertBreakdownSql = @"
INSERT INTO BTR_PortalDashboardInventoryBreakdown (
    InventoryBreakdownId, SnapshotKey, DimensionType, Name, InventoryValue, IsTop10, Top10Rank)
VALUES (
    @InventoryBreakdownId, @SnapshotKey, @DimensionType, @Name, @InventoryValue, @IsTop10, @Top10Rank)";

                foreach (var row in result.Breakdown ?? new List<DashboardInventoryBreakdownRow>())
                {
                    conn.Execute(insertBreakdownSql, new
                    {
                        InventoryBreakdownId = _counter.Generate("PDB", IDFormatEnum.PFnnn),
                        SnapshotKey,
                        row.DimensionType,
                        Name = row.Name ?? string.Empty,
                        row.InventoryValue,
                        IsTop10 = row.IsTop10 ? 1 : 0,
                        row.Top10Rank
                    });
                }
            }
        }

        public static DashboardInventoryResponse MapToResponse(DashboardInventoryAggregateResult snapshot)
        {
            var breakdown = snapshot.Breakdown ?? new List<DashboardInventoryBreakdownRow>();

            var topCategories = breakdown
                .Where(r => r.DimensionType == DashboardInventoryAggregator.DimensionCategory && r.IsTop10)
                .OrderBy(r => r.Top10Rank ?? int.MaxValue)
                .Select(r => new DashboardInventoryRankingItem
                {
                    Rank = r.Top10Rank ?? 0,
                    Name = r.Name,
                    InventoryValue = r.InventoryValue,
                    Investigation = InvestigationMetadataBuilder.Build(
                        InvestigationRegistry.SignalLegacyTopCategory,
                        InvestigationMetadataBuilder.EntityTypeCategory,
                        null,
                        r.Name)
                })
                .ToList();

            var topSuppliers = breakdown
                .Where(r => r.DimensionType == DashboardInventoryAggregator.DimensionSupplier && r.IsTop10)
                .OrderBy(r => r.Top10Rank ?? int.MaxValue)
                .Select(r => new DashboardInventoryRankingItem
                {
                    Rank = r.Top10Rank ?? 0,
                    Name = r.Name,
                    InventoryValue = r.InventoryValue,
                    Investigation = InvestigationMetadataBuilder.Build(
                        InvestigationRegistry.SignalLegacyTopSupplier,
                        InvestigationMetadataBuilder.EntityTypeSupplier,
                        null,
                        r.Name)
                })
                .ToList();

            return new DashboardInventoryResponse
            {
                TotalInventoryValue = snapshot.TotalInventoryValue,
                TotalItem = snapshot.TotalItem,
                GeneratedAt = snapshot.GeneratedAt,
                TopCategories = topCategories,
                TopSuppliers = topSuppliers,
                CategoryBreakdown = MapBreakdown(topCategories),
                SupplierBreakdown = MapBreakdown(topSuppliers)
            };
        }

        private static List<DashboardInventoryBreakdownItem> MapBreakdown(
            List<DashboardInventoryRankingItem> ranking)
            => ranking.Select(r => new DashboardInventoryBreakdownItem
            {
                Name = r.Name,
                InventoryValue = r.InventoryValue
            }).ToList();

        private sealed class KpiRow
        {
            public string SnapshotKey { get; set; }
            public System.DateTime GeneratedAt { get; set; }
            public decimal TotalInventoryValue { get; set; }
            public int TotalItem { get; set; }
            public string LastRefreshLogId { get; set; }
        }

        private sealed class BreakdownRow
        {
            public string DimensionType { get; set; }
            public string Name { get; set; }
            public decimal InventoryValue { get; set; }
            public bool IsTop10 { get; set; }
            public int? Top10Rank { get; set; }
        }
    }
}
