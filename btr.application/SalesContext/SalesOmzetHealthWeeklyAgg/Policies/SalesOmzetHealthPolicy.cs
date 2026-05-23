using System;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Contracts;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Policies
{
    public class SalesOmzetHealthPolicy : ISalesOmzetHealthPolicy
    {
        public const int GoodScoreThreshold = 90;
        public const int WarningScoreThreshold = 70;
        public const int FreshWithinDays = 7;
        public const int StaleAfterDays = 30;
        public const int PoorMissingThreshold = 50;

        public int ComputeScore(SalesOmzetHealthMetrics metrics, DateTime periodStart, DateTime periodEnd)
        {
            if (metrics is null)
                throw new ArgumentNullException(nameof(metrics));

            var weekEnd = periodEnd.Date;
            var weekStart = periodStart.Date;
            var totalMissing = metrics.MissingOrders + metrics.MissingDirectFakturs + metrics.UnlinkedFakturs;

            if (totalMissing >= PoorMissingThreshold)
                return 0;

            if (!IsReconciledFresh(metrics.LastReconciledMax, weekEnd, weekStart))
                return 40;

            var score = 100;

            if (totalMissing > 0)
                score -= Math.Min(30, totalMissing * 5);

            if (metrics.StaleFakturEstimate > 0)
                score -= Math.Min(25, metrics.StaleFakturEstimate * 3);

            if (!IsReconciledGood(metrics.LastReconciledMax, weekEnd))
                score -= 15;

            return Math.Max(0, Math.Min(100, score));
        }

        public SalesOmzetHealthLevelEnum ResolveLevel(int healthScore)
        {
            if (healthScore >= GoodScoreThreshold)
                return SalesOmzetHealthLevelEnum.Good;
            if (healthScore >= WarningScoreThreshold)
                return SalesOmzetHealthLevelEnum.Warning;
            return SalesOmzetHealthLevelEnum.Poor;
        }

        private static bool IsReconciledGood(DateTime? lastReconciledMax, DateTime weekEnd)
        {
            if (!lastReconciledMax.HasValue || SalesOmzetDates.IsSentinel(lastReconciledMax.Value))
                return false;

            return lastReconciledMax.Value.Date >= weekEnd.AddDays(-FreshWithinDays);
        }

        private static bool IsReconciledFresh(DateTime? lastReconciledMax, DateTime weekEnd, DateTime weekStart)
        {
            if (!lastReconciledMax.HasValue || SalesOmzetDates.IsSentinel(lastReconciledMax.Value))
                return false;

            if (lastReconciledMax.Value.Date < weekStart)
                return false;

            return lastReconciledMax.Value.Date >= weekEnd.AddDays(-StaleAfterDays);
        }
    }
}
