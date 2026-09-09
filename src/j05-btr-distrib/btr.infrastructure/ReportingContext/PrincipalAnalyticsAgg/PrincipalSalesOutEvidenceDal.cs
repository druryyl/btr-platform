using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg
{
    public class PrincipalSalesOutEvidenceDal : IPrincipalSalesOutEvidenceDal
    {
        public const string ListFakturItemEvidenceSql = @"
SELECT
    f.FakturId,
    fi.FakturItemId,
    ISNULL(b.SupplierId, '') AS ItemSupplierId,
    ISNULL(sup.SupplierId, '') AS SupplierId,
    ISNULL(sup.SupplierName, '') AS SupplierName,
    ISNULL(fi.SubTotal, 0) AS SubTotal,
    ISNULL(fi.DiscRp, 0) AS DiscRp,
    ISNULL(fi.PpnRp, 0) AS PpnRp,
    ISNULL(fi.Total, 0) AS LineTotal,
    ISNULL(fi.DppRp, 0) AS DppRp,
    ISNULL(f.GrandTotal, 0) AS HeaderGrandTotal
FROM BTR_Faktur f
INNER JOIN BTR_FakturItem fi ON f.FakturId = fi.FakturId
LEFT JOIN BTR_Brg b ON fi.BrgId = b.BrgId
LEFT JOIN BTR_Supplier sup ON b.SupplierId = sup.SupplierId
WHERE f.FakturDate BETWEEN @Tgl1 AND @Tgl2
  AND f.VoidDate = '3000-01-01'";

        private readonly DatabaseOptions _opt;

        public PrincipalSalesOutEvidenceDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<PrincipalSalesOutFakturItemEvidence> ListFakturItemEvidence(Periode periode)
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<PrincipalSalesOutFakturItemEvidence>(ListFakturItemEvidenceSql, new
                {
                    Tgl1 = periode.Tgl1,
                    Tgl2 = periode.Tgl2
                }).ToList();
            }
        }
    }
}
