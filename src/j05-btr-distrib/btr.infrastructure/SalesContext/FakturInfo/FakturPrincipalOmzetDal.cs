using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.SalesContext.FakturInfo;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.FakturInfo
{
    public class FakturPrincipalOmzetDal : IFakturPrincipalOmzetDal
    {
        private readonly DatabaseOptions _opt;

        public FakturPrincipalOmzetDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<FakturPrincipalOmzetDto> ListOmzetBySalesPersonPrincipal(Periode periode)
        {
            const string sql = @"
SELECT
    ISNULL(sp.SalesPersonId, '') AS SalesPersonId,
    ISNULL(sup.SupplierId, '') AS SupplierId,
    ISNULL(sup.SupplierName, '') AS SupplierName,
    SUM(ISNULL(fi.Total, 0)) AS CompletedOmzet
FROM BTR_Faktur f
INNER JOIN BTR_FakturItem fi ON f.FakturId = fi.FakturId
INNER JOIN BTR_Brg b ON fi.BrgId = b.BrgId
LEFT JOIN BTR_Supplier sup ON b.SupplierId = sup.SupplierId
LEFT JOIN BTR_SalesPerson sp ON f.SalesPersonId = sp.SalesPersonId
WHERE f.FakturDate BETWEEN @Tgl1 AND @Tgl2
  AND f.VoidDate = '3000-01-01'
GROUP BY sp.SalesPersonId, sup.SupplierId, sup.SupplierName
HAVING SUM(ISNULL(fi.Total, 0)) > 0";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<FakturPrincipalOmzetDto>(sql, new
                {
                    Tgl1 = periode.Tgl1,
                    Tgl2 = periode.Tgl2
                }).ToList();
            }
        }
    }
}
