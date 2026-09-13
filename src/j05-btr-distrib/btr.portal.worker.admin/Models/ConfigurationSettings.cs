namespace btr.portal.worker.admin.Models
{
    public class ConfigurationSettings
    {
        public string ServerName { get; set; }
        public string DbName { get; set; }
        public bool IsTest { get; set; }
        public bool PresentationEnabled { get; set; }
        public string BusinessDate { get; set; }
        public int PiutangIntervalMinutes { get; set; }
        public int InventoryIntervalMinutes { get; set; }
        public int InventoryRiskIntervalMinutes { get; set; }
        public int SalesIntervalMinutes { get; set; }
        public int PurchasingIntervalMinutes { get; set; }
        public int CustomerIntervalMinutes { get; set; }
        public int SalesmanIntervalMinutes { get; set; }
        public int CollectionIntervalMinutes { get; set; }
        public int FieldActivityIntervalMinutes { get; set; }
        public int LocationIntervalMinutes { get; set; }
        public int PurchasingManagementIntervalMinutes { get; set; }
        public int PurchasingQualifiedBacklogDays { get; set; }
        public string[] EnabledEntityTypes { get; set; }
        public int HistoryRetentionMonths { get; set; }
    }
}
