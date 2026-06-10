using System;
using System.Collections.Generic;
using btr.domain.SalesContext.VisitPlanAgg;

namespace btr.application.SalesContext.VisitPlanAgg
{
    public interface IEffectiveVisitPlanDal
    {
        IEnumerable<EffectiveVisitPlanEntry> ListEffectivePlan(string salesPersonId, DateTime visitDate);
    }
}
