using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts
{
    public interface IEntityAnalyticsReplayAggregateService
    {
        EntityAnalyticsReplayAggregateResult Aggregate(
            EntityAnalyticsReplayContext replayContext,
            object bundle,
            DateTime generatedAt);
    }
}
