using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg
{
    public class PrincipalReturnHistoryEvidenceDal : IPrincipalReturnHistoryEvidenceDal
    {
        public const string ListMonthlyReturnHistorySql = @"
SELECT
    YEAR(r.ReturJualDate) AS PeriodYear,
    MONTH(r.ReturJualDate) AS PeriodMonth,
    LTRIM(RTRIM(sup.SupplierId)) AS SupplierId,
    MAX(LTRIM(RTRIM(ISNULL(sup.SupplierName, '')))) AS SupplierName,
    SUM(CASE WHEN r.JenisRetur = 'BAGUS' THEN ISNULL(ri.SubTotal, 0) - ISNULL(ri.DiscRp, 0) ELSE 0 END) AS GoodReturnAmount,
    SUM(CASE WHEN r.JenisRetur = 'RUSAK' THEN ISNULL(ri.SubTotal, 0) - ISNULL(ri.DiscRp, 0) ELSE 0 END) AS BrokenReturnAmount,
    SUM(ISNULL(ri.SubTotal, 0) - ISNULL(ri.DiscRp, 0)) AS TotalReturnAmount,
    COUNT(*) AS LineCount
FROM BTR_ReturJual r
INNER JOIN BTR_ReturJualItem ri ON r.ReturJualId = ri.ReturJualId
INNER JOIN BTR_Brg b ON ri.BrgId = b.BrgId
INNER JOIN BTR_Supplier sup ON b.SupplierId = sup.SupplierId
WHERE r.VoidDate = '3000-01-01'
  AND r.ReturJualDate < '3000-01-01'
  AND ISNULL(r.JenisRetur, '') IN ('BAGUS', 'RUSAK')
  AND LTRIM(RTRIM(ISNULL(b.SupplierId, ''))) <> ''
  AND LTRIM(RTRIM(ISNULL(sup.SupplierId, ''))) <> ''
GROUP BY YEAR(r.ReturJualDate), MONTH(r.ReturJualDate), LTRIM(RTRIM(sup.SupplierId))";

        private readonly DatabaseOptions _opt;

        public PrincipalReturnHistoryEvidenceDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<PrincipalReturnHistoryMonthEvidence> ListMonthlyReturnHistory()
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<PrincipalReturnHistoryMonthEvidence>(ListMonthlyReturnHistorySql).ToList();
            }
        }
    }
}
