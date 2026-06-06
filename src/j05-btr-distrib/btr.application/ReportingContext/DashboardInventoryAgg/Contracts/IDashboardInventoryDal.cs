using btr.application.ReportingContext.DashboardInventoryAgg.Queries;

namespace btr.application.ReportingContext.DashboardInventoryAgg.Contracts
{
    public interface IDashboardInventoryDal
    {
        DashboardInventoryResponse GetSummary();
    }
}
