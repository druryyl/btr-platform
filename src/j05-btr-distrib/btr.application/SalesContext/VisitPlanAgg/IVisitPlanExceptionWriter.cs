using btr.domain.SalesContext.VisitPlanAgg;

namespace btr.application.SalesContext.VisitPlanAgg
{
    public interface IVisitPlanExceptionWriter
    {
        VisitPlanExceptionModel Save(VisitPlanExceptionModel model);

        void Delete(IVisitPlanExceptionKey key);
    }
}
