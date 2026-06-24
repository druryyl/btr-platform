using System;
using System.Collections.Generic;
using System.Linq;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// Pure ranking calculation: competition ranking (1, 2, 2, 4), rank 1 = best.
    /// Direction from KPI metadata: HigherIsBetter sorts descending; LowerIsBetter sorts ascending.
    public static class EntityRankingCalculator
    {
        public sealed class RankingResult
        {
            public string EntityId { get; set; }

            public int RankPosition { get; set; }

            public int PopulationSize { get; set; }

            public decimal Percentile { get; set; }
        }

        public static IReadOnlyList<RankingResult> Calculate(
            IEnumerable<(string EntityId, decimal Value)> candidates,
            string direction)
        {
            var lowerIsBetter = string.Equals(direction, "LowerIsBetter", StringComparison.OrdinalIgnoreCase);

            var sorted = lowerIsBetter
                ? candidates.OrderBy(c => c.Value).ThenBy(c => c.EntityId, StringComparer.OrdinalIgnoreCase).ToList()
                : candidates.OrderByDescending(c => c.Value).ThenBy(c => c.EntityId, StringComparer.OrdinalIgnoreCase).ToList();

            var populationSize = sorted.Count;
            if (populationSize == 0)
                return Array.Empty<RankingResult>();

            var results = new List<RankingResult>(populationSize);
            var rankPosition = 0;
            var itemsProcessed = 0;
            decimal? previousValue = null;

            foreach (var item in sorted)
            {
                itemsProcessed++;
                if (!previousValue.HasValue || item.Value != previousValue.Value)
                    rankPosition = itemsProcessed;

                previousValue = item.Value;

                results.Add(new RankingResult
                {
                    EntityId = item.EntityId,
                    RankPosition = rankPosition,
                    PopulationSize = populationSize,
                    Percentile = CalculatePercentile(rankPosition, populationSize)
                });
            }

            return results;
        }

        public static decimal CalculatePercentile(int rankPosition, int populationSize)
        {
            if (populationSize <= 0 || rankPosition <= 0)
                return 0m;

            var percentile = 100m * (populationSize - rankPosition + 1) / populationSize;
            return Math.Round(percentile, 2, MidpointRounding.AwayFromZero);
        }
    }
}
