using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalReturnHistoryDal
    {
        PrincipalReturnHistoryResult GetHistory();

        void ReplaceHistory(PrincipalReturnHistoryResult result, string refreshLogId);
    }
}
