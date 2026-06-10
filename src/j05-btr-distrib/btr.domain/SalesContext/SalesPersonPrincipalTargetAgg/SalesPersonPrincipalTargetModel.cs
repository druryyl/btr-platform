using System;

namespace btr.domain.SalesContext.SalesPersonPrincipalTargetAgg
{
    public class SalesPersonPrincipalTargetModel
    {
        public string SalesPersonId { get; set; }
        public string SupplierId { get; set; }
        public int TargetYear { get; set; }
        public int TargetMonth { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
    }
}
