using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.SalesContext.FakturInfo;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.FakturInfoAgg
{
    public class CustomerLastFakturDal : ICustomerLastFakturDal
    {
        private readonly DatabaseOptions _opt;

        public CustomerLastFakturDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<CustomerLastFakturDto> ListLastFakturByCustomer()
        {
            const string sql = @"
SELECT
    ISNULL(cc.CustomerCode, '') AS CustomerCode,
    ISNULL(cc.CustomerName, '') AS CustomerName,
    MAX(aa.FakturDate) AS LastFakturDate
FROM BTR_Faktur aa
INNER JOIN BTR_Customer cc ON aa.CustomerId = cc.CustomerId
WHERE aa.VoidDate = '3000-01-01'
GROUP BY cc.CustomerId, cc.CustomerCode, cc.CustomerName";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<CustomerLastFakturDto>(sql);
            }
        }

        public IEnumerable<CustomerLastFakturWithSalesmanDto> ListLastFakturWithSalesmanByCustomer()
        {
            return QueryLastFakturWithSalesman(null);
        }

        public IEnumerable<CustomerLastFakturDto> ListLastFakturByCustomerAsOf(DateTime asOfDate)
        {
            const string sql = @"
SELECT
    ISNULL(cc.CustomerCode, '') AS CustomerCode,
    ISNULL(cc.CustomerName, '') AS CustomerName,
    MAX(aa.FakturDate) AS LastFakturDate
FROM BTR_Faktur aa
INNER JOIN BTR_Customer cc ON aa.CustomerId = cc.CustomerId
WHERE aa.VoidDate = '3000-01-01'
  AND aa.FakturDate <= @AsOfDate
GROUP BY cc.CustomerId, cc.CustomerCode, cc.CustomerName";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<CustomerLastFakturDto>(sql, new { AsOfDate = asOfDate.Date });
            }
        }

        public IEnumerable<CustomerLastFakturWithSalesmanDto> ListLastFakturWithSalesmanByCustomerAsOf(DateTime asOfDate)
        {
            return QueryLastFakturWithSalesman(asOfDate.Date);
        }

        private IEnumerable<CustomerLastFakturWithSalesmanDto> QueryLastFakturWithSalesman(DateTime? asOfDate)
        {
            var asOfFilter = asOfDate.HasValue ? "AND aa.FakturDate <= @AsOfDate" : string.Empty;
            var sql = $@"
WITH Ranked AS (
    SELECT
        ISNULL(cc.CustomerCode, '') AS CustomerCode,
        ISNULL(cc.CustomerName, '') AS CustomerName,
        aa.FakturDate AS LastFakturDate,
        ISNULL(sp.SalesPersonId, '') AS SalesPersonId,
        ISNULL(sp.SalesPersonName, '') AS SalesPersonName,
        ROW_NUMBER() OVER (
            PARTITION BY cc.CustomerId
            ORDER BY aa.FakturDate DESC, aa.FakturId DESC
        ) AS rn
    FROM BTR_Faktur aa
    INNER JOIN BTR_Customer cc ON aa.CustomerId = cc.CustomerId
    LEFT JOIN BTR_SalesPerson sp ON aa.SalesPersonId = sp.SalesPersonId
    WHERE aa.VoidDate = '3000-01-01'
    {asOfFilter}
)
SELECT CustomerCode, CustomerName, LastFakturDate, SalesPersonId, SalesPersonName
FROM Ranked
WHERE rn = 1";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<CustomerLastFakturWithSalesmanDto>(
                    sql,
                    asOfDate.HasValue ? new { AsOfDate = asOfDate.Value } : null);
            }
        }
    }
}
