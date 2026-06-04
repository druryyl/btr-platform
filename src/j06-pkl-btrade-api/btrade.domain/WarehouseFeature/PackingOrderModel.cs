using btrade.domain.SalesFeature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.domain.WarehouseFeature
{
    public class PackingOrderModel : IPackingOrderKey
    {
        private readonly List<PackingOrderItemModel> _listItem;
        private readonly List<PackingOrderDepoModel> _listDepo;

        public PackingOrderModel(string packingOrderId, DateTime packingOrderDate, 
            string customerId, string customerCode, string customerName, string alamat, string noTelp,
            double latitude, double longitude, double accuracy,
            string fakturId, string fakturCode, DateTime fakturDate, string adminName, decimal grandTotal,
            string driverId, string driverName, string officeCode, string note,
            IEnumerable<PackingOrderItemModel> listItem,
            IEnumerable<PackingOrderDepoModel> listDepo)
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
            OfficeCode = officeCode;
            Note = note;

            _listItem = listItem.ToList();
            _listDepo = listDepo.ToList();
            WarehouseDesc = string.Join(", ", _listDepo.Select(x => x.DepoId));
        }

        //  constructor ini untuk kepentingan DTO (baca dari database)
        public PackingOrderModel(string packingOrderId, DateTime packingOrderDate,
            string customerId, string customerCode, string customerName, string alamat, string noTelp,
            double latitude, double longitude, double accuracy,
            string fakturId, string fakturCode, DateTime fakturDate, string adminName, decimal grandTotal,
            string driverId, string driverName, string warehouseDesc, string officeCode, string note)
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
            Note = note;
            _listItem = new List<PackingOrderItemModel>();
            _listDepo = new List<PackingOrderDepoModel>();
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
        public string Note { get; private set; }

        public IEnumerable<PackingOrderItemModel> ListItem => _listItem;
        public IEnumerable<PackingOrderDepoModel> ListDepo => _listDepo;

        public void SetDownloadTimestamp(DateTime timestamp, string depoId)
        {
            foreach(var item in _listDepo)
            {
                if (item.DepoId == depoId)
                {
                    item.DownloadedAt(timestamp);
                    break;
                }
            }
        }
        public void SetListItem(IEnumerable<PackingOrderItemModel> listItem)
        {
            _listItem.Clear();
            _listItem.AddRange(listItem);
            var listDepo = _listItem
                .GroupBy(x => x.DepoId)
                .Select(g => new PackingOrderDepoModel(PackingOrderId, g.Key, DateTime.Now, new DateTime(3000,1,1)))
                .ToList();
            _listDepo.Clear();
            _listDepo.AddRange(listDepo);
        }
    }

    public interface IPackingOrderKey
    {
        string PackingOrderId { get; }
    }
}
