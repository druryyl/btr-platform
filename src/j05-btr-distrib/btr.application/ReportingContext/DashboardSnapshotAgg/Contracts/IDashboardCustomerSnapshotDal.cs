using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardCustomerSnapshotDal
    {
        DashboardCustomerAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardCustomerAggregateResult result, string refreshLogId);

        void ReplaceCurrent(
            DashboardCustomerAggregateResult result,
            DashboardCustomerRiskForecastAggregateResult forecast,
            string refreshLogId);

        void ReplaceCurrent(
            DashboardCustomerAggregateResult result,
            DashboardCustomerRiskForecastAggregateResult forecast,
            DashboardCollectionOptimizationAggregateResult optimization,
            string refreshLogId);
    }
}
