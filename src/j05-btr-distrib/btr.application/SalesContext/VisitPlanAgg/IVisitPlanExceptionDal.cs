using System.Collections.Generic;
using btr.domain.SalesContext.VisitPlanAgg;
using btr.nuna.Infrastructure;

namespace btr.application.SalesContext.VisitPlanAgg
{
    public interface IVisitPlanExceptionDal :
        IInsert<VisitPlanExceptionModel>,
        IDelete<IVisitPlanExceptionKey>,
        IGetData<VisitPlanExceptionModel, IVisitPlanExceptionKey>,
        IListData<VisitPlanExceptionModel, IVisitPlanDateKey>,
        IListData<VisitPlanExceptionModel, VisitPlanDateRangeFilter>
    {
    }
}
