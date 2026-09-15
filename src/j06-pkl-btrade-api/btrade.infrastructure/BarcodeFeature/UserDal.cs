using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.PatternHelper;
using System.Data;
using System.Data.SqlClient;

namespace btrade.infrastructure.BarcodeFeature;

public class UserDal : IUserDal
{
    private readonly DatabaseOptions _opt;

    public UserDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }

    public MayBe<UserType> GetData(IUserKey key)
    {
        const string sql = @"
            SELECT
                UserId, UserName, Password, RoleId, IsAktif, ServerId
            FROM
                BTRADE_User
            WHERE
                UserId = @UserId";

        var dp = new DynamicParameters();
        dp.AddParam("@UserId", key.UserId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.ReadSingle<UserType>(sql, dp));
    }
}
