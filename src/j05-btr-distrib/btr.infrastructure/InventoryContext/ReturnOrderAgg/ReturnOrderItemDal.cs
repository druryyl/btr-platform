using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.InventoryContext.ReturnOrderAgg
{
    public class ReturnOrderItemDal : IReturnOrderItemDal
    {
        private readonly DatabaseOptions _opt;

        public ReturnOrderItemDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(IEnumerable<ReturnOrderItemModel> listModel)
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            using (var bcp = new SqlBulkCopy(conn))
            {
                conn.Open();

                bcp.ColumnMappings.Add("ReturnOrderId", "ReturnOrderId");
                bcp.ColumnMappings.Add("NoUrut", "NoUrut");
                bcp.ColumnMappings.Add("BrgId", "BrgId");
                bcp.ColumnMappings.Add("BrgCode", "BrgCode");
                bcp.ColumnMappings.Add("Qty", "Qty");
                bcp.ColumnMappings.Add("SatId", "SatId");
                bcp.ColumnMappings.Add("JenisRetur", "JenisRetur");

                var fetched = listModel.ToList();
                bcp.DestinationTableName = "BTR_ReturnOrderItem";
                bcp.BatchSize = fetched.Count;
                bcp.WriteToServer(fetched.AsDataTable());
            }
        }

        public void Delete(IReturnOrderKey key)
        {
            const string sql = @"
                DELETE FROM 
                    BTR_ReturnOrderItem
                WHERE 
                    ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", key.ReturnOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public IEnumerable<ReturnOrderItemModel> ListData(IReturnOrderKey filter)
        {
            const string sql = @"
                SELECT 
                    aa.ReturnOrderId, aa.NoUrut, aa.BrgId, aa.BrgCode,
                    aa.Qty, aa.SatId, aa.JenisRetur,
                    ISNULL(bb.BrgName, '') BrgName
                FROM 
                    BTR_ReturnOrderItem aa
                    LEFT JOIN BTR_Brg bb ON aa.BrgId = bb.BrgId
                WHERE 
                    aa.ReturnOrderId = @ReturnOrderId
                ORDER BY 
                    aa.NoUrut";

            var dp = new DynamicParameters();
            dp.AddParam("@ReturnOrderId", filter.ReturnOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<ReturnOrderItemModel>(sql, dp);
            }
        }
    }
}
