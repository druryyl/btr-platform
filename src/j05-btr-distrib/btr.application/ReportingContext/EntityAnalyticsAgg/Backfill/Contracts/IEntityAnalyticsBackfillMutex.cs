namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts
{
    public interface IEntityAnalyticsBackfillMutex
    {
        void Acquire(string entityType, string jobId, bool skipLiveMutexCheck);
        void Release(string entityType, string jobId);
    }
}
