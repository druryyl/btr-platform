using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardPurchasingSnapshotDal
    {
        DashboardPurchasingAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardPurchasingAggregateResult result, string refreshLogId);
    }
}
