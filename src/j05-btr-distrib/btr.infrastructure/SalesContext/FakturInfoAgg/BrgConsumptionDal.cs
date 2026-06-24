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
    public class BrgConsumptionDal : IBrgConsumptionDal
    {
        private const int CommandTimeoutSeconds = 600;

        private readonly DatabaseOptions _opt;

        public BrgConsumptionDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<BrgConsumptionDto> ListConsumptionByBrg(
            DateTime window30Start,
            DateTime window90Start,
            DateTime windowEnd)
        {
            const string sql = @"
SELECT
    ISNULL(fi.BrgId, '') AS BrgId,
    SUM(CASE WHEN aa.FakturDate BETWEEN @Start30 AND @End THEN fi.QtyJual ELSE 0 END) AS SoldQty30,
    SUM(CASE WHEN aa.FakturDate BETWEEN @Start90 AND @End THEN fi.QtyJual ELSE 0 END) AS SoldQty90,
    MIN(aa.FakturDate) AS FirstFakturDate,
    CAST(ISNULL(bb.IsAktif, 1) AS BIT) AS IsAktif
FROM BTR_Faktur aa
INNER JOIN BTR_FakturItem fi ON aa.FakturId = fi.FakturId
INNER JOIN BTR_Brg bb ON fi.BrgId = bb.BrgId
WHERE aa.VoidDate = '3000-01-01'
  AND aa.FakturDate BETWEEN @Start90 AND @End
GROUP BY fi.BrgId, bb.IsAktif";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<BrgConsumptionDto>(sql, new
                {
                    Start30 = window30Start.Date,
                    Start90 = window90Start.Date,
                    End = windowEnd.Date
                }, commandTimeout: CommandTimeoutSeconds).ToList();
            }
        }

        public IEnumerable<DailyCompanyConsumptionDto> ListDailyCompanyConsumption(
            DateTime windowStart,
            DateTime windowEnd)
        {
            const string sql = @"
SELECT
    aa.FakturDate AS FakturDate,
    SUM(fi.QtyJual) AS UnitsSold
FROM BTR_Faktur aa
INNER JOIN BTR_FakturItem fi ON aa.FakturId = fi.FakturId
WHERE aa.VoidDate = '3000-01-01'
  AND aa.FakturDate BETWEEN @Start AND @End
GROUP BY aa.FakturDate
ORDER BY aa.FakturDate";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<DailyCompanyConsumptionDto>(sql, new
                {
                    Start = windowStart.Date,
                    End = windowEnd.Date
                }, commandTimeout: CommandTimeoutSeconds).ToList();
            }
        }
    }
}
