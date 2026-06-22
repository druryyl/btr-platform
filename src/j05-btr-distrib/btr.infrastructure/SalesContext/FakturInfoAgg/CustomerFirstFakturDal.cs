using System.Collections.Generic;
using System.Data.SqlClient;
using btr.application.SalesContext.FakturInfo;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.FakturInfoAgg
{
    public class CustomerFirstFakturDal : ICustomerFirstFakturDal
    {
        private readonly DatabaseOptions _opt;

        public CustomerFirstFakturDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<CustomerFirstFakturDto> ListFirstFakturByCustomer()
        {
            const string sql = @"
SELECT
    ISNULL(cc.CustomerCode, '') AS CustomerCode,
    ISNULL(cc.CustomerName, '') AS CustomerName,
    MIN(aa.FakturDate) AS FirstFakturDate
FROM BTR_Faktur aa
INNER JOIN BTR_Customer cc ON aa.CustomerId = cc.CustomerId
WHERE aa.VoidDate = '3000-01-01'
GROUP BY cc.CustomerId, cc.CustomerCode, cc.CustomerName";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<CustomerFirstFakturDto>(sql);
            }
        }
    }
}
