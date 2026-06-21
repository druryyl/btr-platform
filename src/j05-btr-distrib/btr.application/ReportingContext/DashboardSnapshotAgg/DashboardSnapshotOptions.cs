namespace btr.application.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardSnapshotOptions
    {
        public const string SECTION_NAME = "DashboardSnapshot";

        public int PiutangIntervalMinutes { get; set; } = 15;

        public int InventoryIntervalMinutes { get; set; } = 60;

        public int InventoryRiskIntervalMinutes { get; set; } = 60;

        public int SalesIntervalMinutes { get; set; } = 30;

        public int PurchasingIntervalMinutes { get; set; } = 30;

        public int CustomerIntervalMinutes { get; set; } = 30;

        public int SalesmanIntervalMinutes { get; set; } = 30;

        public decimal SalesmanExposureTopPercent { get; set; } = 20;

        public int CollectionIntervalMinutes { get; set; } = 30;

        public int LocationIntervalMinutes { get; set; } = 60;

        public int PurchasingManagementIntervalMinutes { get; set; } = 30;

        public int PurchasingQualifiedBacklogDays { get; set; } = 3;

        public decimal CashFlowForecastLargeDueSoonFloorAmount { get; set; } = 50_000_000m;

        public int InventoryForecastPlanningHorizonDays { get; set; } = 30;

        public int InventoryForecastDefaultLeadTimeDays { get; set; } = 7;

        public int InventoryForecastCoverageDays { get; set; } = 14;

        public int InventoryForecastOverstockDosDays { get; set; } = 90;

        public int InventoryForecastMinDosHealthy { get; set; } = 30;

        public int InventoryForecastStockOutCriticalDays { get; set; } = 7;

        public decimal? InventoryOptimizationDefaultBudgetCapIdr { get; set; }

        public int InventoryOptimizationWarehouseShortageDosDays { get; set; } = 14;

        public int InventoryOptimizationWarehouseExcessDosDays { get; set; } = 60;

        public int InventoryOptimizationMaxTopActions { get; set; } = 25;

        public int InventoryOptimizationMaxReorderRows { get; set; } = 15;

        public int InventoryOptimizationMaxTransferRows { get; set; } = 10;

        public decimal InventoryOptimizationReduceQtyFactor { get; set; } = 0.5m;

        public int CustomerRiskForecastHorizonDays { get; set; } = 30;

        public decimal CustomerRiskForecastPriorMonthOmzetFloorIdr { get; set; } = 1_000_000m;

        public int CustomerRiskForecastNoPaymentRecencyDays { get; set; } = 30;

        public int CustomerRiskForecastPaymentLagLookbackDays { get; set; } = 90;

        public int CustomerRiskForecastMinSettledFaktursForLag { get; set; } = 2;

        public int CustomerRiskForecastMaxTopCustomers { get; set; } = 20;

        public int CustomerRiskForecastMaxAttentionRows { get; set; } = 25;

        public int CustomerRiskForecastMaxRecommendations { get; set; } = 15;

        public decimal CollectionOptimizationSalesRecoveryOverdueFloorIdr { get; set; } = 500_000m;

        public decimal CollectionOptimizationStrategicPriorMonthOmzetFloorIdr { get; set; } = 5_000_000m;

        public decimal CollectionOptimizationLargeDueSoonFloorIdr { get; set; } = 10_000_000m;

        public int CollectionOptimizationMaxPriorityRows { get; set; } = 30;

        public int CollectionOptimizationMaxQueueRows { get; set; } = 15;

        public int CollectionOptimizationMaxImpactRows { get; set; } = 15;

        public int CollectionOptimizationMaxWorkloadRows { get; set; } = 10;
    }
}
