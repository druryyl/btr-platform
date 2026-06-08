using btr.application.ReportingContext.DashboardInventoryRiskAgg.Queries;

namespace btr.application.ReportingContext.DashboardInventoryRiskAgg.Contracts
{
    public interface IDashboardInventoryRiskDal
    {
        DashboardInventoryRiskResponse GetSummary();
    }
}
