using System;
using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityAnalyticsProducer
    {
        string EntityType { get; }

        string WorkerDomain { get; }

        void Produce(EntityAnalyticsProduceContext context);
    }

    public class EntityAnalyticsProduceContext
    {
        public string RefreshLogId { get; set; }

        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        /// <summary>
        /// Domain-specific in-memory aggregator output. Cast in entity producer implementation only.
        /// </summary>
        public object DomainInput { get; set; }

        /// <summary>
        /// Set during historical backfill replay; null for live domain workers.
        /// </summary>
        public EntityAnalyticsReplayContext Replay { get; set; }

        /// <summary>
        /// Customer code to internal identity lookup for cross-entity relationship targets.
        /// </summary>
        public IReadOnlyDictionary<string, EntityAnalyticsCustomerIdentity> CustomerIdentityLookup { get; set; }
    }
}
