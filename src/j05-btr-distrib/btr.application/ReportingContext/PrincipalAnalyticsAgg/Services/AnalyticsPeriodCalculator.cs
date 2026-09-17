using System;
using btr.application.Portal;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.SupportContext.TglJamAgg;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    public class AnalyticsPeriodCalculator
    {
        private readonly IBusinessDateProvider _businessDateProvider;
        private readonly ITglJamDal _tglJamDal;

        public AnalyticsPeriodCalculator(
            IBusinessDateProvider businessDateProvider,
            ITglJamDal tglJamDal)
        {
            _businessDateProvider = businessDateProvider;
            _tglJamDal = tglJamDal;
        }

        public AnalyticsPeriodContext Resolve()
        {
            return Create(_businessDateProvider.Today, _tglJamDal.Now);
        }

        public static AnalyticsPeriodContext Create(DateTime asOfDate, DateTime generatedAt)
        {
            var asOf = asOfDate.Date;
            var monthStart = new DateTime(asOf.Year, asOf.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(asOf.Year, asOf.Month);
            var monthEnd = new DateTime(asOf.Year, asOf.Month, daysInMonth);
            var elapsedDays = Math.Max(1, (asOf - monthStart).Days + 1);

            var priorYear = asOf.Year - 1;
            var priorMonth = asOf.Month;
            var priorYearStart = new DateTime(priorYear, priorMonth, 1);
            var priorMonthEnd = new DateTime(priorYear, priorMonth, DateTime.DaysInMonth(priorYear, priorMonth));
            var priorYearEnd = priorYearStart.AddDays(elapsedDays - 1);
            if (priorYearEnd > priorMonthEnd)
                priorYearEnd = priorMonthEnd;

            return new AnalyticsPeriodContext
            {
                AsOfDate = asOf,
                PeriodYear = asOf.Year,
                PeriodMonth = asOf.Month,
                MonthStart = monthStart,
                MonthEnd = monthEnd,
                ElapsedDays = elapsedDays,
                DaysInMonth = daysInMonth,
                PriorYear = priorYear,
                PriorMonth = priorMonth,
                PriorYearStart = priorYearStart,
                PriorYearEnd = priorYearEnd,
                GeneratedAt = generatedAt
            };
        }
    }
}
