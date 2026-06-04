using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;

namespace btrade.application.Contract
{
    public interface IWilayahDal :
        IInsert<WilayahType>,
        IUpdate<WilayahType>,
        IDelete<IWilayahKey>,
        IGetDataMayBe<WilayahType, IWilayahKey>,
        IListDataMayBe<WilayahType, IServerId>
    {
        void Delete(IServerId server);
    }
}
