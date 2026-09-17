using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.PatternHelper;
using System.Data;
using System.Data.SqlClient;

namespace btrade.infrastructure.BarcodeFeature;

public class LocationDal : ILocationDal
{
    private readonly DatabaseOptions _opt;

    public LocationDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }

    public MayBe<LocationType> GetData(ILocationKey key)
    {
        const string sql = @"
            SELECT
                LocationId, LocationName, ServerId
            FROM
                BTRADE_Location
            WHERE
                LocationId = @LocationId";

        var dp = new DynamicParameters();
        dp.AddParam("@LocationId", key.LocationId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.ReadSingle<LocationType>(sql, dp));
    }
}
