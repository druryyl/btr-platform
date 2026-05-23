using System;
using System.Collections.Generic;
using System.Globalization;
using btr.application.SalesContext.OrderFeature;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Services
{
    /// <summary>
    /// Calendar week buckets within a report <see cref="Periode"/> (7-day segments from Tgl1).
    /// Weekly pace chart uses recognized omzet only (see plan phase 2).
    /// </summary>
    public static class SalesOmzetChartWeekGrouper
    {
        public static readonly CultureInfo LabelCulture = CultureInfo.GetCultureInfo("id-ID");

        public sealed class WeekBucket
        {
            public DateTime WeekStart { get; set; }
            public DateTime WeekEnd { get; set; }
            public string WeekLabel { get; set; }
        }

        public static IReadOnlyList<WeekBucket> BuildBuckets(Periode periode)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var rangeStart = periode.Tgl1.Date;
            var rangeEnd = periode.Tgl2.Date;
            if (rangeEnd < rangeStart)
                return new List<WeekBucket>();

            var buckets = new List<WeekBucket>();
            var cursor = rangeStart;
            while (cursor <= rangeEnd)
            {
                var weekEnd = cursor.AddDays(6);
                if (weekEnd > rangeEnd)
                    weekEnd = rangeEnd;

                buckets.Add(new WeekBucket
                {
                    WeekStart = cursor,
                    WeekEnd = weekEnd,
                    WeekLabel = FormatWeekLabel(cursor, weekEnd)
                });

                cursor = weekEnd.AddDays(1);
            }

            return buckets;
        }

        public static string FormatWeekLabel(DateTime weekStart, DateTime weekEnd)
        {
            var startText = weekStart.ToString("dd MMM", LabelCulture);
            if (weekStart.Date == weekEnd.Date)
                return startText;

            var endText = weekEnd.ToString("dd MMM", LabelCulture);
            return $"{startText}–{endText}";
        }

        /// <summary>
        /// Date used to place a row in a weekly bucket.
        /// Omzet Period: <see cref="SalesOmzetView.OmzetDate"/>.
        /// Sales Period: <see cref="SalesOmzetView.SalesDate"/> when set; otherwise view fallbacks.
        /// </summary>
        public static DateTime? ResolveGroupingDate(SalesOmzetView row, SalesOmzetPeriodFilterMode mode)
        {
            if (row is null)
                return null;

            if (mode == SalesOmzetPeriodFilterMode.OmzetPeriod)
                return ToCalendarDate(row.OmzetDate);

            if (ToCalendarDate(row.SalesDate) is DateTime salesDate)
                return salesDate;

            if (row.OmzetStatus == SalesOmzetStatusEnum.Outstanding)
                return ToCalendarDate(row.OrderDate);

            if (ToCalendarDate(row.FakturDate) is DateTime fakturDate)
                return fakturDate;

            return ToCalendarDate(row.OmzetDate);
        }

        public static WeekBucket FindBucket(IReadOnlyList<WeekBucket> buckets, DateTime date)
        {
            if (buckets is null)
                return null;

            var day = date.Date;
            foreach (var bucket in buckets)
            {
                if (day >= bucket.WeekStart && day <= bucket.WeekEnd)
                    return bucket;
            }

            return null;
        }

        private static DateTime? ToCalendarDate(DateTime value)
        {
            if (value == DateTime.MinValue || SalesOmzetDates.IsSentinel(value))
                return null;
            return value.Date;
        }
    }
}
