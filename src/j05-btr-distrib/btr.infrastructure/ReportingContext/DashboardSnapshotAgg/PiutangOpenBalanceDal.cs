using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class PiutangOpenBalanceDal : IPiutangOpenBalanceDal
    {
        private const string Sql = @"
SELECT
    ISNULL(ee.CustomerId, '') AS CustomerId,
    ISNULL(ee.CustomerCode, '') AS CustomerCode,
    ISNULL(ee.CustomerName, '') AS CustomerName,
    ISNULL(aa.DueDate, '3000-01-01') AS JatuhTempo,
    aa.Sisa AS KurangBayar
FROM BTR_Piutang aa
    LEFT JOIN BTR_Faktur bb ON aa.PiutangId = bb.FakturId
    LEFT JOIN BTR_Customer ee ON bb.CustomerId = ee.CustomerId
WHERE aa.Sisa > 1";

        private const string SqlAsOf = @"
SELECT
    ISNULL(ee.CustomerId, '') AS CustomerId,
    ISNULL(ee.CustomerCode, '') AS CustomerCode,
    ISNULL(ee.CustomerName, '') AS CustomerName,
    ISNULL(aa.DueDate, '3000-01-01') AS JatuhTempo,
    aa.Total - ISNULL(pl.PaidThroughAsOf, 0) AS KurangBayar
FROM BTR_Piutang aa
    INNER JOIN BTR_Faktur bb ON aa.PiutangId = bb.FakturId
    LEFT JOIN BTR_Customer ee ON bb.CustomerId = ee.CustomerId
    LEFT JOIN (
        SELECT PiutangId, SUM(Nilai) AS PaidThroughAsOf
        FROM BTR_PiutangLunas
        WHERE LunasDate <= @AsOfDate
          AND LunasDate <> '3000-01-01'
        GROUP BY PiutangId
    ) pl ON aa.PiutangId = pl.PiutangId
WHERE bb.VoidDate = '3000-01-01'
  AND bb.FakturDate <= @AsOfDate
  AND (aa.Total - ISNULL(pl.PaidThroughAsOf, 0)) > 1";

        private readonly DatabaseOptions _opt;

        public PiutangOpenBalanceDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<PiutangOpenBalanceDto> ListOpenBalances()
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<PiutangOpenBalanceDto>(Sql).ToList();
            }
        }

        public IReadOnlyList<PiutangOpenBalanceDto> ListOpenBalancesAsOf(DateTime asOfDate)
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<PiutangOpenBalanceDto>(SqlAsOf, new { AsOfDate = asOfDate.Date }).ToList();
            }
        }
    }
}
