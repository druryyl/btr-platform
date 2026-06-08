using System;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.Shared
{
    public static class ReportPeriodValidator
    {
        public const int MaxDays = 31;

        public static Periode ResolveAndValidate(ReportPeriodRequest request, DateTime referenceDate)
        {
            var today = referenceDate.Date;
            var from = request?.From?.Date ?? new DateTime(today.Year, today.Month, 1);
            var to = request?.To?.Date
                     ?? new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

            if (from > to)
                throw new ArgumentException("Period 'from' must not be after 'to'.");

            var dayCount = (to - from).Days + 1;
            if (dayCount > MaxDays)
                throw new ArgumentException($"Report period must not exceed {MaxDays} days.");

            return new Periode(from, to);
        }
    }
}
