using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityProfileEvidenceResolver
    {
        string EntityType { get; }

        ProfileEvidenceSectionDto BuildEvidence(string entityId, EntityIdentity identity);
    }
}
