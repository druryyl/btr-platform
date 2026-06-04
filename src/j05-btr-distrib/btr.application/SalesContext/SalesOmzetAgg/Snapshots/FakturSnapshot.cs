using System;

namespace btr.application.SalesContext.SalesOmzetAgg.Snapshots
{
    public class FakturSnapshot
    {
        public string FakturId { get; set; }
        public string FakturCode { get; set; }
        public DateTime FakturDate { get; set; }
        public string OrderId { get; set; }
        public decimal FakturTotal { get; set; }
        public string SalesPersonName { get; set; }
        public string CustomerId { get; set; }
        public DateTime VoidDate { get; set; }
    }
}
