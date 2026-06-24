using System;
using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityAttentionEngine
    {
        void DiffAndPersistSignals(
            string entityType,
            int periodYear,
            int periodMonth,
            IReadOnlyDictionary<string, IReadOnlyList<EntityAttentionSignalSnapshot>> signalsByEntity,
            string refreshLogId,
            DateTime generatedAt);

        ProfileAttentionSectionDto BuildAttentionSection(string entityType, string entityId);
    }
}
