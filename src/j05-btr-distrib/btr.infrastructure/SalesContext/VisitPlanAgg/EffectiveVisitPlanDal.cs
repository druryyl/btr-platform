using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.VisitPlanAgg;
using btr.application.SalesContext.VisitPlanAgg.Services;
using btr.domain.SalesContext.VisitPlanAgg;

namespace btr.infrastructure.SalesContext.VisitPlanAgg
{
    public class EffectiveVisitPlanDal : IEffectiveVisitPlanDal
    {
        private readonly IVisitPlanDal _visitPlanDal;
        private readonly IVisitPlanExceptionDal _exceptionDal;
        private readonly IEffectiveVisitPlanResolver _resolver;

        public EffectiveVisitPlanDal(
            IVisitPlanDal visitPlanDal,
            IVisitPlanExceptionDal exceptionDal,
            IEffectiveVisitPlanResolver resolver)
        {
            _visitPlanDal = visitPlanDal;
            _exceptionDal = exceptionDal;
            _resolver = resolver;
        }

        public IEnumerable<EffectiveVisitPlanEntry> ListEffectivePlan(string salesPersonId, DateTime visitDate)
        {
            var basePlan = (_visitPlanDal.ListData(new VisitPlanDateFilter(salesPersonId, visitDate))
                            ?? Enumerable.Empty<VisitPlanModel>())
                .ToList();
            var exceptions = (_exceptionDal.ListData(new VisitPlanDateFilter(salesPersonId, visitDate))
                              ?? Enumerable.Empty<VisitPlanExceptionModel>())
                .ToList();

            return _resolver.Resolve(basePlan, exceptions);
        }
    }
}
