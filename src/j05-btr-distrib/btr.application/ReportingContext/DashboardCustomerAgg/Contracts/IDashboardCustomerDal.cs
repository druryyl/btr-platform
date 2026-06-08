using btr.application.ReportingContext.DashboardCustomerAgg.Queries;

namespace btr.application.ReportingContext.DashboardCustomerAgg.Contracts
{
    public interface IDashboardCustomerDal
    {
        DashboardCustomerResponse GetSummary();
    }
}
