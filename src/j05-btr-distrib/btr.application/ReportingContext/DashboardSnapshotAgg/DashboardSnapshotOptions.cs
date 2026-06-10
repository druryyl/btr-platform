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
    }
}
