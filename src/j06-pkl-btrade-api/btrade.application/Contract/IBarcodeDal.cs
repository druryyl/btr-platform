using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;

namespace btrade.application.Contract;

public interface IBarcodeDal :
    IInsert<BarcodeType>,
    IUpdate<BarcodeType>,
    IDelete<IBarcodeKey>,
    IGetDataMayBe<BarcodeType, IBarcodeKey>,
    IListDataMayBe<BarcodeType, IServerId>
{
}
