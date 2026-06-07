using System;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardPiutangSnapshotDal
    {
        DashboardPiutangAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardPiutangAggregateResult result, string refreshLogId);
    }
}
