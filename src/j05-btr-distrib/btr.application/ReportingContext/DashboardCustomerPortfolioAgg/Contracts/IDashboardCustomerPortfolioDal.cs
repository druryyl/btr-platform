using btr.application.ReportingContext.DashboardCustomerPortfolioAgg.Queries;

namespace btr.application.ReportingContext.DashboardCustomerPortfolioAgg.Contracts
{
    public interface IDashboardCustomerPortfolioDal
    {
        DashboardCustomerPortfolioResponse GetCurrent();

        DashboardCustomerPortfolioKpiDto GetCurrentKpi();
    }
}
