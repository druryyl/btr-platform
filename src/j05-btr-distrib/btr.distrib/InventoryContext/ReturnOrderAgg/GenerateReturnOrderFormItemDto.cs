namespace btr.distrib.InventoryContext.ReturnOrderAgg
{
    public class GenerateReturnOrderFormItemDto
    {
        public GenerateReturnOrderFormItemDto()
        {
        }

        public GenerateReturnOrderFormItemDto(int noUrut, string brgId, string brgCode,
            string brgName, decimal qty, string satId, string jenisRetur)
        {
            NoUrut = noUrut;
            BrgId = brgId;
            BrgCode = brgCode;
            BrgName = brgName;
            Qty = qty;
            SatId = satId;
            JenisRetur = jenisRetur;
        }

        public int NoUrut { get; set; }
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public decimal Qty { get; set; }
        public string SatId { get; set; }
        public string JenisRetur { get; set; }
    }
}
