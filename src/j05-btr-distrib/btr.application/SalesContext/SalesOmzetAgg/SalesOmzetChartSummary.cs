using System;
using System.Collections.Generic;
using btr.domain.SalesContext.SalesOmzetAgg;

namespace btr.application.SalesContext.SalesOmzetAgg
{
    public sealed class SalesOmzetChartSummary
    {
        public decimal RecognizedOmzet { get; set; }

        public decimal PipelineOmzet { get; set; }

        public int RecognizedTransactionCount { get; set; }

        public decimal? Target { get; set; }

        /// <summary>Recognized / target × 100; null when no target.</summary>
        public decimal? AchievementPercent { get; set; }

        public IReadOnlyList<SalesOmzetStatusSlice> ByStatus { get; set; } =
            new List<SalesOmzetStatusSlice>();

        public IReadOnlyList<SalesOmzetWeekSlice> ByWeek { get; set; } =
            new List<SalesOmzetWeekSlice>();

        public IReadOnlyList<SalesOmzetSalesPersonSlice> BySalesPerson { get; set; } =
            new List<SalesOmzetSalesPersonSlice>();
    }

    public sealed class SalesOmzetStatusSlice
    {
        public string Label { get; set; }

        public decimal Amount { get; set; }

        public SalesOmzetStatusEnum Status { get; set; }

        public SaleKindEnum? SaleKind { get; set; }
    }

    public sealed class SalesOmzetWeekSlice
    {
        public DateTime WeekStart { get; set; }

        public DateTime WeekEnd { get; set; }

        public string WeekLabel { get; set; }

        public decimal RecognizedAmount { get; set; }
    }

    public sealed class SalesOmzetSalesPersonSlice
    {
        public string SalesPersonName { get; set; }

        public decimal RecognizedOmzet { get; set; }
    }
}
