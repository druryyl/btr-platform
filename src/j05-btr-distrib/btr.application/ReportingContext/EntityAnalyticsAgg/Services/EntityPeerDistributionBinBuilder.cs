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
    /// Other units: adaptive-count nice-linear bins with Tukey-fence overflow isolation.
    /// </summary>
    public static class EntityPeerDistributionBinBuilder
    {
        public const int DefaultBinCount = 20;

        /// <summary>Minimum peers required before Tukey fencing applies (fences are noise below this).</summary>
        public const int MinPeersForFencing = 10;

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

        /// <summary>
        /// Adaptive bulk bin-count ceiling by peer population size, so the bin count
        /// never approaches the observation count for small peer groups.
        /// </summary>
        public static int ResolveEffectiveBinCount(int peerCount, int requestedCount)
        {
            var bandCap = peerCount <= 30 ? 8 : peerCount <= 100 ? 12 : DefaultBinCount;
            return Math.Max(1, Math.Min(requestedCount, bandCap));
        }

        /// <summary>
        /// Linear-interpolated quartile of pre-sorted values (same interpolation style
        /// as the IDR quantile cuts). Values must be sorted ascending.
        /// </summary>
        public static decimal Quartile(IReadOnlyList<decimal> sortedValues, double quantile)
        {
            if (sortedValues == null || sortedValues.Count == 0)
                throw new ArgumentException("Values are required.", nameof(sortedValues));
            if (sortedValues.Count == 1)
                return sortedValues[0];

            var pos = quantile * (sortedValues.Count - 1);
            var low = (int)Math.Floor(pos);
            var high = Math.Min(sortedValues.Count - 1, low + 1);
            var frac = (decimal)(pos - low);
            return sortedValues[low] + ((sortedValues[high] - sortedValues[low]) * frac);
        }

        private static readonly int[] GenericNiceMantissas = { 1, 2, 5, 10 };

        private static decimal DecimalPow10(int exp)
        {
            if (exp >= 0)
            {
                decimal result = 1m;
                for (var i = 0; i < exp; i++)
                    result *= 10m;
                return result;
            }

            decimal fraction = 1m;
            for (var i = 0; i < -exp; i++)
                fraction /= 10m;
            return fraction;
        }

        private static IEnumerable<decimal> EnumerateNiceLadder(decimal magnitude)
        {
            // Exact decimal powers (0.1, 0.01, …) — never double-based Math.Pow,
            // so fractional ladder steps compare exactly in tests and labels.
            var d = (double)magnitude;
            var exp = (int)Math.Floor(Math.Log10(d));
            var seen = new HashSet<decimal>();
            for (var e = exp - 2; e <= exp + 2; e++)
            {
                var scale = DecimalPow10(e);
                foreach (var m in GenericNiceMantissas)
                {
                    var candidate = m * scale;
                    if (candidate > 0 && seen.Add(candidate))
                        yield return candidate;
                }
            }
        }

        /// <summary>Smallest 1-2-5 × 10ⁿ step at or above the value (sign-aware).</summary>
        public static decimal NiceCeil(decimal value)
        {
            if (value == 0m)
                return 0m;
            var abs = Math.Abs(value);
            var best = abs;
            var found = false;
            foreach (var candidate in EnumerateNiceLadder(abs))
            {
                if (candidate >= abs && (!found || candidate < best))
                {
                    best = candidate;
                    found = true;
                }
            }
            if (!found)
                best = abs;
            return value > 0 ? best : -best;
        }

        /// <summary>Largest 1-2-5 × 10ⁿ step at or below the value (sign-aware).</summary>
        public static decimal NiceFloor(decimal value)
        {
            if (value == 0m)
                return 0m;
            var abs = Math.Abs(value);
            var best = abs;
            var found = false;
            foreach (var candidate in EnumerateNiceLadder(abs))
            {
                if (candidate <= abs && (!found || candidate > best))
                {
                    best = candidate;
                    found = true;
                }
            }
            if (!found)
                best = abs;
            return value > 0 ? best : -best;
        }

        private static List<PeerDistributionBinDto> BuildLinearBins(
            IReadOnlyList<decimal> values,
            int binCount,
            decimal min,
            decimal max)
        {
            var effectiveCount = ResolveEffectiveBinCount(values.Count, binCount);

            // Tukey-fence outlier isolation (both tails). The fence is an implementation
            // mechanism only; the chart shows business ranges plus flagged overflow bins.
            decimal? cap = null;
            decimal? floor = null;
            if (values.Count >= MinPeersForFencing)
            {
                var q1 = Quartile(values, 0.25);
                var q3 = Quartile(values, 0.75);
                var iqr = q3 - q1;
                if (iqr > 0)
                {
                    var hiFence = q3 + (1.5m * iqr);
                    var loFence = q1 - (1.5m * iqr);

                    var niceCap = NiceCeil(hiFence);
                    if (values.Any(v => v > hiFence) && niceCap < max)
                        cap = niceCap;

                    var niceFloor = NiceFloor(loFence);
                    if (values.Any(v => v < loFence) && niceFloor > min)
                        floor = niceFloor;
                }
            }

            var bulkStart = floor ?? min;
            var bulkEnd = cap ?? max;
            var step = NiceCeil((bulkEnd - bulkStart) / effectiveCount);
            if (step <= 0)
                step = bulkEnd - bulkStart;

            // Outer bulk edges clamp to the fenced bounds so bulk bins never overlap
            // the overflow/underflow buckets (counts would otherwise double-book).
            // Without adjacent overflow buckets the start snaps outward to a nice edge.
            var edges = new List<decimal>();
            var cursor = floor.HasValue ? floor.Value : Math.Floor(bulkStart / step) * step;
            while (cursor < bulkEnd)
            {
                edges.Add(cursor);
                cursor += step;
            }
            edges.Add(bulkEnd);

            var bins = new List<PeerDistributionBinDto>();
            if (floor.HasValue)
            {
                bins.Add(new PeerDistributionBinDto
                {
                    BinIndex = bins.Count,
                    BinStart = min,
                    BinEnd = floor.Value,
                    Count = CountInRange(values, min, floor.Value, isLast: false),
                    Label = FormatRangeLabel(min, floor.Value),
                    IsOverflow = true
                });
            }

            var bulkBins = BuildBinsFromEdges(values, edges);
            foreach (var bin in bulkBins)
            {
                bin.BinIndex = bins.Count;
                bins.Add(bin);
            }

            if (cap.HasValue)
            {
                bins.Add(new PeerDistributionBinDto
                {
                    BinIndex = bins.Count,
                    BinStart = cap.Value,
                    BinEnd = max,
                    Count = CountInRange(values, cap.Value, max, isLast: true),
                    Label = FormatOverflowLabel(cap.Value),
                    IsOverflow = true
                });
            }

            return bins;
        }

        private static string FormatOverflowLabel(decimal cap)
        {
            return $"≥ {FormatEdgeLabel(cap)}";
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

        private static string FormatEdgeLabel(decimal value)
        {
            if (value == Math.Truncate(value))
                return value.ToString("N0", CultureInfo.InvariantCulture);
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string FormatRangeLabel(decimal start, decimal end)
        {
            return $"{FormatEdgeLabel(start)} – {FormatEdgeLabel(end)}";
        }
    }
}
