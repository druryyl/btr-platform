using btrade.application.WarehouseFreature;
using btrade.domain.WarehouseFeature;
using btrade.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;
using Nuna.Lib.DataAccessHelper;
using System.Data;
using System.Data.SqlClient;

namespace btrade.infrastructure.WarehouseFeature;

public class PackingOrderItemDal : IPackingOrderItemDal
{
    private readonly DatabaseOptions _opt;

    public PackingOrderItemDal(IOptions<DatabaseOptions> opt)
    {
        _opt = opt.Value;
    }

    public void Insert(IEnumerable<PackingOrderItemModel> listModel)
    {
        using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
        using (var bcp = new SqlBulkCopy(conn))
        {
            conn.Open();
            bcp.AddMap("PackingOrderId", "PackingOrderId");
            bcp.AddMap("NoUrut", "NoUrut");
            bcp.AddMap("BrgId", "BrgId");
            bcp.AddMap("BrgCode", "BrgCode");
            bcp.AddMap("BrgName", "BrgName");
            bcp.AddMap("KategoriName", "KategoriName");
            bcp.AddMap("SupplierName", "SupplierName");
            bcp.AddMap("QtyBesar", "QtyBesar");
            bcp.AddMap("SatBesar", "SatBesar");
            bcp.AddMap("QtyKecil", "QtyKecil");
            bcp.AddMap("SatKecil", "SatKecil");
            bcp.AddMap("DepoId", "DepoId");

            var fetched = listModel.ToList();
            bcp.BatchSize = fetched.Count;
            bcp.DestinationTableName = "dbo.BTRADE_PackingOrderItem";
            bcp.WriteToServer(fetched.AsDataTable());
        }
    }

    public void Delete(IPackingOrderKey key)
    {
        const string sql = @"
            DELETE FROM BTRADE_PackingOrderItem
            WHERE PackingOrderId = @PackingOrderId
            ";

        var dp = new DynamicParameters();
        dp.AddParam("@PackingOrderId", key.PackingOrderId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        conn.Execute(sql, dp);
    }

    public IEnumerable<PackingOrderItemModel> ListData(IPackingOrderKey filter)
    {
        const string sql = @"
            SELECT
                PackingOrderId, NoUrut,
                BrgId, BrgCode, BrgName, KategoriName, SupplierName,
                QtyBesar, SatBesar,
                QtyKecil, SatKecil, DepoId
            FROM BTRADE_PackingOrderItem
            WHERE PackingOrderId = @PackingOrderId
            ORDER BY NoUrut
            ";

        var dp = new DynamicParameters();
        dp.AddParam("@PackingOrderId", filter.PackingOrderId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return conn.Read<PackingOrderItemModel>(sql, dp);
    }

    public IEnumerable<PackingOrderItemModel> ListData(DateTime updateTimestamp, string depoId)
    {
        const string sql = @"
            SELECT
                aa.PackingOrderId, aa.NoUrut,
                aa.BrgId, aa.BrgCode, aa.BrgName, aa.KategoriName, aa.SupplierName,
                aa.QtyBesar, aa.SatBesar,
                aa.QtyKecil, aa.SatKecil, aa.DepoId
            FROM 
                BTRADE_PackingOrderItem aa
                INNER JOIN BTRADE_PackingOrderDepo bb 
                    ON aa.PackingOrderId = bb.PackingOrderId  
                    AND bb.DepoId = @DepoId
            WHERE 
                bb.UpdateTimestamp >= @Timestamp 
                AND bb.DepoId = @DepoId    
            ";

        var dp = new DynamicParameters();
        dp.AddParam("@Timestamp", updateTimestamp, SqlDbType.DateTime);
        dp.AddParam("@DepoId", depoId, SqlDbType.VarChar);

        using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
        return conn.Read<PackingOrderItemModel>(sql, dp);
    }
}