using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg
{
    public class PrincipalTargetEvidenceDal : IPrincipalTargetEvidenceDal
    {
        public const string ListSalesmanPrincipalTargetsSql = @"
SELECT
    t.SalesPersonId,
    t.SupplierId,
    ISNULL(sup.SupplierName, '') AS SupplierName,
    t.TargetYear,
    t.TargetMonth,
    ISNULL(t.TargetAmount, 0) AS TargetAmount
FROM BTR_SalesPersonPrincipalTarget t
LEFT JOIN BTR_Supplier sup ON t.SupplierId = sup.SupplierId
WHERE t.TargetYear = @TargetYear
  AND t.TargetMonth = @TargetMonth";

        private readonly DatabaseOptions _opt;

        public PrincipalTargetEvidenceDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<SalesmanPrincipalTargetEvidence> ListSalesmanPrincipalTargets(int year, int month)
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<SalesmanPrincipalTargetEvidence>(ListSalesmanPrincipalTargetsSql, new
                {
                    TargetYear = year,
                    TargetMonth = month
                }).ToList();
            }
        }
    }
}
