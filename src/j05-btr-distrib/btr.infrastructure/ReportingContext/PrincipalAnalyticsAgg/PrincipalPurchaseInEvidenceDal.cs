using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg
{
    public class PrincipalPurchaseInEvidenceDal : IPrincipalPurchaseInEvidenceDal
    {
        public const string ListPurchaseDetailSql = @"
SELECT
    ii.InvoiceId,
    ii.InvoiceItemId,
    ISNULL(i.SupplierId, '') AS SupplierId,
    ISNULL(sup.SupplierName, '') AS SupplierName,
    ISNULL(ii.Total, 0) AS PurchaseDetailTotal
FROM BTR_InvoiceItem ii
INNER JOIN BTR_Invoice i ON ii.InvoiceId = i.InvoiceId
LEFT JOIN BTR_Supplier sup ON i.SupplierId = sup.SupplierId
WHERE i.InvoiceDate >= @MonthStart
  AND i.InvoiceDate < @NextMonthStart
  AND i.VoidDate = '3000-01-01'";

        private readonly DatabaseOptions _opt;

        public PrincipalPurchaseInEvidenceDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<PurchaseDetailEvidence> ListPurchaseDetail(int year, int month)
        {
            var monthStart = new System.DateTime(year, month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<PurchaseDetailEvidence>(ListPurchaseDetailSql, new
                {
                    MonthStart = monthStart,
                    NextMonthStart = nextMonthStart
                }).ToList();
            }
        }
    }
}
