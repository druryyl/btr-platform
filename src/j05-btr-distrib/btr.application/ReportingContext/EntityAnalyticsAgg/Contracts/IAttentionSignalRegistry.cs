using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    /// <summary>
    /// Entity-type extensible catalog of attention signal metadata (code, category, title).
    /// Producers map domain signals to catalog entries; the Attention Engine stays entity-agnostic.
    /// </summary>
    public interface IAttentionSignalRegistry
    {
        void Register(string entityType, AttentionSignalDefinition definition);

        bool TryResolve(string entityType, string signalCode, out AttentionSignalDefinition definition);
    }
}
