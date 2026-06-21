using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardCollectionSnapshotDal
    {
        DashboardCollectionAggregateResult GetCurrent();

        void ReplaceCurrent(
            DashboardCollectionAggregateResult result,
            DashboardCashFlowForecastAggregateResult forecast,
            string refreshLogId);
    }
}
