using btrade.application.Contract;
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
    public class OrderDal : IOrderDal
    {
        private readonly DatabaseOptions _opt;

        public OrderDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(OrderModel model)
        {
            const string sql = @"
            INSERT INTO BTRADE_Order(
                OrderId, OrderLocalId, CustomerId, CustomerCode, CustomerName, CustomerAddress,
                OrderDate, SalesId, SalesName, TotalAmount, UserEmail, StatusSync, FakturCode, 
                OrderNote, ServerId)
            VALUES (
                @OrderId, @OrderLocalId, @CustomerId, @CustomerCode, @CustomerName, @CustomerAddress,
                @OrderDate, @SalesId, @SalesName, @TotalAmount, @UserEmail, @StatusSync, @FakturCode, 
                @OrderNote, @ServerId)";

            var dp = new DynamicParameters();
            dp.AddParam("@OrderId", model.OrderId, SqlDbType.VarChar);
            dp.AddParam("@OrderLocalId", model.OrderLocalId, SqlDbType.VarChar);
            dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@CustomerCode", model.CustomerCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerName", model.CustomerName, SqlDbType.VarChar);
            dp.AddParam("@CustomerAddress", model.CustomerAddress, SqlDbType.VarChar);
            dp.AddParam("@OrderDate", model.OrderDate, SqlDbType.VarChar);
            dp.AddParam("@SalesId", model.SalesId, SqlDbType.VarChar);
            dp.AddParam("@SalesName", model.SalesName, SqlDbType.VarChar);
            dp.AddParam("@TotalAmount", model.TotalAmount, SqlDbType.Decimal);
            dp.AddParam("@UserEmail", model.UserEmail, SqlDbType.VarChar);
            dp.AddParam("@StatusSync", model.StatusSync, SqlDbType.VarChar);
            dp.AddParam("@FakturCode", model.FakturCode, SqlDbType.VarChar);
            dp.AddParam("@OrderNote", model.OrderNote, SqlDbType.VarChar);
            dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public void Update(OrderModel model)
        {
            const string sql = @"
            UPDATE
                BTRADE_Order
            SET
                OrderLocalId = @OrderLocalId,
                CustomerId = @CustomerId,
                CustomerCode = @CustomerCode,
                CustomerName = @CustomerName,
                CustomerAddress = @CustomerAddress,
                OrderDate = @OrderDate,
                SalesId = @SalesId,
                SalesName = @SalesName,
                TotalAmount = @TotalAmount,
                UserEmail = @UserEmail,
                StatusSync = @StatusSync,
                FakturCode = @FakturCode,
                OrderNote = @OrderNote,
                ServerId = @ServerId
            WHERE
                OrderId = @OrderId";

            var dp = new DynamicParameters();
            
            dp.AddParam("@OrderId", model.OrderId, SqlDbType.VarChar);
            dp.AddParam("@OrderLocalId", model.OrderLocalId, SqlDbType.VarChar);
            dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@CustomerCode", model.CustomerCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerName", model.CustomerName, SqlDbType.VarChar);
            dp.AddParam("@CustomerAddress", model.CustomerAddress, SqlDbType.VarChar);
            dp.AddParam("@OrderDate", model.OrderDate, SqlDbType.VarChar);
            dp.AddParam("@SalesId", model.SalesId, SqlDbType.VarChar);
            dp.AddParam("@SalesName", model.SalesName, SqlDbType.VarChar);
            dp.AddParam("@TotalAmount", model.TotalAmount, SqlDbType.Decimal);
            dp.AddParam("@UserEmail", model.UserEmail, SqlDbType.VarChar);
            dp.AddParam("@StatusSync", model.StatusSync, SqlDbType.VarChar);
            dp.AddParam("@FakturCode", model.FakturCode, SqlDbType.VarChar);
            dp.AddParam("@OrderNote", model.OrderNote, SqlDbType.VarChar);
            dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public void Delete(IOrderKey key)
        {
            const string sql = @"
            DELETE FROM
                BTRADE_Order
            WHERE
                OrderId = @OrderId";

            var dp = new DynamicParameters();
            dp.AddParam("@OrderId", key.OrderId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public void Delete(IServerId server)
        {
            const string sql = @"
            DELETE FROM
                BTRADE_Order
            WHERE
                ServerId = @ServerId ";

            var dp = new DynamicParameters();
            dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public MayBe<OrderModel> GetData(IOrderKey key)
        {
            const string sql = @"
            SELECT
                OrderId, OrderLocalId, CustomerId, CustomerCode, CustomerName, CustomerAddress,
                OrderDate, SalesId, SalesName, TotalAmount, UserEmail, StatusSync, FakturCode, 
                OrderNote, ServerId
            FROM
                BTRADE_Order
            WHERE
                OrderId = @OrderId";

            var dp = new DynamicParameters();
            dp.AddParam("@OrderId", key.OrderId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            return MayBe.From(conn.ReadSingle<OrderModel>(sql, dp));
        }

        public MayBe<IEnumerable<OrderModel>> ListData(Periode periode, IServerId server)
        {
            const string sql = @"
            SELECT
                OrderId, OrderLocalId, CustomerId, CustomerCode, CustomerName, CustomerAddress,
                OrderDate, SalesId, SalesName, TotalAmount, UserEmail, StatusSync, FakturCode, 
                OrderNote, ServerId
            FROM
                BTRADE_Order
            WHERE
                OrderDate BETWEEN @Tgl1 AND @Tgl2
                AND ServerId = @ServerId ";

            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", periode.Tgl1.ToString("yyyy-MM-dd"), SqlDbType.VarChar);
            dp.AddParam("@Tgl2", periode.Tgl2.ToString("yyyy-MM-dd"), SqlDbType.VarChar);
            dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            return MayBe.From(conn.Read<OrderModel>(sql, dp));
        }
    }
}
