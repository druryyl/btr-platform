using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalSalesOutHistoryDal
    {
        PrincipalSalesOutHistoryResult GetHistory();

        void ReplaceHistory(PrincipalSalesOutHistoryResult result, string refreshLogId);
    }
}
