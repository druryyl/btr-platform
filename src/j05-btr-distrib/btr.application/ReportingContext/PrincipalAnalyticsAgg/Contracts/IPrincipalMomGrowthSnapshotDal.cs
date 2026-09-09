using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalMomGrowthSnapshotDal
    {
        PrincipalMomGrowthResult GetCurrent();

        void ReplaceCurrent(PrincipalMomGrowthResult result, string refreshLogId);
    }
}
