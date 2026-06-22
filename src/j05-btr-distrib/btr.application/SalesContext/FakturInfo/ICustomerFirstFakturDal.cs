using System;
using System.Collections.Generic;

namespace btr.application.SalesContext.FakturInfo
{
    public class CustomerFirstFakturDto
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public DateTime FirstFakturDate { get; set; }
    }

    public interface ICustomerFirstFakturDal
    {
        IEnumerable<CustomerFirstFakturDto> ListFirstFakturByCustomer();
    }
}
