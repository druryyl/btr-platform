using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IKpiRegistry
    {
        bool TryGetMetadata(string kpiId, out EntityKpiMetadata metadata);

        IReadOnlyList<string> GetPackKpiIds(string packId);

        IReadOnlyList<string> GetPackKpiIdsForEntityType(string entityTypeCode);

        IReadOnlyList<EntityKpiMetadata> ResolvePackMetadata(string packId);

        IReadOnlyList<string> ValidatePack(string packId);

        void RegisterMetadata(EntityKpiMetadata metadata);

        void RegisterPack(string packId, IReadOnlyList<string> kpiIds);
    }
}
