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
    public class CustomerPelunasanSummaryDal : ICustomerPelunasanSummaryDal
    {
        private readonly DatabaseOptions _opt;

        public CustomerPelunasanSummaryDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<CustomerPelunasanSummaryDto> ListSummary(DateTime windowStart, DateTime windowEnd)
        {
            const string sql = @"
SELECT
    ISNULL(cc.CustomerCode, '') AS CustomerCode,
    ISNULL(cc.CustomerName, '') AS CustomerName,
    MAX(aa.LunasDate) AS LastPaymentDate,
    SUM(CASE WHEN aa.JenisLunas = 0 THEN aa.Nilai ELSE 0 END) AS TotalCash,
    SUM(CASE WHEN aa.JenisLunas IN (0, 1) THEN aa.Nilai ELSE 0 END) AS TotalSettlement,
    COUNT(*) AS PaymentCount
FROM BTR_PiutangLunas aa
INNER JOIN BTR_Piutang bb ON aa.PiutangId = bb.PiutangId
INNER JOIN BTR_Customer cc ON bb.CustomerId = cc.CustomerId
WHERE aa.LunasDate BETWEEN @Tgl1 AND @Tgl2
  AND aa.JenisLunas <> 2
GROUP BY cc.CustomerId, cc.CustomerCode, cc.CustomerName";

            var dp = new DynamicParameters();
            dp.Add("@Tgl1", windowStart.Date, DbType.DateTime);
            dp.Add("@Tgl2", windowEnd.Date, DbType.DateTime);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<CustomerPelunasanSummaryDto>(sql, dp);
            }
        }
    }
}
