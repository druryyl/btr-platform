using System;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Contracts;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Policies
{
    public interface ISalesOmzetHealthPolicy
    {
        int ComputeScore(SalesOmzetHealthMetrics metrics, DateTime periodStart, DateTime periodEnd);

        SalesOmzetHealthLevelEnum ResolveLevel(int healthScore);
    }
}
