using System;
using System.Collections.Generic;

namespace btr.domain.InventoryContext.ReturnOrderAgg
{
    public class ReturnOrderModel : IReturnOrderKey
    {
        public ReturnOrderModel(string id) => ReturnOrderId = id;

        public ReturnOrderModel()
        {
        }

        public string ReturnOrderId { get; set; }
        public string ReturnOrderNo { get; set; }
        public DateTime ReturnOrderDate { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public List<ReturnOrderItemModel> ListItem { get; set; }
    }

    public interface IReturnOrderKey
    {
        string ReturnOrderId { get; }
    }
}
