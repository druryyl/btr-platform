using btr.application.ReportingContext.DashboardCustomerRiskForecastAgg.Queries;

namespace btr.application.ReportingContext.DashboardCustomerRiskForecastAgg.Contracts
{
    public interface IDashboardCustomerRiskForecastDal
    {
        DashboardCustomerRiskForecastResponse GetCurrent();
    }
}
