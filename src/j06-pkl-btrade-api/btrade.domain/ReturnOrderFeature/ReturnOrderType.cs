using btrade.domain.SalesFeature;
using System.Collections.Generic;

namespace btrade.domain.ReturnOrderFeature;

public class ReturnOrderType : IReturnOrderKey, IServerId
{
    public ReturnOrderType()
    {
    }

    public ReturnOrderType(string returnOrderId, string serverId, string returnOrderDate,
        string warehouseCode, string customerId, string customerName,
        string salesPersonId, string salesPersonName, string driverId, string driverName,
        string note, string statusSync, string submittedBy)
    {
        ReturnOrderId = returnOrderId;
        ServerId = serverId;
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
        SubmittedBy = submittedBy;
        ListItems = new List<ReturnOrderItemType>();
    }

    public string ReturnOrderId { get; private set; }
    public string ServerId { get; private set; }
    public string ReturnOrderDate { get; private set; }
    public string WarehouseCode { get; private set; }
    public string CustomerId { get; private set; }
    public string CustomerName { get; private set; }
    public string SalesPersonId { get; private set; }
    public string SalesPersonName { get; private set; }
    public string DriverId { get; private set; }
    public string DriverName { get; private set; }
    public string Note { get; private set; }
    public string StatusSync { get; set; }

    /// <summary>
    /// TD-05/TD-15 — the resolved BTR <c>UserId</c> of the operator who
    /// submitted the return order. Carried in the incremental download so
    /// j07-btrade-sync can relay it into the Main Office return-order audit.
    /// </summary>
    public string SubmittedBy { get; private set; }

    public List<ReturnOrderItemType> ListItems { get; set; }

    public static IReturnOrderKey Key(string id) => new ReturnOrderType(id, "", "", "", "", "", "", "", "", "", "", "", "");
}

public interface IReturnOrderKey
{
    string ReturnOrderId { get; }
}
