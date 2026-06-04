using BtrGudang.Winform.Domain;

namespace BtrGudang.Domain.PackingOrderFeature
{
    public class PackingOrderItemModel
    {
        public PackingOrderItemModel(int noUrut, BrgReff brg, QtyType qtyBesar, QtyType qtyKecil,
            string depoId, string printLogId)
        {
            NoUrut = noUrut;
            Brg = brg;
            QtyBesar = qtyBesar;
            QtyKecil = qtyKecil;
            DepoId = depoId;
            PrintLogId = printLogId;
        }
        public int NoUrut { get; }
        public BrgReff Brg { get; }
        public QtyType QtyBesar { get; }
        public QtyType QtyKecil { get; }
        public string DepoId { get; }
        public string PrintLogId { get; private set; }
        public void PrintLogBrg(PrintLogType printLog)
        {
            if (printLog.DocType != "PER-SUPPLIER")
                return;
            PrintLogId = printLog.PrintLogId;
        }
    }
}
