using System.Collections.Generic;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityTypeRegistry
    {
        void Register(EntityTypeRegistration registration);

        bool TryGet(string entityTypeCode, out EntityTypeRegistration registration);

        bool IsRegistered(string entityTypeCode);

        string NormalizeEntityTypeCode(string entityTypeCode);

        IReadOnlyList<EntityTypeRegistration> GetAll();
    }
}
