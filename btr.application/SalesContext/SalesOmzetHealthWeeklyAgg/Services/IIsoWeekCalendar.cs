using System;
using System.Collections.Generic;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Services
{
    public interface IIsoWeekCalendar
    {
        (DateTime PeriodStart, DateTime PeriodEnd) GetWeekBounds(int isoYear, int isoWeek);

        IsoWeekIdentifier GetIsoWeek(DateTime date);

        IReadOnlyList<IsoWeekIdentifier> ListWeeksIntersecting(Periode reportPeriod);
    }
}
