using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardFieldActivityOverviewAgg
{
    public class FieldActivityBatchOrderDal : IFieldActivityBatchOrderDal
    {
        private readonly DatabaseOptions _opt;

        public FieldActivityBatchOrderDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<FieldActivityOrderBatchRow> ListByDate(DateTime visitDate)
        {
            const string sql = @"
SELECT UserEmail, CustomerId, TotalAmount
FROM BTR_Order
WHERE OrderDate = @VisitDate
ORDER BY UserEmail ASC, CustomerId ASC";

            var dp = new DynamicParameters();
            dp.AddParam("@VisitDate", visitDate.Date.ToString("yyyy-MM-dd"), SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var rows = conn.Read<FieldActivityOrderBatchRow>(sql, dp);
                return (rows ?? Enumerable.Empty<FieldActivityOrderBatchRow>()).ToList();
            }
        }
    }
}
