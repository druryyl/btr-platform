using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// <summary>
    /// Generic L4 relationship engine. Persists producer snapshots and composes profile sections from L4 only.
    /// </summary>
    public class EntityRelationshipEngine : IEntityRelationshipEngine
    {
        private readonly IEntityAnalyticsRepository _repository;
        private readonly IRelationshipDefinitionRegistry _relationships;
        private readonly IEntityTypeRegistry _entityTypes;

        public EntityRelationshipEngine(
            IEntityAnalyticsRepository repository,
            IRelationshipDefinitionRegistry relationships,
            IEntityTypeRegistry entityTypes)
        {
            _repository = repository;
            _relationships = relationships;
            _entityTypes = entityTypes;
        }

        public void PersistRollups(
            string entityType,
            int periodYear,
            int periodMonth,
            IReadOnlyList<EntityRelationshipSnapshot> snapshots,
            string refreshLogId,
            DateTime generatedAt)
        {
            if (string.IsNullOrWhiteSpace(entityType) || snapshots == null)
                return;

            if (_repository.IsMonthClosed(entityType, periodYear, periodMonth))
                return;

            var definitions = _relationships.ResolvePackForEntityType(entityType);
            if (definitions.Count == 0)
                return;

            var definitionByCode = definitions.ToDictionary(
                d => d.RelationshipCode,
                StringComparer.OrdinalIgnoreCase);

            var rows = new List<EntityAnalyticsRelationshipRow>();

            foreach (var group in snapshots
                .Where(s => s != null && !string.IsNullOrWhiteSpace(s.SourceEntityId)
                    && !string.IsNullOrWhiteSpace(s.RelationshipCode))
                .GroupBy(s => $"{s.SourceEntityId}|{s.RelationshipCode}", StringComparer.OrdinalIgnoreCase))
            {
                var first = group.First();
                if (!definitionByCode.TryGetValue(first.RelationshipCode, out var definition))
                    continue;

                var ordered = group
                    .OrderByDescending(s => s.MetricValue ?? 0m)
                    .ThenBy(s => s.TargetDisplayName, StringComparer.OrdinalIgnoreCase)
                    .Take(definition.TopN > 0 ? definition.TopN : 10)
                    .ToList();

                for (var i = 0; i < ordered.Count; i++)
                {
                    var snapshot = ordered[i];
                    rows.Add(new EntityAnalyticsRelationshipRow
                    {
                        SourceEntityType = entityType,
                        SourceEntityId = snapshot.SourceEntityId,
                        SourceEntityCode = snapshot.SourceEntityCode ?? snapshot.SourceEntityId,
                        RelationshipCode = snapshot.RelationshipCode,
                        TargetEntityType = snapshot.TargetEntityType,
                        TargetEntityId = snapshot.TargetEntityId,
                        TargetEntityCode = snapshot.TargetEntityCode ?? snapshot.TargetEntityId,
                        TargetDisplayName = snapshot.TargetDisplayName
                            ?? snapshot.TargetEntityCode
                            ?? snapshot.TargetEntityId,
                        MetricValue = snapshot.MetricValue,
                        PeriodYear = periodYear,
                        PeriodMonth = periodMonth,
                        Rank = i + 1,
                        GeneratedAt = generatedAt
                    });
                }
            }

            _repository.ReplaceRelationshipRollups(entityType, periodYear, periodMonth, rows, refreshLogId);
        }

        public ProfileRelatedEntitiesSectionDto BuildRelatedEntitiesSection(string entityType, string entityId)
        {
            var definitions = _relationships.ResolvePackForEntityType(entityType);
            var rows = _repository.GetRelationshipRollups(entityType, entityId);
            if (rows.Count == 0)
            {
                return new ProfileRelatedEntitiesSectionDto
                {
                    IsAvailable = false,
                    UnavailableReason = EntityAnalyticsUnavailableReasons.NoSnapshotData,
                    Blocks = new List<ProfileRelationshipBlockDto>()
                };
            }

            var rowsByCode = rows
                .GroupBy(r => r.RelationshipCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderBy(r => r.Rank).ToList(), StringComparer.OrdinalIgnoreCase);

            var blocks = new List<ProfileRelationshipBlockDto>();
            foreach (var definition in definitions)
            {
                if (!rowsByCode.TryGetValue(definition.RelationshipCode, out var blockRows) || blockRows.Count == 0)
                    continue;

                blocks.Add(new ProfileRelationshipBlockDto
                {
                    RelationshipCode = definition.RelationshipCode,
                    RelationshipLabel = definition.DisplayName,
                    DisplayName = definition.DisplayName,
                    TargetEntityType = definition.TargetEntityType,
                    Rows = blockRows.Select(r => MapRow(r, definition.TargetEntityType)).ToList()
                });
            }

            return new ProfileRelatedEntitiesSectionDto
            {
                IsAvailable = blocks.Count > 0,
                UnavailableReason = blocks.Count > 0 ? null : EntityAnalyticsUnavailableReasons.NoSnapshotData,
                Blocks = blocks
            };
        }

        private ProfileRelatedEntityRowDto MapRow(EntityAnalyticsRelationshipRow row, string targetEntityType)
        {
            return new ProfileRelatedEntityRowDto
            {
                Rank = row.Rank,
                EntityId = row.TargetEntityId,
                EntityCode = row.TargetEntityCode,
                DisplayName = row.TargetDisplayName,
                TargetEntityType = targetEntityType,
                TargetEntityCode = row.TargetEntityCode,
                TargetEntityName = row.TargetDisplayName,
                MetricValue = row.MetricValue,
                ProfileRoute = BuildProfileRoute(targetEntityType, row.TargetEntityCode ?? row.TargetEntityId)
            };
        }

        private string BuildProfileRoute(string targetEntityType, string targetCode)
        {
            if (string.IsNullOrWhiteSpace(targetEntityType) || string.IsNullOrWhiteSpace(targetCode))
                return null;

            if (!_entityTypes.TryGet(targetEntityType, out var registration)
                || string.IsNullOrWhiteSpace(registration.ProfileRouteTemplate))
            {
                return null;
            }

            return registration.ProfileRouteTemplate.Replace("{code}", targetCode);
        }
    }
}
