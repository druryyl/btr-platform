using btr.nuna.Application;

namespace btr.application.SalesContext.VisitPlanAgg.UseCases
{
    public class MaintainVisitPlanHorizonRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";
    }

    public interface IMaintainVisitPlanHorizonWorker : INunaServiceVoid<MaintainVisitPlanHorizonRequest>
    {
    }
}
