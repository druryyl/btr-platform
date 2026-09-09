using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalTargetSnapshotDal
    {
        PrincipalTargetAggregateResult GetCurrent();

        void ReplaceCurrent(PrincipalTargetAggregateResult result, string refreshLogId);
    }
}
