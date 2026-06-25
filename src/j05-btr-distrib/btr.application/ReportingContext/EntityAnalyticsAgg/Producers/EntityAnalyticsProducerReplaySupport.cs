using System;
using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    internal static class EntityAnalyticsProducerReplaySupport
    {
        public static (int PeriodYear, int PeriodMonth) ResolvePeriod(EntityAnalyticsProduceContext context)
        {
            if (context?.Replay != null)
                return (context.Replay.PeriodYear, context.Replay.PeriodMonth);

            var businessDate = context.BusinessDate == default
                ? context.GeneratedAt.Date
                : context.BusinessDate.Date;

            return (businessDate.Year, businessDate.Month);
        }

        public static void PersistL0(
            IEntityAnalyticsRepository repository,
            EntityAnalyticsProduceContext context,
            string entityType,
            IEnumerable<EntityAnalyticsCurrentRow> rows)
        {
            if (context?.Replay != null)
                return;

            repository.ReplaceCurrentMetrics(
                entityType,
                rows ?? Array.Empty<EntityAnalyticsCurrentRow>(),
                context.RefreshLogId);
        }

        public static void PersistL1(
            IEntityAnalyticsRepository repository,
            IEntityAnalyticsMonthCloseService monthCloseService,
            EntityAnalyticsProduceContext context,
            string entityType,
            IList<EntityAnalyticsMonthlyRow> monthlyRows)
        {
            if (context?.Replay != null)
            {
                if (monthlyRows != null)
                {
                    foreach (var row in monthlyRows)
                        row.IsClosed = true;
                }

                repository.ReplaceMonthlyHistoryForPeriod(
                    entityType,
                    context.Replay.PeriodYear,
                    context.Replay.PeriodMonth,
                    monthlyRows ?? Array.Empty<EntityAnalyticsMonthlyRow>(),
                    context.RefreshLogId);
                return;
            }

            monthCloseService.EnsurePriorMonthClosed(entityType, context);
            repository.SaveMonthlyHistory(entityType, monthlyRows, context.RefreshLogId);
        }
    }
}
