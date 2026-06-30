using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityPopulationMapEngine
    {
        PopulationMapResponseDto BuildPopulationMap(PopulationMapRequest request);
    }
}
