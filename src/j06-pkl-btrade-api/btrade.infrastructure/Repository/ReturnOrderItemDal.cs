using btrade.application.Contract;
using btrade.domain.ReturnOrderFeature;
using btrade.infrastructure.Helpers;
using Microsoft.Extensions.Options;
using Nuna.Lib.DataAccessHelper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace btrade.infrastructure.Repository
{
    public class ReturnOrderItemDal : IReturnOrderItemDal
    {
        private readonly DatabaseOptions _opt;

        public ReturnOrderItemDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(IEnumerable<ReturnOrderItemType> listModel)
        {
            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            using var bcp = new SqlBulkCopy(conn);
            conn.Open();

            bcp.AddMap("ReturnOrderId", "ReturnOrderId");
            bcp.AddMap("NoUrut", "NoUrut");
            bcp.AddMap("BrgId", "BrgId");
            bcp.AddMap("BrgCode", "BrgCode");
            bcp.AddMap("BrgName", "BrgName");
            bcp.AddMap("Qty", "Qty");
            bcp.AddMap("SatId", "SatId");
            bcp.AddMap("JenisRetur", "JenisRetur");

            var fetched = listModel.ToList();
            bcp.DestinationTableName = "dbo.BTRADE_ReturnOrderItem";
            bcp.WriteToServer(fetched.AsDataTable());
        }

        public void Delete(IReturnOrderKey key)
        {
            const string sql = @"
                DELETE FROM BTRADE_ReturnOrderItem
                WHERE ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("ReturnOrderId", key.ReturnOrderId, System.Data.SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            conn.Execute(sql, dp);
        }

        public IEnumerable<ReturnOrderItemType> ListData(IReturnOrderKey filter)
        {
            const string sql = @"
                SELECT 
                    ReturnOrderId, NoUrut, BrgId, BrgCode, BrgName, Qty, SatId, JenisRetur
                FROM BTRADE_ReturnOrderItem
                WHERE ReturnOrderId = @ReturnOrderId";

            var dp = new DynamicParameters();
            dp.AddParam("ReturnOrderId", filter.ReturnOrderId, System.Data.SqlDbType.VarChar);

            using var conn = new SqlConnection(ConnStringHelper.Get(_opt));
            return conn.Read<ReturnOrderItemType>(sql, dp);
        }
    }
}
