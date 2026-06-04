namespace btr.domain.InventoryContext.PackingOrderFeature
{
    public class PackingOrderItemModel
    {
        public PackingOrderItemModel(int noUrut, string brgId, string brgCode, string brgName, 
            string kategoriName, string supplierName,
            int qtyKecil, string satKecil, int qtyBesar, string satBesar, string depoId)
        {
            NoUrut = noUrut;
            BrgId = brgId;
            BrgCode = brgCode;
            BrgName = brgName;
            KategoriName = kategoriName;
            SupplierName = supplierName;
            QtyKecil = qtyKecil;
            SatKecil = satKecil;
            QtyBesar = qtyBesar;
            SatBesar = satBesar;
            DepoId = depoId;
        }

        public int NoUrut { get; }
        public string BrgId { get; private set; }
        public string BrgCode { get; private set; }
        public string BrgName { get; private set; }
        public string KategoriName { get; private set; }
        public string SupplierName { get; private set; }
        public int QtyKecil { get; private set; }
        public string SatKecil { get; private set; }

        public int QtyBesar { get; private set; }
        public string SatBesar { get; private set; }
        public string DepoId { get; private set; }
    }
}
