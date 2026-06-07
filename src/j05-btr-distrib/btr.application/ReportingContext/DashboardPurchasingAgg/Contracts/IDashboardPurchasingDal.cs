using btr.application.ReportingContext.DashboardPurchasingAgg.Queries;

namespace btr.application.ReportingContext.DashboardPurchasingAgg.Contracts
{
    public interface IDashboardPurchasingDal
    {
        DashboardPurchasingResponse GetSummary();
    }
}
