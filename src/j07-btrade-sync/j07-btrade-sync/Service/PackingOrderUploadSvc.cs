using j07_btrade_sync.Model;
using j07_btrade_sync.Repository;
using j07_btrade_sync.Shared;
using Nuna.Lib.TransactionHelper;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Service
{
    public class PackingOrderUploadSvc
    {
        private readonly RegistryHelper _registryHelper;
        private readonly PackingOrderDal _packingOrderDal;
        public PackingOrderUploadSvc()
        {
            _registryHelper = new RegistryHelper();
            _packingOrderDal = new PackingOrderDal();
        }
        public async Task<(bool, string)> UploadPackingOrder(IEnumerable<PackingOrderModel> enumPackingOrder)
        {
            //  BUILD
            var serverTargetId = _registryHelper.ReadString("ServerTargetID");
            var baseUrl = System.Configuration.ConfigurationManager.AppSettings["btrade-cloud-base-url"];
            var endpoint = $"{baseUrl}/api/PackingOrder/bulk";
            var client = new RestClient(endpoint);
            var listPackingOrder = enumPackingOrder.ToList();
            var uploadDto = listPackingOrder
                .Select(x => PackingOrderUploadDto.FromModel(x, _registryHelper.ReadString("OfficeCode")))
                .ToList();
            var request = new PackingOrderRequest
            {
                ListPackingOrder = uploadDto
            };
            var requestBody = System.Text.Json.JsonSerializer.Serialize(request);
            var req = new RestRequest()
                .AddJsonBody(requestBody)
                .AddHeader("Content-Type", "application/json");
            //  EXECUTE
            var response = await client.ExecutePostAsync(req);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return (false, response.ErrorMessage);
            }
            else
            {
                using (var trans = TransHelper.NewScope())
                {
                    foreach (var item in listPackingOrder)
                    {
                        item.Upload(DateTime.Now);
                        _packingOrderDal.Update(item);
                    }
                    trans.Complete();
                }
                var responseStr = string.Join("\r", listPackingOrder.Select(x => $"{x.FakturCode} - {x.CustomerName}"));
                return (true, responseStr);
            }
        }
    }
    public class PackingOrderRequest
    {
        public List<PackingOrderUploadDto> ListPackingOrder { get; set; }
    }
    public class PackingOrderUploadDto
    {
        public PackingOrderUploadDto(
            string packingOrderId, string packingOrderDate,
            string customerId, string customerCode, string customerName, string alamat, string noTelp, 
            double latitude, double longitude, double accuracy,
            string fakturId, string fakturCode, string fakturDate, string adminName, decimal grandTotal,
            string driverId, string driverName, string officeCode, string note,
            IEnumerable<PackingOrderUploadItemDto> listItem)
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
            ListItem = listItem;
        }

        public static PackingOrderUploadDto FromModel(PackingOrderModel model, string officeCode)
        {
            var listItem = model.ListItem.Select(x => new PackingOrderUploadItemDto(x.NoUrut, x.BrgId, x.BrgCode, x.BrgName,
                x.KategoriName, x.SupplierName, x.QtyBesar, x.SatBesar, x.QtyKecil,
                x.SatKecil, x.DepoId)).ToList();
            var fakturDate = model.FakturDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            return new PackingOrderUploadDto(model.PackingOrderId, model.PackingOrderDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                model.CustomerId, model.CustomerCode, model.CustomerName, model.Alamat, model.NoTelp,
                model.Latitude, model.Longitude, model.Accuracy,
                model.FakturId, model.FakturCode, fakturDate, model.AdminName, model.GrandTotal, 
                model.DriverId, model.DriverName, officeCode, model.Note, listItem);
        }

        public string PackingOrderId { get; }
        public string PackingOrderDate { get; }
        public string CustomerId { get; }
        public string CustomerCode { get; }
        public string CustomerName { get; }
        public string Alamat { get; }
        public string NoTelp { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public double Accuracy { get; }
        public string FakturId { get; }
        public string FakturCode { get; }
        public string FakturDate { get; }
        public string AdminName { get; }
        public decimal GrandTotal { get; }
        public string DriverId { get; }
        public string DriverName { get; }
        public string OfficeCode { get; }
        public string Note { get; }
        public IEnumerable<PackingOrderUploadItemDto> ListItem { get; }
    }

    public class PackingOrderUploadItemDto
    {
        public PackingOrderUploadItemDto(
            int noUrut, string brgId, string brgCode, string brgNme,
            string kategoriName, string supplierName,
            int qtyBesar, string satBesar, int qtyKecil,
            string satKecil, string depoId)
        {
            NoUrut = noUrut;
            BrgId = brgId;
            BrgCode = brgCode;
            BrgNme = brgNme;
            KategoriName = kategoriName;
            SupplierName = supplierName;
            QtyBesar = qtyBesar;
            SatBesar = satBesar;
            QtyKecil = qtyKecil;
            SatKecil = satKecil;
            DepoId = depoId;
        }

        public int NoUrut { get; }
        public string BrgId { get; }
        public string BrgCode { get; }
        public string BrgNme { get; }
        public string KategoriName { get; }
        public string SupplierName { get; }
        public int QtyBesar { get; }
        public string SatBesar { get; }
        public int QtyKecil { get; }
        public string SatKecil { get; }
        public string DepoId { get; }
    }
}
