using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IDashboardInventoryRiskSnapshotDal
    {
        DashboardInventoryRiskAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardInventoryRiskAggregateResult result, string refreshLogId);

        void ReplaceCurrent(
            DashboardInventoryRiskAggregateResult result,
            DashboardInventoryForecastAggregateResult forecast,
            string refreshLogId);

        void ReplaceCurrent(
            DashboardInventoryRiskAggregateResult result,
            DashboardInventoryForecastAggregateResult forecast,
            DashboardInventoryOptimizationAggregateResult optimization,
            string refreshLogId);
    }
}
