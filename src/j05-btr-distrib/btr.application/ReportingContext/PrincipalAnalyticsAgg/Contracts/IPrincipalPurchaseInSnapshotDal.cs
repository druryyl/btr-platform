using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalPurchaseInSnapshotDal
    {
        PrincipalPurchaseInAggregateResult GetCurrent();

        void ReplaceCurrent(PrincipalPurchaseInAggregateResult result, string refreshLogId);
    }
}
