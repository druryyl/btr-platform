using btr.application.ReportingContext.DashboardInventoryForecastAgg.Queries;

namespace btr.application.ReportingContext.DashboardInventoryForecastAgg.Contracts
{
    public interface IDashboardInventoryForecastDal
    {
        DashboardInventoryForecastResponse GetSummary();
    }
}
