using btrade.application.Contract;
using btrade.domain.WarehouseFeature;
using btrade.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.PatternHelper;
using System.Data;
using System.Data.SqlClient;

namespace btrade.infrastructure.WarehouseFeature;

public class WarehouseMappingDal : IWarehouseMappingDal
{
    private readonly DatabaseOptions _opt;

    public WarehouseMappingDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }

    public MayBe<WarehouseMappingType> GetData(IWarehouseMappingKey key)
    {
        const string sql = @"
            SELECT
                WarehouseCode, ServerId
            FROM
                BTR_WarehouseMapping
            WHERE
                WarehouseCode = @WarehouseCode";

        var dp = new DynamicParameters();
        dp.AddParam("@WarehouseCode", key.WarehouseCode, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.ReadSingle<WarehouseMappingType>(sql, dp));
    }

    public MayBe<IEnumerable<WarehouseMappingType>> ListData()
    {
        //  TD-02/TD-04 — the full distinct locationId(WarehouseCode)→ServerId
        //  mapping returned to BGud at session establishment.
        const string sql = @"
            SELECT
                WarehouseCode, ServerId
            FROM
                BTR_WarehouseMapping
            ORDER BY
                WarehouseCode";

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return MayBe.From(conn.Read<WarehouseMappingType>(sql, new DynamicParameters()));
    }
}
