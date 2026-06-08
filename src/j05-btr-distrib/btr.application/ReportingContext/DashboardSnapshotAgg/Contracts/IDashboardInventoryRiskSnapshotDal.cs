using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardInventoryRiskSnapshotDal
    {
        DashboardInventoryRiskAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardInventoryRiskAggregateResult result, string refreshLogId);
    }
}
