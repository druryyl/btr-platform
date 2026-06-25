using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Loaders
{
    public class EntityAnalyticsReplayDataLoaderResolver : IEntityAnalyticsReplayDataLoaderResolver
    {
        private readonly IReadOnlyDictionary<string, IEntityAnalyticsReplayDataLoader> _loaders;

        public EntityAnalyticsReplayDataLoaderResolver(IEnumerable<IEntityAnalyticsReplayDataLoader> loaders)
        {
            _loaders = (loaders ?? Array.Empty<IEntityAnalyticsReplayDataLoader>())
                .ToDictionary(l => l.EntityType, StringComparer.OrdinalIgnoreCase);
        }

        public IEntityAnalyticsReplayDataLoader Resolve(string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("Entity type is required.", nameof(entityType));

            if (!_loaders.TryGetValue(entityType, out var loader))
            {
                throw new InvalidOperationException(
                    $"No replay data loader registered for entity type '{entityType}'.");
            }

            return loader;
        }
    }
}
