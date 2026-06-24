using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityProfileBuilder
    {
        EntityPerformanceProfileResponse Build(string entityType, string entityId);
    }
}
