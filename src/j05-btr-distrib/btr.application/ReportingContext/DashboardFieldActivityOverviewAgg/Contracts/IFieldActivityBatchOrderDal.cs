using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardFieldActivityOverviewAgg.Contracts
{
    public interface IFieldActivityBatchOrderDal
    {
        IReadOnlyList<FieldActivityOrderBatchRow> ListByDate(DateTime visitDate);
    }

    public class FieldActivityOrderBatchRow
    {
        public string UserEmail { get; set; }
        public string CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
