using System;

namespace btr.application.SalesContext.SalesOmzetAgg.Snapshots
{
    public class OrderSnapshot
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal OrderTotal { get; set; }
        public string SalesName { get; set; }
        public string UserEmail { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string StatusSync { get; set; }
    }
}
