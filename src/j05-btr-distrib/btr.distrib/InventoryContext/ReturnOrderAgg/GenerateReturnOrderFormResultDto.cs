namespace btr.distrib.InventoryContext.ReturnOrderAgg
{
    public class GenerateReturnOrderFormResultDto
    {
        public GenerateReturnOrderFormResultDto()
        {
        }

        public GenerateReturnOrderFormResultDto(string returJualId, string returJualCode,
            string customerId, string jenisRetur, string salesPersonId, string driverId)
        {
            ReturJualId = returJualId;
            ReturJualCode = returJualCode;
            CustomerId = customerId;
            JenisRetur = jenisRetur;
            SalesPersonId = salesPersonId;
            DriverId = driverId;
        }

        public string ReturJualId { get; set; }
        public string ReturJualCode { get; set; }
        public string CustomerId { get; set; }
        public string JenisRetur { get; set; }
        public string SalesPersonId { get; set; }
        public string DriverId { get; set; }
    }
}
