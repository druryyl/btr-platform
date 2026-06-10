namespace btr.application.SalesContext.SalesPersonPrincipalTargetAgg
{
    public class SalesPersonPrincipalTargetViewDto
    {
        public SalesPersonPrincipalTargetViewDto(
            string supplierId,
            string supplierCode,
            string supplierName,
            decimal targetAmount)
        {
            SupplierId = supplierId;
            SupplierCode = supplierCode;
            SupplierName = supplierName;
            TargetAmount = targetAmount;
        }

        public string SupplierId { get; }
        public string SupplierCode { get; }
        public string SupplierName { get; }
        public decimal TargetAmount { get; set; }
    }
}
