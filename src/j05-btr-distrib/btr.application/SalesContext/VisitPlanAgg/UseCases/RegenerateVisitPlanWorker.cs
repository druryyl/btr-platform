using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.SalesPersonAgg;
using btr.application.SalesContext.VisitPlanAgg.Services;
using btr.application.SupportContext.ParamSistemAgg;
using btr.application.SupportContext.TglJamAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.VisitPlanAgg;
using btr.domain.SupportContext.ParamSistemAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using NLog;

namespace btr.application.SalesContext.VisitPlanAgg.UseCases
{
    public class RegenerateVisitPlanWorker : IRegenerateVisitPlanWorker
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public const string HorizonDaysParamCode = "VISIT_PLAN_HORIZON_DAYS";

        private readonly IVisitPlanDal _visitPlanDal;
        private readonly ISalesRuteDal _salesRuteDal;
        private readonly ISalesRuteItemDal _salesRuteItemDal;
        private readonly IRuteCycleCalendar _calendar;
        private readonly IParamSistemDal _paramSistemDal;
        private readonly ITglJamDal _tglJamDal;

        public RegenerateVisitPlanWorker(
            IVisitPlanDal visitPlanDal,
            ISalesRuteDal salesRuteDal,
            ISalesRuteItemDal salesRuteItemDal,
            IRuteCycleCalendar calendar,
            IParamSistemDal paramSistemDal,
            ITglJamDal tglJamDal)
        {
            _visitPlanDal = visitPlanDal;
            _salesRuteDal = salesRuteDal;
            _salesRuteItemDal = salesRuteItemDal;
            _calendar = calendar;
            _paramSistemDal = paramSistemDal;
            _tglJamDal = tglJamDal;
        }

        public void Execute(RegenerateVisitPlanRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var today = _tglJamDal.Now.Date;
            var fromDate = request.FromDate.Date < today ? today : request.FromDate.Date;
            var toDate = request.ToDate?.Date ?? today.AddDays(GetHorizonDays());

            if (fromDate > toDate)
                return;

            var salesPersonIds = string.IsNullOrWhiteSpace(request.SalesPersonId)
                ? _visitPlanDal.ListSalesPersonIdsWithRoutes().ToList()
                : new List<string> { request.SalesPersonId };

            foreach (var salesPersonId in salesPersonIds)
            {
                try
                {
                    RegenerateForSalesPerson(salesPersonId, fromDate, toDate);
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        ex,
                        "Visit plan regeneration failed for SalesPersonId={SalesPersonId}, TriggeredBy={TriggeredBy}",
                        salesPersonId,
                        request.TriggeredBy);
                }
            }
        }

        private void RegenerateForSalesPerson(string salesPersonId, DateTime fromDate, DateTime toDate)
        {
            var routes = (_salesRuteDal.ListData(new SalesPersonModel(salesPersonId)) ?? Enumerable.Empty<SalesRuteModel>())
                .ToList();
            var routeItemsByHariRute = new Dictionary<string, List<SalesRuteItemModel>>(StringComparer.Ordinal);

            foreach (var route in routes)
            {
                var items = (_salesRuteItemDal.ListData(route) ?? Enumerable.Empty<SalesRuteItemModel>())
                    .OrderBy(x => x.NoUrut)
                    .ToList();
                routeItemsByHariRute[route.HariRuteId] = items;
            }

            var rows = new List<VisitPlanModel>();
            var materializedAt = _tglJamDal.Now;

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var hariRuteId = _calendar.ResolveHariRuteId(date);
                if (string.IsNullOrWhiteSpace(hariRuteId))
                    continue;

                if (!routeItemsByHariRute.TryGetValue(hariRuteId, out var items) || items.Count == 0)
                    continue;

                foreach (var item in items)
                {
                    rows.Add(new VisitPlanModel
                    {
                        SalesPersonId = salesPersonId,
                        VisitDate = date,
                        CustomerId = item.CustomerId,
                        NoUrut = item.NoUrut,
                        HariRuteId = hariRuteId,
                        PlanSource = "Template",
                        MaterializedAt = materializedAt
                    });
                }
            }

            using (var trans = TransHelper.NewScope())
            {
                _visitPlanDal.DeleteFuture(salesPersonId, fromDate, toDate);
                if (rows.Count > 0)
                    _visitPlanDal.BulkInsert(rows);
                trans.Complete();
            }
        }

        private int GetHorizonDays()
        {
            var param = _paramSistemDal.GetData(new ParamSistemModel(HorizonDaysParamCode));
            if (param == null || !int.TryParse(param.ParamValue, out var days) || days <= 0)
                return 90;

            return days;
        }
    }
}
