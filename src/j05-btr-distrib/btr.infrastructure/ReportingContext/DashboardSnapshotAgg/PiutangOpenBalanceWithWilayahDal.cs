using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class PiutangOpenBalanceWithWilayahDal : IPiutangOpenBalanceWithWilayahDal
    {
        private const string Sql = @"
SELECT
    ISNULL(w.WilayahId, '') AS WilayahId,
    ISNULL(w.WilayahName, '') AS WilayahName,
    ISNULL(c.CustomerCode, '') AS CustomerCode,
    ISNULL(c.CustomerName, '') AS CustomerName,
    ISNULL(p.DueDate, '3000-01-01') AS JatuhTempo,
    p.Sisa AS KurangBayar
FROM BTR_Piutang p
    LEFT JOIN BTR_Faktur f ON p.PiutangId = f.FakturId
    LEFT JOIN BTR_Customer c ON f.CustomerId = c.CustomerId
    LEFT JOIN BTR_Wilayah w ON c.WilayahId = w.WilayahId
WHERE p.Sisa > 1";

        private readonly DatabaseOptions _opt;

        public PiutangOpenBalanceWithWilayahDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<PiutangOpenBalanceWithWilayahDto> ListOpenBalances()
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<PiutangOpenBalanceWithWilayahDto>(Sql).ToList();
            }
        }
    }
}
