using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Contracts;

namespace btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts
{
    public interface IFieldActivityBatchCheckInDal
    {
        IReadOnlyList<FieldActivityBatchCheckInRow> ListByDate(DateTime visitDate);
    }

    public class FieldActivityBatchCheckInRow : FieldActivityCheckInRow
    {
        public string UserEmail { get; set; }
    }
}
