using btrade.application.Contract;
using btrade.domain.ReturnOrderFeature;
using btrade.domain.SalesFeature;
using btrade.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.PatternHelper;
using Nuna.Lib.ValidationHelper;
using System.Data;
using System.Data.SqlClient;

namespace btrade.infrastructure.Repository
{
    public class ReturnOrderDal : IReturnOrderDal
    {
        private readonly DatabaseOptions _opt;

        public ReturnOrderDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(ReturnOrderType model)
        {
            const string sql = @"
            INSERT INTO BTRADE_ReturnOrder(
                ReturnOrderId, ServerId, ReturnOrderDate, WarehouseCode,
                CustomerId, CustomerName, SalesPersonId, SalesPersonName,
                DriverId, DriverName, Note, StatusSync)
            VALUES (
                @ReturnOrderId, @ServerId, @ReturnOrderDate, @WarehouseCode,
                @CustomerId, @CustomerName, @SalesPersonId, @SalesPersonName,
                @DriverId, @DriverName, @Note, @StatusSync)";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", model.ReturnOrderId, SqlDbType.VarChar);
            dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);
            dp.AddParam("@ReturnOrderDate", model.ReturnOrderDate, SqlDbType.VarChar);
            dp.AddParam("@WarehouseCode", model.WarehouseCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@CustomerName", model.CustomerName, SqlDbType.VarChar);
            dp.AddParam("@SalesPersonId", model.SalesPersonId, SqlDbType.VarChar);
            dp.AddParam("@SalesPersonName", model.SalesPersonName, SqlDbType.VarChar);
            dp.AddParam("@DriverId", model.DriverId, SqlDbType.VarChar);
            dp.AddParam("@DriverName", model.DriverName, SqlDbType.VarChar);
            dp.AddParam("@Note", model.Note, SqlDbType.VarChar);
            dp.AddParam("@StatusSync", model.StatusSync, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public void Update(ReturnOrderType model)
        {
            const string sql = @"
            UPDATE
                BTRADE_ReturnOrder
            SET
                ServerId = @ServerId,
                ReturnOrderDate = @ReturnOrderDate,
                WarehouseCode = @WarehouseCode,
                CustomerId = @CustomerId,
                CustomerName = @CustomerName,
                SalesPersonId = @SalesPersonId,
                SalesPersonName = @SalesPersonName,
                DriverId = @DriverId,
                DriverName = @DriverName,
                Note = @Note,
                StatusSync = @StatusSync
            WHERE
                ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", model.ReturnOrderId, SqlDbType.VarChar);
            dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);
            dp.AddParam("@ReturnOrderDate", model.ReturnOrderDate, SqlDbType.VarChar);
            dp.AddParam("@WarehouseCode", model.WarehouseCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@CustomerName", model.CustomerName, SqlDbType.VarChar);
            dp.AddParam("@SalesPersonId", model.SalesPersonId, SqlDbType.VarChar);
            dp.AddParam("@SalesPersonName", model.SalesPersonName, SqlDbType.VarChar);
            dp.AddParam("@DriverId", model.DriverId, SqlDbType.VarChar);
            dp.AddParam("@DriverName", model.DriverName, SqlDbType.VarChar);
            dp.AddParam("@Note", model.Note, SqlDbType.VarChar);
            dp.AddParam("@StatusSync", model.StatusSync, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public void Delete(IReturnOrderKey key)
        {
            const string sql = @"
            DELETE FROM
                BTRADE_ReturnOrder
            WHERE
                ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", key.ReturnOrderId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public void Delete(IServerId server)
        {
            const string sql = @"
            DELETE FROM
                BTRADE_ReturnOrder
            WHERE
                ServerId = @ServerId ";

            var dp = new DynamicParameters();
            dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public MayBe<ReturnOrderType> GetData(IReturnOrderKey key)
        {
            const string sql = @"
            SELECT
                ReturnOrderId, ServerId, ReturnOrderDate, WarehouseCode,
                CustomerId, CustomerName, SalesPersonId, SalesPersonName,
                DriverId, DriverName, Note, StatusSync
            FROM
                BTRADE_ReturnOrder
            WHERE
                ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", key.ReturnOrderId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            return MayBe.From(conn.ReadSingle<ReturnOrderType>(sql, dp));
        }

        public MayBe<IEnumerable<ReturnOrderType>> ListData(Periode periode, IServerId server)
        {
            const string sql = @"
            SELECT
                ReturnOrderId, ServerId, ReturnOrderDate, WarehouseCode,
                CustomerId, CustomerName, SalesPersonId, SalesPersonName,
                DriverId, DriverName, Note, StatusSync
            FROM
                BTRADE_ReturnOrder
            WHERE
                ReturnOrderDate BETWEEN @Tgl1 AND @Tgl2
                AND ServerId = @ServerId ";

            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", periode.Tgl1.ToString("yyyy-MM-dd"), SqlDbType.VarChar);
            dp.AddParam("@Tgl2", periode.Tgl2.ToString("yyyy-MM-dd"), SqlDbType.VarChar);
            dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            return MayBe.From(conn.Read<ReturnOrderType>(sql, dp));
        }
    }
}
