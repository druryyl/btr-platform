using System;

namespace btr.application.SalesContext.VisitPlanAgg.Services
{
    public interface IRuteCycleCalendar
    {
        string ResolveHariRuteId(DateTime visitDate);

        string GetCycleWeekLabel(DateTime visitDate);
    }
}
