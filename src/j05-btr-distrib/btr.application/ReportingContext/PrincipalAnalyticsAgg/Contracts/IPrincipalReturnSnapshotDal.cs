using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalReturnSnapshotDal
    {
        PrincipalReturnAggregateResult GetCurrent();

        void ReplaceCurrent(PrincipalReturnAggregateResult result, string refreshLogId);
    }
}
