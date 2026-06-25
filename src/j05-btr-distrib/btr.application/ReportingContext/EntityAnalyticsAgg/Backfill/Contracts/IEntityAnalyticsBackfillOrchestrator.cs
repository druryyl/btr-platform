using System.Threading;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts
{
    public interface IEntityAnalyticsBackfillOrchestrator
    {
        EntityAnalyticsBackfillResult Run(EntityAnalyticsBackfillRequest request, CancellationToken cancellationToken);
    }
}
