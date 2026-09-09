using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalSalesmanContributionSnapshotDal
    {
        PrincipalSalesmanContributionResult GetCurrent();

        void ReplaceCurrent(PrincipalSalesmanContributionResult result, string refreshLogId);
    }
}
