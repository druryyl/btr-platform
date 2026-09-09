using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalActiveCustomerSnapshotDal
    {
        PrincipalActiveCustomerResult GetCurrent();

        void ReplaceCurrent(PrincipalActiveCustomerResult result, string refreshLogId);
    }
}
