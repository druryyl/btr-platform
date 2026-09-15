using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;

namespace btrade.application.Contract;

public interface IUserDal :
    IInsert<UserType>,
    IGetDataMayBe<UserType, IUserKey>
{
    void Delete(IServerId serverId);
}
