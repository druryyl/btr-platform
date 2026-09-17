using System.Collections.Generic;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalSalesOutHistoryEvidenceDal
    {
        IReadOnlyList<PrincipalSalesOutHistoryMonthEvidence> ListMonthlySalesOutHistory();
    }
}
