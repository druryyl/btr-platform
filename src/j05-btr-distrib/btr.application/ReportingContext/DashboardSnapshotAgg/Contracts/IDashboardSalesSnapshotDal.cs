using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardSalesSnapshotDal
    {
        DashboardSalesAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardSalesAggregateResult result, string refreshLogId);
    }
}
