using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    public interface IEntityPeerDistributionEngine
    {
        PeerDistributionResponseDto BuildDistribution(PeerDistributionRequest request);
    }
}
