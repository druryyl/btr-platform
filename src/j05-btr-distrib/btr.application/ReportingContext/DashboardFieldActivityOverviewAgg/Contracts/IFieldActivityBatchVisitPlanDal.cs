using System;
using System.Collections.Generic;
using btr.domain.SalesContext.VisitPlanAgg;

namespace btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts
{
    public interface IFieldActivityBatchVisitPlanDal
    {
        FieldActivityVisitPlanBatchResult ListByDate(DateTime visitDate);
    }

    public class FieldActivityVisitPlanBatchResult
    {
        public IReadOnlyList<VisitPlanModel> BasePlans { get; set; } = new List<VisitPlanModel>();
        public IReadOnlyList<VisitPlanExceptionModel> Exceptions { get; set; } = new List<VisitPlanExceptionModel>();
    }
}
