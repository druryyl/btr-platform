using System;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg
{
    public class SalesOmzetMaterializeHealth
    {
        public Periode Window { get; set; }
        public int MissingOrders { get; set; }
        public int MissingDirectFakturs { get; set; }
        public int UnlinkedFakturs { get; set; }
        public int AggregateRowsInScope { get; set; }
        public DateTime? LastReconciledMax { get; set; }
        public int StaleFakturEstimate { get; set; }
        public SalesOmzetMaterializeHealthLevel Level { get; set; }

        public int TotalMissing => MissingOrders + MissingDirectFakturs + UnlinkedFakturs;
    }
}
