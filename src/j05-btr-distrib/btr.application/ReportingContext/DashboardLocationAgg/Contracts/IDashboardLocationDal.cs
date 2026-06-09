using btr.application.ReportingContext.DashboardLocationAgg.Queries;

namespace btr.application.ReportingContext.DashboardLocationAgg.Contracts
{
    public interface IDashboardLocationDal
    {
        DashboardLocationResponse GetSummary();
    }
}
