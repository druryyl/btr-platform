using System;
using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityRelationshipEngine
    {
        void PersistRollups(
            string entityType,
            int periodYear,
            int periodMonth,
            IReadOnlyList<EntityRelationshipSnapshot> snapshots,
            string refreshLogId,
            DateTime generatedAt,
            EntityAnalyticsReplayContext replay = null);

        ProfileRelatedEntitiesSectionDto BuildRelatedEntitiesSection(string entityType, string entityId);
    }
}
