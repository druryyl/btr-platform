using btr.application.ReportingContext.DashboardAlertCenterAgg.Queries;

namespace btr.application.ReportingContext.DashboardAlertCenterAgg.Contracts
{
    public interface IDashboardAlertCenterDal
    {
        DashboardAlertCenterResponse GetAlerts();
    }
}
