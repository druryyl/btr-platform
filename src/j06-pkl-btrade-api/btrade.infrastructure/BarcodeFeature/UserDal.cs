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
                UserId, UserName, Password, RoleId, IsAktif, ServerId, Email)
            VALUES (
                @UserId, @UserName, @Password, @RoleId, @IsAktif, @ServerId, @Email)";

        var dp = new DynamicParameters();
        dp.AddParam("@UserId", model.UserId, SqlDbType.VarChar);
        dp.AddParam("@UserName", model.UserName, SqlDbType.VarChar);
        dp.AddParam("@Password", model.Password, SqlDbType.VarChar);
        dp.AddParam("@RoleId", model.RoleId, SqlDbType.VarChar);
        dp.AddParam("@IsAktif", model.IsAktif, SqlDbType.Bit);
        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);
        //  EXT-02 — the legacy j07-btrade-sync payload has no Email member; the
        //  model default ('') keeps the NOT NULL column safe.
        dp.AddParam("@Email", model.Email ?? string.Empty, SqlDbType.VarChar);

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
                UserId, UserName, Password, RoleId, IsAktif, ServerId, Email
            FROM
                BTRADE_User
            WHERE
                UserId = @UserId";

        var dp = new DynamicParameters();
        dp.AddParam("@UserId", key.UserId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.ReadSingle<UserType>(sql, dp));
    }

    public MayBe<UserType> GetByEmail(string email)
    {
        //  TD-14 — the lookup email is trimmed; case-insensitivity is provided
        //  by the database's case-insensitive collation on the indexed Email
        //  column (IX_BTRADE_User_Email), matching SalesPersonDal.GetByEmail.
        //  A blank email is never a match: unmapped rows hold Email = ''.
        var normalized = (email ?? string.Empty).Trim();
        if (normalized.Length == 0)
            return MayBe<UserType>.None;

        const string sql = @"
            SELECT
                UserId, UserName, Password, RoleId, IsAktif, ServerId, Email
            FROM
                BTRADE_User
            WHERE
                Email = @Email";

        var dp = new DynamicParameters();
        dp.AddParam("@Email", normalized, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.ReadSingle<UserType>(sql, dp));
    }
}
