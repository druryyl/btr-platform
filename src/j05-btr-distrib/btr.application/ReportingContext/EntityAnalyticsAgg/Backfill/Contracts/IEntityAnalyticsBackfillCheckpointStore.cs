using System;
using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts
{
    public interface IEntityAnalyticsBackfillCheckpointStore
    {
        string CreateJob(EntityAnalyticsBackfillJobModel job);
        EntityAnalyticsBackfillJobModel GetJob(string jobId);
        void UpdateJob(string jobId, Action<EntityAnalyticsBackfillJobModel> mutate);
        EntityAnalyticsBackfillCheckpointModel GetCheckpoint(string jobId, string entityType, int year, int month);
        IReadOnlyList<EntityAnalyticsBackfillCheckpointModel> GetCheckpointsForJob(string jobId, string entityType);
        void UpsertCheckpoint(EntityAnalyticsBackfillCheckpointModel checkpoint);
        void DeleteCheckpointsForScope(
            string jobId,
            string entityType,
            int fromYear,
            int fromMonth,
            int toYear,
            int toMonth);
    }
}
