using btr.application.ReportingContext.DashboardInventoryOptimizationAgg.Queries;

namespace btr.application.ReportingContext.DashboardInventoryOptimizationAgg.Contracts
{
    public interface IDashboardInventoryOptimizationDal
    {
        DashboardInventoryOptimizationResponse GetSummary();
    }
}
