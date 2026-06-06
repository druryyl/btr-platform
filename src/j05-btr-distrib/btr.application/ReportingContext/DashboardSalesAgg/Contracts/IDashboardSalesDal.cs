using btr.application.ReportingContext.DashboardSalesAgg.Queries;

namespace btr.application.ReportingContext.DashboardSalesAgg.Contracts
{
    public interface IDashboardSalesDal
    {
        DashboardSalesResponse GetSummary();
    }
}
