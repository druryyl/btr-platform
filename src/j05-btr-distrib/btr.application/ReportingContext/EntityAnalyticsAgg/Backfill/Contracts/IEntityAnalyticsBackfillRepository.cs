using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts
{
    /// <summary>
    /// Period-scoped replace writes for historical backfill (ADR-004).
    /// Invoked only when <see cref="EntityAnalyticsProduceContext.Replay"/> is set.
    /// </summary>
    public interface IEntityAnalyticsBackfillRepository
    {
        void ReplaceMonthlyHistoryForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsMonthlyRow> rows,
            string refreshLogId);

        void ReplaceRankingForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsRankingRow> rows,
            string refreshLogId);

        void ReplaceAttentionForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsAttentionEventRow> rows,
            string refreshLogId);

        void ReplaceRelationshipForPeriod(
            string sourceEntityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsRelationshipRow> rows,
            string refreshLogId);

        void ReplaceRadarForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsRadarScoreRow> rows,
            string refreshLogId);
    }
}
