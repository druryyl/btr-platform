using btr.application.SalesContext.SalesPersonAgg;
using btr.application.SalesContext.VisitPlanAgg.UseCases;
using btr.application.SupportContext.TglJamAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.nuna.Application;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btr.application.SalesContext.SalesRuteAgg
{
    public interface ISalesRuteWriter : INunaWriter2<SalesRuteModel>
    {
    }
    public class SalesRuteWriter : ISalesRuteWriter
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly ISalesRuteDal _salesRuteDal;
        private readonly ISalesRuteItemDal _salesRuteItemDal;
        private readonly INunaCounterBL _counter;
        private readonly IRegenerateVisitPlanWorker _regenerateVisitPlanWorker;
        private readonly ITglJamDal _tglJamDal;

        public SalesRuteWriter(ISalesRuteDal salesRuteDal, 
            ISalesRuteItemDal salesRuteItemDal, 
            INunaCounterBL counter,
            IRegenerateVisitPlanWorker regenerateVisitPlanWorker,
            ITglJamDal tglJamDal)
        {
            _salesRuteDal = salesRuteDal;
            _salesRuteItemDal = salesRuteItemDal;
            _counter = counter;
            _regenerateVisitPlanWorker = regenerateVisitPlanWorker;
            _tglJamDal = tglJamDal;
        }

        public SalesRuteModel Save(SalesRuteModel model)
        {
            if (model.SalesRuteId == string.Empty)
                model.SalesRuteId = _counter.Generate("RT", IDFormatEnum.PFnnn);
            model.ListCustomer.ForEach(x => x.SalesRuteId = model.SalesRuteId);

            using (var trans = TransHelper.NewScope())
            {
                var db = _salesRuteDal.GetData(model);
                if (db is null)
                    _salesRuteDal.Insert(model);
                else
                    _salesRuteDal.Update(model);

                _salesRuteItemDal.Delete(model);
                _salesRuteItemDal.Insert(model.ListCustomer);
                trans.Complete();
            }

            TryRegenerateVisitPlan(model.SalesPersonId);

            return model;
        }

        private void TryRegenerateVisitPlan(string salesPersonId)
        {
            try
            {
                _regenerateVisitPlanWorker.Execute(new RegenerateVisitPlanRequest
                {
                    SalesPersonId = salesPersonId,
                    FromDate = _tglJamDal.Now.Date,
                    ToDate = null,
                    TriggeredBy = "TemplateSave"
                });
            }
            catch (Exception ex)
            {
                Logger.Error(
                    ex,
                    "Visit plan regeneration failed after template save for SalesPersonId={SalesPersonId}",
                    salesPersonId);
            }
        }
    }
}
