using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalSalesOutSnapshotDal
    {
        PrincipalSalesOutAggregateResult GetCurrent();

        void ReplaceCurrent(PrincipalSalesOutAggregateResult result, string refreshLogId);
    }
}
