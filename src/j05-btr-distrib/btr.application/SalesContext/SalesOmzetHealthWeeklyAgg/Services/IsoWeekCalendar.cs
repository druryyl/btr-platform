using System;
using System.Collections.Generic;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Services
{
    /// <summary>ISO-8601 week calendar (Monday first day, week 1 contains first Thursday).</summary>
    public class IsoWeekCalendar : IIsoWeekCalendar
    {
        public (DateTime PeriodStart, DateTime PeriodEnd) GetWeekBounds(int isoYear, int isoWeek)
        {
            if (isoWeek < 1 || isoWeek > 53)
                throw new ArgumentOutOfRangeException(nameof(isoWeek), "ISO week must be between 1 and 53.");

            var jan4 = new DateTime(isoYear, 1, 4);
            var weekOneMonday = StartOfWeekMonday(jan4);
            var periodStart = weekOneMonday.AddDays((isoWeek - 1) * 7);
            var periodEnd = periodStart.AddDays(6);
            return (periodStart, periodEnd);
        }

        public IsoWeekIdentifier GetIsoWeek(DateTime date)
        {
            var d = date.Date;
            var thursday = StartOfWeekMonday(d).AddDays(3);
            var isoYear = thursday.Year;
            var weekOneMonday = StartOfWeekMonday(new DateTime(isoYear, 1, 4));
            var isoWeek = 1 + (int)((StartOfWeekMonday(d) - weekOneMonday).TotalDays / 7);
            return new IsoWeekIdentifier(isoYear, isoWeek);
        }

        public IReadOnlyList<IsoWeekIdentifier> ListWeeksIntersecting(Periode reportPeriod)
        {
            if (reportPeriod is null)
                throw new ArgumentNullException(nameof(reportPeriod));

            var rangeStart = reportPeriod.Tgl1.Date;
            var rangeEnd = reportPeriod.Tgl2.Date;
            if (rangeEnd < rangeStart)
                throw new ArgumentException("Report period end must be on or after start.");

            var seen = new HashSet<(int Year, int Week)>();
            var result = new List<IsoWeekIdentifier>();
            var cursor = rangeStart;

            while (cursor <= rangeEnd)
            {
                var id = GetIsoWeek(cursor);
                var key = (id.YearNumber, id.WeekNumber);
                if (seen.Add(key))
                    result.Add(id);

                var bounds = GetWeekBounds(id.YearNumber, id.WeekNumber);
                cursor = bounds.PeriodEnd.AddDays(1);
            }

            return result;
        }

        private static DateTime StartOfWeekMonday(DateTime date)
        {
            var d = date.Date;
            var diff = ((int)d.DayOfWeek + 6) % 7;
            return d.AddDays(-diff);
        }
    }
}
