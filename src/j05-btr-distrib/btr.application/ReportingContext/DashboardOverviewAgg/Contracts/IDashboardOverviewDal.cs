using btr.application.ReportingContext.DashboardOverviewAgg.Queries;

namespace btr.application.ReportingContext.DashboardOverviewAgg.Contracts
{
    public interface IDashboardOverviewDal
    {
        DashboardOverviewResponse GetOverview();
    }
}
