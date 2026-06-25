using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityRadarEngine
    {
        void ComputeAndPersistScores(
            string entityType,
            int periodYear,
            int periodMonth,
            string refreshLogId,
            DateTime generatedAt,
            EntityAnalyticsReplayContext replay = null);

        ProfileRadarSectionDto BuildRadarSection(string entityType, string entityId);
    }
}
