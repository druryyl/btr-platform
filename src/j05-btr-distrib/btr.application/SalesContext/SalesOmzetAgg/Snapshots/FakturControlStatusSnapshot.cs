using System;
using btr.domain.SalesContext.FakturControlAgg;

namespace btr.application.SalesContext.SalesOmzetAgg.Snapshots
{
    public class FakturControlStatusSnapshot
    {
        public string FakturId { get; set; }
        public StatusFakturEnum StatusFaktur { get; set; }
        public DateTime StatusDate { get; set; }
    }
}
