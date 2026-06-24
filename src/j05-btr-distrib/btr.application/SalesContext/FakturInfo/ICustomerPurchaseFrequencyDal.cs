using System;
using System.Collections.Generic;

namespace btr.application.SalesContext.FakturInfo
{
    public class CustomerPurchaseFrequencyDto
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public int FakturCount { get; set; }
    }

    public interface ICustomerPurchaseFrequencyDal
    {
        IEnumerable<CustomerPurchaseFrequencyDto> ListFakturCountByCustomer(DateTime from, DateTime to);
    }
}
