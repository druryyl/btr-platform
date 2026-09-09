using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalInventorySnapshotDal
    {
        PrincipalInventoryAggregateResult GetCurrent();

        void ReplaceCurrent(PrincipalInventoryAggregateResult result, string refreshLogId);
    }
}
