using btrade.application.Contract;
using btrade.domain.DriverFeature;
using btrade.domain.SalesFeature;
using btrade.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.PatternHelper;
using System.Data;
using System.Data.SqlClient;

namespace btrade.infrastructure.Repository;

public class DriverDal : IDriverDal
{
    private readonly DatabaseOptions _opt;

    public DriverDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }

    public void Insert(DriverType model)
    {
        const string sql = @"
            INSERT INTO BTRADE_Driver(
                DriverId, DriverName, IsAktif, ServerId
            ) VALUES (
                @DriverId, @DriverName, @IsAktif, @ServerId)";

        var dp = new DynamicParameters();
        dp.AddParam("@DriverId", model.DriverId, SqlDbType.VarChar);
        dp.AddParam("@DriverName", model.DriverName, SqlDbType.VarChar);
        dp.AddParam("@IsAktif", model.IsAktif, SqlDbType.Bit);
        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Update(DriverType model)
    {
        const string sql = @"
            UPDATE
                BTRADE_Driver
            SET
                DriverName = @DriverName,
                IsAktif = @IsAktif
            WHERE
                DriverId = @DriverId
                AND ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@DriverId", model.DriverId, SqlDbType.VarChar);
        dp.AddParam("@DriverName", model.DriverName, SqlDbType.VarChar);
        dp.AddParam("@IsAktif", model.IsAktif, SqlDbType.Bit);
        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Delete(IDriverKey key)
    {
        const string sql = @"
            DELETE FROM
                BTRADE_Driver
            WHERE
                DriverId = @DriverId
                AND ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@DriverId", key.DriverId, SqlDbType.VarChar);
        dp.AddParam("@ServerId", key.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Delete(IServerId server)
    {
        const string sql = @"
            DELETE FROM
                BTRADE_Driver
            WHERE
                ServerId = @ServerId ";

        var dp = new DynamicParameters();
        dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public MayBe<DriverType> GetData(IDriverKey key)
    {
        const string sql = @"
            SELECT
                DriverId, DriverName, IsAktif, ServerId
            FROM
                BTRADE_Driver
            WHERE
                DriverId = @DriverId
                AND ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@DriverId", key.DriverId, SqlDbType.VarChar);
        dp.AddParam("@ServerId", key.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.ReadSingle<DriverType>(sql, dp));
    }

    public MayBe<IEnumerable<DriverType>> ListData(IServerId server)
    {
        const string sql = @"
            SELECT
                DriverId, DriverName, IsAktif, ServerId
            FROM
                BTRADE_Driver
            WHERE
                ServerId = @ServerId ";

        var dp = new DynamicParameters();
        dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.Read<DriverType>(sql, dp));
    }
}
