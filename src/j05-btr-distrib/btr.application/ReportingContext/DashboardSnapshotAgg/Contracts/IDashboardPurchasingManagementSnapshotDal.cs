using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardPurchasingManagementSnapshotDal
    {
        DashboardPurchasingManagementAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardPurchasingManagementAggregateResult result, string refreshLogId);
    }
}
