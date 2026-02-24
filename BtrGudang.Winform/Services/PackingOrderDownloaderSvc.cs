using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using BtrGudang.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BtrGudang.Winform.Services
{
    public class PackingOrderDownloaderSvc
    {
        private readonly RegistryHelper _registryHelper;
        private readonly BtradeCloudOptions _btradeCloudOpt;
        public PackingOrderDownloaderSvc(IOptions<BtradeCloudOptions> btradeCloudOpt)
        {
            _btradeCloudOpt = btradeCloudOpt.Value;
            _registryHelper = new RegistryHelper();
        }

        public async Task<(bool, string, DateTime, IEnumerable<PackingOrderModel>)> Execute(DateTime startTimestamp, string warehouseCode, int pageSize)
        {
            var serverTargetId = _registryHelper.ReadString("ServerTargetID");
            var baseUrl = _btradeCloudOpt.BaseUrl;
            var endpoint = $"{baseUrl}/api/PackingOrder/{{startTimestamp}}/{{warehouseCode}}/{{pageSize}}";
            var client = new RestClient(endpoint);

            var request = new RestRequest()
                .AddUrlSegment("startTimestamp", startTimestamp.ToString("yyyy-MM-dd HH:mm:ss"))
                .AddUrlSegment("warehouseCode", warehouseCode)
                .AddUrlSegment("pageSize", pageSize);
            var response = await client.ExecuteGetAsync(request);

            var defaultDate = new DateTime(3000,1,1);
            if (!response.IsSuccessful)
            {
                return (false, response.ErrorMessage ?? response.StatusDescription, defaultDate, null);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<PackingOrderDownloadResponse>>(response.Content, options);

            if (apiResponse == null)
            {
                return (false, "Failed to deserialize API response", defaultDate, null);
            }

            if (apiResponse.Status?.ToLower() != "success")
            {
                return (false, $"API returned non-success status: {apiResponse.Status}", defaultDate, null);
            }
            var respData = apiResponse.Data;
            var responseStr = "Downloaded Packing Order:\r";
            responseStr += string.Join("\r",
                respData.ListData.Select(x => $"{x.FakturCode} - {x.FakturDate:yyyy-MM-dd} - {x.CustomerName}"));
            var listPackingOrder = respData.ListData.Select(x => x.ToModel());
            var lastTimestamp = DateTime.ParseExact(respData.LastTimestamp, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            return (true, responseStr, lastTimestamp, listPackingOrder);
        }
    }

    public class PackingOrderDownloadCmd
    {
        public string StartTimestamp { get; set; }
        public string DepoId { get; set; }
        public int PageSize { get; set; }
    }

    public class PackingOrderDownloadResponse
    {
        public string LastTimestamp { get; set; }
        public IEnumerable<PackingOrderDownloadTrsResponse> ListData { get; set; }
    }

    public class PackingOrderDownloadTrsResponse
    {
        public string PackingOrderId {get; set;}
        public string PackingOrderDate {get;set;}
        public string CustomerId {get;set;}
        public string CustomerCode {get;set;}
        public string CustomerName {get;set;}
        public string Alamat {get;set;}
        public string NoTelp {get;set;}
        public double Latitude {get;set;}
        public double Longitude {get;set;}
        public double Accuracy {get;set;}
        public string FakturId {get;set;}
        public string FakturCode {get;set;}
        public string FakturDate {get;set;}
        public string AdminName {get;set;}
        public decimal GrandTotal { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string WarehouseDesc {get;set;}
        public string OfficeCode {get;set;}
        public string Note { get; set; }
        public IEnumerable<PackingOrderDownloadItemResponse> ListItem { get; set;  }

        public PackingOrderModel ToModel()
        {
            var packingOrderDate = DateTime.ParseExact(PackingOrderDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var customer = new CustomerReff(CustomerId, CustomerCode, CustomerName, Alamat, NoTelp);
            var location = new LocationReff(Latitude, Longitude, Accuracy);

            var fakturDate = DateTime.ParseExact(FakturDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var faktur = new FakturReff(FakturId, FakturCode, fakturDate, AdminName, GrandTotal);
            var driver = new DriverReff(DriverId, DriverName);

            var listItem = ListItem.Select(x => new PackingOrderItemModel(
                x.NoUrut, new BrgReff(x.BrgId, x.BrgCode, x.BrgNme, x.KategoriName, x.SupplierName),
                new Domain.QtyType(x.QtyBesar, x.SatBesar),
                new Domain.QtyType(x.QtyKecil, x.SatKecil),
                x.DepoId, string.Empty));

            var result = new PackingOrderModel(
                PackingOrderId, packingOrderDate, customer, location, faktur, driver,
                DateTime.Now, OfficeCode, string.Empty, DateTime.Now, string.Empty, Note,
                listItem);
            return result;
        }
    }

    public class PackingOrderDownloadItemResponse
    {
        public int NoUrut { get; set; }
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgNme { get; set; }
        public string KategoriName { get; set; }
        public string SupplierName { get; set; }
        public int QtyBesar { get; set; }
        public string SatBesar { get; set; }
        public int QtyKecil { get; set; }
        public string SatKecil { get; set; }
        public string DepoId { get; set; }
    }

    public class ApiResponse<T>
    {
        public string Status { get; set; }
        public string Code { get; set; }
        public T Data { get; set; }
    }
}