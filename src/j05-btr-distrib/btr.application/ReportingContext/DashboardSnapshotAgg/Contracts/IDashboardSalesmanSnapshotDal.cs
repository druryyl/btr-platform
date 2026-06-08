using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardSalesmanSnapshotDal
    {
        DashboardSalesmanAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardSalesmanAggregateResult result, string refreshLogId);
    }
}
