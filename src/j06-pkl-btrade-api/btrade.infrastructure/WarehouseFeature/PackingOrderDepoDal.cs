using btrade.application.WarehouseFreature;
using btrade.domain.WarehouseFeature;
using btrade.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;
using Nuna.Lib.DataAccessHelper;
using System.Data;
using System.Data.SqlClient;

namespace btrade.infrastructure.WarehouseFeature;

public class PackingOrderDepoDal : IPackingOrderDepoDal
{
    private readonly DatabaseOptions _opt;

    public PackingOrderDepoDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }

    public void Insert(IEnumerable<PackingOrderDepoModel> listModel)
    {
        using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
        using (var bcp = new SqlBulkCopy(conn))
        {
            conn.Open();
            bcp.AddMap("PackingOrderId", "PackingOrderId");
            bcp.AddMap("DepoId", "DepoId");
            bcp.AddMap("UpdateTimestamp", "UpdateTimestamp");
            bcp.AddMap("DownloadTimestamp", "DownloadTimestamp");

            var fetched = listModel.ToList();
            bcp.BatchSize = fetched.Count;
            bcp.DestinationTableName = "dbo.BTRADE_PackingOrderDepo";
            bcp.WriteToServer(fetched.AsDataTable());
        }
    }
    public void Update(PackingOrderDepoModel model)
    {
        const string sql = @"
            UPDATE BTRADE_PackingOrderDepo
            SET 
                DownloadTimestamp = @DownloadTimestamp
            WHERE 
                PackingOrderId = @PackingOrderId
                AND DepoId = @DepoId
            ";  
        var dp = new DynamicParameters();
        dp.AddParam("@PackingOrderId", model.PackingOrderId, SqlDbType.VarChar);
        dp.AddParam("@DepoId", model.DepoId, SqlDbType.VarChar);
        dp.AddParam("@DownloadTimestamp", model.DownloadTimestamp, SqlDbType.DateTime);
        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public void Delete(IPackingOrderKey key)
    {
        const string sql = @"
            DELETE FROM BTRADE_PackingOrderDepo
            WHERE PackingOrderId = @PackingOrderId
            ";

        var dp = new DynamicParameters();
        dp.AddParam("@PackingOrderId", key.PackingOrderId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public IEnumerable<PackingOrderDepoModel> ListData(IPackingOrderKey filter)
    {
        const string sql = @"
            SELECT
                PackingOrderId, DepoId,
                UpdateTimestamp, DownloadTimestamp  
            FROM BTRADE_PackingOrderDepo
            WHERE PackingOrderId = @PackingOrderId
            ORDER BY NoUrut
            ";

        var dp = new DynamicParameters();
        dp.AddParam("@PackingOrderId", filter.PackingOrderId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return conn.Read<PackingOrderDepoModel>(sql, dp);
    }

    public IEnumerable<PackingOrderDepoModel> ListData(DateTime updateTimestamp, string depoId)
    {
        const string sql = @"
            SELECT
                aa.PackingOrderId, aa.DepoId,
                aa.UpdateTimestamp, aa.DownloadTimestamp  
            FROM 
                BTRADE_PackingOrderDepo aa
                INNER JOIN BTRADE_PackingOrder bb ON aa.PackingOrderId = bb.PackingOrderId
                INNER JOIN BTRADE_PackingOrderDepo cc ON bb.PackingOrderId = cc.PackingOrderId 
            WHERE 
                cc.UpdateTimestamp >= @Timestamp
                AND cc.DepoId = @DepoId
            ";

        var dp = new DynamicParameters();
        dp.AddParam("@Timestamp", updateTimestamp, SqlDbType.DateTime);
        dp.AddParam("@DepoId", depoId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return conn.Read<PackingOrderDepoModel>(sql, dp);
    }

}