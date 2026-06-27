using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class SalesmanMtdItemRollupDal : ISalesmanMtdItemRollupDal
    {
        private readonly DatabaseOptions _opt;

        public SalesmanMtdItemRollupDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<SalesmanMtdItemRollupDto> ListMtdItemRollups(Periode periode)
        {
            const string sql = @"
SELECT
    ISNULL(ee.SalesPersonId, '') AS SalesPersonId,
    ISNULL(ee.SalesPersonCode, '') AS SalesPersonCode,
    ISNULL(ee.SalesPersonName, '') AS SalesPersonName,
    ISNULL(dd.CustomerCode, '') AS CustomerCode,
    ISNULL(dd.CustomerName, '') AS CustomerName,
    ISNULL(aa.BrgId, '') AS BrgId,
    ISNULL(cc.BrgCode, '') AS BrgCode,
    ISNULL(cc.BrgName, '') AS BrgName,
    ISNULL(gg.SupplierId, '') AS SupplierId,
    ISNULL(gg.SupplierCode, '') AS SupplierCode,
    ISNULL(gg.SupplierName, '') AS SupplierName,
    aa.Total AS LineTotal
FROM BTR_FakturItem aa
    INNER JOIN BTR_Faktur bb ON aa.FakturId = bb.FakturId
    INNER JOIN BTR_Customer dd ON bb.CustomerId = dd.CustomerId
    LEFT JOIN BTR_Brg cc ON aa.BrgId = cc.BrgId
    LEFT JOIN BTR_Supplier gg ON cc.SupplierId = gg.SupplierId
    LEFT JOIN BTR_SalesPerson ee ON bb.SalesPersonId = ee.SalesPersonId
WHERE bb.FakturDate BETWEEN @Tgl1 AND @Tgl2
  AND bb.VoidDate = '3000-01-01'";

            var dp = new DynamicParameters();
            dp.AddParam("@Tgl1", periode.Tgl1, SqlDbType.DateTime);
            dp.AddParam("@Tgl2", periode.Tgl2, SqlDbType.DateTime);

            using (var cn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return cn.Read<SalesmanMtdItemRollupDto>(sql, dp);
            }
        }
    }
}
