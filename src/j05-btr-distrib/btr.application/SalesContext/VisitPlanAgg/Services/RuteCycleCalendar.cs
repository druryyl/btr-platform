using System;
using btr.application.SupportContext.ParamSistemAgg;
using btr.domain.SupportContext.ParamSistemAgg;

namespace btr.application.SalesContext.VisitPlanAgg.Services
{
    public class RuteCycleCalendar : IRuteCycleCalendar
    {
        public const string AnchorDateParamCode = "ROUTE_CYCLE_ANCHOR_DATE";

        private readonly IParamSistemDal _paramSistemDal;
        private DateTime? _cachedAnchorDate;

        public RuteCycleCalendar(IParamSistemDal paramSistemDal)
        {
            _paramSistemDal = paramSistemDal;
        }

        public string ResolveHariRuteId(DateTime visitDate)
        {
            if (visitDate.DayOfWeek == DayOfWeek.Sunday)
                return null;

            var anchorDate = GetAnchorDate();
            var daysSinceAnchor = (visitDate.Date - anchorDate.Date).Days;
            if (daysSinceAnchor < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(visitDate),
                    $"Visit date {visitDate:yyyy-MM-dd} is before route cycle anchor {anchorDate:yyyy-MM-dd}.");

            var cycleDayIndex = daysSinceAnchor % 14;
            var weekInCycle = cycleDayIndex < 7 ? 1 : 2;
            var weekdayIndex = (int)visitDate.DayOfWeek;

            return weekInCycle == 1
                ? $"H1{weekdayIndex}"
                : $"H2{weekdayIndex}";
        }

        public string GetCycleWeekLabel(DateTime visitDate)
        {
            if (visitDate.DayOfWeek == DayOfWeek.Sunday)
                return string.Empty;

            var anchorDate = GetAnchorDate();
            var daysSinceAnchor = (visitDate.Date - anchorDate.Date).Days;
            if (daysSinceAnchor < 0)
                return string.Empty;

            var cycleDayIndex = daysSinceAnchor % 14;
            return cycleDayIndex < 7 ? "Minggu1" : "Minggu2";
        }

        internal DateTime GetAnchorDate()
        {
            if (_cachedAnchorDate.HasValue)
                return _cachedAnchorDate.Value;

            var param = _paramSistemDal.GetData(new ParamSistemModel(AnchorDateParamCode));
            if (param == null || string.IsNullOrWhiteSpace(param.ParamValue))
                throw new InvalidOperationException(
                    $"System parameter '{AnchorDateParamCode}' is not configured.");

            if (!DateTime.TryParse(param.ParamValue, out var anchorDate))
                throw new InvalidOperationException(
                    $"System parameter '{AnchorDateParamCode}' has invalid date value '{param.ParamValue}'.");

            _cachedAnchorDate = anchorDate.Date;
            return _cachedAnchorDate.Value;
        }
    }
}
