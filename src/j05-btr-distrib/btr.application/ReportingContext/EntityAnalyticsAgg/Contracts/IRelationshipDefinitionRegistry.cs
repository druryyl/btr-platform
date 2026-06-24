using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IRelationshipDefinitionRegistry
    {
        void Register(string entityType, RelationshipDefinition definition);

        void RegisterPack(string packId, IReadOnlyList<string> relationshipCodes);

        bool TryGet(string entityType, string relationshipCode, out RelationshipDefinition definition);

        IReadOnlyList<RelationshipDefinition> ResolvePackForEntityType(string entityType);
    }
}
