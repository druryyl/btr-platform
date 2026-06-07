namespace btr.application.ReportingContext.DashboardSnapshotAgg
{
    public class DashboardSnapshotOptions
    {
        public const string SECTION_NAME = "DashboardSnapshot";

        public int PiutangIntervalMinutes { get; set; } = 15;

        public int InventoryIntervalMinutes { get; set; } = 60;

        public int SalesIntervalMinutes { get; set; } = 30;

        public int PurchasingIntervalMinutes { get; set; } = 30;
    }
}
