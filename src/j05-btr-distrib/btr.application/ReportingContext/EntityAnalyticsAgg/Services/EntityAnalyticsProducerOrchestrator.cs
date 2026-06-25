using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// <summary>
    /// Resolves and invokes entity analytics producers registered through DI.
    /// M32.2: after domain snapshot save, inside the same transaction:
    /// _entityAnalyticsOrchestrator.ProduceForDomain("Customer", context);
    /// </summary>
    public class EntityAnalyticsProducerOrchestrator
    {
        private readonly IReadOnlyList<IEntityAnalyticsProducer> _producers;
        private readonly IEntityTypeRegistry _entityTypes;

        public EntityAnalyticsProducerOrchestrator(
            IEnumerable<IEntityAnalyticsProducer> producers,
            IEntityTypeRegistry entityTypes)
        {
            _producers = producers?.ToList() ?? new List<IEntityAnalyticsProducer>();
            _entityTypes = entityTypes;
        }

        public void ProduceForDomain(string workerDomain, EntityAnalyticsProduceContext context)
        {
            if (string.IsNullOrWhiteSpace(workerDomain) || context is null)
                return;

            var matched = _producers
                .Where(p => string.Equals(p.WorkerDomain, workerDomain, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matched.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No {nameof(IEntityAnalyticsProducer)} is registered for worker domain '{workerDomain}'.");
            }

            foreach (var producer in matched)
            {
                producer.Produce(context);
            }
        }

        public void ProduceForEntityType(string entityTypeCode, EntityAnalyticsProduceContext context)
        {
            if (string.IsNullOrWhiteSpace(entityTypeCode) || context is null)
                return;

            var normalizedType = _entityTypes.NormalizeEntityTypeCode(entityTypeCode);
            if (normalizedType is null)
                return;

            foreach (var producer in _producers.Where(p =>
                string.Equals(p.EntityType, normalizedType, System.StringComparison.OrdinalIgnoreCase)))
            {
                producer.Produce(context);
            }
        }
    }
}
