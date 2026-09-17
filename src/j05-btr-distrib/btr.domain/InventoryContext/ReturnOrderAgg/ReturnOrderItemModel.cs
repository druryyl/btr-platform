using btr.domain.BrgContext.BrgAgg;

namespace btr.domain.InventoryContext.ReturnOrderAgg
{
    public class ReturnOrderItemModel : IReturnOrderKey, IBrgKey
    {
        public string ReturnOrderId { get; set; }
        public int NoUrut { get; set; }
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public decimal Qty { get; set; }
        public string SatId { get; set; }
        public string JenisRetur { get; set; }
    }
}
