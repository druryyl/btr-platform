using System;
using System.Collections.Generic;
using btr.domain.SalesContext.VisitPlanAgg;

namespace btr.application.SalesContext.VisitPlanAgg
{
    public class VisitPlanDateFilter : IVisitPlanDateKey
    {
        public VisitPlanDateFilter(string salesPersonId, DateTime visitDate)
        {
            SalesPersonId = salesPersonId;
            VisitDate = visitDate.Date;
        }

        public string SalesPersonId { get; }
        public DateTime VisitDate { get; }
    }

    public class VisitPlanDateRangeFilter
    {
        public VisitPlanDateRangeFilter(string salesPersonId, DateTime fromDate, DateTime toDate)
        {
            SalesPersonId = salesPersonId;
            FromDate = fromDate.Date;
            ToDate = toDate.Date;
        }

        public string SalesPersonId { get; }
        public DateTime FromDate { get; }
        public DateTime ToDate { get; }
    }

    public interface IVisitPlanDal
    {
        IEnumerable<VisitPlanModel> ListData(IVisitPlanDateKey filter);
        IEnumerable<VisitPlanModel> ListData(VisitPlanDateRangeFilter filter);
        IEnumerable<string> ListSalesPersonIdsWithRoutes();
        void DeleteFuture(string salesPersonId, DateTime fromDate, DateTime toDate);
        void BulkInsert(IEnumerable<VisitPlanModel> rows);
    }
}
