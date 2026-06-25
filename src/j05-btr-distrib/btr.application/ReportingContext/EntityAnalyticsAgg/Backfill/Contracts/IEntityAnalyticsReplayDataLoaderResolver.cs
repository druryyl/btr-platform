namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts
{
    public interface IEntityAnalyticsReplayDataLoaderResolver
    {
        IEntityAnalyticsReplayDataLoader Resolve(string entityType);
    }
}
