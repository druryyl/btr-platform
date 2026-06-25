using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public readonly struct YearMonthPeriod : IEquatable<YearMonthPeriod>, IComparable<YearMonthPeriod>
    {
        public YearMonthPeriod(int year, int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");

            Year = year;
            Month = month;
        }

        public int Year { get; }
        public int Month { get; }

        public DateTime PeriodStart => new DateTime(Year, Month, 1);
        public DateTime PeriodEnd => PeriodStart.AddMonths(1).AddDays(-1);

        public string Label => $"{Year:D4}-{Month:D2}";

        public YearMonthPeriod AddMonths(int months) => FromDateTime(PeriodStart.AddMonths(months));

        public static YearMonthPeriod FromDateTime(DateTime date) =>
            new YearMonthPeriod(date.Year, date.Month);

        public static YearMonthPeriod Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Period value is required.", nameof(value));

            var parts = value.Split('-');
            if (parts.Length != 2
                || !int.TryParse(parts[0], out var year)
                || !int.TryParse(parts[1], out var month))
            {
                throw new ArgumentException($"Invalid period format '{value}'. Expected YYYY-MM.");
            }

            return new YearMonthPeriod(year, month);
        }

        public bool Equals(YearMonthPeriod other) => Year == other.Year && Month == other.Month;

        public override bool Equals(object obj) => obj is YearMonthPeriod other && Equals(other);

        public override int GetHashCode() => (Year * 397) ^ Month;

        public int CompareTo(YearMonthPeriod other)
        {
            var yearCompare = Year.CompareTo(other.Year);
            return yearCompare != 0 ? yearCompare : Month.CompareTo(other.Month);
        }

        public static bool operator <(YearMonthPeriod left, YearMonthPeriod right) => left.CompareTo(right) < 0;
        public static bool operator >(YearMonthPeriod left, YearMonthPeriod right) => left.CompareTo(right) > 0;
        public static bool operator <=(YearMonthPeriod left, YearMonthPeriod right) => left.CompareTo(right) <= 0;
        public static bool operator >=(YearMonthPeriod left, YearMonthPeriod right) => left.CompareTo(right) >= 0;
    }

    public static class EntityAnalyticsBackfillPeriodHelper
    {
        public static YearMonthPeriod GetDefaultFromPeriod(DateTime businessDate, int historyRetentionMonths)
        {
            var anchor = businessDate.Date;
            return YearMonthPeriod.FromDateTime(anchor.AddMonths(-historyRetentionMonths));
        }

        public static YearMonthPeriod GetDefaultToPeriod(DateTime businessDate)
        {
            var priorMonth = businessDate.Date.AddMonths(-1);
            return YearMonthPeriod.FromDateTime(priorMonth);
        }

        public static IReadOnlyList<YearMonthPeriod> EnumeratePeriods(
            YearMonthPeriod from,
            YearMonthPeriod to)
        {
            if (from > to)
                throw new ArgumentException("From period must be less than or equal to to period.");

            var periods = new List<YearMonthPeriod>();
            var current = from;
            while (current <= to)
            {
                periods.Add(current);
                current = current.AddMonths(1);
            }

            return periods;
        }

        public static bool IsWithinRetention(
            YearMonthPeriod from,
            YearMonthPeriod to,
            DateTime businessDate,
            int historyRetentionMonths)
        {
            var earliest = GetDefaultFromPeriod(businessDate, historyRetentionMonths);
            return from >= earliest && to <= GetDefaultToPeriod(businessDate);
        }
    }
}
