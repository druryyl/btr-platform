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
    public class FieldActivityBatchCheckInDal : IFieldActivityBatchCheckInDal
    {
        private readonly DatabaseOptions _opt;

        public FieldActivityBatchCheckInDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyList<FieldActivityBatchCheckInRow> ListByDate(DateTime visitDate)
        {
            const string sql = @"
WITH Ranked AS (
    SELECT
        aa.CheckInId, aa.UserEmail, aa.CustomerId, aa.CustomerCode, aa.CustomerName,
        aa.CheckInTime, aa.CheckInLatitude, aa.CheckInLongitude,
        aa.CustomerLatitude, aa.CustomerLongitude, aa.Accuracy,
        ROW_NUMBER() OVER (
            PARTITION BY aa.UserEmail, aa.CustomerId
            ORDER BY aa.CheckInTime ASC, aa.CheckInId ASC
        ) AS rn
    FROM BTR_CheckIn aa
    WHERE aa.CheckInDate = @VisitDate
)
SELECT
    CheckInId, UserEmail, CustomerId, CustomerCode, CustomerName,
    CheckInTime, CheckInLatitude, CheckInLongitude,
    CustomerLatitude, CustomerLongitude, Accuracy
FROM Ranked
WHERE rn = 1
ORDER BY UserEmail ASC, CheckInTime ASC, CustomerId ASC";

            var dp = new DynamicParameters();
            dp.AddParam("@VisitDate", visitDate.Date.ToString("yyyy-MM-dd"), SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var rows = conn.Read<FieldActivityBatchCheckInRow>(sql, dp);
                return (rows ?? Enumerable.Empty<FieldActivityBatchCheckInRow>()).ToList();
            }
        }
    }
}
