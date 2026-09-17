using Dapper;
using j07_btrade_sync.Model;
using Nuna.Lib.DataAccessHelper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace j07_btrade_sync.Repository
{
    //  Stages downloaded Return Order items into the Main Office
    //  BTR_ReturnOrderItem (Arch §4.3, §8.2, I-RO-07), mirroring OrderItemDal:
    //  delete-by-parent then SqlBulkCopy, so re-download never duplicates.
    //  BrgName is not mapped: BTR_ReturnOrderItem has no such column (§6.1).
    public class ReturnOrderItemDal
    {
        public void Insert(IEnumerable<ReturnOrderItemType> listModel)
        {
            var fetched = (listModel ?? Enumerable.Empty<ReturnOrderItemType>()).ToList();
            if (fetched.Count == 0)
                return;

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            using (var bcp = new SqlBulkCopy(conn))
            {
                conn.Open();

                bcp.AddMap("ReturnOrderId", "ReturnOrderId");
                bcp.AddMap("NoUrut", "NoUrut");
                bcp.AddMap("BrgId", "BrgId");
                bcp.AddMap("BrgCode", "BrgCode");
                bcp.AddMap("Qty", "Qty");
                bcp.AddMap("SatId", "SatId");
                bcp.AddMap("JenisRetur", "JenisRetur");

                bcp.BatchSize = fetched.Count;
                bcp.DestinationTableName = "dbo.BTR_ReturnOrderItem";
                bcp.WriteToServer(fetched.AsDataTable());
            }
        }

        public void Delete(IReturnOrderKey key)
        {
            const string sql = @"
                DELETE FROM BTR_ReturnOrderItem
                WHERE ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("ReturnOrderId", key.ReturnOrderId, System.Data.SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                conn.Execute(sql, dp);
            }
        }

        public IEnumerable<ReturnOrderItemType> ListData(IReturnOrderKey filter)
        {
            const string sql = @"
                SELECT
                    ReturnOrderId, NoUrut, BrgId, BrgCode, Qty, SatId, JenisRetur
                FROM BTR_ReturnOrderItem
                WHERE ReturnOrderId = @ReturnOrderId
                ORDER BY NoUrut";

            var dp = new DynamicParameters();
            dp.AddParam("ReturnOrderId", filter.ReturnOrderId, System.Data.SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                return conn.Read<ReturnOrderItemType>(sql, dp);
            }
        }
    }
}
