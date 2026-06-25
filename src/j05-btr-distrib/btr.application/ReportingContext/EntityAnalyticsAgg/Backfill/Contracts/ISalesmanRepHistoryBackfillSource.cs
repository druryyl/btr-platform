using System;
using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts
{
    public interface ISalesmanRepHistoryBackfillSource
    {
        bool HasCoverage(int periodYear, int periodMonth);

        IReadOnlyList<EntityAnalyticsMonthlyRow> MapToL1Rows(
            int periodYear,
            int periodMonth,
            string refreshLogId,
            DateTime generatedAt);
    }
}
