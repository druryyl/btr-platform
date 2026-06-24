using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardInventoryRiskAgg.Contracts;
using btr.application.ReportingContext.DashboardInventoryRiskAgg.Queries;
using btr.application.ReportingContext.Shared;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardInventoryRiskAgg
{
    public class DashboardInventoryRiskDal : IDashboardInventoryRiskDal
    {
        private const string InventoryReportRoute = "/reports/inventory";

        private static string BuildItemProfileRoute(string brgCode)
        {
            if (string.IsNullOrWhiteSpace(brgCode))
                return null;

            return $"/analytics/items/{Uri.EscapeDataString(brgCode.Trim())}";
        }

        private readonly IDashboardInventoryRiskSnapshotDal _snapshotDal;
        private readonly DashboardSnapshotOptions _options;

        public DashboardInventoryRiskDal(
            IDashboardInventoryRiskSnapshotDal snapshotDal,
            IOptions<DashboardSnapshotOptions> options)
        {
            _snapshotDal = snapshotDal;
            _options = options?.Value ?? new DashboardSnapshotOptions();
        }

        public DashboardInventoryRiskResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot == null)
            {
                return new DashboardInventoryRiskResponse
                {
                    IsAvailable = false,
                    IsDataFresh = false,
                    Navigation = BuildNavigation()
                };
            }

            return MapToResponse(snapshot);
        }

        private DashboardInventoryRiskResponse MapToResponse(DashboardInventoryRiskAggregateResult snapshot)
        {
            var utcNow = DateTime.UtcNow;
            var isFresh = IsDomainFresh(snapshot.GeneratedAt, _options.InventoryRiskIntervalMinutes, utcNow);

            var categoryBreakdown = snapshot.Breakdown?
                .Where(r => string.Equals(
                    r.DimensionType,
                    DashboardInventoryRiskAggregator.DimensionCategory,
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => r.Rank)
                .ToList() ?? new List<DashboardInventoryRiskBreakdownRow>();

            var supplierBreakdown = snapshot.Breakdown?
                .Where(r => string.Equals(
                    r.DimensionType,
                    DashboardInventoryRiskAggregator.DimensionSupplier,
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => r.Rank)
                .ToList() ?? new List<DashboardInventoryRiskBreakdownRow>();

            return new DashboardInventoryRiskResponse
            {
                IsAvailable = true,
                IsDataFresh = isFresh,
                GeneratedAt = snapshot.GeneratedAt,
                AttentionCards = new DashboardInventoryRiskAttentionCards
                {
                    TotalInventoryValue = snapshot.TotalInventoryValue,
                    DeadStockItemCount = snapshot.DeadStockItemCount,
                    DeadStockValue = snapshot.DeadStockValue,
                    SlowMovingItemCount = snapshot.SlowMovingItemCount,
                    SlowMovingValue = snapshot.SlowMovingValue,
                    AtRiskInventoryPercent = snapshot.AtRiskInventoryPercent ?? 0,
                    RequiresAttention = snapshot.RequiresAttention
                },
                AgingBuckets = snapshot.AgingBuckets?
                    .Select(r => new DashboardInventoryRiskAgingBucket
                    {
                        BucketKey = r.BucketKey,
                        BucketLabel = r.BucketLabel,
                        Amount = r.InventoryValue,
                        ItemCount = r.ItemCount,
                        SortOrder = r.SortOrder
                    })
                    .ToList() ?? new List<DashboardInventoryRiskAgingBucket>(),
                CategoryRiskExposure = categoryBreakdown
                    .Select(MapBreakdownItem)
                    .ToList(),
                SupplierRiskExposure = supplierBreakdown
                    .Select(MapBreakdownItem)
                    .ToList(),
                AttentionList = snapshot.AttentionList?
                    .Select(MapAttentionItem)
                    .ToList() ?? new List<DashboardInventoryRiskAttentionItem>(),
                Rankings = new DashboardInventoryRiskRankings
                {
                    TopDead = snapshot.TopDead?
                        .Select(r => MapRankingRow(r, DashboardInventoryRiskAggregator.SignalDeadStock))
                        .ToList() ?? new List<DashboardInventoryRiskRankingRow>(),
                    TopSlow = snapshot.TopSlow?
                        .Select(r => MapRankingRow(r, DashboardInventoryRiskAggregator.SignalSlowMoving))
                        .ToList() ?? new List<DashboardInventoryRiskRankingRow>()
                },
                Navigation = BuildNavigation()
            };
        }

        private static DashboardInventoryRiskBreakdownItem MapBreakdownItem(
            DashboardInventoryRiskBreakdownRow row) =>
            new DashboardInventoryRiskBreakdownItem
            {
                Name = row.Name,
                AtRiskValue = row.AtRiskValue,
                ItemCount = row.ItemCount,
                PercentOfAtRisk = row.PercentOfAtRisk
            };

        private static DashboardInventoryRiskAttentionItem MapAttentionItem(
            DashboardInventoryRiskAttentionRow row) =>
            new DashboardInventoryRiskAttentionItem
            {
                BrgCode = row.BrgCode,
                BrgName = row.BrgName,
                KategoriName = row.KategoriName,
                SupplierName = row.SupplierName,
                Qty = row.Qty,
                InventoryValue = row.InventoryValue,
                DaysSinceLastFaktur = row.DaysSinceLastFaktur,
                SignalKey = row.SignalKey,
                SignalLabel = row.SignalLabel,
                ReportRoute = InventoryReportRoute,
                ProfileRoute = BuildItemProfileRoute(row.BrgCode),
                Investigation = InvestigationMetadataBuilder.Build(
                    row.SignalKey,
                    InvestigationMetadataBuilder.EntityTypeItem,
                    row.BrgCode,
                    row.BrgName,
                    signalLabelOverride: row.SignalLabel,
                    reportRouteOverride: InventoryReportRoute)
            };

        private static DashboardInventoryRiskRankingRow MapRankingRow(
            DashboardInventoryRiskTopRow row,
            string signalKey) =>
            new DashboardInventoryRiskRankingRow
            {
                Rank = row.Rank,
                BrgCode = row.BrgCode,
                BrgName = row.BrgName,
                KategoriName = row.KategoriName,
                SupplierName = row.SupplierName,
                Qty = row.Qty,
                InventoryValue = row.InventoryValue,
                DaysSinceLastFaktur = row.DaysSinceLastFaktur,
                PercentOfAtRisk = row.PercentOfAtRisk,
                ReportRoute = InventoryReportRoute,
                ProfileRoute = BuildItemProfileRoute(row.BrgCode),
                Investigation = InvestigationMetadataBuilder.Build(
                    signalKey,
                    InvestigationMetadataBuilder.EntityTypeItem,
                    row.BrgId,
                    row.BrgName,
                    reportRouteOverride: InventoryReportRoute)
            };

        private static DashboardInventoryRiskNavigationLinks BuildNavigation() =>
            new DashboardInventoryRiskNavigationLinks
            {
                InventoryDashboardRoute = "/dashboard/inventory",
                InventoryReportRoute = InventoryReportRoute
            };

        private static bool IsDomainFresh(DateTime generatedAt, int intervalMinutes, DateTime utcNow)
        {
            if (intervalMinutes <= 0)
                return true;

            return (utcNow - generatedAt.ToUniversalTime()).TotalMinutes <= intervalMinutes;
        }
    }
}
