namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    /// <summary>Normalized attention signal emitted by an entity producer at refresh time.</summary>
    public class EntityAttentionSignalSnapshot
    {
        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string SignalCode { get; set; }

        public string SignalCategory { get; set; }

        public string SignalTitle { get; set; }
    }
}
