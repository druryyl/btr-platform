using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalCustomerCoverageSnapshotDal
    {
        PrincipalCustomerCoverageResult GetCurrent();

        void ReplaceCurrent(PrincipalCustomerCoverageResult result, string refreshLogId);
    }
}
