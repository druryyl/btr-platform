using btrade.application.WarehouseFreature;
using btrade.domain.WarehouseFeature;
using btrade.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;
using Nuna.Lib.DataAccessHelper;
using System.Data;
using System.Data.SqlClient;

namespace btrade.infrastructure.WarehouseFeature;
public class PackingOrderDal : IPackingOrderDal
{
    private readonly DatabaseOptions _opt;

    public PackingOrderDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }

    public void Insert(PackingOrderModel model)
    {
        const string sql = @"
            INSERT INTO BTRADE_PackingOrder(
                PackingOrderId, PackingOrderDate,
                CustomerId, CustomerCode, CustomerName, Alamat, NoTelp,
                Latitude, Longitude, Accuracy,
                FakturId, FakturCode, FakturDate, AdminName, GrandTotal,
                DriverId, DriverName, WarehouseDesc, OfficeCode, Note)
            VALUES(
                @PackingOrderId, @PackingOrderDate, 
                @CustomerId, @CustomerCode, @CustomerName, @Alamat, @NoTelp,
                @Latitude, @Longitude, @Accuracy,
                @FakturId, @FakturCode, @FakturDate, @AdminName, @GrandTotal,
                @DriverId, @DriverName, @WarehouseDesc, @OfficeCode, @Note)
            ";

        var dp = new DynamicParameters();
        dp.AddParam("@PackingOrderId", model.PackingOrderId, SqlDbType.VarChar);
        dp.AddParam("@PackingOrderDate", model.PackingOrderDate, SqlDbType.DateTime);

        dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
        dp.AddParam("@CustomerCode", model.CustomerCode, SqlDbType.VarChar);
        dp.AddParam("@CustomerName", model.CustomerName, SqlDbType.VarChar);
        dp.AddParam("@Alamat", model.Alamat, SqlDbType.VarChar);
        dp.AddParam("@NoTelp", model.NoTelp, SqlDbType.VarChar);
        dp.AddParam("@Latitude", model.Latitude, SqlDbType.Float);
        dp.AddParam("@Longitude", model.Longitude, SqlDbType.Float);
        dp.AddParam("@Accuracy", model.Accuracy, SqlDbType.Float);

        dp.AddParam("@FakturId", model.FakturId, SqlDbType.VarChar);
        dp.AddParam("@FakturCode", model.FakturCode, SqlDbType.VarChar);
        dp.AddParam("@FakturDate", model.FakturDate, SqlDbType.DateTime);
        dp.AddParam("@AdminName", model.AdminName, SqlDbType.VarChar);
        dp.AddParam("@GrandTotal", model.GrandTotal, SqlDbType.Decimal);

        dp.AddParam("@DriverId", model.DriverId, SqlDbType.VarChar);
        dp.AddParam("@DriverName", model.DriverName, SqlDbType.VarChar);

        dp.AddParam("@WarehouseDesc", model.WarehouseDesc, SqlDbType.VarChar);
        dp.AddParam("@OfficeCode", model.OfficeCode, SqlDbType.VarChar);
        dp.AddParam("@Note", model.Note, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Update(PackingOrderModel model)
    {
        const string sql = @"
            UPDATE BTRADE_PackingOrder
            SET
                PackingOrderDate = @PackingOrderDate,

                CustomerId = @CustomerId,
                CustomerCode = @CustomerCode,
                CustomerName = @CustomerName,
                Alamat = @Alamat,
                NoTelp = @NoTelp,
                Latitude = @Latitude,
                Longitude = @Longitude,
                Accuracy = @Accuracy,

                FakturId = @FakturId,
                FakturCode = @FakturCode,
                FakturDate = @FakturDate,
                AdminName = @AdminName,
                GrandTotal = @GrandTotal,

                DriverId = @DriverId,
                DriverName = @DriverName,

                WarehouseDesc = @WarehouseDesc,
                OfficeCode = @OfficeCode,
                Note = @Note
                
            WHERE
                PackingOrderId = @PackingOrderId
            ";

        var dp = new DynamicParameters();
        dp.AddParam("@PackingOrderId", model.PackingOrderId, SqlDbType.VarChar);
        dp.AddParam("@PackingOrderDate", model.PackingOrderDate, SqlDbType.DateTime);

        dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
        dp.AddParam("@CustomerCode", model.CustomerCode, SqlDbType.VarChar);
        dp.AddParam("@CustomerName", model.CustomerName, SqlDbType.VarChar);
        dp.AddParam("@Alamat", model.Alamat, SqlDbType.VarChar);
        dp.AddParam("@NoTelp", model.NoTelp, SqlDbType.VarChar);
        dp.AddParam("@Latitude", model.Latitude, SqlDbType.Float);
        dp.AddParam("@Longitude", model.Longitude, SqlDbType.Float);
        dp.AddParam("@Accuracy", model.Accuracy, SqlDbType.Float);

        dp.AddParam("@FakturId", model.FakturId, SqlDbType.VarChar);
        dp.AddParam("@FakturCode", model.FakturCode, SqlDbType.VarChar);
        dp.AddParam("@FakturDate", model.FakturDate, SqlDbType.DateTime);
        dp.AddParam("@AdminName", model.AdminName, SqlDbType.VarChar);
        dp.AddParam("@GrandTotal", model.GrandTotal, SqlDbType.Decimal);

        dp.AddParam("@DriverId", model.DriverId, SqlDbType.VarChar);
        dp.AddParam("@DriverName", model.DriverName, SqlDbType.VarChar);

        dp.AddParam("@WarehouseDesc", model.WarehouseDesc, SqlDbType.VarChar);
        dp.AddParam("@OfficeCode", model.OfficeCode, SqlDbType.VarChar);
        dp.AddParam("@Note", model.Note, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Delete(IPackingOrderKey key)
    {
        const string sql = @"
            DELETE FROM BTRADE_PackingOrder
            WHERE PackingOrderId = @PackingOrderId
            ";

        var dp = new DynamicParameters();
        dp.AddParam("@PackingOrderId", key.PackingOrderId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public PackingOrderModel GetData(IPackingOrderKey key)
    {
        const string sql = @"
            SELECT
                PackingOrderId, PackingOrderDate, 
                CustomerId, CustomerCode, CustomerName, Alamat, NoTelp,
                Latitude, Longitude, Accuracy,
                FakturId, FakturCode, FakturDate, AdminName, GrandTotal,
                DriverId, DriverName, WarehouseDesc, OfficeCode, Note
            FROM BTRADE_PackingOrder
            WHERE PackingOrderId = @PackingOrderId
            ";

        var dp = new DynamicParameters();
        dp.AddParam("@PackingOrderId", key.PackingOrderId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return conn.ReadSingle<PackingOrderModel>(sql, dp);
    }

    public IEnumerable<PackingOrderView> ListData(DateTime timeStamp, string depoId)
    {
        const string sql = @"
            SELECT TOP 500
                aa.PackingOrderId, aa.PackingOrderDate, 
                aa.CustomerId, aa.CustomerCode, aa.CustomerName, aa.Alamat, aa.NoTelp,
                aa.Latitude, aa.Longitude, aa.Accuracy,
                aa.FakturId, aa.FakturCode, aa.FakturDate, aa.AdminName, aa.GrandTotal,
                aa.DriverId, aa.DriverName, aa.WarehouseDesc, aa.OfficeCode, bb.UpdateTimestamp, Note
            FROM 
                BTRADE_PackingOrder aa
                INNER JOIN BTRADE_PackingOrderDepo bb ON aa.PackingOrderId = bb.PackingOrderId
            WHERE 
                bb.UpdateTimestamp >= @Timestamp 
                AND bb.DepoId = @DepoId
            ORDER BY bb.UpdateTimestamp ASC
            ";

        var dp = new DynamicParameters();
        dp.AddParam("@Timestamp", timeStamp, SqlDbType.DateTime);
        dp.AddParam("@DepoId", depoId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return conn.Read<PackingOrderView>(sql, dp);
    }
}