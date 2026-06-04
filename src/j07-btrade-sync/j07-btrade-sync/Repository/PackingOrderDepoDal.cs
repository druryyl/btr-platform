using Dapper;
using j07_btrade_sync.Model;
using Nuna.Lib.DataAccessHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Repository
{
    public class PackingOrderDepoDal
    {

        public void Insert(IEnumerable<PackingOrderDepoModel> listDto)
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            using (var bcp = new SqlBulkCopy(conn))
            {
                conn.Open();
                bcp.AddMap("PackingOrderId", "PackingOrderId");
                bcp.AddMap("DepoId", "DepoId");

                var fetched = listDto.ToList();
                bcp.BatchSize = fetched.Count;
                bcp.DestinationTableName = "BTR_PackingOrderDepo";
                bcp.WriteToServer(fetched.AsDataTable());
            }
        }

        public void Delete(IPackingOrderKey key)
        {
            const string sql = @"
                DELETE FROM BTR_PackingOrderDepo
                WHERE PackingOrderId = @PackingOrderId
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", key.PackingOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                conn.Execute(sql, dp);
            }
        }

        public IEnumerable<PackingOrderDepoModel> ListData(IPackingOrderKey filter)
        {
            const string sql = @"
                SELECT
                    aa.PackingOrderId, 
                    aa.DepoId, 
                    ISNULL(bb.DepoName, '') AS DepoName
                FROM BTR_PackingOrderDepo aa
                LEFT JOIN BTR_Depo bb ON aa.DepoId = bb.DepoId  
                WHERE PackingOrderId = @PackingOrderId
                ";

            var dp = new DynamicParameters();
            dp.AddParam("@PackingOrderId", filter.PackingOrderId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                return conn.Read<PackingOrderDepoModel>(sql, dp);
            }
        }
    }
}
