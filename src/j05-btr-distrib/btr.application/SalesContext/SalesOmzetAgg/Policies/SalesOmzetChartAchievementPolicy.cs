using System;

namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public static class SalesOmzetChartAchievementPolicy
    {
        /// <summary>
        /// Achievement % = recognized / target × 100. Not capped (over-achievement shown).
        /// </summary>
        public static decimal? ComputePercent(decimal recognizedOmzet, decimal? targetAmount)
        {
            if (!targetAmount.HasValue || targetAmount.Value <= 0)
                return null;

            return Math.Round(recognizedOmzet / targetAmount.Value * 100m, 1);
        }

        public static string FormatPercentDisplay(decimal? percent) =>
            percent.HasValue ? $"{percent.Value:N1}%" : "—";
    }
}
