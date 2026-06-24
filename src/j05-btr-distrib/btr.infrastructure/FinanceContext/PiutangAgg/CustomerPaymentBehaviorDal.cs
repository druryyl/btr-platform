using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.FinanceContext.PiutangAgg
{
    public class CustomerPaymentBehaviorDal : ICustomerPaymentBehaviorDal
    {
        private readonly DatabaseOptions _opt;

        public CustomerPaymentBehaviorDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<CustomerPaymentBehaviorDto> ListPaymentBehavior(
            DateTime windowStart,
            DateTime windowEnd,
            int minSettledFakturs)
        {
            const string sql = @"
SELECT
    ISNULL(cc.CustomerCode, '') AS CustomerCode,
    ISNULL(cc.CustomerName, '') AS CustomerName,
    COUNT(*) AS SettledFakturCount,
    AVG(CAST(DATEDIFF(day, bb.DueDate, settled.LastLunasDate) AS DECIMAL(18, 2))) AS AvgPaymentLagDays
FROM BTR_Piutang bb
INNER JOIN BTR_Customer cc ON bb.CustomerId = cc.CustomerId
INNER JOIN (
    SELECT
        pl.PiutangId,
        MAX(pl.LunasDate) AS LastLunasDate
    FROM BTR_PiutangLunas pl
    WHERE pl.JenisLunas <> 2
      AND pl.LunasDate BETWEEN @Tgl1 AND @Tgl2
    GROUP BY pl.PiutangId
) settled ON bb.PiutangId = settled.PiutangId
WHERE bb.Sisa <= 1
GROUP BY cc.CustomerId, cc.CustomerCode, cc.CustomerName
HAVING COUNT(*) >= @MinSettledFakturs";

            var dp = new DynamicParameters();
            dp.Add("@Tgl1", windowStart.Date, DbType.DateTime);
            dp.Add("@Tgl2", windowEnd.Date, DbType.DateTime);
            dp.Add("@MinSettledFakturs", minSettledFakturs, DbType.Int32);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<CustomerPaymentBehaviorDto>(sql, dp);
            }
        }
    }
}
