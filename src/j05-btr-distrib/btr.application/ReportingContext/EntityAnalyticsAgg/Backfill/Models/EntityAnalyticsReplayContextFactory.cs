using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public static class EntityAnalyticsReplayContextFactory
    {
        public static EntityAnalyticsReplayContext Create(
            YearMonthPeriod period,
            string entityType,
            string backfillJobId,
            EntityAnalyticsBackfillRequest request)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("Entity type is required.", nameof(entityType));

            var resumeMode = EntityAnalyticsReplayResumeMode.SkipCompleted;
            if (request != null)
            {
                if (request.Force)
                    resumeMode = EntityAnalyticsReplayResumeMode.ForceRerun;
                else if (!request.Resume)
                    resumeMode = EntityAnalyticsReplayResumeMode.FromCheckpoint;
            }

            return new EntityAnalyticsReplayContext
            {
                PeriodYear = period.Year,
                PeriodMonth = period.Month,
                PeriodStart = period.PeriodStart,
                PeriodEnd = period.PeriodEnd,
                EntityTypeCode = entityType,
                IsDryRun = request?.DryRun ?? false,
                ResumeMode = resumeMode,
                SkipLiveMutexCheck = request?.SkipLiveMutexCheck ?? false,
                BackfillJobId = backfillJobId ?? string.Empty,
                BatchSize = request?.BatchSize ?? 0
            };
        }

        public static EntityAnalyticsProduceContext CreateProduceContext(
            EntityAnalyticsReplayContext replay,
            object domainInput,
            string refreshLogId,
            DateTime generatedAt)
        {
            if (replay is null)
                throw new ArgumentNullException(nameof(replay));

            return new EntityAnalyticsProduceContext
            {
                RefreshLogId = refreshLogId ?? string.Empty,
                GeneratedAt = generatedAt,
                BusinessDate = replay.PeriodEnd,
                DomainInput = domainInput,
                Replay = replay
            };
        }
    }
}
