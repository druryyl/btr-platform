using System;

namespace btr.distrib.InventoryContext.ReturnOrderAgg
{
    public class GenerateReturnOrderFormOrderDto
    {
        public GenerateReturnOrderFormOrderDto()
        {
        }

        public GenerateReturnOrderFormOrderDto(string returnOrderId, string returnOrderNo,
            DateTime returnOrderDate, string customerId, string customerName,
            string warehouseCode, string warehouseName, string salesPersonId,
            string salesPersonName, string driverId, string driverName, string status)
        {
            ReturnOrderId = returnOrderId;
            ReturnOrderNo = returnOrderNo;
            ReturnOrderDate = returnOrderDate;
            CustomerId = customerId;
            CustomerName = customerName;
            WarehouseCode = warehouseCode;
            WarehouseName = warehouseName;
            SalesPersonId = salesPersonId;
            SalesPersonName = salesPersonName;
            DriverId = driverId;
            DriverName = driverName;
            Status = status;
        }

        public bool Pilih { get; set; }
        public string ReturnOrderId { get; set; }
        public string ReturnOrderNo { get; set; }
        public DateTime ReturnOrderDate { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string Status { get; set; }
    }
}
