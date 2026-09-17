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
    public class CustomerPrincipalRelationshipEvidenceDal : ICustomerPrincipalRelationshipEvidenceDal
    {
        public const string ListHistoricalPairEvidenceSql = @"
SELECT
    LTRIM(RTRIM(f.CustomerId)) AS CustomerId,
    MAX(LTRIM(RTRIM(ISNULL(c.CustomerName, '')))) AS CustomerName,
    LTRIM(RTRIM(sup.SupplierId)) AS SupplierId,
    MAX(LTRIM(RTRIM(ISNULL(sup.SupplierName, '')))) AS SupplierName,
    MIN(f.FakturDate) AS FirstTransactionDate,
    MAX(f.FakturDate) AS LastTransactionDate,
    SUM(ISNULL(fi.SubTotal, 0) - ISNULL(fi.DiscRp, 0)) AS SalesOutAmount,
    COUNT(*) AS LineCount
FROM BTR_Faktur f
INNER JOIN BTR_FakturItem fi ON f.FakturId = fi.FakturId
INNER JOIN BTR_Brg b ON fi.BrgId = b.BrgId
INNER JOIN BTR_Supplier sup ON b.SupplierId = sup.SupplierId
LEFT JOIN BTR_Customer c ON f.CustomerId = c.CustomerId
WHERE f.VoidDate = '3000-01-01'
  AND f.FakturDate < '3000-01-01'
  AND LTRIM(RTRIM(ISNULL(f.CustomerId, ''))) <> ''
  AND LTRIM(RTRIM(ISNULL(b.SupplierId, ''))) <> ''
  AND LTRIM(RTRIM(ISNULL(sup.SupplierId, ''))) <> ''
GROUP BY LTRIM(RTRIM(f.CustomerId)), LTRIM(RTRIM(sup.SupplierId))";

        private readonly DatabaseOptions _opt;

        public CustomerPrincipalRelationshipEvidenceDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<CustomerPrincipalRelationshipPairEvidence> ListHistoricalPairEvidence()
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<CustomerPrincipalRelationshipPairEvidence>(
                    ListHistoricalPairEvidenceSql,
                    commandTimeout: 180).ToList();
            }
        }
    }
}
