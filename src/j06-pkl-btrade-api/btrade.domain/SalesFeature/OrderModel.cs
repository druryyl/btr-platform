using btrade.domain.SalesFeature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.domain.SalesFeature;

public  class OrderModel : IOrderKey, IServerId
{
    public OrderModel()
    {
    }
    public OrderModel(string orderId, string orderLocalCode, 
        string customerId, string customerCode, string customerName, string customerAddress, 
        string orderDate, string salesId, string salesName, decimal totalAmount, 
        string userEmail, string statusSync, string fakturCode, string orderNote,
        string serverId)
    {
        OrderId = orderId;
        OrderLocalId = orderLocalCode;
        CustomerId = customerId;
        CustomerCode = customerCode;
        CustomerName = customerName;
        CustomerAddress = customerAddress;
        OrderDate = orderDate;
        SalesId = salesId;
        SalesName = salesName;
        TotalAmount = totalAmount;
        UserEmail = userEmail;
        StatusSync = statusSync;
        FakturCode = fakturCode;
        OrderNote = orderNote;
        ServerId = serverId;
        ListItems = new List<OrderItemType>();
    }

    public string OrderId {get; private set;}
    public string OrderLocalId { get; private set; }
    public string CustomerId { get; private set; }
    public string CustomerCode { get; private set; }
    public string CustomerName { get; private set; }
    public string CustomerAddress { get; private set; }
    
    public string OrderDate { get; private set; }
    public string SalesId { get; private set; }
    public string SalesName { get; private set; }
    public decimal TotalAmount { get; private set; }
    
    public string UserEmail { get; private set; }
    public string StatusSync { get; set; }

    public string FakturCode { get; private set; }
    public string OrderNote { get; private set; }
    public string ServerId { get; private set; }

    public List<OrderItemType> ListItems { get; set; }

    public static IOrderKey Key(string id) => new OrderModel(id, "", "", "", "", "", "", "", "", 0, "", "", "", "", "");
}

public interface IOrderKey
{
    string OrderId { get; }
}