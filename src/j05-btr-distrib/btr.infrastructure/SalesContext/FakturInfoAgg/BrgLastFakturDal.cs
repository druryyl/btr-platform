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
    public class BrgLastFakturDal : IBrgLastFakturDal
    {
        private const int CommandTimeoutSeconds = 600;

        private readonly DatabaseOptions _opt;

        public BrgLastFakturDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<BrgLastFakturDto> ListLastFakturByBrg()
        {
            return QueryLastFakturByBrg(null);
        }

        public IEnumerable<BrgLastFakturDto> ListLastFakturByBrgAsOf(DateTime asOfDate)
        {
            return QueryLastFakturByBrg(asOfDate.Date);
        }

        private IEnumerable<BrgLastFakturDto> QueryLastFakturByBrg(DateTime? asOfDate)
        {
            var asOfFilter = asOfDate.HasValue ? "AND aa.FakturDate <= @AsOfDate" : string.Empty;
            var sql = $@"
SELECT
    ISNULL(bb.BrgId, '') AS BrgId,
    ISNULL(bb.BrgCode, '') AS BrgCode,
    ISNULL(bb.BrgName, '') AS BrgName,
    MAX(aa.FakturDate) AS LastFakturDate
FROM BTR_Faktur aa
INNER JOIN BTR_FakturItem fi ON aa.FakturId = fi.FakturId
INNER JOIN BTR_Brg bb ON fi.BrgId = bb.BrgId
WHERE aa.VoidDate = '3000-01-01'
{asOfFilter}
GROUP BY bb.BrgId, bb.BrgCode, bb.BrgName";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<BrgLastFakturDto>(
                    sql,
                    asOfDate.HasValue ? new { AsOfDate = asOfDate.Value } : null,
                    commandTimeout: CommandTimeoutSeconds).ToList();
            }
        }
    }
}
