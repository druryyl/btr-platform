using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Model
{
    public class ReturnOrderModel : IReturnOrderKey
    {
        public ReturnOrderModel()
        {
        }

        public ReturnOrderModel(string returnOrderId, string returnOrderDate,
            string warehouseCode, string customerId, string customerName,
            string salesPersonId, string salesPersonName,
            string driverId, string driverName,
            string note, string statusSync)
        {
            ReturnOrderId = returnOrderId;
            ReturnOrderDate = returnOrderDate;
            WarehouseCode = warehouseCode;
            CustomerId = customerId;
            CustomerName = customerName;
            SalesPersonId = salesPersonId;
            SalesPersonName = salesPersonName;
            DriverId = driverId;
            DriverName = driverName;
            Note = note;
            StatusSync = statusSync;
            ListItems = new List<ReturnOrderItemType>();
        }

        public string ReturnOrderId { get; set; }
        public string ReturnOrderDate { get; set; }
        public string WarehouseCode { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string Note { get; set; }
        public string StatusSync { get; set; }

        //  TD-15 — the Cloud-resolved BTR UserId of the operator who submitted
        //  the return order. Read case-insensitively from the incremental
        //  download response (PascalCase per §10; the download service sets
        //  PropertyNameCaseInsensitive) and relayed to the Main Office import as
        //  the office audit identity (BTR_ReturnOrder.CreatedBy). Empty for
        //  legacy Cloud rows that carry no attribution.
        public string SubmittedBy { get; set; }

        public List<ReturnOrderItemType> ListItems { get; set; }

        public static IReturnOrderKey Key(string id) => new ReturnOrderModel(id, "", "", "", "", "", "", "", "", "", "");
    }

    public interface IReturnOrderKey
    {
        string ReturnOrderId { get; }
    }
}
