using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg
{
    public class PrincipalReturnEvidenceDal : IPrincipalReturnEvidenceDal
    {
        public const string ListReturnItemEvidenceSql = @"
SELECT
    r.ReturJualId,
    ri.ReturJualItemId,
    ISNULL(r.JenisRetur, '') AS JenisRetur,
    ISNULL(b.SupplierId, '') AS ItemSupplierId,
    ISNULL(sup.SupplierId, '') AS SupplierId,
    ISNULL(sup.SupplierName, '') AS SupplierName,
    ISNULL(ri.SubTotal, 0) AS SubTotal,
    ISNULL(ri.DiscRp, 0) AS DiscRp,
    ISNULL(ri.PpnRp, 0) AS PpnRp,
    ISNULL(ri.Total, 0) AS LineTotal
FROM BTR_ReturJual r
INNER JOIN BTR_ReturJualItem ri ON r.ReturJualId = ri.ReturJualId
LEFT JOIN BTR_Brg b ON ri.BrgId = b.BrgId
LEFT JOIN BTR_Supplier sup ON b.SupplierId = sup.SupplierId
WHERE r.ReturJualDate >= @MonthStart
  AND r.ReturJualDate < @NextMonthStart
  AND r.VoidDate = '3000-01-01'
  AND ISNULL(r.JenisRetur, '') IN ('BAGUS', 'RUSAK')";

        public const string ListReturnItemEvidenceForPrincipalSql = @"
SELECT
    r.ReturJualId,
    ISNULL(r.ReturJualCode, '') AS ReturJualCode,
    r.ReturJualDate,
    ri.ReturJualItemId,
    ISNULL(ri.BrgId, '') AS BrgId,
    ISNULL(ri.BrgCode, '') AS BrgCode,
    ISNULL(r.JenisRetur, '') AS JenisRetur,
    ISNULL(b.SupplierId, '') AS ItemSupplierId,
    ISNULL(sup.SupplierId, '') AS SupplierId,
    ISNULL(sup.SupplierName, '') AS SupplierName,
    ISNULL(ri.SubTotal, 0) AS SubTotal,
    ISNULL(ri.DiscRp, 0) AS DiscRp
FROM BTR_ReturJual r
INNER JOIN BTR_ReturJualItem ri ON r.ReturJualId = ri.ReturJualId
LEFT JOIN BTR_Brg b ON ri.BrgId = b.BrgId
LEFT JOIN BTR_Supplier sup ON b.SupplierId = sup.SupplierId
WHERE r.ReturJualDate >= @MonthStart
  AND r.ReturJualDate < @NextMonthStart
  AND r.VoidDate = '3000-01-01'
  AND ISNULL(r.JenisRetur, '') IN ('BAGUS', 'RUSAK')
  AND ISNULL(b.SupplierId, '') <> ''
  AND ISNULL(sup.SupplierId, '') = @SupplierId";

        private readonly DatabaseOptions _opt;

        public PrincipalReturnEvidenceDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<ReturnItemEvidence> ListReturnItemEvidence(int year, int month)
        {
            var monthStart = new DateTime(year, month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<ReturnItemEvidence>(ListReturnItemEvidenceSql, new
                {
                    MonthStart = monthStart,
                    NextMonthStart = nextMonthStart
                }).ToList();
            }
        }

        public IReadOnlyList<PrincipalReturnItemEvidenceLine> ListReturnItemEvidenceForPrincipal(
            int year,
            int month,
            string supplierId)
        {
            var monthStart = new DateTime(year, month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<PrincipalReturnItemEvidenceLine>(ListReturnItemEvidenceForPrincipalSql, new
                {
                    MonthStart = monthStart,
                    NextMonthStart = nextMonthStart,
                    SupplierId = supplierId ?? string.Empty
                }).ToList();
            }
        }
    }
}
