using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts
{
    /// <summary>
    /// Ensures prior calendar months are frozen and applies L1 retention policy during producer refresh.
    /// </summary>
    public interface IEntityAnalyticsMonthCloseService
    {
        void EnsurePriorMonthClosed(string entityType, EntityAnalyticsProduceContext context);
    }
}
