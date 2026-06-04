namespace BtrGudang.Winform.Domain
{
    public class QtyType
    {
        public QtyType(int qty, string satuan)
        {
            Qty = qty;
            Satuan = satuan;
        }

        public static QtyType Default => new QtyType(
            0, "-");

        public int Qty { get; private set; }
        public string Satuan { get; private set; }
    }
}
