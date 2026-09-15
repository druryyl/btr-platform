using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;

namespace btrade.application.Contract;

public interface IBarcodeRegistrationDal :
    IInsert<BarcodeRegistrationRequestType>,
    IUpdate<BarcodeRegistrationRequestType>,
    IDelete<IBarcodeRegistrationRequestKey>,
    IGetDataMayBe<BarcodeRegistrationRequestType, IBarcodeRegistrationRequestKey>,
    IListDataMayBe<BarcodeRegistrationRequestType, IServerId>
{
}
