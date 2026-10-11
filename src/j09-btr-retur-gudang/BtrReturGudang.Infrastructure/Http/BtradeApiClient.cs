using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.Infrastructure.Helpers;
using RestSharp;

namespace BtrReturGudang.Infrastructure.Http
{
    /// <summary>
    /// RestSharp HTTP API client targeting j06-pkl-btrade-api relay service.
    /// Injects X-Session-Location (WarehouseCode) and X-Session-Actor (OperatorName) headers on all requests.
    /// Traceable to: ARCHITECTURE §4 TD-007, §5.1, §6, P2-S05.
    /// </summary>
    public class BtradeApiClient : IBtradeApiClient
    {
        public const string HeaderLocation = "X-Session-Location";
        public const string HeaderActor = "X-Session-Actor";
        public const string DefaultBaseUrl = "http://localhost:5000";

        private readonly IRestClient _client;
        private readonly Func<string> _warehouseCodeProvider;
        private readonly Func<string> _operatorNameProvider;
        private readonly string _baseUrl;

        public string BaseUrl => _baseUrl;

        /// <summary>
        /// Constructs client with explicit endpoint and session providers.
        /// </summary>
        public BtradeApiClient(
            string baseUrl = null,
            Func<string> warehouseCodeProvider = null,
            Func<string> operatorNameProvider = null,
            IRestClient client = null)
        {
            _baseUrl = !string.IsNullOrWhiteSpace(baseUrl)
                ? baseUrl
                : ConfigurationManager.AppSettings["BtradeApiBaseUrl"] ?? DefaultBaseUrl;

            _warehouseCodeProvider = warehouseCodeProvider
                ?? (() => RegistryHelper.ReadString(RegistryHelper.KeyWarehouseCode, string.Empty));

            _operatorNameProvider = operatorNameProvider
                ?? (() => RegistryHelper.ReadString(RegistryHelper.KeyLastOperatorName, string.Empty));

            if (client != null)
            {
                _client = client;
            }
            else
            {
                var options = new RestClientOptions(_baseUrl)
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };
                _client = new RestClient(options);
            }
        }

        /// <summary>
        /// Constructs client using IRegistryHelper for session header resolution.
        /// </summary>
        public BtradeApiClient(
            IRegistryHelper registryHelper,
            string baseUrl = null,
            IRestClient client = null)
            : this(
                baseUrl,
                () => registryHelper?.WarehouseCode ?? string.Empty,
                () => registryHelper?.LastOperatorName ?? string.Empty,
                client)
        {
        }

        /// <summary>
        /// Creates a RestRequest with session headers attached.
        /// </summary>
        public RestRequest CreateRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method);

            var location = _warehouseCodeProvider?.Invoke();
            if (!string.IsNullOrWhiteSpace(location))
            {
                request.AddHeader(HeaderLocation, location);
            }

            var actor = _operatorNameProvider?.Invoke();
            if (!string.IsNullOrWhiteSpace(actor))
            {
                request.AddHeader(HeaderActor, actor);
            }

            return request;
        }

        /// <summary>
        /// Uploads a completed Return Order to POST api/return-order.
        /// </summary>
        public async Task<bool> UploadReturnOrderAsync(ReturnOrderDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var request = CreateRequest("api/return-order", Method.Post);

            var warehouseCode = !string.IsNullOrWhiteSpace(dto.WarehouseCode)
                ? dto.WarehouseCode
                : _warehouseCodeProvider?.Invoke() ?? string.Empty;

            var payload = new
            {
                ReturnOrderId = dto.ReturnOrderId,
                ReturnOrderDate = dto.ReturnOrderDate.ToString("yyyy-MM-dd"),
                WarehouseCode = warehouseCode,
                CustomerId = dto.CustId ?? dto.CustomerId,
                CustomerName = dto.CustName ?? dto.CustomerName,
                SalesPersonId = dto.SalesPersonId,
                SalesPersonName = dto.SalesPersonName,
                DriverId = dto.DriverId,
                DriverName = dto.DriverName,
                Note = dto.Keterangan ?? dto.Note,
                ListItem = (dto.Items ?? new List<ReturnOrderItemDto>()).Select(item => new
                {
                    ReturnOrderId = dto.ReturnOrderId,
                    NoUrut = item.NoUrut,
                    BrgId = item.BrgId,
                    BrgCode = !string.IsNullOrWhiteSpace(item.BrgCode) ? item.BrgCode : item.BrgId,
                    BrgName = item.BrgName,
                    Qty = item.Qty,
                    SatId = item.SatId,
                    JenisRetur = item.JenisRetur ?? "BAGUS"
                }).ToList()
            };

            request.AddJsonBody(payload);

            var response = await _client.ExecuteAsync(request).ConfigureAwait(false);
            return response != null && (response.IsSuccessful || ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300));
        }

        /// <summary>
        /// Uploads an unmapped barcode registration request to POST api/barcode-registration.
        /// </summary>
        public async Task<bool> UploadBarcodeRegistrationAsync(BarcodeRegistrationDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var request = CreateRequest("api/barcode-registration", Method.Post);

            var payload = new
            {
                ClientRequestId = dto.RequestId ?? dto.ClientRequestId,
                BarcodeValue = dto.Barcode ?? dto.BarcodeValue,
                BrgId = dto.BrgId,
                Satuan = dto.Satuan
            };

            request.AddJsonBody(payload);

            var response = await _client.ExecuteAsync(request).ConfigureAwait(false);
            return response != null && (response.IsSuccessful || ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300));
        }

        /// <summary>
        /// Downloads master reference deltas: GET api/barcodes/sync, GET api/Customer/{serverId}, GET api/Brg/{serverId}.
        /// </summary>
        public async Task<MasterSyncDto> DownloadMasterDataAsync(string serverId)
        {
            var effectiveServerId = !string.IsNullOrWhiteSpace(serverId)
                ? serverId
                : "DEFAULT";

            var result = new MasterSyncDto
            {
                ServerId = effectiveServerId,
                Timestamp = DateTime.Now
            };

            // 1. Download active barcodes: GET api/barcodes/sync
            try
            {
                var barcodeRequest = CreateRequest("api/barcodes/sync", Method.Get);
                var barcodeResponse = await _client.ExecuteAsync(barcodeRequest).ConfigureAwait(false);
                if (barcodeResponse != null && barcodeResponse.IsSuccessful && !string.IsNullOrWhiteSpace(barcodeResponse.Content))
                {
                    result.Barcodes = ParseBarcodes(barcodeResponse.Content);
                }
            }
            catch
            {
                // Non-fatal; continue with other master tables
            }

            // 2. Download customer master: GET api/Customer/{serverId}
            try
            {
                var custRequest = CreateRequest($"api/Customer/{effectiveServerId}", Method.Get);
                var custResponse = await _client.ExecuteAsync(custRequest).ConfigureAwait(false);
                if (custResponse != null && custResponse.IsSuccessful && !string.IsNullOrWhiteSpace(custResponse.Content))
                {
                    result.Customers = ParseCustomers(custResponse.Content);
                }
            }
            catch
            {
            }

            // 3. Download product catalog: GET api/Brg/{serverId}
            try
            {
                var brgRequest = CreateRequest($"api/Brg/{effectiveServerId}", Method.Get);
                var brgResponse = await _client.ExecuteAsync(brgRequest).ConfigureAwait(false);
                if (brgResponse != null && brgResponse.IsSuccessful && !string.IsNullOrWhiteSpace(brgResponse.Content))
                {
                    result.Items = ParseBarang(brgResponse.Content);
                }
            }
            catch
            {
            }

            // 4. Download drivers: GET api/Driver/{serverId}
            try
            {
                var driverRequest = CreateRequest($"api/Driver/{effectiveServerId}", Method.Get);
                var driverResponse = await _client.ExecuteAsync(driverRequest).ConfigureAwait(false);
                if (driverResponse != null && driverResponse.IsSuccessful && !string.IsNullOrWhiteSpace(driverResponse.Content))
                {
                    result.Drivers = ParseDrivers(driverResponse.Content);
                }
            }
            catch
            {
            }

            // 5. Download salespersons: GET api/SalesPerson/{serverId}
            try
            {
                var spRequest = CreateRequest($"api/SalesPerson/{effectiveServerId}", Method.Get);
                var spResponse = await _client.ExecuteAsync(spRequest).ConfigureAwait(false);
                if (spResponse != null && spResponse.IsSuccessful && !string.IsNullOrWhiteSpace(spResponse.Content))
                {
                    result.SalesPersons = ParseSalesPersons(spResponse.Content);
                }
            }
            catch
            {
            }

            return result;
        }

        #region JSON Parsing Helpers

        private static JsonElement? ExtractDataArray(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                using (var doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement.Clone();

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        return root;
                    }

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("data", out var dataElem) || root.TryGetProperty("Data", out dataElem))
                        {
                            if (dataElem.ValueKind == JsonValueKind.Array)
                            {
                                return dataElem;
                            }
                        }
                    }

                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private static string GetStringProp(JsonElement elem, params string[] propNames)
        {
            foreach (var name in propNames)
            {
                if (elem.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String)
                {
                    return p.GetString();
                }
            }
            return string.Empty;
        }

        public static List<BarcodeDto> ParseBarcodes(string json)
        {
            var list = new List<BarcodeDto>();
            var dataArray = ExtractDataArray(json);
            if (!dataArray.HasValue) return list;

            foreach (var item in dataArray.Value.EnumerateArray())
            {
                var barcodeVal = GetStringProp(item, "barcodeValue", "BarcodeValue", "barcode", "Barcode");
                var brgId = GetStringProp(item, "brgId", "BrgId");
                var brgName = GetStringProp(item, "brgName", "BrgName");
                var satuan = GetStringProp(item, "satuan", "Satuan");

                if (!string.IsNullOrWhiteSpace(barcodeVal) && !string.IsNullOrWhiteSpace(brgId))
                {
                    list.Add(new BarcodeDto
                    {
                        BarcodeValue = barcodeVal,
                        BrgId = brgId,
                        BrgName = brgName,
                        Satuan = satuan,
                        Pengali = 1.0m,
                        IsActive = true
                    });
                }
            }

            return list;
        }

        public static List<CustomerDto> ParseCustomers(string json)
        {
            var list = new List<CustomerDto>();
            var dataArray = ExtractDataArray(json);
            if (!dataArray.HasValue) return list;

            foreach (var item in dataArray.Value.EnumerateArray())
            {
                var custId = GetStringProp(item, "customerId", "CustomerId", "custId", "CustId", "customerCode", "CustomerCode");
                var custName = GetStringProp(item, "customerName", "CustomerName", "custName", "CustName");
                var alamat = GetStringProp(item, "alamat", "Alamat");

                if (!string.IsNullOrWhiteSpace(custId))
                {
                    list.Add(new CustomerDto
                    {
                        CustId = custId,
                        CustName = custName,
                        Alamat = alamat
                    });
                }
            }

            return list;
        }

        public static List<BarangDto> ParseBarang(string json)
        {
            var list = new List<BarangDto>();
            var dataArray = ExtractDataArray(json);
            if (!dataArray.HasValue) return list;

            foreach (var item in dataArray.Value.EnumerateArray())
            {
                var brgId = GetStringProp(item, "brgId", "BrgId", "brgCode", "BrgCode");
                var brgName = GetStringProp(item, "brgName", "BrgName");
                var satBesar = GetStringProp(item, "satBesar", "SatBesar", "satuan1", "Satuan1");
                var satKecil = GetStringProp(item, "satKecil", "SatKecil", "satuan2", "Satuan2");
                var sat3 = GetStringProp(item, "satuan3", "Satuan3");

                if (!string.IsNullOrWhiteSpace(brgId))
                {
                    list.Add(new BarangDto
                    {
                        BrgId = brgId,
                        BrgName = brgName,
                        Satuan1 = satBesar,
                        Satuan2 = satKecil,
                        Satuan3 = sat3,
                        IsActive = true
                    });
                }
            }

            return list;
        }

        public static List<DriverDto> ParseDrivers(string json)
        {
            var list = new List<DriverDto>();
            var dataArray = ExtractDataArray(json);
            if (!dataArray.HasValue) return list;

            foreach (var item in dataArray.Value.EnumerateArray())
            {
                var driverId = GetStringProp(item, "driverId", "DriverId");
                var driverName = GetStringProp(item, "driverName", "DriverName");

                if (!string.IsNullOrWhiteSpace(driverId))
                {
                    list.Add(new DriverDto
                    {
                        DriverId = driverId,
                        DriverName = driverName
                    });
                }
            }

            return list;
        }

        public static List<SalesPersonDto> ParseSalesPersons(string json)
        {
            var list = new List<SalesPersonDto>();
            var dataArray = ExtractDataArray(json);
            if (!dataArray.HasValue) return list;

            foreach (var item in dataArray.Value.EnumerateArray())
            {
                var spId = GetStringProp(item, "salesPersonId", "SalesPersonId");
                var spName = GetStringProp(item, "salesPersonName", "SalesPersonName");

                if (!string.IsNullOrWhiteSpace(spId))
                {
                    list.Add(new SalesPersonDto
                    {
                        SalesPersonId = spId,
                        SalesPersonName = spName
                    });
                }
            }

            return list;
        }

        #endregion
    }
}
