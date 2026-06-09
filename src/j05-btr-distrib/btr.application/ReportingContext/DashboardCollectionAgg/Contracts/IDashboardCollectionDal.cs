using btr.application.ReportingContext.DashboardCollectionAgg.Queries;

namespace btr.application.ReportingContext.DashboardCollectionAgg.Contracts
{
    public interface IDashboardCollectionDal
    {
        DashboardCollectionResponse GetSummary();
    }
}
