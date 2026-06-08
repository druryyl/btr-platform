using System;
using System.Collections.Generic;

namespace btr.application.SalesContext.FakturInfo
{
    public interface ICustomerLastFakturDal
    {
        IEnumerable<CustomerLastFakturDto> ListLastFakturByCustomer();

        IEnumerable<CustomerLastFakturWithSalesmanDto> ListLastFakturWithSalesmanByCustomer();
    }

    public class CustomerLastFakturDto
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public DateTime LastFakturDate { get; set; }
    }

    public class CustomerLastFakturWithSalesmanDto
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public DateTime LastFakturDate { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonName { get; set; }
    }
}
