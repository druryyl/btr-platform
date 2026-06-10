using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Contracts;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardFieldActivityAgg
{
    public class FieldActivityOrderDal : IFieldActivityOrderDal
    {
        private readonly DatabaseOptions _opt;

        public FieldActivityOrderDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public ISet<string> ListCustomerIdsWithOrder(
            string salesPersonEmail, DateTime visitDate)
        {
            const string sql = @"
SELECT DISTINCT CustomerId
FROM BTR_Order
WHERE OrderDate = @VisitDate
  AND UserEmail = @UserEmail";

            var dp = new DynamicParameters();
            dp.AddParam("@VisitDate", visitDate.Date.ToString("yyyy-MM-dd"), SqlDbType.VarChar);
            dp.AddParam("@UserEmail", salesPersonEmail, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var rows = conn.Read<string>(sql, dp);
                return new HashSet<string>(rows ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
