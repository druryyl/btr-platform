using btr.application.ReportingContext.DashboardSalesmanAgg.Queries;

namespace btr.application.ReportingContext.DashboardSalesmanAgg.Contracts
{
    public interface IDashboardSalesmanDal
    {
        DashboardSalesmanResponse GetSummary();
    }
}
