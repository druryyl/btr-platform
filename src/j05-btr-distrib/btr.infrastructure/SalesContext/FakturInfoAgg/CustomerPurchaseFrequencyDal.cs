using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using btr.application.SalesContext.FakturInfo;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.FakturInfoAgg
{
    public class CustomerPurchaseFrequencyDal : ICustomerPurchaseFrequencyDal
    {
        private readonly DatabaseOptions _opt;

        public CustomerPurchaseFrequencyDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<CustomerPurchaseFrequencyDto> ListFakturCountByCustomer(DateTime from, DateTime to)
        {
            const string sql = @"
SELECT
    ISNULL(cc.CustomerCode, '') AS CustomerCode,
    ISNULL(cc.CustomerName, '') AS CustomerName,
    COUNT(*) AS FakturCount
FROM BTR_Faktur aa
INNER JOIN BTR_Customer cc ON aa.CustomerId = cc.CustomerId
WHERE aa.VoidDate = '3000-01-01'
  AND aa.FakturDate >= @From
  AND aa.FakturDate <= @To
GROUP BY cc.CustomerId, cc.CustomerCode, cc.CustomerName";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<CustomerPurchaseFrequencyDto>(sql, new { From = from.Date, To = to.Date });
            }
        }
    }
}
