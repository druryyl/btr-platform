namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class EntityMapPresetDefinition
    {
        public string PresetId { get; set; }

        public string EntityType { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string AxisXKpiId { get; set; }

        public string AxisYKpiId { get; set; }

        public string BubbleKpiId { get; set; }

        public bool IsDefault { get; set; }

        public string FilterDimensionKpiId { get; set; }
    }
}
