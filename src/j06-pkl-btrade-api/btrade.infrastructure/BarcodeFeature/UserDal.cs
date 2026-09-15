using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
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

    public void Insert(UserType model)
    {
        const string sql = @"
            INSERT INTO BTRADE_User(
                UserId, UserName, Password, RoleId, IsAktif, ServerId)
            VALUES (
                @UserId, @UserName, @Password, @RoleId, @IsAktif, @ServerId)";

        var dp = new DynamicParameters();
        dp.AddParam("@UserId", model.UserId, SqlDbType.VarChar);
        dp.AddParam("@UserName", model.UserName, SqlDbType.VarChar);
        dp.AddParam("@Password", model.Password, SqlDbType.VarChar);
        dp.AddParam("@RoleId", model.RoleId, SqlDbType.VarChar);
        dp.AddParam("@IsAktif", model.IsAktif, SqlDbType.Bit);
        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Delete(IServerId server)
    {
        const string sql = @"
            DELETE FROM
                BTRADE_User
            WHERE
                ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
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
