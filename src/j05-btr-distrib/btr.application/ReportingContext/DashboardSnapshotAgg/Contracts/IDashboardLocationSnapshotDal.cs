using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardLocationSnapshotDal
    {
        DashboardLocationAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardLocationAggregateResult result, string refreshLogId);
    }
}
