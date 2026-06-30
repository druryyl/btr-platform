namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot
{
    public class EntityPopulationRow
    {
        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public bool IsActive { get; set; }

        public string DimensionValue { get; set; }
    }
}
