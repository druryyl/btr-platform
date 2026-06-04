using Dapper;
using j07_btrade_sync.Model;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.ValidationHelper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace j07_btrade_sync.Repository
{
    public class PackingOrderDal
    {
        public void Insert(PackingOrderModel dto)
        {
            const string sql = @"
                INSERT INTO BTR_PackingOrder(
                    PackingOrderId, PackingOrderDate,
                    CustomerId, CustomerCode, CustomerName, Alamat, NoTelp,
                    Latitude, Longitude, Accuracy,
                    FakturId, FakturCode, FakturDate, AdminName, GrandTotal,
                    DriverId, DriverName, UploadTimestamp, Note)
                VALUES(
                    @PackingOrderId, @PackingOrderDate,
                    @CustomerId, @CustomerCode, @CustomerName, @Alamat, @NoTelp,
                    @Latitude, @Longitude, @Accuracy,
                    @FakturId, @FakturCode, @FakturDate, @AdminName, @GrandTotal,
                    @DriverId, @DriverName, @UploadTimestamp, @Note)
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", dto.PackingOrderId, SqlDbType.VarChar);
            dp.AddParam("@PackingOrderDate", dto.PackingOrderDate, SqlDbType.DateTime);

            dp.AddParam("@CustomerId", dto.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@CustomerCode", dto.CustomerCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerName", dto.CustomerName, SqlDbType.VarChar);
            dp.AddParam("@Alamat", dto.Alamat, SqlDbType.VarChar);
            dp.AddParam("@NoTelp", dto.NoTelp, SqlDbType.VarChar);

            dp.AddParam("@Latitude", dto.Latitude, SqlDbType.Decimal);
            dp.AddParam("@Longitude", dto.Longitude, SqlDbType.Decimal);
            dp.AddParam("@Accuracy", dto.Accuracy, SqlDbType.Int);

            dp.AddParam("@FakturId", dto.FakturId, SqlDbType.VarChar);
            dp.AddParam("@FakturCode", dto.FakturCode, SqlDbType.VarChar);
            dp.AddParam("@FakturDate", dto.FakturDate, SqlDbType.DateTime);
            dp.AddParam("@AdminName", dto.AdminName, SqlDbType.VarChar);
            dp.AddParam("@GrandTotal", dto.GrandTotal, SqlDbType.Decimal);

            dp.AddParam("@DriverId", dto.DriverId, SqlDbType.VarChar);
            dp.AddParam("@DriverName", dto.DriverName, SqlDbType.VarChar);
            dp.AddParam("@UploadTimestamp", dto.UploadTimestamp, SqlDbType.DateTime);
            dp.AddParam("@Note", dto.Note, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                conn.Execute(sql, dp);
            }
        }

        public void Update(PackingOrderModel dto)
        {
            const string sql = @"
                UPDATE BTR_PackingOrder
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
                    UploadTimestamp = @UploadTimestamp,
                    Note = @note
                WHERE
                    PackingOrderId = @PackingOrderId
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", dto.PackingOrderId, SqlDbType.VarChar);
            dp.AddParam("@PackingOrderDate", dto.PackingOrderDate, SqlDbType.DateTime);

            dp.AddParam("@CustomerId", dto.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@CustomerCode", dto.CustomerCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerName", dto.CustomerName, SqlDbType.VarChar);
            dp.AddParam("@Alamat", dto.Alamat, SqlDbType.VarChar);
            dp.AddParam("@NoTelp", dto.NoTelp, SqlDbType.VarChar);

            dp.AddParam("@Latitude", dto.Latitude, SqlDbType.Decimal);
            dp.AddParam("@Longitude", dto.Longitude, SqlDbType.Decimal);
            dp.AddParam("@Accuracy", dto.Accuracy, SqlDbType.Int);

            dp.AddParam("@FakturId", dto.FakturId, SqlDbType.VarChar);
            dp.AddParam("@FakturCode", dto.FakturCode, SqlDbType.VarChar);
            dp.AddParam("@FakturDate", dto.FakturDate, SqlDbType.DateTime);
            dp.AddParam("@AdminName", dto.AdminName, SqlDbType.VarChar);
            dp.AddParam("@GrandTotal", dto.GrandTotal, SqlDbType.Decimal);

            dp.AddParam("@DriverId", dto.DriverId, SqlDbType.VarChar);
            dp.AddParam("@DriverName", dto.DriverName, SqlDbType.VarChar);
            dp.AddParam("@UploadTimestamp", dto.UploadTimestamp, SqlDbType.DateTime);
            dp.AddParam("@Note", dto.Note, SqlDbType.VarChar);


            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                conn.Execute(sql, dp);
            }
        }

        public void Delete(IPackingOrderKey key)
        {
            const string sql = @"
                DELETE FROM BTR_PackingOrder
                WHERE PackingOrderId = @PackingOrderId
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", key.PackingOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                conn.Execute(sql, dp);
            }
        }

        public PackingOrderModel GetData(IPackingOrderKey key)
        {
            const string sql = @"
                SELECT
                    PackingOrderId, PackingOrderDate, 
                    CustomerId, CustomerCode, CustomerName, Alamat, NoTelp,
                    Latitude, Longitude, Accuracy,
                    FakturId, FakturCode, FakturDate, AdminName, GrandTotal,
                    DriverId, DriverName, UploadTimestamp, Note
                FROM BTR_PackingOrder
                WHERE PackingOrderId = @PackingOrderId
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", key.PackingOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                return conn.ReadSingle<PackingOrderModel>(sql, dp);
            }
        }

        //  TODO: Periode seharusnya UpdateTimestamp => hanya list yang belum terupload.
        public IEnumerable<PackingOrderModel> ListData(Periode filter)
        {
            const string sql = @"
                SELECT
                    PackingOrderId, PackingOrderDate, 
                    CustomerId, CustomerCode, CustomerName, Alamat, NoTelp,
                    Latitude, Longitude, Accuracy,
                    FakturId, FakturCode, FakturDate, AdminName, GrandTotal,
                    DriverId, DriverName, UploadTimestamp, Note
                FROM BTR_PackingOrder
                WHERE PackingOrderDate BETWEEN @Tgl1 AND @Tgl2
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", filter.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", filter.Tgl2, SqlDbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                return conn.Read<PackingOrderModel>(sql, dp);
            }
        }

        public PackingOrderModel GetData(IFakturKey key)
        {
            const string sql = @"
                SELECT
                    PackingOrderId, PackingOrderDate,
                    CustomerId, CustomerCode, CustomerName, Alamat, NoTelp,
                    Latitude, Longitude, Accuracy,
                    FakturId, FakturCode, FakturDate, AdminName, GrandTotal,
                    DriverId, DriverName, UploadTimestamp, Note
                FROM BTR_PackingOrder
                WHERE FakturId = @FakturId
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@FakturId", key.FakturId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                return conn.ReadSingle<PackingOrderModel>(sql, dp);
            }
        }
    }
}
