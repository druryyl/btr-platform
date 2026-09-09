using System;
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
    public class PrincipalContributionEvidenceDal : IPrincipalContributionEvidenceDal
    {
        public const string ListContributionEvidenceSql = @"
SELECT
    f.FakturId,
    fi.FakturItemId,
    ISNULL(f.SalesPersonId, '') AS SalesPersonId,
    ISNULL(sp.SalesPersonCode, '') AS SalesPersonCode,
    ISNULL(sp.SalesPersonName, '') AS SalesPersonName,
    ISNULL(b.SupplierId, '') AS ItemSupplierId,
    ISNULL(sup.SupplierId, '') AS SupplierId,
    ISNULL(sup.SupplierName, '') AS SupplierName,
    ISNULL(fi.SubTotal, 0) AS SubTotal,
    ISNULL(fi.DiscRp, 0) AS DiscRp
FROM BTR_Faktur f
INNER JOIN BTR_FakturItem fi ON f.FakturId = fi.FakturId
LEFT JOIN BTR_Brg b ON fi.BrgId = b.BrgId
LEFT JOIN BTR_Supplier sup ON b.SupplierId = sup.SupplierId
LEFT JOIN BTR_SalesPerson sp ON f.SalesPersonId = sp.SalesPersonId
WHERE f.FakturDate BETWEEN @Tgl1 AND @Tgl2
  AND f.VoidDate = '3000-01-01'";

        private readonly DatabaseOptions _opt;

        public PrincipalContributionEvidenceDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<PrincipalContributionEvidenceLine> ListContributionEvidence(Periode periode)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<PrincipalContributionEvidenceLine>(ListContributionEvidenceSql, new
                {
                    Tgl1 = periode.Tgl1,
                    Tgl2 = periode.Tgl2
                }).ToList();
            }
        }
    }
}
