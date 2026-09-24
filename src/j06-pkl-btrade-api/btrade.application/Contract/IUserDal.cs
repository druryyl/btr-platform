using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.PatternHelper;

namespace btrade.application.Contract;

public interface IUserDal :
    IInsert<UserType>,
    IGetDataMayBe<UserType, IUserKey>
{
    void Delete(IServerId serverId);

    /// <summary>
    /// TD-14 — read the BTR user mapped to a Google email (trimmed,
    /// case-insensitive against <c>IX_BTRADE_User_Email</c>). Returns
    /// <see cref="MayBe{T}.None"/> for a blank or unmapped email.
    /// </summary>
    MayBe<UserType> GetByEmail(string email);
}
