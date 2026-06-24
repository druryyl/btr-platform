using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityComparisonEngine
    {
        ProfileComparisonSectionDto BuildCrossPeriodSection(string entityType, string entityId);

        EntityCompareResponse BuildMultiEntityComparison(ComparisonContext context);
    }
}
