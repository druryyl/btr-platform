using btr.application.ReportingContext.DashboardCashFlowForecastAgg.Queries;

namespace btr.application.ReportingContext.DashboardCashFlowForecastAgg.Contracts
{
    public interface IDashboardCashFlowForecastDal
    {
        DashboardCashFlowForecastResponse GetSummary();
    }
}
