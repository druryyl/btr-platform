using btr.application.ReportingContext.DashboardSalesForecastAgg.Queries;

namespace btr.application.ReportingContext.DashboardSalesForecastAgg.Contracts
{
    public interface IDashboardSalesForecastDal
    {
        DashboardSalesForecastResponse GetSummary();
    }
}
