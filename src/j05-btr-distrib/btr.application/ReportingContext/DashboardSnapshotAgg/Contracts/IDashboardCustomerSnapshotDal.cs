using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardCustomerSnapshotDal
    {
        DashboardCustomerAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardCustomerAggregateResult result, string refreshLogId);
    }
}
