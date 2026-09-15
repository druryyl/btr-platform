using System.Collections.Generic;
using btr.domain.BrgContext.BrgAgg;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.nuna.Infrastructure;

namespace btr.application.BrgContext.BrgBarcodeAgg
{
    public interface IBrgBarcodeDal :
        IInsert<BrgBarcodeModel>,
        IUpdate<BrgBarcodeModel>,
        IGetData<BrgBarcodeModel, IBrgBarcodeKey>,
        IListData<BrgBarcodeModel>
    {
        BrgBarcodeModel GetByValue(string barcodeValueKey);
        IEnumerable<BrgBarcodeModel> ListByBrg(IBrgKey brg);
        IEnumerable<BrgBarcodeModel> ListChanged(byte[] watermark);
        bool ExistsByValue(string barcodeValueKey);
    }
}
