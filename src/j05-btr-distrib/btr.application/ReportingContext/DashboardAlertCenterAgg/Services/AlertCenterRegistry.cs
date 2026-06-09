using System;
using System.Collections.Generic;
using System.Linq;

namespace btr.application.ReportingContext.DashboardAlertCenterAgg.Services
{
    public enum AlertCenterSection
    {
        Alerts,
        Concentrations,
        Platform,
        InventorySummaryExcluded
    }

    public sealed class AlertCenterRegistryEntry
    {
        public AlertCenterRegistryEntry(
            string signalKey,
            string defaultLabel,
            string category,
            AlertCenterSection section,
            string dashboardRoute,
            int priority)
        {
            SignalKey = signalKey;
            DefaultLabel = defaultLabel;
            Category = category;
            Section = section;
            DashboardRoute = dashboardRoute;
            Priority = priority;
        }

        public string SignalKey { get; }

        public string DefaultLabel { get; }

        public string Category { get; }

        public AlertCenterSection Section { get; }

        public string DashboardRoute { get; }

        public int Priority { get; }
    }

    public static class AlertCenterRegistry
    {
        public const string CategorySales = "Sales";
        public const string CategoryCustomer = "Customer";
        public const string CategoryCollection = "Collection";
        public const string CategoryInventory = "Inventory";
        public const string CategoryPurchasing = "Purchasing";
        public const string CategoryLocation = "Location";
        public const string CategoryPlatform = "Platform";

        public const string SignalSalesAchievementWarning = "SalesAchievementWarning";
        public const string SignalSalesAchievementCritical = "SalesAchievementCritical";
        public const string SignalSnapshotStale = "SnapshotStale";
        public const string SignalSnapshotDegraded = "SnapshotDegraded";
        public const string SignalDomainUnavailable = "DomainUnavailable";
        public const string SignalAtRiskInventoryPercent = "AtRiskInventoryPercent";
        public const string SignalTopOmzetCustomerPercent = "TopOmzetCustomerPercent";
        public const string SignalTopPiutangCustomerPercent = "TopPiutangCustomerPercent";
        public const string SignalRecoveryVsBillingPercent = "RecoveryVsBillingPercent";
        public const string SignalOverdueConcentrationPercent = "OverdueConcentrationPercent";
        public const string SignalTopCustomerPiutangPercent = "TopCustomerPiutangPercent";
        public const string SignalTop1WarehouseInventoryPercent = "Top1WarehouseInventoryPercent";
        public const string SignalTop1WarehouseAtRiskPercent = "Top1WarehouseAtRiskPercent";
        public const string SignalTop1WarehouseSalesPercent = "Top1WarehouseSalesPercent";
        public const string SignalTop1WilayahSalesPercent = "Top1WilayahSalesPercent";

        private static readonly IReadOnlyList<AlertCenterRegistryEntry> Entries = new List<AlertCenterRegistryEntry>
        {
            // Sales
            new AlertCenterRegistryEntry(SignalSalesAchievementWarning, "Achievement Warning", CategorySales, AlertCenterSection.Alerts, "/dashboard/sales", 1),
            new AlertCenterRegistryEntry(SignalSalesAchievementCritical, "Achievement Critical", CategorySales, AlertCenterSection.Alerts, "/dashboard/sales", 0),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardSalesmanAggregator.SignalBelowTarget, "Below Target", CategorySales, AlertCenterSection.Alerts, "/dashboard/salesmen", 2),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardSalesmanAggregator.SignalNoTarget, "No Target", CategorySales, AlertCenterSection.Alerts, "/dashboard/salesmen", 3),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardSalesmanAggregator.SignalHighOverdueExposure, "High Overdue Exposure", CategorySales, AlertCenterSection.Alerts, "/dashboard/salesmen", 4),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardSalesmanAggregator.SignalDormantCustomerPortfolio, "Dormant Customer Portfolio", CategorySales, AlertCenterSection.Alerts, "/dashboard/salesmen", 5),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardSalesmanAggregator.SignalCustomerConcentration, "Customer Concentration", CategorySales, AlertCenterSection.Concentrations, "/dashboard/salesmen", 20),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardSalesmanAggregator.SignalHighPiutangExposure, "High Piutang Exposure", CategorySales, AlertCenterSection.Concentrations, "/dashboard/salesmen", 21),

            // Customer
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardCustomerAggregator.SignalOverdue, "Overdue", CategoryCustomer, AlertCenterSection.Alerts, "/dashboard/customers", 1),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardCustomerAggregator.SignalDormant, "Dormant", CategoryCustomer, AlertCenterSection.Alerts, "/dashboard/customers", 2),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardCustomerAggregator.SignalPlafondBreach, "Plafond Breach", CategoryCustomer, AlertCenterSection.Alerts, "/dashboard/customers", 3),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardCustomerAggregator.SignalSuspendedWithSales, "Suspended + Sales", CategoryCustomer, AlertCenterSection.Alerts, "/dashboard/customers", 4),

            // Collection
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardCollectionAggregator.SignalChronicOverdue, "Chronic Overdue", CategoryCollection, AlertCenterSection.Alerts, "/dashboard/collection", 1),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardCollectionAggregator.SignalPlafondBreachOverdue, "Plafond Breach + Overdue", CategoryCollection, AlertCenterSection.Alerts, "/dashboard/collection", 2),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardCollectionAggregator.SignalLegacyDebt, "Legacy Debt", CategoryCollection, AlertCenterSection.Alerts, "/dashboard/collection", 3),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardCollectionAggregator.SignalOverdue, "Overdue", CategoryCollection, AlertCenterSection.Alerts, "/dashboard/collection", 4),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardCollectionAggregator.SignalHighOverdueWorkload, "High Overdue Workload", CategoryCollection, AlertCenterSection.Alerts, "/dashboard/collection", 5),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardCollectionAggregator.SignalLowRecoveryVsBilling, "Low Recovery vs Billing", CategoryCollection, AlertCenterSection.Alerts, "/dashboard/collection", 6),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardCollectionAggregator.SignalWilayahHotspot, "Wilayah Hotspot", CategoryCollection, AlertCenterSection.Alerts, "/dashboard/collection", 7),

            // Inventory (M19 item signals excluded from alerts; M21 cross-risk)
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardInventoryRiskAggregator.SignalDeadStock, "Dead Stock", CategoryInventory, AlertCenterSection.InventorySummaryExcluded, "/dashboard/inventory-risk", 1),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardInventoryRiskAggregator.SignalSlowMoving, "Slow Moving", CategoryInventory, AlertCenterSection.InventorySummaryExcluded, "/dashboard/inventory-risk", 2),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardInventoryRiskAggregator.SignalNeverSold, "Never Sold", CategoryInventory, AlertCenterSection.InventorySummaryExcluded, "/dashboard/inventory-risk", 3),
            new AlertCenterRegistryEntry(SignalAtRiskInventoryPercent, "At-Risk Inventory %", CategoryInventory, AlertCenterSection.Concentrations, "/dashboard/inventory-risk", 5),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardPurchasingManagementAggregator.SignalPrincipalAtRiskExposure, "At-Risk Exposure", CategoryInventory, AlertCenterSection.Alerts, "/dashboard/purchasing", 4),

            // Purchasing
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardPurchasingManagementAggregator.SignalQualifiedBacklog, "Qualified Backlog", CategoryPurchasing, AlertCenterSection.Alerts, "/dashboard/purchasing", 1),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardPurchasingManagementAggregator.SignalCompoundDependency, "Compound Dependency", CategoryPurchasing, AlertCenterSection.Alerts, "/dashboard/purchasing", 2),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardPurchasingManagementAggregator.SignalPrincipalInventoryNoPurchase, "Inventory, No Purchase", CategoryPurchasing, AlertCenterSection.Alerts, "/dashboard/purchasing", 3),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardPurchasingManagementAggregator.SignalPurchasingInactivity, "Purchasing Inactivity", CategoryPurchasing, AlertCenterSection.Alerts, "/dashboard/purchasing", 4),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardPurchasingManagementAggregator.SignalUnknownPrincipal, "Unknown Principal", CategoryPurchasing, AlertCenterSection.Alerts, "/dashboard/purchasing", 5),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardPurchasingManagementAggregator.SignalPrincipalSpendConcentration, "Spend Concentration", CategoryPurchasing, AlertCenterSection.Concentrations, "/dashboard/purchasing", 10),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardPurchasingManagementAggregator.SignalPrincipalInventoryConcentration, "Inventory Concentration", CategoryPurchasing, AlertCenterSection.Concentrations, "/dashboard/purchasing", 11),

            // Location
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardLocationAggregator.SignalWarehouseInactiveWithStock, "Inactive With Stock", CategoryLocation, AlertCenterSection.Alerts, "/dashboard/locations", 1),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardLocationAggregator.SignalWarehouseNoSalesWithInventory, "Stock Without Sales", CategoryLocation, AlertCenterSection.Alerts, "/dashboard/locations", 2),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardLocationAggregator.SignalWarehouseAtRiskConcentration, "At-Risk Concentration", CategoryLocation, AlertCenterSection.Concentrations, "/dashboard/locations", 10),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardLocationAggregator.SignalWarehouseInventoryConcentration, "Inventory Concentration", CategoryLocation, AlertCenterSection.Concentrations, "/dashboard/locations", 11),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardLocationAggregator.SignalWarehouseSalesConcentration, "Sales Concentration", CategoryLocation, AlertCenterSection.Concentrations, "/dashboard/locations", 12),
            new AlertCenterRegistryEntry(DashboardSnapshotAgg.Services.DashboardLocationAggregator.SignalWarehousePurchasingConcentration, "Purchasing Concentration", CategoryLocation, AlertCenterSection.Concentrations, "/dashboard/locations", 13),

            // Platform
            new AlertCenterRegistryEntry(SignalSnapshotDegraded, "Snapshot Refresh Failed", CategoryPlatform, AlertCenterSection.Platform, "/dashboard", 0),
            new AlertCenterRegistryEntry(SignalSnapshotStale, "Dashboard Data Not Fresh", CategoryPlatform, AlertCenterSection.Platform, "/dashboard", 1),
            new AlertCenterRegistryEntry(SignalDomainUnavailable, "Domain Snapshot Unavailable", CategoryPlatform, AlertCenterSection.Platform, "/dashboard", 2),

            // KPI-only concentration keys
            new AlertCenterRegistryEntry(SignalTopOmzetCustomerPercent, "Top Customer Omzet %", CategoryCustomer, AlertCenterSection.Concentrations, "/dashboard/customers", 30),
            new AlertCenterRegistryEntry(SignalTopPiutangCustomerPercent, "Top Customer Piutang %", CategoryCustomer, AlertCenterSection.Concentrations, "/dashboard/customers", 31),
            new AlertCenterRegistryEntry(SignalRecoveryVsBillingPercent, "Recovery vs Billing %", CategoryCollection, AlertCenterSection.Concentrations, "/dashboard/collection", 1),
            new AlertCenterRegistryEntry(SignalOverdueConcentrationPercent, "Overdue Concentration %", CategoryCollection, AlertCenterSection.Concentrations, "/dashboard/collection", 2),
            new AlertCenterRegistryEntry(SignalTopCustomerPiutangPercent, "Top Customer Piutang %", CategoryCollection, AlertCenterSection.Concentrations, "/dashboard/piutang", 3),
            new AlertCenterRegistryEntry(SignalTop1WarehouseInventoryPercent, "Top Warehouse Inventory %", CategoryLocation, AlertCenterSection.Concentrations, "/dashboard/locations", 6),
            new AlertCenterRegistryEntry(SignalTop1WarehouseAtRiskPercent, "Top Warehouse At-Risk %", CategoryLocation, AlertCenterSection.Concentrations, "/dashboard/locations", 7),
            new AlertCenterRegistryEntry(SignalTop1WarehouseSalesPercent, "Top Warehouse Sales %", CategoryLocation, AlertCenterSection.Concentrations, "/dashboard/locations", 8),
            new AlertCenterRegistryEntry(SignalTop1WilayahSalesPercent, "Top Wilayah Sales %", CategoryLocation, AlertCenterSection.Concentrations, "/dashboard/locations", 9),
        };

        public static readonly IReadOnlyList<string> CategoryDisplayOrder = new[]
        {
            CategorySales,
            CategoryCustomer,
            CategoryCollection,
            CategoryInventory,
            CategoryPurchasing,
            CategoryLocation
        };

        public const int AlertsPerCategoryCap = 20;

        public static bool TryGet(string signalKey, out AlertCenterRegistryEntry entry)
        {
            entry = Entries.FirstOrDefault(e =>
                string.Equals(e.SignalKey, signalKey, StringComparison.OrdinalIgnoreCase));
            return entry != null;
        }

        public static bool TryGetForProducer(string source, string signalKey, out AlertCenterRegistryEntry entry)
        {
            if (string.Equals(source, "M17", StringComparison.OrdinalIgnoreCase)
                && string.Equals(signalKey, DashboardSnapshotAgg.Services.DashboardCustomerAggregator.SignalOverdue, StringComparison.OrdinalIgnoreCase))
            {
                entry = Entries.First(e =>
                    string.Equals(e.SignalKey, signalKey, StringComparison.OrdinalIgnoreCase)
                    && e.Category == CategoryCustomer);
                return true;
            }

            if (string.Equals(source, "M20", StringComparison.OrdinalIgnoreCase)
                && string.Equals(signalKey, DashboardSnapshotAgg.Services.DashboardCollectionAggregator.SignalOverdue, StringComparison.OrdinalIgnoreCase))
            {
                entry = Entries.First(e =>
                    string.Equals(e.SignalKey, signalKey, StringComparison.OrdinalIgnoreCase)
                    && e.Category == CategoryCollection);
                return true;
            }

            return TryGet(signalKey, out entry);
        }

        public static IReadOnlyList<AlertCenterRegistryEntry> GetAll() => Entries;
    }
}
