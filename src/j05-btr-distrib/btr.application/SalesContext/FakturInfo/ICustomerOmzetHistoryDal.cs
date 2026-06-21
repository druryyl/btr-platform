using System.Collections.Generic;
using btr.nuna.Domain;

namespace btr.application.SalesContext.FakturInfo
{
    public interface ICustomerOmzetHistoryDal
    {
        IEnumerable<CustomerOmzetHistoryDto> ListOmzetByCustomer(Periode currentMonth, Periode priorMonth);
    }

    public class CustomerOmzetHistoryDto
    {
        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal CurrentMonthOmzet { get; set; }

        public decimal PriorMonthOmzet { get; set; }
    }
}
