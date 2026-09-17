using btrade.domain.ReturnOrderFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.ValidationHelper;

namespace btrade.application.Contract
{
    public interface IReturnOrderDal :
        IInsert<ReturnOrderType>,
        IUpdate<ReturnOrderType>,
        IDelete<IReturnOrderKey>,
        IGetDataMayBe<ReturnOrderType, IReturnOrderKey>,
        IListDataMayBe<ReturnOrderType, Periode, IServerId>
    {
        void Delete(IServerId server);
    }
}
