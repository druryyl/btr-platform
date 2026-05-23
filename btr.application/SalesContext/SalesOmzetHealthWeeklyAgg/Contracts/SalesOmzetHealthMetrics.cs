using System;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Contracts
{
    public class SalesOmzetHealthMetrics
    {
        public int MissingOrders { get; set; }
        public int MissingDirectFakturs { get; set; }
        public int UnlinkedFakturs { get; set; }
        public int AggregateRowsInScope { get; set; }
        public DateTime? LastReconciledMax { get; set; }
        public int StaleFakturEstimate { get; set; }
    }
}
