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

public class CustomerDal : ICustomerDal
{
    private readonly DatabaseOptions _opt;

    public CustomerDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }

    public void Insert(CustomerType model)
    {
        const string sql = @"
            INSERT INTO BTRADE_Customer(
                CustomerId, CustomerCode, CustomerName, Alamat, Wilayah,
                Latitude, Longitude, Accuracy, CoordinateTimeStamp, CoordinateUser, IsUpdated, 
                ServerId)
            VALUES (
                @CustomerId, @CustomerCode, @CustomerName, @Alamat, @Wilayah,
                @Latitude, @Longitude, @Accuracy, @CoordinateTimeStamp, @CoordinateUser, @IsUpdated,
                @ServerId)";

        var dp = new DynamicParameters();
        dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
        dp.AddParam("@CustomerCode", model.CustomerCode, SqlDbType.VarChar);
        dp.AddParam("@CustomerName", model.CustomerName, SqlDbType.VarChar);
        dp.AddParam("@Alamat", model.Alamat, SqlDbType.VarChar);
        dp.AddParam("@Wilayah", model.Wilayah, SqlDbType.VarChar);

        dp.AddParam("@Latitude", model.Latitude, SqlDbType.Float);
        dp.AddParam("@Longitude", model.Longitude, SqlDbType.Float);
        dp.AddParam("@Accuracy", model.Accuracy, SqlDbType.Float);
        dp.AddParam("@CoordinateTimeStamp", model.CoordinateTimeStamp, SqlDbType.BigInt);
        dp.AddParam("@CoordinateUser", model.CoordinateUser, SqlDbType.VarChar);
        dp.AddParam("@IsUpdated", model.IsUpdated, SqlDbType.Bit);

        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Update(CustomerType model)
    {
        const string sql = @"
            UPDATE
                BTRADE_Customer
            SET
                CustomerCode = @CustomerCode,
                CustomerName = @CustomerName,
                Alamat = @Alamat,
                Wilayah = @Wilayah,
                Latitude = @Latitude,
                Longitude = @Longitude,
                Accuracy = @Accuracy,
                CoordinateTimeStamp = @CoordinateTimeStamp,
                CoordinateUser = @CoordinateUser,
                IsUpdated = @IsUpdated,
                ServerId = @ServerId
            WHERE
                CustomerId = @CustomerId
                AND ServerId = @ServerId ";

        var dp = new DynamicParameters();
        dp.AddParam("@CustomerId", model.CustomerId, SqlDbType.VarChar);
        dp.AddParam("@CustomerCode", model.CustomerCode, SqlDbType.VarChar);
        dp.AddParam("@CustomerName", model.CustomerName, SqlDbType.VarChar);
        dp.AddParam("@Alamat", model.Alamat, SqlDbType.VarChar);
        dp.AddParam("@Wilayah", model.Wilayah, SqlDbType.VarChar);

        dp.AddParam("@Latitude", model.Latitude, SqlDbType.Float);
        dp.AddParam("@Longitude", model.Longitude, SqlDbType.Float);
        dp.AddParam("@Accuracy", model.Accuracy, SqlDbType.Float);
        dp.AddParam("@CoordinateTimeStamp", model.CoordinateTimeStamp, SqlDbType.BigInt);
        dp.AddParam("@CoordinateUser", model.CoordinateUser, SqlDbType.VarChar);
        dp.AddParam("@IsUpdated", model.IsUpdated, SqlDbType.Bit);
        dp.AddParam("@ServerId", model.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Delete(ICustomerKey key)
    {
        const string sql = @"
            DELETE FROM
                BTRADE_Customer
            WHERE
                CustomerId = @CustomerId
                AND ServerId = @ServerId ";

        var dp = new DynamicParameters();
        dp.AddParam("@CustomerId", key.CustomerId, SqlDbType.VarChar);
        dp.AddParam("@ServerId", key.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Delete(IServerId server)
    {
        const string sql = @"
            DELETE FROM
                BTRADE_Customer
            WHERE
                ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@ServerId", server.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }


    public MayBe<CustomerType> GetData(ICustomerKey key)
    {
        const string sql = @"
            SELECT
                CustomerId, CustomerCode, CustomerName, Alamat, Wilayah,
                Latitude, Longitude, Accuracy, CoordinateTimeStamp, CoordinateUser, IsUpdated,
                ServerId
            FROM
                BTRADE_Customer
            WHERE
                CustomerId = @CustomerId
                AND ServerId = @ServerId";

        var dp = new DynamicParameters();
        dp.AddParam("@CustomerId", key.CustomerId, SqlDbType.VarChar);
        dp.AddParam("@ServerId", key.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.ReadSingle<CustomerType>(sql, dp));
    }

    public MayBe<IEnumerable<CustomerType>> ListData(IServerId server)
    {
        const string sql = @"
            SELECT
                CustomerId, CustomerCode, CustomerName, Alamat, Wilayah,
                Latitude, Longitude, Accuracy, CoordinateTimeStamp, CoordinateUser, IsUpdated,
                ServerId
            FROM
                BTRADE_Customer
            WHERE
                ServerId = @ServerId ";

        var dp = new DynamicParameters();
        dp.AddParam(@"ServerId", server.ServerId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.Read<CustomerType>(sql, dp));
    }
}
