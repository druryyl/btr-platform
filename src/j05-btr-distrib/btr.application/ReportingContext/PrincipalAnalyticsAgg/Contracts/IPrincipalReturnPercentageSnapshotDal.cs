using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalReturnPercentageSnapshotDal
    {
        PrincipalReturnPercentageResult GetCurrent();

        void ReplaceCurrent(PrincipalReturnPercentageResult result, string refreshLogId);
    }
}
