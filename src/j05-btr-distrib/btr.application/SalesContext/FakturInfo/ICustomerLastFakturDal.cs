using System;
using System.Collections.Generic;

namespace btr.application.SalesContext.FakturInfo
{
    public interface ICustomerLastFakturDal
    {
        IEnumerable<CustomerLastFakturDto> ListLastFakturByCustomer();
    }

    public class CustomerLastFakturDto
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public DateTime LastFakturDate { get; set; }
    }
}
