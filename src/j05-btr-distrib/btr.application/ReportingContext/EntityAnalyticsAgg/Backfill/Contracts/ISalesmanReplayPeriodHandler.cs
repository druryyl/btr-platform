using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts
{
    public interface ISalesmanReplayPeriodHandler
    {
        bool CanUseFastPath(int periodYear, int periodMonth);

        EntityAnalyticsReplayAggregateResult BuildFastPathPlan(
            int periodYear,
            int periodMonth,
            string refreshLogId,
            DateTime generatedAt);

        void PersistFastPathL1(
            int periodYear,
            int periodMonth,
            string refreshLogId,
            DateTime generatedAt);

        SalesmanEntityAnalyticsProduceInput CreateLayersOnlyInput();
    }
}
