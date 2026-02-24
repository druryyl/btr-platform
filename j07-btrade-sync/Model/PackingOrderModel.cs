using btr.domain.InventoryContext.PackingOrderFeature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Model
{
    public class PackingOrderModel : IPackingOrderKey
    {
        private readonly List<PackingOrderItemModel> _listItem;

        public PackingOrderModel(string packingOrderId, DateTime packingOrderDate, 
            string customerId, string customerCode, string customerName, string alamat, string noTelp,
            double latitude, double longitude, double accuracy,
            string fakturId, string fakturCode, DateTime fakturDate, string adminName, decimal grandTotal,
            string driverId, string driverName, DateTime uploadTimestamp, string note,
            List<PackingOrderItemModel> listItem)
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
            UploadTimestamp = uploadTimestamp;
            Note = note;
            _listItem = listItem;
        }
        public PackingOrderModel(string packingOrderId, DateTime packingOrderDate,
            string customerId, string customerCode, string customerName, string alamat, string noTelp,
            double latitude, double longitude, double accuracy,
            string fakturId, string fakturCode, DateTime fakturDate, string adminName, decimal grandTotal,
            string driverId, string driverName, DateTime uploadTimestamp, string note)
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
            UploadTimestamp = uploadTimestamp;
            Note = note;
            _listItem = new List<PackingOrderItemModel>();
        }
        public static PackingOrderModel Default => new PackingOrderModel(
            packingOrderId: string.Empty,
            packingOrderDate: DateTime.MinValue,
            customerId: string.Empty,
            customerCode: string.Empty,
            customerName: string.Empty,
            alamat: string.Empty,
            noTelp: string.Empty,
            latitude: 0,
            longitude: 0,
            accuracy: 0,
            fakturId: string.Empty,
            fakturCode: string.Empty,
            fakturDate: DateTime.MinValue,
            adminName: string.Empty,
            grandTotal: 0,
            driverId : string.Empty,
            driverName: string.Empty,
            uploadTimestamp: new DateTime(3000,1,1),
            note: string.Empty,
            listItem: new List<PackingOrderItemModel>());

        public static IPackingOrderKey Key(string id)
        {
            var result = Default;
            result.PackingOrderId = id;
            return result;
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

        public DateTime UploadTimestamp { get; private set; }
        public string Note { get; private set; }

        public void Upload(DateTime uploadTimestamp)
        {
            UploadTimestamp = uploadTimestamp;
        }
        public void SetListItem(IEnumerable<PackingOrderItemModel> listItem)
        {
            _listItem.Clear();
            _listItem.AddRange(listItem.ToList());
        }
        public IEnumerable<PackingOrderItemModel> ListItem => _listItem;
    }

    public interface IPackingOrderKey
    {
        string PackingOrderId { get; }
    }

    public interface IFakturKey
    {
        string FakturId { get; }
    }
}
