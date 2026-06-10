using System;
using System.Collections.Generic;
using btr.domain.SalesContext.VisitPlanAgg;

namespace btr.application.SalesContext.VisitPlanAgg.Services
{
    public interface IEffectiveVisitPlanResolver
    {
        IEnumerable<EffectiveVisitPlanEntry> Resolve(
            IEnumerable<VisitPlanModel> basePlan,
            IEnumerable<VisitPlanExceptionModel> exceptions);
    }
}
