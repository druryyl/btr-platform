using System;

namespace btr.domain.SalesContext.VisitPlanAgg
{
    public interface IVisitPlanKey
    {
        string VisitPlanId { get; }
    }

    public interface IVisitPlanDateKey
    {
        string SalesPersonId { get; }
        DateTime VisitDate { get; }
    }

    public interface IVisitPlanExceptionKey
    {
        string VisitPlanExceptionId { get; }
    }
}
