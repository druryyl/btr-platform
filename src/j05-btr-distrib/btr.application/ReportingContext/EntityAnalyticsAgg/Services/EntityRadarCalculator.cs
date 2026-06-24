using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardExecutiveAgg.Services;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public static class EntityRadarCalculator
    {
        public sealed class RadarScoreResult
        {
            public string EntityId { get; set; }

            public decimal Score { get; set; }
        }

        public static IReadOnlyList<RadarScoreResult> CalculatePeerPercentiles(
            IEnumerable<(string EntityId, decimal Value)> candidates,
            string direction)
        {
            var rankings = EntityRankingCalculator.Calculate(candidates, direction);
            return rankings
                .Select(r => new RadarScoreResult
                {
                    EntityId = r.EntityId,
                    Score = r.Percentile
                })
                .ToList();
        }

        public static decimal? TryResolveBandMidpointScore(decimal? value, string normalizationRule)
        {
            if (!value.HasValue)
                return null;

            if (!string.Equals(normalizationRule, "AchievementBand", StringComparison.OrdinalIgnoreCase))
                return null;

            var band = ExecutiveSalesAchievementBandResolver.Resolve(value);
            return ResolveBandMidpoint(band);
        }

        public static decimal? ResolveBandMidpoint(string band)
        {
            if (string.Equals(band, ExecutiveSalesAchievementBandResolver.Healthy, StringComparison.OrdinalIgnoreCase))
                return 100m;

            if (string.Equals(band, ExecutiveSalesAchievementBandResolver.Warning, StringComparison.OrdinalIgnoreCase))
                return 50m;

            if (string.Equals(band, ExecutiveSalesAchievementBandResolver.Critical, StringComparison.OrdinalIgnoreCase))
                return 0m;

            return null;
        }
    }
}
