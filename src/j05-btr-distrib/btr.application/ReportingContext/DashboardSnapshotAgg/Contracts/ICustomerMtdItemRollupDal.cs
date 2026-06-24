using System.Collections.Generic;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface ICustomerMtdItemRollupDal
    {
        IEnumerable<CustomerMtdItemRollupDto> ListMtdItemRollups(Periode periode);
    }

    public class CustomerMtdItemRollupDto
    {
        public string CustomerCode { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string SupplierId { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }

        public decimal LineTotal { get; set; }
    }
}
