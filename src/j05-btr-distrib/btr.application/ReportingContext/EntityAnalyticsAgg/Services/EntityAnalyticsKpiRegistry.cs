using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public class EntityAnalyticsKpiRegistry : IKpiRegistry
    {
        private readonly IEntityTypeRegistry _entityTypes;
        private readonly Dictionary<string, EntityKpiMetadata> _metadataByKpiId
            = new Dictionary<string, EntityKpiMetadata>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, IReadOnlyList<string>> _packsById
            = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

        public EntityAnalyticsKpiRegistry(IEntityTypeRegistry entityTypes)
        {
            _entityTypes = entityTypes ?? throw new ArgumentNullException(nameof(entityTypes));
        }

        public bool TryGetMetadata(string kpiId, out EntityKpiMetadata metadata)
        {
            metadata = null;
            if (string.IsNullOrWhiteSpace(kpiId))
                return false;

            return _metadataByKpiId.TryGetValue(kpiId, out metadata);
        }

        public IReadOnlyList<string> GetPackKpiIds(string packId)
        {
            if (string.IsNullOrWhiteSpace(packId))
                return Array.Empty<string>();

            return _packsById.TryGetValue(packId, out var ids)
                ? ids
                : Array.Empty<string>();
        }

        public IReadOnlyList<string> GetPackKpiIdsForEntityType(string entityTypeCode)
        {
            if (!_entityTypes.TryGet(entityTypeCode, out var registration)
                || string.IsNullOrWhiteSpace(registration.KpiPackId))
            {
                return Array.Empty<string>();
            }

            return GetPackKpiIds(registration.KpiPackId);
        }

        public IReadOnlyList<EntityKpiMetadata> ResolvePackMetadata(string packId)
        {
            return GetPackKpiIds(packId)
                .Select(id =>
                {
                    TryGetMetadata(id, out var metadata);
                    return metadata;
                })
                .Where(metadata => metadata != null)
                .ToList();
        }

        public IReadOnlyList<string> ValidatePack(string packId)
        {
            return GetPackKpiIds(packId)
                .Where(id => !_metadataByKpiId.ContainsKey(id))
                .ToList();
        }

        public void RegisterMetadata(EntityKpiMetadata metadata)
        {
            if (metadata is null)
                throw new ArgumentNullException(nameof(metadata));
            if (string.IsNullOrWhiteSpace(metadata.KpiId))
                throw new ArgumentException("KpiId is required.", nameof(metadata));

            ValidateCategory(metadata.Category);
            _metadataByKpiId[metadata.KpiId] = metadata;
        }

        public void RegisterPack(string packId, IReadOnlyList<string> kpiIds)
        {
            if (string.IsNullOrWhiteSpace(packId))
                throw new ArgumentException("PackId is required.", nameof(packId));

            _packsById[packId] = kpiIds?.ToList() ?? new List<string>();
        }

        public static void ValidateCategory(EntityKpiCategory category)
        {
            if (!Enum.IsDefined(typeof(EntityKpiCategory), category))
                throw new ArgumentOutOfRangeException(nameof(category), "Invalid KPI category.");
        }
    }
}
