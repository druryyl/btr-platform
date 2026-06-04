using btrade.domain.WarehouseFeature;
using Nuna.Lib.DataAccessHelper;
using Nuna.Lib.ValidationHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.application.WarehouseFreature
{
    public interface IPackingOrderDal : 
        IInsert<PackingOrderModel>,
        IUpdate<PackingOrderModel>,
        IDelete<IPackingOrderKey>,
        IGetData<PackingOrderModel, IPackingOrderKey>,
        IListData<PackingOrderView, DateTime, string>
    {
    }

    public class PackingOrderView : IPackingOrderKey
    {
        public PackingOrderView(string packingOrderId, DateTime packingOrderDate, 
            string customerId, string customerCode, string customerName, string alamat, string noTelp,
            double latitude, double longitude, double accuracy, 
            string fakturId, string fakturCode, DateTime fakturDate, string adminName, decimal grandTotal,
            string driverId, string driverName, string warehouseDesc, string officeCode, DateTime updateTimestamp, string note)
        {
            PackingOrderId = packingOrderId;
            PackingOrderDate = packingOrderDate;
            CustomerId = customerId;
            CustomerCode = customerCode;
            CustomerName = customerName;
            Alamat = alamat;
            NoTelp = noTelp;
            Latitude = latitude;
            Longitude = longitude;
            Accuracy = accuracy;
            FakturId = fakturId;
            FakturCode = fakturCode;
            FakturDate = fakturDate;
            AdminName = adminName;
            GrandTotal = grandTotal;

            DriverId = driverId;
            DriverName = driverName;
            WarehouseDesc = warehouseDesc;
            OfficeCode = officeCode;
            UpdateTimestamp = updateTimestamp;
            Note = note;

        }

        public string PackingOrderId { get; private set; }
        public DateTime PackingOrderDate { get; private set; }

        public string CustomerId { get; private set; }
        public string CustomerCode { get; private set; }
        public string CustomerName { get; private set; }
        public string Alamat { get; private set; }
        public string NoTelp { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double Accuracy { get; private set; }

        public string FakturId { get; private set; }
        public string FakturCode { get; private set; }
        public DateTime FakturDate { get; private set; }
        public string AdminName { get; private set; }
        public decimal GrandTotal { get; private set; }

        public string DriverId { get; private set; }
        public string DriverName { get; private set; }
        public string WarehouseDesc { get; private set; }
        public string OfficeCode { get; private set; }
        public DateTime UpdateTimestamp { get; private set; }
        public string Note { get; private set; }
    }
}
