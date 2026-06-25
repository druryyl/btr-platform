using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts
{
    public interface IEntityAnalyticsReplayDataLoader
    {
        string EntityType { get; }

        object Load(EntityAnalyticsReplayContext replayContext);
    }
}
