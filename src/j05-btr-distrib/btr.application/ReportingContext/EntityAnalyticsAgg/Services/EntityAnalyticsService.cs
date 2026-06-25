using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Options;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;
using Microsoft.Extensions.Options;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public class EntityAnalyticsService : IEntityAnalyticsService
    {
        private readonly IEntityProfileBuilder _profileBuilder;
        private readonly IEntityTypeRegistry _entityTypes;
        private readonly IEntityAnalyticsRepository _repository;
        private readonly EntityAnalyticsOptions _options;

        public EntityAnalyticsService(
            IEntityProfileBuilder profileBuilder,
            IEntityTypeRegistry entityTypes,
            IEntityAnalyticsRepository repository,
            IOptions<EntityAnalyticsOptions> options)
        {
            _profileBuilder = profileBuilder;
            _entityTypes = entityTypes;
            _repository = repository;
            _options = options?.Value ?? new EntityAnalyticsOptions();
        }

        public EntityPerformanceProfileResponse GetProfile(string entityType, string entityId)
        {
            var normalizedType = _entityTypes.NormalizeEntityTypeCode(entityType);
            if (normalizedType is null)
                throw new System.ArgumentException($"Unknown entity type: {entityType}", nameof(entityType));

            if (!IsEntityTypeEnabled(normalizedType))
                return EntityPerformanceProfileComposer.BuildDisabledProfile(normalizedType, entityId?.Trim());

            return _profileBuilder.Build(entityType, entityId);
        }

        public IReadOnlyList<EntityAnalyticsTypeDto> GetEnabledTypes()
        {
            var enabledTypes = _options.EnabledEntityTypes ?? new string[0];
            var enabled = new HashSet<string>(enabledTypes, System.StringComparer.OrdinalIgnoreCase);
            var restrictToConfiguredList = enabledTypes.Length > 0;

            return _entityTypes.GetAll()
                .Select(registration => new EntityAnalyticsTypeDto
                {
                    EntityType = registration.EntityTypeCode,
                    DisplayName = registration.DisplayName,
                    IsEnabled = !restrictToConfiguredList || enabled.Contains(registration.EntityTypeCode),
                    IsAvailable = (!restrictToConfiguredList || enabled.Contains(registration.EntityTypeCode))
                        && _repository.HasAnyCurrentMetrics(registration.EntityTypeCode),
                    ProfileRouteTemplate = registration.ProfileRouteTemplate
                })
                .ToList();
        }

        private bool IsEntityTypeEnabled(string normalizedEntityType)
        {
            var enabled = _options.EnabledEntityTypes ?? new string[0];
            if (enabled.Length == 0)
                return true;

            return enabled.Any(type =>
                string.Equals(type, normalizedEntityType, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
