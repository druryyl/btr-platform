using System.Collections.Generic;
using System.Data.SqlClient;
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
    }
}
