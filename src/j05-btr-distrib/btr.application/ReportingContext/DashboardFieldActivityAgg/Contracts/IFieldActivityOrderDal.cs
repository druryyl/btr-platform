using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardFieldActivityAgg.Contracts
{
    public interface IFieldActivityOrderDal
    {
        ISet<string> ListCustomerIdsWithOrder(
            string salesPersonEmail, DateTime visitDate);
    }
}
