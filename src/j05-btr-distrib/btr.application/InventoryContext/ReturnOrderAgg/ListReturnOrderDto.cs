using System;

namespace btr.application.InventoryContext.ReturnOrderAgg
{
    public class ListReturnOrderDto
    {
        public ListReturnOrderDto()
        {
            ReturnOrderId = string.Empty;
            ReturnOrderNo = string.Empty;
            CustomerId = string.Empty;
            CustomerName = string.Empty;
            WarehouseCode = string.Empty;
            WarehouseName = string.Empty;
            SalesPersonId = string.Empty;
            SalesPersonName = string.Empty;
            DriverId = string.Empty;
            DriverName = string.Empty;
            Status = string.Empty;
            ReturJualBagusCode = string.Empty;
            ReturJualRusakCode = string.Empty;
            Note = string.Empty;
        }

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
        public int TotalItem { get; set; }
        public string ReturJualBagusCode { get; set; }
        public string ReturJualRusakCode { get; set; }
        public string Note { get; set; }
    }
}
