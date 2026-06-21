using System;
using System.Collections.Generic;

namespace btr.application.FinanceContext.PiutangAgg.Contracts
{
    public interface ICustomerPaymentBehaviorDal
    {
        IEnumerable<CustomerPaymentBehaviorDto> ListPaymentBehavior(
            DateTime windowStart,
            DateTime windowEnd,
            int minSettledFakturs);
    }

    public class CustomerPaymentBehaviorDto
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public int SettledFakturCount { get; set; }

        public decimal? AvgPaymentLagDays { get; set; }
    }
}
