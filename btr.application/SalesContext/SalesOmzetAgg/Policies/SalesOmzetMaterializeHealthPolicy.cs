using System;
using btr.application.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public static class SalesOmzetMaterializeHealthPolicy
    {
        public const int FreshWithinDays = 7;
        public const int StaleAfterDays = 30;
        public const int PoorMissingThreshold = 50;

        public static SalesOmzetMaterializeHealthLevel Evaluate(SalesOmzetMaterializeHealthMetrics metrics, Periode window)
        {
            if (metrics is null)
                throw new ArgumentNullException(nameof(metrics));
            if (window is null)
                throw new ArgumentNullException(nameof(window));

            var windowEnd = window.Tgl2.Date;
            var totalMissing = metrics.MissingOrders + metrics.MissingDirectFakturs + metrics.UnlinkedFakturs;

            if (totalMissing >= PoorMissingThreshold)
                return SalesOmzetMaterializeHealthLevel.Poor;

            if (!IsReconciledFresh(metrics.LastReconciledMax, windowEnd, window.Tgl1.Date))
                return SalesOmzetMaterializeHealthLevel.Poor;

            if (totalMissing > 0 || metrics.StaleFakturEstimate > 0)
                return SalesOmzetMaterializeHealthLevel.Warning;

            if (IsReconciledGood(metrics.LastReconciledMax, windowEnd))
                return SalesOmzetMaterializeHealthLevel.Good;

            return SalesOmzetMaterializeHealthLevel.Warning;
        }

        public static string FormatDisplayText(
            SalesOmzetMaterializeHealthMetrics metrics,
            SalesOmzetMaterializeHealthLevel level,
            Periode window)
        {
            var totalMissing = metrics.MissingOrders + metrics.MissingDirectFakturs + metrics.UnlinkedFakturs;
            var refreshText = FormatLastReconciled(metrics.LastReconciledMax);
            var tgl1 = window.Tgl1.ToString("dd-MMM-yyyy");
            var tgl2 = window.Tgl2.ToString("dd-MMM-yyyy");

            return $"~{totalMissing} belum sinkron (60 hari: {tgl1}–{tgl2}) • terakhir refresh: {refreshText}";
        }

        private static string FormatLastReconciled(DateTime? lastReconciledMax)
        {
            if (!lastReconciledMax.HasValue || SalesOmzetDates.IsSentinel(lastReconciledMax.Value))
                return "belum ada";

            return lastReconciledMax.Value.ToString("dd-MMM-yyyy HH:mm");
        }

        private static bool IsReconciledGood(DateTime? lastReconciledMax, DateTime windowEnd)
        {
            if (!lastReconciledMax.HasValue || SalesOmzetDates.IsSentinel(lastReconciledMax.Value))
                return false;

            return lastReconciledMax.Value.Date >= windowEnd.AddDays(-FreshWithinDays);
        }

        private static bool IsReconciledFresh(DateTime? lastReconciledMax, DateTime windowEnd, DateTime windowStart)
        {
            if (!lastReconciledMax.HasValue || SalesOmzetDates.IsSentinel(lastReconciledMax.Value))
                return false;

            if (lastReconciledMax.Value.Date < windowStart)
                return false;

            return lastReconciledMax.Value.Date >= windowEnd.AddDays(-StaleAfterDays);
        }
    }

    public class SalesOmzetMaterializeHealthMetrics
    {
        public int MissingOrders { get; set; }
        public int MissingDirectFakturs { get; set; }
        public int UnlinkedFakturs { get; set; }
        public int AggregateRowsInScope { get; set; }
        public DateTime? LastReconciledMax { get; set; }
        public int StaleFakturEstimate { get; set; }
    }
}
