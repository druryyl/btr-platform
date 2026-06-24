namespace btr.application.ReportingContext.EntityAnalyticsAgg.Options
{
    public class EntityAnalyticsOptions
    {
        public const string SECTION_NAME = "EntityAnalytics";

        public string[] EnabledEntityTypes { get; set; } = new string[0];

        public int HistoryRetentionMonths { get; set; } = 36;
    }
}
