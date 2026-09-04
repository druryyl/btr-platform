using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Queries;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    /// <summary>
    /// Builds Peer Position histogram bins.
    /// IDR: fixed first bin 0–100,000, quantile cuts snapped to a 1-2-5 Rupiah ladder,
    /// then forced high-end edges 50M / 100M / 200M / 500M before peer max.
    /// Days: week/month calendar ladder ending with 365 → peer max.
    /// Other units: linear equal-width bins.
    /// </summary>
    public static class EntityPeerDistributionBinBuilder
    {
        public const int DefaultBinCount = 20;

        /// <summary>Fixed first IDR segment end (and floor for remaining ladder).</summary>
        public const decimal IdrFirstBinEnd = 100_000m;

        /// <summary>Forced high-end IDR edges before peer max (when each step is below max).</summary>
        public static readonly decimal[] IdrHighEndEdges =
        {
            50_000_000m,
            100_000_000m,
            200_000_000m,
            500_000_000m
        };

        /// <summary>One-year threshold — last Days bin starts here when peer max is greater.</summary>
        public const decimal DaysYearEdge = 365m;

        /// <summary>
        /// Calendar Days ladder (weeks then months), clipped below peer max; 365 forced when max &gt; 365.
        /// </summary>
        public static readonly decimal[] DaysCalendarEdges =
        {
            0m, 7m, 14m, 21m, 30m, 60m, 90m, 120m, 180m, 270m, DaysYearEdge
        };

        private static readonly int[] NiceMantissas = { 1, 2, 5 };

        public static List<PeerDistributionBinDto> BuildBins(
            IReadOnlyList<decimal> values,
            int binCount,
            string unit)
        {
            if (values == null || values.Count == 0)
                return new List<PeerDistributionBinDto>();

            if (binCount < 1)
                throw new ArgumentOutOfRangeException(nameof(binCount));

            var min = values.First();
            var max = values.Last();
            if (min == max)
            {
                return new List<PeerDistributionBinDto>
                {
                    new PeerDistributionBinDto
                    {
                        BinIndex = 0,
                        BinStart = min,
                        BinEnd = max,
                        Count = values.Count,
                        Label = min.ToString(CultureInfo.InvariantCulture)
                    }
                };
            }

            if (IsIdrUnit(unit))
                return BuildIdrBins(values, binCount, max);

            if (IsDaysUnit(unit))
                return BuildDaysBins(values, max);

            return BuildLinearBins(values, binCount, min, max);
        }

        private static bool IsIdrUnit(string unit)
        {
            return string.Equals(unit, "IDR", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDaysUnit(string unit)
        {
            return string.Equals(unit, "Days", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Snap to nearest 1-2-5 × 10^n Rupiah step (at least <see cref="IdrFirstBinEnd"/>).
        /// </summary>
        public static decimal SnapToNiceIdr(decimal value)
        {
            if (value <= IdrFirstBinEnd)
                return IdrFirstBinEnd;

            var d = (double)value;
            var exp = (int)Math.Floor(Math.Log10(d));
            decimal best = IdrFirstBinEnd;
            var bestDist = decimal.MaxValue;

            for (var e = exp - 1; e <= exp + 1; e++)
            {
                if (e < 0)
                    continue;

                var scale = Pow10Decimal(e);
                foreach (var m in NiceMantissas)
                {
                    var candidate = m * scale;
                    if (candidate < IdrFirstBinEnd)
                        continue;

                    var dist = Math.Abs(candidate - value);
                    if (dist < bestDist || (dist == bestDist && candidate > best))
                    {
                        bestDist = dist;
                        best = candidate;
                    }
                }
            }

            return best;
        }

        private static decimal Pow10Decimal(int exp)
        {
            decimal result = 1m;
            for (var i = 0; i < exp; i++)
                result *= 10m;
            return result;
        }

        private static List<PeerDistributionBinDto> BuildIdrBins(
            IReadOnlyList<decimal> values,
            int binCount,
            decimal max)
        {
            return BuildBinsFromEdges(values, BuildIdrEdges(values, binCount, max));
        }

        private static List<PeerDistributionBinDto> BuildDaysBins(
            IReadOnlyList<decimal> values,
            decimal max)
        {
            return BuildBinsFromEdges(values, BuildDaysEdges(max));
        }

        /// <summary>
        /// Week/month calendar edges clipped below peer max; forces 365 when max &gt; 365, then peer max.
        /// </summary>
        private static List<decimal> BuildDaysEdges(decimal max)
        {
            var edges = new List<decimal>();
            foreach (var step in DaysCalendarEdges)
            {
                if (step < max)
                    edges.Add(step);
            }

            // Ensure year threshold exists when the year+ tail is needed
            if (max > DaysYearEdge && !edges.Contains(DaysYearEdge))
                edges.Add(DaysYearEdge);

            AppendEdge(edges, max);
            return edges;
        }

        private static List<PeerDistributionBinDto> BuildBinsFromEdges(
            IReadOnlyList<decimal> values,
            IReadOnlyList<decimal> edges)
        {
            if (edges == null || edges.Count < 2)
                return new List<PeerDistributionBinDto>();

            var bins = new List<PeerDistributionBinDto>(edges.Count - 1);
            for (var i = 0; i < edges.Count - 1; i++)
            {
                var start = edges[i];
                var end = edges[i + 1];
                var isLast = i == edges.Count - 2;
                bins.Add(new PeerDistributionBinDto
                {
                    BinIndex = i,
                    BinStart = start,
                    BinEnd = end,
                    Count = CountInRange(values, start, end, isLast),
                    Label = FormatRangeLabel(start, end)
                });
            }

            return bins;
        }

        /// <summary>
        /// Edge list: 0 → 100,000 → quantile/snapped mid edges → forced 50M/100M/200M/500M → peer max.
        /// </summary>
        private static List<decimal> BuildIdrEdges(
            IReadOnlyList<decimal> values,
            int binCount,
            decimal max)
        {
            var edges = new List<decimal> { 0m, IdrFirstBinEnd };

            if (max <= IdrFirstBinEnd)
                return edges;

            var above = values.Where(v => v >= IdrFirstBinEnd).ToList();
            if (above.Count == 0)
                return edges;

            var remainingBinCount = Math.Max(1, binCount - 1);
            if (above.Count > 1 && remainingBinCount > 1)
            {
                // Internal quantile cuts at k/remainingBinCount for k = 1 .. remainingBinCount-1
                for (var k = 1; k < remainingBinCount; k++)
                {
                    var pos = (double)k / remainingBinCount * (above.Count - 1);
                    var idxLow = (int)Math.Floor(pos);
                    var idxHigh = Math.Min(above.Count - 1, idxLow + 1);
                    var frac = pos - idxLow;
                    var rawCut = above[idxLow] + ((above[idxHigh] - above[idxLow]) * (decimal)frac);
                    var snapped = SnapToNiceIdr(rawCut);

                    if (snapped > edges[edges.Count - 1] && snapped < max)
                        AppendEdge(edges, snapped);
                }
            }

            EnsureIdrHighEndEdges(edges, max);
            AppendEdge(edges, max);
            return edges;
        }

        /// <summary>
        /// Inserts 50M / 100M / 200M / 500M in sorted order when each step is strictly below peer max.
        /// </summary>
        private static void EnsureIdrHighEndEdges(List<decimal> edges, decimal max)
        {
            foreach (var step in IdrHighEndEdges)
            {
                if (step >= max)
                    break;

                if (edges.Contains(step))
                    continue;

                var idx = edges.FindIndex(e => e > step);
                if (idx < 0)
                    edges.Add(step);
                else
                    edges.Insert(idx, step);
            }
        }

        private static void AppendEdge(List<decimal> edges, decimal edge)
        {
            if (edges.Count == 0 || edge > edges[edges.Count - 1])
                edges.Add(edge);
        }

        private static List<PeerDistributionBinDto> BuildLinearBins(
            IReadOnlyList<decimal> values,
            int binCount,
            decimal min,
            decimal max)
        {
            var span = max - min;
            var width = span / binCount;
            var bins = new List<PeerDistributionBinDto>(binCount);

            for (var i = 0; i < binCount; i++)
            {
                var start = min + (width * i);
                var end = i == binCount - 1 ? max : min + (width * (i + 1));
                var count = CountInRange(values, start, end, isLast: i == binCount - 1);

                bins.Add(new PeerDistributionBinDto
                {
                    BinIndex = i,
                    BinStart = start,
                    BinEnd = end,
                    Count = count,
                    Label = FormatRangeLabel(start, end)
                });
            }

            return bins;
        }

        private static int CountInRange(
            IReadOnlyList<decimal> values,
            decimal start,
            decimal end,
            bool isLast)
        {
            var count = 0;
            for (var i = 0; i < values.Count; i++)
            {
                var v = values[i];
                if (isLast)
                {
                    if (v >= start && v <= end)
                        count++;
                }
                else if (v >= start && v < end)
                {
                    count++;
                }
            }

            return count;
        }

        private static string FormatRangeLabel(decimal start, decimal end)
        {
            return $"{start:N0} – {end:N0}";
        }
    }
}
