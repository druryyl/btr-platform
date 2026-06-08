using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardSnapshotAgg
{
    public class PiutangOpenBalanceWithSalesmanDal : IPiutangOpenBalanceWithSalesmanDal
    {
        private const string Sql = @"
SELECT
    ISNULL(sp.SalesPersonId, '') AS SalesPersonId,
    ISNULL(sp.SalesPersonName, '') AS SalesPersonName,
    ISNULL(ee.CustomerCode, '') AS CustomerCode,
    ISNULL(ee.CustomerName, '') AS CustomerName,
    ISNULL(aa.DueDate, '3000-01-01') AS JatuhTempo,
    aa.Sisa AS KurangBayar
FROM BTR_Piutang aa
    LEFT JOIN BTR_Faktur bb ON aa.PiutangId = bb.FakturId
    LEFT JOIN BTR_SalesPerson sp ON bb.SalesPersonId = sp.SalesPersonId
    LEFT JOIN BTR_Customer ee ON bb.CustomerId = ee.CustomerId
WHERE aa.Sisa > 1";

        private readonly DatabaseOptions _opt;

        public PiutangOpenBalanceWithSalesmanDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<PiutangOpenBalanceWithSalesmanDto> ListOpenBalances()
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<PiutangOpenBalanceWithSalesmanDto>(Sql).ToList();
            }
        }
    }
}
