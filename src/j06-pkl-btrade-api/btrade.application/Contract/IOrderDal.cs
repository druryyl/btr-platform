using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.ValidationHelper;

namespace btrade.application.Contract
{
    public interface IOrderDal :
        IInsert<OrderModel>,
        IUpdate<OrderModel>,
        IDelete<IOrderKey>,
        IGetDataMayBe<OrderModel, IOrderKey>,
        IListDataMayBe<OrderModel, Periode, IServerId>
    {
        void Delete(IServerId server);
    }
}
