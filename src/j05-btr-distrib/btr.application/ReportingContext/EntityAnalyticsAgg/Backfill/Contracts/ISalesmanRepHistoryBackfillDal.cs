using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts
{
    public interface ISalesmanRepHistoryBackfillDal
    {
        IReadOnlyList<DashboardSalesmanRepHistoryRow> ListForPeriod(int periodYear, int periodMonth);

        bool HasCoverage(int periodYear, int periodMonth);
    }
}
