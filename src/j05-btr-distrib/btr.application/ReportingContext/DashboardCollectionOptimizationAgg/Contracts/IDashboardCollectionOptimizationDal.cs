using btr.application.ReportingContext.DashboardCollectionOptimizationAgg.Queries;

namespace btr.application.ReportingContext.DashboardCollectionOptimizationAgg.Contracts
{
    public interface IDashboardCollectionOptimizationDal
    {
        DashboardCollectionOptimizationResponse GetCurrent();
    }
}
