using System;

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
    }
}
