using btr.application.ReportingContext.DashboardExecutiveAgg.Queries;

namespace btr.application.ReportingContext.DashboardExecutiveAgg.Contracts
{
    public interface IDashboardExecutiveDal
    {
        DashboardExecutiveResponse GetExecutive();
    }
}
