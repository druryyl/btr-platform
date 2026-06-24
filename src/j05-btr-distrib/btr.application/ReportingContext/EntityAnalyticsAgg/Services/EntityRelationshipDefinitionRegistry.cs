using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public class EntityRelationshipDefinitionRegistry : IRelationshipDefinitionRegistry
    {
        private readonly IEntityTypeRegistry _entityTypes;
        private readonly Dictionary<string, Dictionary<string, RelationshipDefinition>> _definitionsByEntityType
            = new Dictionary<string, Dictionary<string, RelationshipDefinition>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IReadOnlyList<string>> _packCodes
            = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

        public EntityRelationshipDefinitionRegistry(IEntityTypeRegistry entityTypes)
        {
            _entityTypes = entityTypes;
        }

        public void Register(string entityType, RelationshipDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(entityType) || definition == null
                || string.IsNullOrWhiteSpace(definition.RelationshipCode))
            {
                return;
            }

            if (!_definitionsByEntityType.TryGetValue(entityType, out var byCode))
            {
                byCode = new Dictionary<string, RelationshipDefinition>(StringComparer.OrdinalIgnoreCase);
                _definitionsByEntityType[entityType] = byCode;
            }

            byCode[definition.RelationshipCode.Trim()] = definition;
        }

        public void RegisterPack(string packId, IReadOnlyList<string> relationshipCodes)
        {
            if (string.IsNullOrWhiteSpace(packId) || relationshipCodes == null || relationshipCodes.Count == 0)
                return;

            _packCodes[packId.Trim()] = relationshipCodes
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToList();
        }

        public bool TryGet(string entityType, string relationshipCode, out RelationshipDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(relationshipCode))
                return false;

            return _definitionsByEntityType.TryGetValue(entityType, out var byCode)
                && byCode.TryGetValue(relationshipCode.Trim(), out definition);
        }

        public IReadOnlyList<RelationshipDefinition> ResolvePackForEntityType(string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType)
                || !_entityTypes.TryGet(entityType, out var registration)
                || string.IsNullOrWhiteSpace(registration.RelationshipPackId)
                || !_packCodes.TryGetValue(registration.RelationshipPackId, out var codes))
            {
                return Array.Empty<RelationshipDefinition>();
            }

            if (!_definitionsByEntityType.TryGetValue(entityType, out var byCode))
                return Array.Empty<RelationshipDefinition>();

            var result = new List<RelationshipDefinition>();
            foreach (var code in codes)
            {
                if (byCode.TryGetValue(code, out var definition))
                    result.Add(definition);
            }

            return result;
        }
    }
}
