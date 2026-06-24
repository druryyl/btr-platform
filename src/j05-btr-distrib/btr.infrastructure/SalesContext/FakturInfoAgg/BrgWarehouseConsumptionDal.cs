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
    public class BrgWarehouseConsumptionDal : IBrgWarehouseConsumptionDal
    {
        private const int CommandTimeoutSeconds = 600;

        private readonly DatabaseOptions _opt;

        public BrgWarehouseConsumptionDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IEnumerable<BrgWarehouseConsumptionDto> ListConsumptionByBrgWarehouse(
            DateTime windowStart,
            DateTime windowEnd)
        {
            const string sql = @"
SELECT
    ISNULL(fi.BrgId, '') AS BrgId,
    ISNULL(aa.WarehouseId, '') AS WarehouseId,
    ISNULL(wh.WarehouseName, '') AS WarehouseName,
    SUM(fi.QtyJual) AS SoldQty30
FROM BTR_Faktur aa
INNER JOIN BTR_FakturItem fi ON aa.FakturId = fi.FakturId
LEFT JOIN BTR_Warehouse wh ON aa.WarehouseId = wh.WarehouseId
WHERE aa.VoidDate = '3000-01-01'
  AND aa.FakturDate BETWEEN @Start AND @End
GROUP BY fi.BrgId, aa.WarehouseId, wh.WarehouseName";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<BrgWarehouseConsumptionDto>(sql, new
                {
                    Start = windowStart.Date,
                    End = windowEnd.Date
                }, commandTimeout: CommandTimeoutSeconds).ToList();
            }
        }
    }
}
