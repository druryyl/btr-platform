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

public class BarcodeDal : IBarcodeDal
{
    private readonly DatabaseOptions _opt;

    public BarcodeDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }

    public void Insert(BarcodeType model)
    {
        const string sql = @"
            INSERT INTO BTRADE_BrgBarcode(
                BrgBarcodeId, BarcodeValue, BrgId, BrgCode, BrgName,
                Satuan, ServerId)
            VALUES (
                @BrgBarcodeId, @BarcodeValue, @BrgId, @BrgCode, @BrgName,
                @Satuan, @ServerId)";

        var dp = new DynamicParameters();
        dp.AddParam("@BrgBarcodeId", model.BrgBarcodeId, SqlDbType.VarChar);
        dp.AddParam("@BarcodeValue", model.BarcodeValue, SqlDbType.VarChar);
        dp.AddParam("@BrgId", model.BrgId, SqlDbType.VarChar);
        dp.AddParam("@BrgCode", model.BrgCode, SqlDbType.VarChar);
        dp.AddParam("@BrgName", model.BrgName, SqlDbType.VarChar);
        dp.AddParam("@Satuan", model.Satuan, SqlDbType.VarChar);
        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Update(BarcodeType model)
    {
        const string sql = @"
            UPDATE
                BTRADE_BrgBarcode
            SET
                BarcodeValue = @BarcodeValue,
                BrgId = @BrgId,
                BrgCode = @BrgCode,
                BrgName = @BrgName,
                Satuan = @Satuan
            WHERE
                BrgBarcodeId = @BrgBarcodeId
                AND ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@BrgBarcodeId", model.BrgBarcodeId, SqlDbType.VarChar);
        dp.AddParam("@BarcodeValue", model.BarcodeValue, SqlDbType.VarChar);
        dp.AddParam("@BrgId", model.BrgId, SqlDbType.VarChar);
        dp.AddParam("@BrgCode", model.BrgCode, SqlDbType.VarChar);
        dp.AddParam("@BrgName", model.BrgName, SqlDbType.VarChar);
        dp.AddParam("@Satuan", model.Satuan, SqlDbType.VarChar);
        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Delete(IBarcodeKey key)
    {
        const string sql = @"
            DELETE FROM
                BTRADE_BrgBarcode
            WHERE
                BrgBarcodeId = @BrgBarcodeId
                AND ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@BrgBarcodeId", key.BrgBarcodeId, SqlDbType.VarChar);
        dp.AddParam("@ServerId", key.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public MayBe<BarcodeType> GetData(IBarcodeKey key)
    {
        const string sql = @"
            SELECT
                BrgBarcodeId, BarcodeValue, BrgId, BrgCode, BrgName,
                Satuan, ServerId
            FROM
                BTRADE_BrgBarcode
            WHERE
                BrgBarcodeId = @BrgBarcodeId
                AND ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@BrgBarcodeId", key.BrgBarcodeId, SqlDbType.VarChar);
        dp.AddParam("@ServerId", key.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.ReadSingle<BarcodeType>(sql, dp));
    }

    public MayBe<IEnumerable<BarcodeType>> ListData(IServerId server)
    {
        const string sql = @"
            SELECT
                BrgBarcodeId, BarcodeValue, BrgId, BrgCode, BrgName,
                Satuan, ServerId
            FROM
                BTRADE_BrgBarcode
            WHERE
                ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.Read<BarcodeType>(sql, dp));
    }
}
