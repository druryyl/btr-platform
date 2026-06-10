using btr.application.SalesContext.SalesPersonSupplierAgg.Contracts;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.SalesPersonSupplierAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace btr.infrastructure.SalesContext.SalesPersonSupplierAgg
{
    public class SalesPersonSupplierDal : ISalesPersonSupplierDal
    {
        private readonly DatabaseOptions _opt;

        public SalesPersonSupplierDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(IEnumerable<SalesPersonSupplierModel> listModel)
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            using (var bcp = new SqlBulkCopy(conn))
            {
                bcp.AddMap("SalesPersonId", "SalesPersonId");
                bcp.AddMap("SupplierId", "SupplierId");

                var fetched = listModel.ToList();
                if (fetched.Count == 0)
                    return;

                bcp.BatchSize = fetched.Count;
                bcp.DestinationTableName = "dbo.BTR_SalesPersonSupplier";
                conn.Open();
                bcp.WriteToServer(fetched.AsDataTable());
            }
        }

        public void Delete(ISalesPersonKey key)
        {
            const string sql = @"
                DELETE FROM
                    BTR_SalesPersonSupplier
                WHERE
                    SalesPersonId = @SalesPersonId";

            var dp = new DynamicParameters();
            dp.AddParam("@SalesPersonId", key.SalesPersonId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public IEnumerable<SalesPersonSupplierModel> ListData(ISalesPersonKey filter)
        {
            const string sql = @"
                SELECT
                    aa.SalesPersonId,
                    aa.SupplierId,
                    ISNULL(bb.SupplierCode, '') SupplierCode,
                    ISNULL(bb.SupplierName, '') SupplierName
                FROM
                    BTR_SalesPersonSupplier aa
                    LEFT JOIN BTR_Supplier bb ON aa.SupplierId = bb.SupplierId
                WHERE
                    aa.SalesPersonId = @SalesPersonId
                ORDER BY
                    bb.SupplierName";

            var dp = new DynamicParameters();
            dp.AddParam("@SalesPersonId", filter.SalesPersonId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<SalesPersonSupplierModel>(sql, dp);
            }
        }
    }
}
