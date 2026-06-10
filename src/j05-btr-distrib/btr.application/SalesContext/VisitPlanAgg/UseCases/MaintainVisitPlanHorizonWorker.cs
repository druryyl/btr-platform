using btr.application.SupportContext.TglJamAgg;

namespace btr.application.SalesContext.VisitPlanAgg.UseCases
{
    public class MaintainVisitPlanHorizonWorker : IMaintainVisitPlanHorizonWorker
    {
        private readonly IRegenerateVisitPlanWorker _regenerateWorker;
        private readonly ITglJamDal _tglJamDal;

        public MaintainVisitPlanHorizonWorker(
            IRegenerateVisitPlanWorker regenerateWorker,
            ITglJamDal tglJamDal)
        {
            _regenerateWorker = regenerateWorker;
            _tglJamDal = tglJamDal;
        }

        public void Execute(MaintainVisitPlanHorizonRequest request)
        {
            var today = _tglJamDal.Now.Date;
            _regenerateWorker.Execute(new RegenerateVisitPlanRequest
            {
                SalesPersonId = null,
                FromDate = today,
                ToDate = null,
                TriggeredBy = request?.TriggeredBy ?? "Scheduler"
            });
        }
    }
}
