using btrade.domain.BarcodeFeature;
using Nuna.Lib.DataAccessHelper;

namespace btrade.application.Contract;

public interface IUserDal :
    IGetDataMayBe<UserType, IUserKey>
{
}
