using System;
using System.Collections.Generic;
using System.Globalization;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class InventoryConsumptionGrouper
    {
        public static readonly CultureInfo LabelCulture = CultureInfo.GetCultureInfo("id-ID");

        public sealed class DayBucket
        {
            public DateTime ConsumptionDate { get; set; }

            public int DayIndex { get; set; }

            public string DayLabel { get; set; }
        }

        public static IReadOnlyList<DayBucket> BuildBuckets(DateTime windowStart, DateTime windowEnd)
        {
            var rangeStart = windowStart.Date;
            var rangeEnd = windowEnd.Date;
            if (rangeEnd < rangeStart)
                return new List<DayBucket>();

            var buckets = new List<DayBucket>();
            var cursor = rangeStart;
            var index = 1;
            while (cursor <= rangeEnd)
            {
                buckets.Add(new DayBucket
                {
                    ConsumptionDate = cursor,
                    DayIndex = index++,
                    DayLabel = cursor.ToString("dd MMM", LabelCulture)
                });

                cursor = cursor.AddDays(1);
            }

            return buckets;
        }

        public static string FormatDayLabel(DateTime date) =>
            date.ToString("dd MMM", LabelCulture);
    }
}
