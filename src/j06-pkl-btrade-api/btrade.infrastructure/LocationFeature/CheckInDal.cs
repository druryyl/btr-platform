using btrade.application.Contract;
using btrade.domain.CheckInFeature;
using btrade.domain.SalesFeature;
using btrade.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.PatternHelper;
using Nuna.Lib.ValidationHelper;
using System.Data;
using System.Data.SqlClient;

namespace btrade.infrastructure.LocationFeature
{

    public class CheckInDal : ICheckInDal
    {
        private readonly DatabaseOptions _opt;

        public CheckInDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(CheckInType model)
        {
            const string sql = @"
                INSERT INTO BTRADE_CheckIn(
                    CheckInId, CheckInDate, CheckInTime, UserEmail, 
                    CheckInLatitude, CheckInLongitude, Accuracy,
                    CustomerId, CustomerCode, CustomerName, CustomerAddress,
                    CustomerLatitude, CustomerLongitude, StatusSync, ServerId)
                VALUES (
                    @CheckInId, @CheckInDate, @CheckInTime, @UserEmail, 
                    @CheckInLatitude, @CheckInLongitude, @Accuracy,
                    @CustomerId, @CustomerCode, @CustomerName, @CustomerAddress,
                    @CustomerLatitude, @CustomerLongitude, @StatusSync, @ServerId)";

            var dp = new DynamicParameters();
            dp.AddParam("@CheckInId", model.CheckInId, SqlDbType.VarChar);
            dp.AddParam("@CheckInDate", model.CheckInDate, SqlDbType.VarChar);
            dp.AddParam("@CheckInTime", model.CheckInTime, SqlDbType.VarChar);
            dp.AddParam("@UserEmail", model.UserEmail, SqlDbType.VarChar);
            dp.AddParam("@CheckInLatitude", model.CheckInLatitude, SqlDbType.Float);
            dp.AddParam("@CheckInLongitude", model.CheckInLongitude, SqlDbType.Float);
            dp.AddParam("@Accuracy", model.Accuracy, SqlDbType.Float);
            dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@CustomerCode", model.CustomerCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerName", model.CustomerName, SqlDbType.VarChar);
            dp.AddParam("@CustomerAddress", model.CustomerAddress, SqlDbType.VarChar);
            dp.AddParam("@CustomerLatitude", model.CustomerLatitude, SqlDbType.Float);
            dp.AddParam("@CustomerLongitude", model.CustomerLongitude, SqlDbType.Float);
            dp.AddParam("@StatusSync", model.StatusSync, SqlDbType.VarChar);
            dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public void Update(CheckInType model)
        {
            const string sql = @"
            UPDATE
                BTRADE_CheckIn
            SET
                CheckInDate = @CheckInDate,
                CheckInTime = @CheckInTime,
                UserEmail = @UserEmail,
                CheckInLatitude = @CheckInLatitude,
                CheckInLongitude = @CheckInLongitude,
                Accuracy = @Accuracy,
                CustomerId = @CustomerId,
                CustomerCode = @CustomerCode,
                CustomerName = @CustomerName,
                CustomerAddress = @CustomerAddress,
                CustomerLatitude = @CustomerLatitude,
                CustomerLongitude = @CustomerLongitude,
                StatusSync = @StatusSync,
                ServerId = @ServerId    
            WHERE
                CheckInId = @CheckInId";

            var dp = new DynamicParameters();
            dp.AddParam("@CheckInId", model.CheckInId, SqlDbType.VarChar);
            dp.AddParam("@CheckInDate", model.CheckInDate, SqlDbType.VarChar);
            dp.AddParam("@CheckInTime", model.CheckInTime, SqlDbType.VarChar);
            dp.AddParam("@UserEmail", model.UserEmail, SqlDbType.VarChar);
            dp.AddParam("@CheckInLatitude", model.CheckInLatitude, SqlDbType.Float);
            dp.AddParam("@CheckInLongitude", model.CheckInLongitude, SqlDbType.Float);
            dp.AddParam("@Accuracy", model.Accuracy, SqlDbType.Float);
            dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
            dp.AddParam("@CustomerCode", model.CustomerCode, SqlDbType.VarChar);
            dp.AddParam("@CustomerName", model.CustomerName, SqlDbType.VarChar);
            dp.AddParam("@CustomerAddress", model.CustomerAddress, SqlDbType.VarChar);
            dp.AddParam("@CustomerLatitude", model.CustomerLatitude, SqlDbType.Float);
            dp.AddParam("@CustomerLongitude", model.CustomerLongitude, SqlDbType.Float);
            dp.AddParam("@StatusSync", model.StatusSync, SqlDbType.VarChar);
            dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public void Delete(ICheckInKey key)
        {
            const string sql = @"
            DELETE FROM
                BTRADE_CheckIn
            WHERE
                CheckInId = @CheckInId";

            var dp = new DynamicParameters();
            dp.AddParam("@CheckInId", key.CheckInId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public MayBe<CheckInType> GetData(ICheckInKey key)
        {
            const string sql = @"
            SELECT
                CheckInId, CheckInDate, CheckInTime, UserEmail, 
                CheckInLatitude, CheckInLongitude, Accuracy,
                CustomerId, CustomerCode, CustomerName, CustomerAddress,
                CustomerLatitude, CustomerLongitude, StatusSync, ServerId
            FROM
                BTRADE_CheckIn
            WHERE
                CheckInId = @CheckInId";

            var dp = new DynamicParameters();
            dp.AddParam("@CheckInId", key.CheckInId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            return MayBe.From(conn.ReadSingle<CheckInType>(sql, dp));
        }

        public IEnumerable<CheckInType> ListData(Periode periode, IServerId serverId)
        {
            const string sql = @"
            SELECT
                CheckInId, CheckInDate, CheckInTime, UserEmail, 
                CheckInLatitude, CheckInLongitude, Accuracy,
                CustomerId, CustomerCode, CustomerName, CustomerAddress,
                CustomerLatitude, CustomerLongitude, StatusSync, ServerId
            FROM
                BTRADE_CheckIn
            WHERE
                CheckInDate BETWEEN @Tgl1 AND @Tgl2 
                AND ServerId = @ServerId ";

            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", periode.Tgl1.ToString("yyyy-MM-dd"), SqlDbType.VarChar);
            dp.AddParam("@Tgl2", periode.Tgl2.ToString("yyyy-MM-dd"), SqlDbType.VarChar);
            dp.AddParam("@ServerId", serverId.ServerId, SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            return conn.Read<CheckInType>(sql, dp);
        }
    }
}
