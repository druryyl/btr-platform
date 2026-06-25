using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    /// <summary>
    /// Unified read/write access to Entity Analytics snapshot layers L0–L5.
    /// L0–L5 implemented through M32.8.
    /// </summary>
    public interface IEntityAnalyticsRepository
        : IEntityAnalyticsCurrentRepository,
            IEntityAnalyticsMonthlyRepository,
            IEntityAnalyticsRankingRepository,
            IEntityAnalyticsAttentionRepository,
            IEntityAnalyticsRelationshipRepository,
            IEntityAnalyticsRadarRepository,
            IEntityAnalyticsBackfillRepository
    {
        EntityIdentity TryResolveIdentity(string entityType, string entityId);
    }
}
