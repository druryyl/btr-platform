using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public class EntityPerformanceProfileComposer : IEntityProfileBuilder
    {
        private readonly IEntityAnalyticsRepository _repository;
        private readonly IKpiRegistry _kpiRegistry;
        private readonly IEntityTypeRegistry _entityTypes;
        private readonly IEnumerable<IEntityProfileEvidenceResolver> _evidenceResolvers;
        private readonly EntityKpiEnvelopeFormatter _envelopeFormatter;

        private readonly IEntityTrendEngine _trendEngine;
        private readonly IEntityRankingEngine _rankingEngine;
        private readonly IEntityAttentionEngine _attentionEngine;
        private readonly IEntityRelationshipEngine _relationshipEngine;
        private readonly IEntityComparisonEngine _comparisonEngine;
        private readonly IEntityRadarEngine _radarEngine;

        public EntityPerformanceProfileComposer(
            IEntityAnalyticsRepository repository,
            IKpiRegistry kpiRegistry,
            IEntityTypeRegistry entityTypes,
            IEnumerable<IEntityProfileEvidenceResolver> evidenceResolvers,
            EntityKpiEnvelopeFormatter envelopeFormatter,
            IEntityTrendEngine trendEngine,
            IEntityRankingEngine rankingEngine,
            IEntityAttentionEngine attentionEngine,
            IEntityRelationshipEngine relationshipEngine,
            IEntityComparisonEngine comparisonEngine,
            IEntityRadarEngine radarEngine)
        {
            _repository = repository;
            _kpiRegistry = kpiRegistry;
            _entityTypes = entityTypes;
            _evidenceResolvers = evidenceResolvers ?? Enumerable.Empty<IEntityProfileEvidenceResolver>();
            _envelopeFormatter = envelopeFormatter ?? new EntityKpiEnvelopeFormatter();
            _trendEngine = trendEngine;
            _rankingEngine = rankingEngine;
            _attentionEngine = attentionEngine;
            _relationshipEngine = relationshipEngine;
            _comparisonEngine = comparisonEngine;
            _radarEngine = radarEngine;
        }

        public EntityPerformanceProfileResponse Build(string entityType, string entityId)
        {
            var normalizedType = _entityTypes.NormalizeEntityTypeCode(entityType);
            if (normalizedType is null)
                throw new ArgumentException($"Unknown entity type: {entityType}", nameof(entityType));
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentException("EntityId is required.", nameof(entityId));

            var identity = _repository.TryResolveIdentity(normalizedType, entityId);
            var resolvedEntityId = identity?.EntityCode ?? entityId.Trim();
            var metrics = _repository.GetCurrentMetrics(normalizedType, resolvedEntityId);
            var generatedAt = _repository.GetLatestGeneratedAt(normalizedType, resolvedEntityId);
            var isAvailable = metrics.Count > 0;

            return new EntityPerformanceProfileResponse
            {
                IsAvailable = isAvailable,
                EntityType = normalizedType,
                EntityId = resolvedEntityId,
                GeneratedAt = generatedAt,
                SnapshotVersion = EntityAnalyticsConstants.CurrentSnapshotVersion,
                ContractVersion = EntityAnalyticsConstants.ProfileContractVersion,
                Overview = BuildOverview(identity, normalizedType, resolvedEntityId, generatedAt),
                KpiSummary = BuildKpiSummary(metrics, isAvailable),
                Comparison = _comparisonEngine.BuildCrossPeriodSection(normalizedType, resolvedEntityId),
                Trend = _trendEngine.BuildTrendSection(normalizedType, resolvedEntityId),
                Radar = _radarEngine.BuildRadarSection(normalizedType, resolvedEntityId),
                Ranking = _rankingEngine.BuildRankingSection(normalizedType, resolvedEntityId),
                Attention = _attentionEngine.BuildAttentionSection(normalizedType, resolvedEntityId),
                RelatedEntities = _relationshipEngine.BuildRelatedEntitiesSection(normalizedType, resolvedEntityId),
                Evidence = BuildEvidence(normalizedType, resolvedEntityId, identity, isAvailable)
            };
        }

        public static EntityPerformanceProfileResponse BuildDisabledProfile(string entityType, string entityId)
        {
            var unavailable = EntityAnalyticsUnavailableReasons.EntityTypeDisabled;
            return new EntityPerformanceProfileResponse
            {
                IsAvailable = false,
                EntityType = entityType,
                EntityId = entityId,
                SnapshotVersion = EntityAnalyticsConstants.CurrentSnapshotVersion,
                ContractVersion = EntityAnalyticsConstants.ProfileContractVersion,
                Overview = CreateUnavailableSection<ProfileOverviewSectionDto>(unavailable),
                KpiSummary = CreateUnavailableSection<ProfileKpiSummarySectionDto>(unavailable),
                Comparison = CreateUnavailableSection<ProfileComparisonSectionDto>(unavailable),
                Trend = CreateUnavailableSection<ProfileTrendSectionDto>(unavailable),
                Radar = CreateUnavailableSection<ProfileRadarSectionDto>(unavailable),
                Ranking = CreateUnavailableSection<ProfileRankingSectionDto>(unavailable),
                Attention = CreateUnavailableSection<ProfileAttentionSectionDto>(unavailable),
                RelatedEntities = CreateUnavailableSection<ProfileRelatedEntitiesSectionDto>(unavailable),
                Evidence = CreateUnavailableSection<ProfileEvidenceSectionDto>(unavailable)
            };
        }

        private ProfileEvidenceSectionDto BuildEvidence(
            string entityType,
            string entityId,
            EntityIdentity identity,
            bool isAvailable)
        {
            if (!isAvailable)
            {
                return new ProfileEvidenceSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoSnapshotData,
                    Links = new List<ProfileEvidenceLinkDto>()
                };
            }

            var resolver = _evidenceResolvers.FirstOrDefault(r =>
                string.Equals(r.EntityType, entityType, StringComparison.OrdinalIgnoreCase));

            if (resolver == null)
            {
                return new ProfileEvidenceSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NotImplemented,
                    Links = new List<ProfileEvidenceLinkDto>()
                };
            }

            return resolver.BuildEvidence(entityId, identity);
        }

        private static ProfileOverviewSectionDto BuildOverview(
            EntityIdentity identity,
            string entityType,
            string entityId,
            DateTime? generatedAt)
        {
            identity = identity ?? new EntityIdentity
            {
                EntityType = entityType,
                EntityId = entityId,
                EntityCode = entityId,
                DisplayName = entityId,
                IsActive = true
            };

            return new ProfileOverviewSectionDto
            {
                IsAvailable = true,
                EntityType = identity.EntityType,
                EntityId = identity.EntityCode,
                EntityCode = identity.EntityCode,
                DisplayName = identity.DisplayName,
                IsActive = identity.IsActive,
                GeneratedAt = generatedAt,
                Dimensions = identity.Dimensions?.ToDictionary(k => k.Key, v => v.Value)
                    ?? new Dictionary<string, string>()
            };
        }

        private ProfileKpiSummarySectionDto BuildKpiSummary(
            IReadOnlyList<EntityAnalyticsCurrentRow> metrics,
            bool isAvailable)
        {
            if (!isAvailable)
            {
                return new ProfileKpiSummarySectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoSnapshotData,
                    Categories = new List<ProfileKpiCategoryGroupDto>()
                };
            }

            var grouped = new Dictionary<EntityKpiCategory, List<KpiEnvelopeDto>>();

            foreach (var row in metrics)
            {
                if (EntityAnalyticsMetaKpiIds.IsMetaOrDimension(row.KpiId))
                    continue;

                if (!_kpiRegistry.TryGetMetadata(row.KpiId, out var metadata))
                    continue;

                if (!grouped.TryGetValue(metadata.Category, out var list))
                {
                    list = new List<KpiEnvelopeDto>();
                    grouped[metadata.Category] = list;
                }

                list.Add(_envelopeFormatter.Map(row, metadata));
            }

            return new ProfileKpiSummarySectionDto
            {
                IsAvailable = grouped.Count > 0,
                UnavailableReason = grouped.Count > 0
                    ? null
                    : EntityAnalyticsUnavailableReasons.NoRegisteredKpis,
                Categories = grouped
                    .OrderBy(g => g.Key)
                    .Select(g => new ProfileKpiCategoryGroupDto
                    {
                        Category = g.Key.ToString(),
                        Kpis = g.Value
                    })
                    .ToList()
            };
        }

        private static T CreatePlaceholderSection<T>()
            where T : ProfileSectionDtoBase, new()
        {
            return new T
            {
                IsAvailable = false,
                UnavailableReason = EntityAnalyticsUnavailableReasons.NotImplemented
            };
        }

        private static T CreateUnavailableSection<T>(string reason)
            where T : ProfileSectionDtoBase, new()
        {
            return new T
            {
                IsAvailable = false,
                UnavailableReason = reason
            };
        }
    }
}
