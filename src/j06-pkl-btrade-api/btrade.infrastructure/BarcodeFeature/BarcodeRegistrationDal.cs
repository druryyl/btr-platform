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

public class BarcodeRegistrationDal : IBarcodeRegistrationDal
{
    private readonly DatabaseOptions _opt;

    public BarcodeRegistrationDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }

    public void Insert(BarcodeRegistrationRequestType model)
    {
        const string sql = @"
            INSERT INTO BTRADE_BarcodeRegistrationRequest(
                BarcodeRegistrationId, ClientRequestId, ServerId, BarcodeValue,
                BrgId, Satuan, RequestedBy, RequestedAt, Status, ProcessedAt,
                ProcessedNote)
            VALUES (
                @BarcodeRegistrationId, @ClientRequestId, @ServerId, @BarcodeValue,
                @BrgId, @Satuan, @RequestedBy, @RequestedAt, @Status, @ProcessedAt,
                @ProcessedNote)";

        var dp = new DynamicParameters();
        dp.AddParam("@BarcodeRegistrationId", model.BarcodeRegistrationId, SqlDbType.VarChar);
        dp.AddParam("@ClientRequestId", model.ClientRequestId, SqlDbType.VarChar);
        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);
        dp.AddParam("@BarcodeValue", model.BarcodeValue, SqlDbType.VarChar);
        dp.AddParam("@BrgId", model.BrgId, SqlDbType.VarChar);
        dp.AddParam("@Satuan", model.Satuan, SqlDbType.VarChar);
        dp.AddParam("@RequestedBy", model.RequestedBy, SqlDbType.VarChar);
        dp.AddParam("@RequestedAt", model.RequestedAt, SqlDbType.DateTime);
        dp.AddParam("@Status", model.Status, SqlDbType.VarChar);
        dp.AddParam("@ProcessedAt", model.ProcessedAt, SqlDbType.DateTime);
        dp.AddParam("@ProcessedNote", model.ProcessedNote, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Update(BarcodeRegistrationRequestType model)
    {
        const string sql = @"
            UPDATE
                BTRADE_BarcodeRegistrationRequest
            SET
                ClientRequestId = @ClientRequestId,
                BarcodeValue = @BarcodeValue,
                BrgId = @BrgId,
                Satuan = @Satuan,
                RequestedBy = @RequestedBy,
                RequestedAt = @RequestedAt,
                Status = @Status,
                ProcessedAt = @ProcessedAt,
                ProcessedNote = @ProcessedNote
            WHERE
                BarcodeRegistrationId = @BarcodeRegistrationId
                AND ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@BarcodeRegistrationId", model.BarcodeRegistrationId, SqlDbType.VarChar);
        dp.AddParam("@ClientRequestId", model.ClientRequestId, SqlDbType.VarChar);
        dp.AddParam("@BarcodeValue", model.BarcodeValue, SqlDbType.VarChar);
        dp.AddParam("@BrgId", model.BrgId, SqlDbType.VarChar);
        dp.AddParam("@Satuan", model.Satuan, SqlDbType.VarChar);
        dp.AddParam("@RequestedBy", model.RequestedBy, SqlDbType.VarChar);
        dp.AddParam("@RequestedAt", model.RequestedAt, SqlDbType.DateTime);
        dp.AddParam("@Status", model.Status, SqlDbType.VarChar);
        dp.AddParam("@ProcessedAt", model.ProcessedAt, SqlDbType.DateTime);
        dp.AddParam("@ProcessedNote", model.ProcessedNote, SqlDbType.VarChar);
        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Delete(IBarcodeRegistrationRequestKey key)
    {
        const string sql = @"
            DELETE FROM
                BTRADE_BarcodeRegistrationRequest
            WHERE
                BarcodeRegistrationId = @BarcodeRegistrationId";

        var dp = new DynamicParameters();
        dp.AddParam("@BarcodeRegistrationId", key.BarcodeRegistrationId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public MayBe<BarcodeRegistrationRequestType> GetData(IBarcodeRegistrationRequestKey key)
    {
        const string sql = @"
            SELECT
                BarcodeRegistrationId, ClientRequestId, ServerId, BarcodeValue,
                BrgId, Satuan, RequestedBy, RequestedAt, Status, ProcessedAt,
                ProcessedNote
            FROM
                BTRADE_BarcodeRegistrationRequest
            WHERE
                BarcodeRegistrationId = @BarcodeRegistrationId";

        var dp = new DynamicParameters();
        dp.AddParam("@BarcodeRegistrationId", key.BarcodeRegistrationId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.ReadSingle<BarcodeRegistrationRequestType>(sql, dp));
    }

    public MayBe<IEnumerable<BarcodeRegistrationRequestType>> ListData(IServerId server)
    {
        const string sql = @"
            SELECT
                BarcodeRegistrationId, ClientRequestId, ServerId, BarcodeValue,
                BrgId, Satuan, RequestedBy, RequestedAt, Status, ProcessedAt,
                ProcessedNote
            FROM
                BTRADE_BarcodeRegistrationRequest
            WHERE
                ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.Read<BarcodeRegistrationRequestType>(sql, dp));
    }
}
