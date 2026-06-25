using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.EntityAnalyticsAgg
{
    public class SalesmanRepHistoryBackfillDal : ISalesmanRepHistoryBackfillDal
    {
        private readonly DatabaseOptions _opt;

        public SalesmanRepHistoryBackfillDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public bool HasCoverage(int periodYear, int periodMonth)
        {
            return ListForPeriod(periodYear, periodMonth).Count > 0;
        }

        public IReadOnlyList<DashboardSalesmanRepHistoryRow> ListForPeriod(int periodYear, int periodMonth)
        {
            const string sql = @"
SELECT
    PeriodYear,
    PeriodMonth,
    SalesPersonId,
    SalesPersonCode,
    SalesPersonName,
    TargetAmount,
    CompletedOmzet,
    AchievementPercent,
    AchievementBand,
    OpenBalance,
    IsActive
FROM BTRPD_SalesmanRepHistory
WHERE PeriodYear = @PeriodYear
  AND PeriodMonth = @PeriodMonth";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<DashboardSalesmanRepHistoryRow>(sql, new
                {
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth
                }).ToList();
            }
        }
    }
}
