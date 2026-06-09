using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardLocationAgg.Contracts;
using btr.application.ReportingContext.DashboardLocationAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardLocationAgg
{
    public class DashboardLocationDal : IDashboardLocationDal
    {
        private const string InventoryReportRoute = "/reports/inventory";

        private readonly IDashboardLocationSnapshotDal _snapshotDal;
        private readonly DashboardSnapshotOptions _options;

        public DashboardLocationDal(
            IDashboardLocationSnapshotDal snapshotDal,
            IOptions<DashboardSnapshotOptions> options)
        {
            _snapshotDal = snapshotDal;
            _options = options?.Value ?? new DashboardSnapshotOptions();
        }

        public DashboardLocationResponse GetSummary()
        {
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot == null)
            {
                return new DashboardLocationResponse
                {
                    IsAvailable = false,
                    IsDataFresh = false,
                    Navigation = BuildNavigation()
                };
            }

            return MapToResponse(snapshot);
        }

        private DashboardLocationResponse MapToResponse(DashboardLocationAggregateResult snapshot)
        {
            var utcNow = DateTime.UtcNow;
            var isFresh = IsDomainFresh(snapshot.GeneratedAt, _options.LocationIntervalMinutes, utcNow);

            return new DashboardLocationResponse
            {
                IsAvailable = true,
                IsDataFresh = isFresh,
                GeneratedAt = snapshot.GeneratedAt,
                AttentionCards = new DashboardLocationAttentionCards
                {
                    Top1WarehouseInventoryPercent = snapshot.Top1WarehouseInventoryPercent,
                    Top3WarehouseInventoryPercent = snapshot.Top3WarehouseInventoryPercent,
                    Top1WarehouseAtRiskPercent = snapshot.Top1WarehouseAtRiskPercent,
                    Top1WarehouseSalesPercent = snapshot.Top1WarehouseSalesPercent,
                    Top1WilayahSalesPercent = snapshot.Top1WilayahSalesPercent,
                    InactiveWarehouseWithStockCount = snapshot.InactiveWarehouseWithStockCount,
                    WarehouseNoSalesWithInventoryCount = snapshot.WarehouseNoSalesWithInventoryCount
                },
                TopWarehouseInventory = snapshot.TopWarehouseInventory?
                    .Select(r => new DashboardLocationRankingRow
                    {
                        Rank = r.Rank,
                        EntityCode = r.WarehouseId,
                        EntityName = r.WarehouseName,
                        Amount = r.InventoryValue,
                        PercentOfTotal = r.PercentOfTotal,
                        ReportRoute = r.ReportRoute ?? InventoryReportRoute
                    })
                    .ToList() ?? new List<DashboardLocationRankingRow>(),
                TopWarehouseAtRisk = snapshot.TopWarehouseAtRisk?
                    .Select(r => new DashboardLocationRankingRow
                    {
                        Rank = r.Rank,
                        EntityCode = r.WarehouseId,
                        EntityName = r.WarehouseName,
                        Amount = r.AtRiskValue,
                        PercentOfTotal = r.PercentOfTotal,
                        ReportRoute = r.ReportRoute ?? InventoryReportRoute
                    })
                    .ToList() ?? new List<DashboardLocationRankingRow>(),
                TopWarehouseSales = snapshot.TopWarehouseSales?
                    .Select(r => new DashboardLocationRankingRow
                    {
                        Rank = r.Rank,
                        EntityCode = r.WarehouseId,
                        EntityName = r.WarehouseName,
                        Amount = r.MtdOmzet,
                        PercentOfTotal = r.PercentOfTotal,
                        ReportRoute = r.ReportRoute
                    })
                    .ToList() ?? new List<DashboardLocationRankingRow>(),
                TopWarehousePurchasing = snapshot.TopWarehousePurchasing?
                    .Select(r => new DashboardLocationRankingRow
                    {
                        Rank = r.Rank,
                        EntityCode = r.WarehouseId,
                        EntityName = r.WarehouseName,
                        Amount = r.MtdPurchaseAmount,
                        PercentOfTotal = r.PercentOfTotal,
                        ReportRoute = r.ReportRoute
                    })
                    .ToList() ?? new List<DashboardLocationRankingRow>(),
                TopWilayahSales = snapshot.TopWilayahSales?
                    .Select(r => new DashboardLocationWilayahRankingRow
                    {
                        Rank = r.Rank,
                        EntityCode = r.WilayahId,
                        EntityName = r.WilayahName,
                        Amount = r.MtdOmzet,
                        PercentOfTotal = r.PercentOfTotal,
                        DashboardRoute = r.DashboardRoute ?? "/dashboard/collection"
                    })
                    .ToList() ?? new List<DashboardLocationWilayahRankingRow>(),
                AttentionList = snapshot.AttentionList?
                    .Select(r => new DashboardLocationAttentionItem
                    {
                        EntityType = r.EntityType,
                        EntityCode = r.EntityCode,
                        EntityName = r.EntityName,
                        SignalKey = r.SignalKey,
                        SignalLabel = r.SignalLabel,
                        ValueAmount = r.ValueAmount,
                        ValueText = r.ValueText,
                        ReportRoute = r.ReportRoute ?? InventoryReportRoute
                    })
                    .ToList() ?? new List<DashboardLocationAttentionItem>(),
                Navigation = BuildNavigation()
            };
        }

        private static DashboardLocationNavigationLinks BuildNavigation()
        {
            return new DashboardLocationNavigationLinks
            {
                InventoryDashboardRoute = "/dashboard/inventory",
                InventoryRiskDashboardRoute = "/dashboard/inventory-risk",
                SalesDashboardRoute = "/dashboard/sales",
                PurchasingDashboardRoute = "/dashboard/purchasing",
                CollectionDashboardRoute = "/dashboard/collection",
                CustomerDashboardRoute = "/dashboard/customers",
                SalesmanDashboardRoute = "/dashboard/salesmen"
            };
        }

        private static bool IsDomainFresh(DateTime generatedAt, int intervalMinutes, DateTime utcNow)
        {
            if (intervalMinutes <= 0)
                return true;

            return (utcNow - generatedAt.ToUniversalTime()).TotalMinutes <= intervalMinutes;
        }
    }
}
