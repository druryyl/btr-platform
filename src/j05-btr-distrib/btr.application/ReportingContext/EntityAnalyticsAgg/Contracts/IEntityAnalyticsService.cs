using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityAnalyticsService
    {
        EntityPerformanceProfileResponse GetProfile(string entityType, string entityId);

        IReadOnlyList<EntityAnalyticsTypeDto> GetEnabledTypes();
    }
}
