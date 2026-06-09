using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;

namespace btr.application.ReportingContext.DashboardAlertCenterAgg.Services
{
    public static class DashboardSnapshotHealthHelper
    {
        public static bool IsDomainFresh(DateTime generatedAt, int intervalMinutes, DateTime utcNow)
        {
            if (intervalMinutes <= 0)
                return true;

            return (utcNow - generatedAt.ToUniversalTime()).TotalMinutes <= intervalMinutes;
        }

        public static bool IsExecutiveDataFresh(
            DashboardSalesAggregateResult sales,
            DashboardPiutangAggregateResult piutang,
            DashboardInventoryAggregateResult inventory,
            DashboardPurchasingAggregateResult purchasing,
            DashboardSnapshotOptions options,
            DateTime utcNow)
        {
            var hasAnyDomain = sales != null || piutang != null || inventory != null || purchasing != null;
            if (!hasAnyDomain)
                return false;

            if (sales != null && !IsDomainFresh(sales.GeneratedAt, options.SalesIntervalMinutes, utcNow))
                return false;

            if (piutang != null && !IsDomainFresh(piutang.GeneratedAt, options.PiutangIntervalMinutes, utcNow))
                return false;

            if (inventory != null && !IsDomainFresh(inventory.GeneratedAt, options.InventoryIntervalMinutes, utcNow))
                return false;

            if (purchasing != null && !IsDomainFresh(purchasing.GeneratedAt, options.PurchasingIntervalMinutes, utcNow))
                return false;

            return true;
        }

        public static bool IsAlertCenterDataFresh(AlertCenterComposeInput input, DateTime utcNow)
        {
            var options = input.Options ?? new DashboardSnapshotOptions();
            var checks = new List<bool>();

            if (input.Sales != null)
                checks.Add(IsDomainFresh(input.Sales.GeneratedAt, options.SalesIntervalMinutes, utcNow));
            if (input.Piutang != null)
                checks.Add(IsDomainFresh(input.Piutang.GeneratedAt, options.PiutangIntervalMinutes, utcNow));
            if (input.Inventory != null)
                checks.Add(IsDomainFresh(input.Inventory.GeneratedAt, options.InventoryIntervalMinutes, utcNow));
            if (input.Purchasing != null)
                checks.Add(IsDomainFresh(input.Purchasing.GeneratedAt, options.PurchasingIntervalMinutes, utcNow));
            if (input.Customer != null)
                checks.Add(IsDomainFresh(input.Customer.GeneratedAt, options.CustomerIntervalMinutes, utcNow));
            if (input.Salesman != null)
                checks.Add(IsDomainFresh(input.Salesman.GeneratedAt, options.SalesmanIntervalMinutes, utcNow));
            if (input.Collection != null)
                checks.Add(IsDomainFresh(input.Collection.GeneratedAt, options.CollectionIntervalMinutes, utcNow));
            if (input.InventoryRisk != null)
                checks.Add(IsDomainFresh(input.InventoryRisk.GeneratedAt, options.InventoryRiskIntervalMinutes, utcNow));
            if (input.PurchasingManagement != null)
                checks.Add(IsDomainFresh(input.PurchasingManagement.GeneratedAt, options.PurchasingManagementIntervalMinutes, utcNow));
            if (input.Location != null)
                checks.Add(IsDomainFresh(input.Location.GeneratedAt, options.LocationIntervalMinutes, utcNow));

            return checks.Count > 0 && checks.All(c => c);
        }

        public static bool HasExecutiveUnavailableDomain(
            DashboardSalesAggregateResult sales,
            DashboardPiutangAggregateResult piutang,
            DashboardInventoryAggregateResult inventory,
            DashboardPurchasingAggregateResult purchasing)
        {
            return sales == null || piutang == null || inventory == null || purchasing == null;
        }

        public static bool HasAlertCenterUnavailableDomain(AlertCenterComposeInput input)
        {
            return input.Sales == null
                || input.Piutang == null
                || input.Inventory == null
                || input.Purchasing == null;
        }

        public static DateTime? ResolveLastRefreshed(params DateTime?[] generatedAtValues)
        {
            var values = generatedAtValues.Where(v => v.HasValue).Select(v => v.Value).ToList();
            return values.Count > 0 ? (DateTime?)values.Min() : null;
        }

        public static string ResolveOverallHealth(IReadOnlyList<DashboardSnapshotRefreshStatusModel> refreshStatuses)
        {
            var statuses = refreshStatuses ?? new List<DashboardSnapshotRefreshStatusModel>();
            return DashboardSnapshotHealthStatusResolver.ResolveOverallStatus(
                statuses.Select(s => s?.Status).ToList());
        }
    }
}
