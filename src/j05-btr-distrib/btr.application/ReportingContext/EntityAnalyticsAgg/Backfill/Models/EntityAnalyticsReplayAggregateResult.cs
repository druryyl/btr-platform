namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public sealed class EntityAnalyticsReplayAggregateResult
    {
        public string EntityType { get; set; }

        public object ProduceInput { get; set; }

        public int EntityCount { get; set; }

        public EntityAnalyticsReplayRowCounts RowCounts { get; set; }
            = new EntityAnalyticsReplayRowCounts();
    }

    public sealed class EntityAnalyticsReplayRowCounts
    {
        public int TransactionRowCount { get; set; }

        public int MasterRowCount { get; set; }

        public int SupportingRowCount { get; set; }
    }
}
