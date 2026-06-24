using System;
using System.Collections.Generic;
using System.Globalization;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Services
{
    /// <summary>
    /// Calendar day buckets within a report <see cref="Periode"/> (one bucket per day from Tgl1..Tgl2).
    /// </summary>
    public static class SalesOmzetChartDayGrouper
    {
        public static readonly CultureInfo LabelCulture = CultureInfo.GetCultureInfo("id-ID");

        public sealed class DayBucket
        {
            public DateTime PaceDate { get; set; }

            public int DayOfMonth { get; set; }

            public string DayLabel { get; set; }
        }

        public static IReadOnlyList<DayBucket> BuildBuckets(Periode periode)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var rangeStart = periode.Tgl1.Date;
            var rangeEnd = periode.Tgl2.Date;
            if (rangeEnd < rangeStart)
                return new List<DayBucket>();

            var buckets = new List<DayBucket>();
            var cursor = rangeStart;
            while (cursor <= rangeEnd)
            {
                buckets.Add(new DayBucket
                {
                    PaceDate = cursor,
                    DayOfMonth = cursor.Day,
                    DayLabel = FormatDayLabel(cursor)
                });

                cursor = cursor.AddDays(1);
            }

            return buckets;
        }

        public static string FormatDayLabel(DateTime date) =>
            date.ToString("dd MMM", LabelCulture);

        public static DayBucket FindBucket(IReadOnlyList<DayBucket> buckets, DateTime date)
        {
            if (buckets is null)
                return null;

            var day = date.Date;
            foreach (var bucket in buckets)
            {
                if (bucket.PaceDate == day)
                    return bucket;
            }

            return null;
        }
    }
}
