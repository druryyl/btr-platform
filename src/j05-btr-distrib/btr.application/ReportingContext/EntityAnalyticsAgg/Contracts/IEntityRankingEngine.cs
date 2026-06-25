using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityRankingEngine
    {
        void ComputeAndPersistRanks(
            string entityType,
            int periodYear,
            int periodMonth,
            string refreshLogId,
            DateTime generatedAt,
            EntityAnalyticsReplayContext replay = null);

        ProfileRankingSectionDto BuildRankingSection(string entityType, string entityId);
    }
}
