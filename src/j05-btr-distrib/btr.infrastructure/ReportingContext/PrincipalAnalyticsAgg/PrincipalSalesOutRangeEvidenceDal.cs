using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg
{
    public class PrincipalSalesOutRangeEvidenceDal : IPrincipalSalesOutRangeEvidenceDal
    {
        public const string ListSalesOutByRangeSql = @"
SELECT
    LTRIM(RTRIM(sup.SupplierId)) AS SupplierId,
    MAX(LTRIM(RTRIM(ISNULL(sup.SupplierName, '')))) AS SupplierName,
    SUM(ISNULL(fi.SubTotal, 0) - ISNULL(fi.DiscRp, 0)) AS SalesOutAmount,
    COUNT(*) AS LineCount
FROM BTR_Faktur f
INNER JOIN BTR_FakturItem fi ON f.FakturId = fi.FakturId
INNER JOIN BTR_Brg b ON fi.BrgId = b.BrgId
INNER JOIN BTR_Supplier sup ON b.SupplierId = sup.SupplierId
WHERE f.VoidDate = '3000-01-01'
  AND f.FakturDate BETWEEN @StartDate AND @EndDate
  AND LTRIM(RTRIM(ISNULL(b.SupplierId, ''))) <> ''
  AND LTRIM(RTRIM(ISNULL(sup.SupplierId, ''))) <> ''
GROUP BY LTRIM(RTRIM(sup.SupplierId))";

        private readonly DatabaseOptions _opt;

        public PrincipalSalesOutRangeEvidenceDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<PrincipalSalesOutRangeEvidenceRow> ListSalesOutByRange(
            DateTime startDate,
            DateTime endDate)
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<PrincipalSalesOutRangeEvidenceRow>(
                    ListSalesOutByRangeSql,
                    new
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    }).ToList();
            }
        }
    }
}
