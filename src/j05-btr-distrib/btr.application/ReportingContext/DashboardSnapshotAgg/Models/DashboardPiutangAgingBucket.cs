namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardPiutangAgingBucket
    {
        public string BucketKey { get; set; }

        public string BucketLabel { get; set; }

        public decimal Amount { get; set; }

        public int SortOrder { get; set; }
    }
}
