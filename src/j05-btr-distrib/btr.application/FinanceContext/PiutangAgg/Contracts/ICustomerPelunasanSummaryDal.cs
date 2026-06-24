using System;
using System.Collections.Generic;

namespace btr.application.FinanceContext.PiutangAgg.Contracts
{
    public interface ICustomerPelunasanSummaryDal
    {
        IEnumerable<CustomerPelunasanSummaryDto> ListSummary(DateTime windowStart, DateTime windowEnd);
    }

    public class CustomerPelunasanSummaryDto
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public DateTime? LastPaymentDate { get; set; }

        public decimal TotalCash { get; set; }

        public decimal TotalSettlement { get; set; }

        public int PaymentCount { get; set; }
    }
}
