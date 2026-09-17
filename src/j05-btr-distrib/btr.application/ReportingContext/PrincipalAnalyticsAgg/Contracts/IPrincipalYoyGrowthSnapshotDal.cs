using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalYoyGrowthSnapshotDal
    {
        PrincipalYoyGrowthResult GetCurrent();

        void ReplaceCurrent(PrincipalYoyGrowthResult result, string refreshLogId);
    }
}
