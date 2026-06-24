using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using btr.application.SalesContext.FakturInfo;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.SalesContext.FakturInfoAgg
{
    public class CustomerOmzetHistoryDal : ICustomerOmzetHistoryDal
    {
        private readonly DatabaseOptions _opt;

        public CustomerOmzetHistoryDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<CustomerOmzetHistoryDto> ListOmzetByCustomer(Periode currentMonth, Periode priorMonth)
        {
            const string sql = @"
SELECT
    ISNULL(cc.CustomerCode, '') AS CustomerCode,
    ISNULL(cc.CustomerName, '') AS CustomerName,
    SUM(CASE WHEN aa.FakturDate BETWEEN @CurrentTgl1 AND @CurrentTgl2 THEN aa.GrandTotal ELSE 0 END) AS CurrentMonthOmzet,
    SUM(CASE WHEN aa.FakturDate BETWEEN @PriorTgl1 AND @PriorTgl2 THEN aa.GrandTotal ELSE 0 END) AS PriorMonthOmzet
FROM BTR_Faktur aa
INNER JOIN BTR_Customer cc ON aa.CustomerId = cc.CustomerId
WHERE aa.VoidDate = '3000-01-01'
  AND (
        aa.FakturDate BETWEEN @CurrentTgl1 AND @CurrentTgl2
        OR aa.FakturDate BETWEEN @PriorTgl1 AND @PriorTgl2
      )
GROUP BY cc.CustomerId, cc.CustomerCode, cc.CustomerName";

            var dp = new DynamicParameters();
            dp.Add("@CurrentTgl1", currentMonth.Tgl1.Date, DbType.DateTime);
            dp.Add("@CurrentTgl2", currentMonth.Tgl2.Date, DbType.DateTime);
            dp.Add("@PriorTgl1", priorMonth.Tgl1.Date, DbType.DateTime);
            dp.Add("@PriorTgl2", priorMonth.Tgl2.Date, DbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<CustomerOmzetHistoryDto>(sql, dp);
            }
        }
    }
}
