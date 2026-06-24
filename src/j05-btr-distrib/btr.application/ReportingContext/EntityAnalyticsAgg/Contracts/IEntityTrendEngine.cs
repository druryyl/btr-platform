using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityTrendEngine
    {
        ProfileTrendSectionDto BuildTrendSection(string entityType, string entityId);
    }
}
