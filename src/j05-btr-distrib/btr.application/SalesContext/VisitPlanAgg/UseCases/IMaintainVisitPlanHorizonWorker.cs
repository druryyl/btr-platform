using System;
using btr.nuna.Application;

namespace btr.application.SalesContext.VisitPlanAgg.UseCases
{
    public class MaintainVisitPlanHorizonRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool AllowPast { get; set; }
    }

    public interface IMaintainVisitPlanHorizonWorker : INunaServiceVoid<MaintainVisitPlanHorizonRequest>
    {
    }
}
