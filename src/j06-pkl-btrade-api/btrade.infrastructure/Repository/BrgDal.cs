using btrade.application.Contract;
using btrade.domain.SalesFeature;
using btrade.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.PatternHelper;
using System.Data;
using System.Data.SqlClient;

namespace btrade.infrastructure.Repository;

public class BrgDal : IBrgDal
{
    private readonly DatabaseOptions _opt;

    public BrgDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }

    public void Insert(BrgType model)
    {
        const string sql = @"
            INSERT INTO BTRADE_Brg(
                BrgId, BrgCode, BrgName, KategoriName, 
                SatBesar, SatKecil, Konversi, HrgSat, Stok, ServerId)
            VALUES (
                @BrgId, @BrgCode, @BrgName, @KategoriName, 
                @SatBesar, @SatKecil, @Konversi, @HrgSat, @Stok, @ServerId)";

        var dp = new DynamicParameters();
        dp.AddParam("@BrgId", model.BrgId, SqlDbType.VarChar); 
        dp.AddParam("@BrgCode", model.BrgCode, SqlDbType.VarChar); 
        dp.AddParam("@BrgName", model.BrgName, SqlDbType.VarChar); 
        dp.AddParam("@KategoriName", model.KategoriName, SqlDbType.VarChar); 
        dp.AddParam("@SatBesar", model.SatBesar, SqlDbType.VarChar); 
        dp.AddParam("@SatKecil", model.SatKecil, SqlDbType.VarChar); 
        dp.AddParam("@Konversi", model.Konversi, SqlDbType.Int); 
        dp.AddParam("@HrgSat", model.HrgSat, SqlDbType.Decimal); 
        dp.AddParam("@Stok", model.Stok, SqlDbType.Int);
        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Update(BrgType model)
    {
        const string sql = @"
            UPDATE
                BTRADE_Brg
            SET
                BrgCode = @BrgCode, 
                BrgName = @BrgName, 
                KategoriName = @KategoriName, 
                SatBesar = @SatBesar, 
                SatKecil = @SatKecil, 
                Konversi = @Konversi, 
                HrgSat = @HrgSat, 
                Stok = @Stok,
                ServerId = @ServerId
            WHERE
                BrgId = @BrgId";

        var dp = new DynamicParameters();
        dp.AddParam("@BrgId", model.BrgId, SqlDbType.VarChar);
        dp.AddParam("@BrgCode", model.BrgCode, SqlDbType.VarChar);
        dp.AddParam("@BrgName", model.BrgName, SqlDbType.VarChar);
        dp.AddParam("@KategoriName", model.KategoriName, SqlDbType.VarChar);
        dp.AddParam("@SatBesar", model.SatBesar, SqlDbType.VarChar);
        dp.AddParam("@SatKecil", model.SatKecil, SqlDbType.VarChar);
        dp.AddParam("@Konversi", model.Konversi, SqlDbType.Int);
        dp.AddParam("@HrgSat", model.HrgSat, SqlDbType.Decimal);
        dp.AddParam("@Stok", model.Stok, SqlDbType.Int);
        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Delete(IBrgKey key)
    {
        const string sql = @"
            DELETE FROM
                BTRADE_Brg
            WHERE
                BrgId = @BrgId
                AND ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@BrgId", key.BrgId, SqlDbType.VarChar);
        dp.AddParam("@ServerId", key.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt)); 
        conn.Execute(sql, dp);
    }

    public void Delete(IServerId server)
    {
        const string sql = @"
            DELETE FROM
                BTRADE_Brg
            WHERE
                ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public MayBe<BrgType> GetData(IBrgKey key)
    {
        const string sql = @"
            SELECT
                BrgId, BrgCode, BrgName, KategoriName, 
                SatBesar, SatKecil, Konversi, HrgSat, Stok, ServerId
            FROM
                BTRADE_Brg
            WHERE
                BrgId = @BrgId
                AND ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@BrgId", key.BrgId, SqlDbType.VarChar);
        dp.AddParam("@ServerId", key.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.ReadSingle<BrgType>(sql, dp));
    }

    public MayBe<IEnumerable<BrgType>> ListData(IServerId server)
    {
        const string sql = @"
            SELECT
                BrgId, BrgCode, BrgName, KategoriName, 
                SatBesar, SatKecil, Konversi, HrgSat, Stok, ServerId
            FROM
                BTRADE_Brg 
            WHERE
                ServerId = @ServerId ";

        var dp = new DynamicParameters();
        dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.Read<BrgType>(sql, dp));
    }
}