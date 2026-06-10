namespace btr.application.SalesContext.SalesPersonSupplierAgg
{
    public class SalesPersonSupplierViewDto
    {
        public SalesPersonSupplierViewDto(string supplierId, string supplierCode, string supplierName)
        {
            SupplierId = supplierId;
            SupplierCode = supplierCode;
            SupplierName = supplierName;
        }

        public string SupplierId { get; }
        public string SupplierCode { get; }
        public string SupplierName { get; }
    }
}
