using btrade.domain.DriverFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;

namespace btrade.application.Contract;

public interface IDriverDal :
    IInsert<DriverType>,
    IUpdate<DriverType>,
    IDelete<IDriverKey>,
    IGetDataMayBe<DriverType, IDriverKey>,
    IListDataMayBe<DriverType, IServerId>
{
    void Delete(IServerId server);
}
