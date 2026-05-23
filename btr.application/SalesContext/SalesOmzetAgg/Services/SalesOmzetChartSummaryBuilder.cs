using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Services
{
    public class SalesOmzetChartSummaryBuilder : ISalesOmzetChartSummaryBuilder
    {
        public const int ManagerComparisonTopCount = 15;

        public const string LabelSelesai = "Selesai";
        public const string LabelDirectSale = "Penjualan langsung";
        public const string LabelPendingOmzet = "Menunggu omzet";
        public const string LabelOutstanding = "Order outstanding";

        private static readonly string[] SliceDisplayOrder =
        {
            LabelSelesai,
            LabelDirectSale,
            LabelPendingOmzet,
            LabelOutstanding
        };

        private readonly ISalesOmzetChartAmountPolicy _amountPolicy;

        public SalesOmzetChartSummaryBuilder(ISalesOmzetChartAmountPolicy amountPolicy)
        {
            _amountPolicy = amountPolicy;
        }

        public SalesOmzetChartSummary Build(
            IEnumerable<SalesOmzetView> rows,
            Periode periode,
            SalesOmzetPeriodFilterMode mode,
            decimal? targetAmount = null)
        {
            var list = rows?.ToList() ?? new List<SalesOmzetView>();
            var sliceTotals = new Dictionary<string, SalesOmzetStatusSlice>();

            foreach (var row in list)
            {
                var label = ResolveSliceLabel(row);
                if (label is null)
                    continue;

                var amount = _amountPolicy.ResolveAmount(row);
                if (!sliceTotals.TryGetValue(label, out var slice))
                {
                    slice = new SalesOmzetStatusSlice
                    {
                        Label = label,
                        Status = row.OmzetStatus,
                        SaleKind = row.SaleKind
                    };
                    sliceTotals[label] = slice;
                }

                slice.Amount += amount;
            }

            var recognizedRows = list.Where(_amountPolicy.IncludeInRecognizedTotal).ToList();
            var recognizedOmzet = recognizedRows.Sum(r => _amountPolicy.ResolveAmount(r));
            var pipelineOmzet = list
                .Where(_amountPolicy.IncludeInPipelineTotal)
                .Sum(r => _amountPolicy.ResolveAmount(r));

            var byStatus = SliceDisplayOrder
                .Where(sliceTotals.ContainsKey)
                .Select(label => sliceTotals[label])
                .ToList();

            var byWeek = BuildByWeek(recognizedRows, periode, mode);

            return new SalesOmzetChartSummary
            {
                RecognizedOmzet = recognizedOmzet,
                PipelineOmzet = pipelineOmzet,
                RecognizedTransactionCount = recognizedRows.Count,
                Target = targetAmount,
                AchievementPercent = SalesOmzetChartAchievementPolicy.ComputePercent(
                    recognizedOmzet,
                    targetAmount),
                ByStatus = byStatus,
                ByWeek = byWeek
            };
        }

        /// <summary>
        /// Weekly pace: recognized (completed) omzet only — same in Omzet and Sales period modes.
        /// </summary>
        private List<SalesOmzetWeekSlice> BuildByWeek(
            List<SalesOmzetView> recognizedRows,
            Periode periode,
            SalesOmzetPeriodFilterMode mode)
        {
            var buckets = SalesOmzetChartWeekGrouper.BuildBuckets(periode);
            var totals = buckets.ToDictionary(
                b => b.WeekStart,
                b => new SalesOmzetWeekSlice
                {
                    WeekStart = b.WeekStart,
                    WeekEnd = b.WeekEnd,
                    WeekLabel = b.WeekLabel,
                    RecognizedAmount = 0m
                });

            foreach (var row in recognizedRows)
            {
                var groupingDate = SalesOmzetChartWeekGrouper.ResolveGroupingDate(row, mode);
                if (!groupingDate.HasValue)
                    continue;

                var bucket = SalesOmzetChartWeekGrouper.FindBucket(buckets, groupingDate.Value);
                if (bucket is null)
                    continue;

                totals[bucket.WeekStart].RecognizedAmount += _amountPolicy.ResolveAmount(row);
            }

            return totals.Values.ToList();
        }

        public IReadOnlyList<SalesOmzetSalesPersonSlice> BuildManagerComparison(
            IEnumerable<SalesOmzetView> rows,
            int topCount = ManagerComparisonTopCount)
        {
            var list = rows?.ToList() ?? new List<SalesOmzetView>();
            if (topCount <= 0)
                return new List<SalesOmzetSalesPersonSlice>();

            return list
                .Where(_amountPolicy.IncludeInRecognizedTotal)
                .GroupBy(r => r.SalesPersonName?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Key.Length > 0)
                .Select(g => new SalesOmzetSalesPersonSlice
                {
                    SalesPersonName = g.Key,
                    RecognizedOmzet = g.Sum(r => _amountPolicy.ResolveAmount(r))
                })
                .OrderByDescending(s => s.RecognizedOmzet)
                .ThenBy(s => s.SalesPersonName, StringComparer.OrdinalIgnoreCase)
                .Take(topCount)
                .ToList();
        }

        internal static string ResolveSliceLabel(SalesOmzetView row)
        {
            switch (row.OmzetStatus)
            {
                case SalesOmzetStatusEnum.Outstanding:
                    return LabelOutstanding;
                case SalesOmzetStatusEnum.PendingOmzet:
                    return LabelPendingOmzet;
                case SalesOmzetStatusEnum.Completed:
                    return row.SaleKind == SaleKindEnum.DirectSale || string.IsNullOrEmpty(row.OrderId)
                        ? LabelDirectSale
                        : LabelSelesai;
                default:
                    return null;
            }
        }
    }
}
