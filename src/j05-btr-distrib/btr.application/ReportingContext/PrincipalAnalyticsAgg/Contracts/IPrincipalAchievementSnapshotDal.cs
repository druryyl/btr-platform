using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalAchievementSnapshotDal
    {
        PrincipalAchievementResult GetCurrent();

        void ReplaceCurrent(PrincipalAchievementResult result, string refreshLogId);
    }
}
