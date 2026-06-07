using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardInventorySnapshotDal
    {
        DashboardInventoryAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardInventoryAggregateResult result, string refreshLogId);
    }
}
