using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;

namespace btr.application.ReportingContext.Shared
{
    public sealed class InvestigationRegistryEntry
    {
        public InvestigationRegistryEntry(
            string signalKey,
            string defaultSignalLabel,
            string entityType,
            string dashboardRoute,
            string reportRoute,
            string desktopNextStep = null,
            string defaultPeriodMode = null,
            string defaultPostingFilter = null,
            IReadOnlyList<InvestigationStep> steps = null)
        {
            SignalKey = signalKey;
            DefaultSignalLabel = defaultSignalLabel;
            EntityType = entityType;
            DashboardRoute = dashboardRoute;
            ReportRoute = reportRoute;
            DesktopNextStep = desktopNextStep;
            DefaultPeriodMode = defaultPeriodMode;
            DefaultPostingFilter = defaultPostingFilter;
            Steps = steps ?? Array.Empty<InvestigationStep>();
        }

        public string SignalKey { get; }

        public string DefaultSignalLabel { get; }

        public string EntityType { get; }

        public string DashboardRoute { get; }

        public string ReportRoute { get; }

        public string DesktopNextStep { get; }

        public string DefaultPeriodMode { get; }

        public string DefaultPostingFilter { get; }

        public IReadOnlyList<InvestigationStep> Steps { get; }
    }

    public static class InvestigationRegistry
    {
        public const string PeriodModeAllOpenBalances = "allOpenBalances";
        public const string PeriodModeCurrentMonth = "currentMonth";
        public const string PostingFilterBelum = "BELUM";

        public const string SalesReportRoute = "/reports/sales";
        public const string PiutangReportRoute = "/reports/piutang";
        public const string InventoryReportRoute = "/reports/inventory";
        public const string PurchasingReportRoute = "/reports/purchasing";

        public const string SignalExecutiveTopCustomerExposure = "ExecutiveTopCustomerExposure";
        public const string SignalExecutiveTopCategoryExposure = "ExecutiveTopCategoryExposure";
        public const string SignalExecutiveTopSupplierExposure = "ExecutiveTopSupplierExposure";
        public const string SignalExecutiveTopPrincipalExposure = "ExecutiveTopPrincipalExposure";

        public const string SignalLegacyTopSalesman = "LegacyTopSalesman";
        public const string SignalLegacyTopCustomer = "LegacyTopCustomer";
        public const string SignalLegacyTopCategory = "LegacyTopCategory";
        public const string SignalLegacyTopSupplier = "LegacyTopSupplier";

        public const string SignalRankingCustomerTopOmzet = "RankingCustomerTopOmzet";
        public const string SignalRankingCustomerTopPiutang = "RankingCustomerTopPiutang";
        public const string SignalRankingSalesmanTopOmzet = "RankingSalesmanTopOmzet";
        public const string SignalRankingSalesmanTopAchievement = "RankingSalesmanTopAchievement";
        public const string SignalRankingSalesmanTopPiutang = "RankingSalesmanTopPiutang";
        public const string SignalRankingCollectionTopOverdueCustomer = "RankingCollectionTopOverdueCustomer";
        public const string SignalRankingCollectionTopOverdueSalesman = "RankingCollectionTopOverdueSalesman";
        public const string SignalRankingTopPrincipal = "RankingTopPrincipal";

        private static readonly IReadOnlyList<InvestigationRegistryEntry> Entries = BuildEntries();

        public static bool TryGet(string signalKey, out InvestigationRegistryEntry entry)
        {
            entry = Entries.FirstOrDefault(e =>
                string.Equals(e.SignalKey, signalKey, StringComparison.OrdinalIgnoreCase));
            return entry != null;
        }

        public static IReadOnlyList<InvestigationRegistryEntry> GetAll() => Entries;

        public static IReadOnlyList<string> GetAllSignalKeys() =>
            Entries.Select(e => e.SignalKey).ToList();

        private static IReadOnlyList<InvestigationRegistryEntry> BuildEntries()
        {
            var compoundDependencySteps = new List<InvestigationStep>
            {
                new InvestigationStep
                {
                    Order = 1,
                    Label = "Purchasing evidence",
                    ReportRoute = PurchasingReportRoute
                },
                new InvestigationStep
                {
                    Order = 2,
                    Label = "Inventory held",
                    ReportRoute = InventoryReportRoute
                },
                new InvestigationStep
                {
                    Order = 3,
                    Label = "At-risk exposure",
                    DashboardRoute = "/dashboard/inventory-risk"
                }
            };

            var warehouseAtRiskSteps = new List<InvestigationStep>
            {
                new InvestigationStep
                {
                    Order = 1,
                    Label = "Inventory risk context",
                    DashboardRoute = "/dashboard/inventory-risk"
                },
                new InvestigationStep
                {
                    Order = 2,
                    Label = "Warehouse stock evidence",
                    ReportRoute = InventoryReportRoute
                }
            };

            return new List<InvestigationRegistryEntry>
            {
                // Customer
                Entry(DashboardCustomerAggregator.SignalOverdue, "Overdue", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/customers", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(DashboardCustomerAggregator.SignalPlafondBreach, "Plafond Breach", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/customers", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(DashboardCustomerAggregator.SignalDormant, "Dormant", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/customers", SalesReportRoute, "Next validation: Customer master / Faktur history"),
                Entry(DashboardCustomerAggregator.SignalSuspendedWithSales, "Suspended + Sales", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/customers", SalesReportRoute, "Next validation: Customer master / Faktur history"),

                // Salesman
                Entry(DashboardSalesmanAggregator.SignalBelowTarget, "Below Target", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/salesmen", SalesReportRoute, "Next validation: Sales Omzet Chart (RO2)", PeriodModeCurrentMonth),
                Entry(DashboardSalesmanAggregator.SignalNoTarget, "No Target", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/salesmen", SalesReportRoute, null, PeriodModeCurrentMonth),
                Entry(DashboardSalesmanAggregator.SignalHighOverdueExposure, "High Overdue Exposure", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/salesmen", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(DashboardSalesmanAggregator.SignalHighPiutangExposure, "High Piutang Exposure", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/salesmen", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(DashboardSalesmanAggregator.SignalCustomerConcentration, "Customer Concentration", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/salesmen", SalesReportRoute, "Next validation: Sales Omzet Chart (RO2)", PeriodModeCurrentMonth),
                Entry(DashboardSalesmanAggregator.SignalDormantCustomerPortfolio, "Dormant Customer Portfolio", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/salesmen", SalesReportRoute, "Next validation: Sales Omzet Chart (RO2)", PeriodModeCurrentMonth),

                // Collection
                Entry(DashboardCollectionAggregator.SignalChronicOverdue, "Chronic Overdue", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/collection", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(DashboardCollectionAggregator.SignalLegacyDebt, "Legacy Debt", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/collection", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(DashboardCollectionAggregator.SignalPlafondBreachOverdue, "Plafond Breach + Overdue", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/collection", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(DashboardCollectionAggregator.SignalOverdue, "Overdue", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/collection", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(DashboardCollectionAggregator.SignalHighOverdueWorkload, "High Overdue Workload", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/collection", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(DashboardCollectionAggregator.SignalLowRecoveryVsBilling, "Low Recovery vs Billing", InvestigationMetadataBuilder.EntityTypeCompany,
                    "/dashboard/collection", null),
                Entry(DashboardCollectionAggregator.SignalWilayahHotspot, "Wilayah Hotspot", InvestigationMetadataBuilder.EntityTypeWilayah,
                    "/dashboard/collection", null),

                // Inventory risk
                Entry(DashboardInventoryRiskAggregator.SignalDeadStock, "Dead Stock", InvestigationMetadataBuilder.EntityTypeItem,
                    "/dashboard/inventory-risk", InventoryReportRoute, "Next validation: Kartu Stok (IF8)"),
                Entry(DashboardInventoryRiskAggregator.SignalSlowMoving, "Slow Moving", InvestigationMetadataBuilder.EntityTypeItem,
                    "/dashboard/inventory-risk", InventoryReportRoute, "Next validation: Kartu Stok (IF8)"),
                Entry(DashboardInventoryRiskAggregator.SignalNeverSold, "Never Sold", InvestigationMetadataBuilder.EntityTypeItem,
                    "/dashboard/inventory-risk", InventoryReportRoute, "Next validation: Kartu Stok (IF8)"),

                // Purchasing
                Entry(DashboardPurchasingManagementAggregator.SignalQualifiedBacklog, "Qualified Backlog", InvestigationMetadataBuilder.EntityTypePrincipal,
                    "/dashboard/purchasing", PurchasingReportRoute, "Next validation: Posting Stok (PT2)", PeriodModeCurrentMonth, PostingFilterBelum),
                Entry(DashboardPurchasingManagementAggregator.SignalCompoundDependency, "Compound Dependency", InvestigationMetadataBuilder.EntityTypePrincipal,
                    "/dashboard/purchasing", PurchasingReportRoute, "Next validation: Principal master", steps: compoundDependencySteps),
                Entry(DashboardPurchasingManagementAggregator.SignalPrincipalSpendConcentration, "Spend Concentration", InvestigationMetadataBuilder.EntityTypePrincipal,
                    "/dashboard/purchasing", PurchasingReportRoute, PeriodModeCurrentMonth),
                Entry(DashboardPurchasingManagementAggregator.SignalPrincipalInventoryConcentration, "Inventory Concentration", InvestigationMetadataBuilder.EntityTypePrincipal,
                    "/dashboard/purchasing", InventoryReportRoute),
                Entry(DashboardPurchasingManagementAggregator.SignalPrincipalAtRiskExposure, "At-Risk Exposure", InvestigationMetadataBuilder.EntityTypePrincipal,
                    "/dashboard/purchasing", InventoryReportRoute),
                Entry(DashboardPurchasingManagementAggregator.SignalPurchasingInactivity, "Purchasing Inactivity", InvestigationMetadataBuilder.EntityTypeCompany,
                    "/dashboard/purchasing", null),
                Entry(DashboardPurchasingManagementAggregator.SignalPrincipalInventoryNoPurchase, "Inventory, No Purchase", InvestigationMetadataBuilder.EntityTypePrincipal,
                    "/dashboard/purchasing", InventoryReportRoute),
                Entry(DashboardPurchasingManagementAggregator.SignalUnknownPrincipal, "Unknown Principal", InvestigationMetadataBuilder.EntityTypePrincipal,
                    "/dashboard/purchasing", PurchasingReportRoute, PeriodModeCurrentMonth),

                // Location
                Entry(DashboardLocationAggregator.SignalWarehouseInactiveWithStock, "Inactive With Stock", InvestigationMetadataBuilder.EntityTypeWarehouse,
                    "/dashboard/locations", InventoryReportRoute, "Next validation: Kartu Stok (IF8)"),
                Entry(DashboardLocationAggregator.SignalWarehouseNoSalesWithInventory, "Stock Without Sales", InvestigationMetadataBuilder.EntityTypeWarehouse,
                    "/dashboard/locations", InventoryReportRoute, "Next validation: Kartu Stok (IF8)"),
                Entry(DashboardLocationAggregator.SignalWarehouseAtRiskConcentration, "At-Risk Concentration", InvestigationMetadataBuilder.EntityTypeWarehouse,
                    "/dashboard/locations", null, steps: warehouseAtRiskSteps),
                Entry(DashboardLocationAggregator.SignalWarehouseInventoryConcentration, "Inventory Concentration", InvestigationMetadataBuilder.EntityTypeWarehouse,
                    "/dashboard/locations", InventoryReportRoute),
                Entry(DashboardLocationAggregator.SignalWarehouseSalesConcentration, "Sales Concentration", InvestigationMetadataBuilder.EntityTypeWarehouse,
                    "/dashboard/locations", SalesReportRoute, PeriodModeCurrentMonth),
                Entry(DashboardLocationAggregator.SignalWarehousePurchasingConcentration, "Purchasing Concentration", InvestigationMetadataBuilder.EntityTypeWarehouse,
                    "/dashboard/locations", PurchasingReportRoute, PeriodModeCurrentMonth),

                // Executive synthetic
                Entry(SignalExecutiveTopCustomerExposure, "Top Customer Exposure", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/customers", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(SignalExecutiveTopCategoryExposure, "Top Category Exposure", InvestigationMetadataBuilder.EntityTypeCategory,
                    "/dashboard/inventory", InventoryReportRoute),
                Entry(SignalExecutiveTopSupplierExposure, "Top Supplier Exposure", InvestigationMetadataBuilder.EntityTypeSupplier,
                    "/dashboard/inventory", InventoryReportRoute),
                Entry(SignalExecutiveTopPrincipalExposure, "Top Principal Exposure", InvestigationMetadataBuilder.EntityTypePrincipal,
                    "/dashboard/purchasing", PurchasingReportRoute, PeriodModeCurrentMonth),

                // Legacy rankings
                Entry(SignalLegacyTopSalesman, "Top Salesman", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/sales", SalesReportRoute, "Next validation: Sales Omzet Chart (RO2)", PeriodModeCurrentMonth),
                Entry(SignalLegacyTopCustomer, "Top Customer", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/piutang", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(SignalLegacyTopCategory, "Top Category", InvestigationMetadataBuilder.EntityTypeCategory,
                    "/dashboard/inventory", InventoryReportRoute),
                Entry(SignalLegacyTopSupplier, "Top Supplier", InvestigationMetadataBuilder.EntityTypeSupplier,
                    "/dashboard/inventory", InventoryReportRoute),

                // Domain dashboard rankings (M17–M22)
                Entry(SignalRankingCustomerTopOmzet, "Top Customer Omzet", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/customers", SalesReportRoute, null, PeriodModeCurrentMonth),
                Entry(SignalRankingCustomerTopPiutang, "Top Customer Piutang", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/customers", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(SignalRankingSalesmanTopOmzet, "Top Salesman Omzet", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/salesmen", SalesReportRoute, "Next validation: Sales Omzet Chart (RO2)", PeriodModeCurrentMonth),
                Entry(SignalRankingSalesmanTopAchievement, "Top Salesman Achievement", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/salesmen", SalesReportRoute, "Next validation: Sales Omzet Chart (RO2)", PeriodModeCurrentMonth),
                Entry(SignalRankingSalesmanTopPiutang, "Top Salesman Piutang", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/salesmen", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(SignalRankingCollectionTopOverdueCustomer, "Top Overdue Customer", InvestigationMetadataBuilder.EntityTypeCustomer,
                    "/dashboard/collection", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(SignalRankingCollectionTopOverdueSalesman, "Top Overdue Salesman", InvestigationMetadataBuilder.EntityTypeSalesman,
                    "/dashboard/collection", PiutangReportRoute, "Next validation: Piutang Tracker (FT5)", PeriodModeAllOpenBalances),
                Entry(SignalRankingTopPrincipal, "Top Principal", InvestigationMetadataBuilder.EntityTypePrincipal,
                    "/dashboard/purchasing", PurchasingReportRoute, null, PeriodModeCurrentMonth),
            };
        }

        private static InvestigationRegistryEntry Entry(
            string signalKey,
            string label,
            string entityType,
            string dashboardRoute,
            string reportRoute,
            string desktopNextStep = null,
            string periodMode = null,
            string postingFilter = null,
            IReadOnlyList<InvestigationStep> steps = null)
        {
            return new InvestigationRegistryEntry(
                signalKey,
                label,
                entityType,
                dashboardRoute,
                reportRoute,
                desktopNextStep,
                periodMode,
                postingFilter,
                steps);
        }
    }
}
