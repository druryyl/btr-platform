using btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Models;

namespace btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts
{
    public interface IDashboardFieldActivitySnapshotDal
    {
        DashboardFieldActivityAggregateResult GetCurrent();

        void ReplaceCurrent(DashboardFieldActivityAggregateResult aggregate, string refreshLogId);
    }
}
