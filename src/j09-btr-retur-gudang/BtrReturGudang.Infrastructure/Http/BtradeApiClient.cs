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
    /// Injects X-Session-Location (WarehouseCode) and X-Session-Actor (registered Google email) headers on all requests.
    /// Traceable to: ARCHITECTURE §4 TD-007, TD-012, §5.1, §6, P2-S05.
    /// </summary>
    public class BtradeApiClient : IBtradeApiClient
    {
        public const string HeaderLocation = "X-Session-Location";
        public const string HeaderActor = "X-Session-Actor";
        public const string DefaultBaseUrl = RegistryHelper.DefaultApiBaseUrl;

        private readonly IRegistryHelper _registryHelper;
        private readonly IRestClient _injectedClient;
        private readonly Func<string> _warehouseCodeProvider;
        private readonly Func<string> _operatorEmailProvider;
        private readonly string _configuredBaseUrl;
        private readonly object _clientLock = new object();
        private IRestClient _restClient;
        private string _activeBaseUrl;

        public string BaseUrl => GetCurrentBaseUrl();

        private string GetCurrentBaseUrl()
        {
            if (!string.IsNullOrWhiteSpace(_configuredBaseUrl))
                return RegistryHelper.NormalizeApiUrl(_configuredBaseUrl);

            var fromRegistry = _registryHelper?.ApiBaseUrl;
            if (!string.IsNullOrWhiteSpace(fromRegistry))
                return RegistryHelper.NormalizeApiUrl(fromRegistry);

            var fromConfig = ConfigurationManager.AppSettings["BtradeApiBaseUrl"];
            if (!string.IsNullOrWhiteSpace(fromConfig))
                return RegistryHelper.NormalizeApiUrl(fromConfig);

            return RegistryHelper.NormalizeApiUrl(DefaultBaseUrl);
        }

        private IRestClient Client
        {
            get
            {
                if (_injectedClient != null)
                {
                    return _injectedClient;
                }

                var currentBase = GetCurrentBaseUrl();
                lock (_clientLock)
                {
                    if (_restClient == null || !string.Equals(_activeBaseUrl, currentBase, StringComparison.OrdinalIgnoreCase))
                    {
                        _restClient?.Dispose();
                        _activeBaseUrl = currentBase;
                        var options = new RestClientOptions(_activeBaseUrl)
                        {
                            Timeout = TimeSpan.FromSeconds(30)
                        };
                        _restClient = new RestClient(options);
                    }
                    return _restClient;
                }
            }
        }

        /// <summary>
        /// Constructs client with explicit endpoint and session providers.
        /// </summary>
        public BtradeApiClient(
            string baseUrl = null,
            Func<string> warehouseCodeProvider = null,
            Func<string> operatorEmailProvider = null,
            IRestClient client = null)
        {
            _configuredBaseUrl = baseUrl;
            _warehouseCodeProvider = warehouseCodeProvider
                ?? (() => RegistryHelper.ReadString(RegistryHelper.KeyWarehouseCode, string.Empty));

            _operatorEmailProvider = operatorEmailProvider
                ?? (() => RegistryHelper.ReadString(RegistryHelper.KeyLastOperatorEmail, string.Empty));

            _injectedClient = client;
        }

        /// <summary>
        /// Constructs client using IRegistryHelper for session header and endpoint resolution.
        /// </summary>
        public BtradeApiClient(
            IRegistryHelper registryHelper,
            string baseUrl = null,
            IRestClient client = null)
            : this(
                baseUrl,
                () => registryHelper?.WarehouseCode ?? string.Empty,
                () => registryHelper?.LastOperatorEmail ?? string.Empty,
                client)
        {
            _registryHelper = registryHelper;
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

            var actor = _operatorEmailProvider?.Invoke();
            if (!string.IsNullOrWhiteSpace(actor))
            {
                request.AddHeader(HeaderActor, actor);
            }

            return request;
        }

        /// <summary>
        /// Uploads a completed Return Order to POST api/return-order.
        /// Returns a rich result carrying the HTTP status code, success flag, and server error detail
        /// so callers can surface detailed diagnostics for failed uploads.
        /// Traceable to: ARCHITECTURE-NOT-REQUIRED, P2-S03.
        /// </summary>
        public async Task<ApiUploadResultDto> UploadReturnOrderAsync(ReturnOrderDto dto)
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
                CustomerCode = dto.CustomerCode,
                Wilayah = dto.Wilayah,
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

            try
            {
                var response = await Client.ExecuteAsync(request).ConfigureAwait(false);

                if (IsSuccessResponse(response))
                {
                    return ApiUploadResultDto.Ok((int)response.StatusCode, response.Content);
                }

                return ApiUploadResultDto.Fail(
                    response != null ? (int)response.StatusCode : 0,
                    ExtractErrorDetail(response),
                    response?.Content);
            }
            catch (Exception ex)
            {
                return ApiUploadResultDto.Fail(0, ex.Message, null);
            }
        }

        /// <summary>
        /// Uploads an unmapped barcode registration request to POST api/barcode-registration.
        /// Returns a rich result carrying the HTTP status code, success flag, and server error detail.
        /// Traceable to: ARCHITECTURE-NOT-REQUIRED, P2-S03.
        /// </summary>
        public async Task<ApiUploadResultDto> UploadBarcodeRegistrationAsync(BarcodeRegistrationDto dto)
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

            try
            {
                var response = await Client.ExecuteAsync(request).ConfigureAwait(false);

                if (IsSuccessResponse(response))
                {
                    return ApiUploadResultDto.Ok((int)response.StatusCode, response.Content);
                }

                return ApiUploadResultDto.Fail(
                    response != null ? (int)response.StatusCode : 0,
                    ExtractErrorDetail(response),
                    response?.Content);
            }
            catch (Exception ex)
            {
                return ApiUploadResultDto.Fail(0, ex.Message, null);
            }
        }

        /// <summary>
        /// Resolves an operator Google email through the Cloud anonymous endpoint
        /// <c>POST api/session/resolve</c>. No <c>X-Session-*</c> headers are attached; the
        /// email is the sole input. A 2xx response yields the authoritative BTR identity
        /// (UserId/UserName/RoleId) and the warehouse mapping; a non-2xx response or a
        /// transport failure yields a failed result carrying the server refusal detail
        /// (e.g. "Google account is not registered or is not active. Contact your administrator.").
        /// Traceable to: ARCHITECTURE §4 TD-012, §5.1, §6.
        /// </summary>
        public async Task<SessionResolveResultDto> ResolveSessionAsync(string email)
        {
            var request = new RestRequest("api/session/resolve", Method.Post);
            request.AddJsonBody(new { Email = email });

            try
            {
                var response = await Client.ExecuteAsync(request).ConfigureAwait(false);

                if (IsSuccessResponse(response))
                {
                    var result = SessionResolveResultDto.Ok(
                        (int)response.StatusCode,
                        response.Content);
                    PopulateResolvedIdentity(result, response.Content);
                    return result;
                }

                return SessionResolveResultDto.Fail(
                    response != null ? (int)response.StatusCode : 0,
                    ExtractErrorDetail(response),
                    response?.Content);
            }
            catch (Exception ex)
            {
                return SessionResolveResultDto.Fail(0, ex.Message, null);
            }
        }

        /// <summary>
        /// Populates the resolved identity and warehouse mapping from a successful
        /// <c>api/session/resolve</c> payload. Tolerates the JSendOk envelope
        /// (<c>{ "status": ..., "data": { ... } }</c>) as well as a bare payload object.
        /// The raw response is preserved regardless of parseability.
        /// </summary>
        private static void PopulateResolvedIdentity(SessionResolveResultDto result, string json)
        {
            if (result == null || string.IsNullOrWhiteSpace(json)) return;

            try
            {
                using (var doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    if (root.ValueKind != JsonValueKind.Object) return;

                    var payload = root;
                    if (root.TryGetProperty("data", out var dataElem) || root.TryGetProperty("Data", out dataElem))
                    {
                        payload = dataElem;
                    }

                    if (payload.ValueKind != JsonValueKind.Object) return;

                    result.UserId = GetStringProp(payload, "UserId", "userId");
                    result.UserName = GetStringProp(payload, "UserName", "userName");
                    result.RoleId = GetStringProp(payload, "RoleId", "roleId");

                    if (payload.TryGetProperty("Warehouses", out var warehousesElem)
                        || payload.TryGetProperty("warehouses", out warehousesElem))
                    {
                        if (warehousesElem.ValueKind == JsonValueKind.Array)
                        {
                            var warehouses = new List<SessionWarehouseDto>();
                            foreach (var item in warehousesElem.EnumerateArray())
                            {
                                warehouses.Add(new SessionWarehouseDto
                                {
                                    LocationId = GetStringProp(item, "LocationId", "locationId"),
                                    ServerId = GetStringProp(item, "ServerId", "serverId")
                                });
                            }

                            result.Warehouses = warehouses;
                        }
                    }
                }
            }
            catch
            {
                // Structured fields remain unset; the raw response is still returned to callers.
            }
        }

        /// <summary>
        /// Returns true when the response represents a successful HTTP 2xx outcome.
        /// </summary>
        private static bool IsSuccessResponse(RestResponse response)
        {
            return response != null
                && (response.IsSuccessful
                    || ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300));
        }

        /// <summary>
        /// Extracts the most descriptive error detail available from a failed response,
        /// preferring the raw response content, then transport/status messages.
        /// </summary>
        private static string ExtractErrorDetail(RestResponse response)
        {
            if (response == null)
            {
                return "No response received from Cloud API relay.";
            }

            if (!string.IsNullOrWhiteSpace(response.Content))
            {
                return response.Content;
            }

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                return response.ErrorMessage;
            }

            if (!string.IsNullOrWhiteSpace(response.StatusDescription))
            {
                return response.StatusDescription;
            }

            return $"HTTP {(int)response.StatusCode}";
        }

        /// <summary>
        /// Downloads master reference deltas: GET api/barcodes/sync, GET api/Customer/{serverId}, GET api/Brg/{serverId}.
        /// </summary>
        public async Task<MasterSyncDto> DownloadMasterDataAsync(string serverId)
        {
            var target = !string.IsNullOrWhiteSpace(serverId)
                ? serverId
                : (!string.IsNullOrWhiteSpace(_registryHelper?.ServerTargetId)
                    ? _registryHelper.ServerTargetId
                    : RegistryHelper.DefaultServerTargetId);

            var effectiveServerId = !string.IsNullOrWhiteSpace(target)
                ? target
                : RegistryHelper.DefaultServerTargetId;

            var result = new MasterSyncDto
            {
                ServerId = effectiveServerId,
                Timestamp = DateTime.Now
            };

            // 1. Download active barcodes: GET api/barcodes/sync
            try
            {
                var barcodeRequest = CreateRequest("api/barcodes/sync", Method.Get);
                var barcodeResponse = await Client.ExecuteAsync(barcodeRequest).ConfigureAwait(false);
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
                var custResponse = await Client.ExecuteAsync(custRequest).ConfigureAwait(false);
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
                var brgResponse = await Client.ExecuteAsync(brgRequest).ConfigureAwait(false);
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
                var driverResponse = await Client.ExecuteAsync(driverRequest).ConfigureAwait(false);
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
                var spResponse = await Client.ExecuteAsync(spRequest).ConfigureAwait(false);
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
                var custId = GetStringProp(item, "customerId", "CustomerId", "custId", "CustId");
                var custCode = GetStringProp(item, "customerCode", "CustomerCode", "custCode", "CustCode");
                var custName = GetStringProp(item, "customerName", "CustomerName", "custName", "CustName");
                var alamat = GetStringProp(item, "alamat", "Alamat");
                var wilayah = GetStringProp(item, "wilayah", "Wilayah");

                if (!string.IsNullOrWhiteSpace(custId))
                {
                    list.Add(new CustomerDto
                    {
                        CustomerId = custId,
                        CustomerCode = custCode,
                        CustomerName = custName,
                        Alamat = alamat,
                        Wilayah = wilayah
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
