using System;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public static class EntityComparisonCalculator
    {
        public static decimal? ComputeDelta(decimal? current, decimal? prior)
        {
            if (!current.HasValue || !prior.HasValue)
                return null;

            return current.Value - prior.Value;
        }

        public static decimal? ComputeGrowthPercent(decimal? current, decimal? prior)
        {
            if (!current.HasValue || !prior.HasValue || prior.Value == 0m)
                return null;

            return (current.Value - prior.Value) / Math.Abs(prior.Value) * 100m;
        }

        public static (int Year, int Month) ShiftMonth(int year, int month, int monthOffset)
        {
            var date = new DateTime(year, month, 1).AddMonths(monthOffset);
            return (date.Year, date.Month);
        }
    }
}
