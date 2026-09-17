using j07_btrade_sync.Shared;
using RestSharp;
using System;
using System.Configuration;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace j07_btrade_sync.Service
{
    //  S3.6 — sync client authentication (Arch §9.1, I-RO-02 / I-RO-06).
    //  Dedicated non-human service account (barcode-registry AUTH-GAP-001
    //  precedent, C-7): used exclusively by j07-btrade-sync, mapped to a
    //  single Office / ServerId via BTR_WarehouseMapping (S2.4). Credentials
    //  are managed by system administrators in the Windows registry
    //  (RegistryHelper, same store as ServerTargetID) with App.config
    //  fallback; no new authentication mechanism (Arch §9.1).
    public class BtradeAuthService
    {
        private static string _cachedToken;
        private static DateTime _cachedExpiresAt = DateTime.MinValue;
        private static readonly SemaphoreSlim _loginLock = new SemaphoreSlim(1, 1);

        private readonly RegistryHelper _registryHelper;

        public BtradeAuthService()
        {
            _registryHelper = new RegistryHelper();
        }

        //  Returns the cached JWT, logging in only when absent, expired,
        //  or forceRefresh is set. Never logs in per request.
        public async Task<string> GetTokenAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && !string.IsNullOrEmpty(_cachedToken)
                && DateTime.UtcNow < _cachedExpiresAt.AddMinutes(-1))
                return _cachedToken;

            await _loginLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!forceRefresh && !string.IsNullOrEmpty(_cachedToken)
                    && DateTime.UtcNow < _cachedExpiresAt.AddMinutes(-1))
                    return _cachedToken;

                var result = await LoginAsync().ConfigureAwait(false);
                _cachedToken = result.Token;
                _cachedExpiresAt = result.ExpiresAt;
                return _cachedToken;
            }
            finally
            {
                _loginLock.Release();
            }
        }

        public async Task AddAuthHeaderAsync(RestRequest request, bool forceRefresh = false)
        {
            var token = await GetTokenAsync(forceRefresh).ConfigureAwait(false);
            request.AddHeader("Authorization", "Bearer " + token);
        }

        public static bool IsUnauthorized(RestResponse response)
        {
            return response != null && response.StatusCode == HttpStatusCode.Unauthorized;
        }

        private async Task<LoginData> LoginAsync()
        {
            var baseUrl = ConfigurationManager.AppSettings["btrade-cloud-base-url"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("Missing AppSettings 'btrade-cloud-base-url'.");

            //  Service-account identity. Registry is the store of record
            //  (same as ServerTargetID); App.config is the fallback so ops
            //  can seed values without registry access. LocationId is the
            //  WarehouseCode (GAMPING/CONCAT/MAGELANG); the Cloud resolves
            //  its ServerId via BTR_WarehouseMapping (S2.4, IR-RO-04).
            var userId = ReadCredential("SyncUserId", "btrade-sync-user-id", "");
            var password = ReadCredential("SyncPassword", "btrade-sync-password", "");
            var locationId = ReadCredential("SyncLocationId", "btrade-sync-location-id", "");
            if (string.IsNullOrWhiteSpace(userId)
                || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(locationId))
                throw new InvalidOperationException(
                    "Sync service-account credentials are not configured " +
                    "(registry SyncUserId/SyncPassword/SyncLocationId).");

            var endpoint = baseUrl.TrimEnd('/') + "/api/Auth/login";
            var client = new RestClient(endpoint);
            var requestBody = JsonSerializer.Serialize(new LoginRequest(userId, password, locationId));
            var request = new RestRequest().AddJsonBody(requestBody);
            var response = await client.ExecutePostAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessful)
                throw new InvalidOperationException(
                    "Login failed: " + (response.ErrorMessage ?? response.StatusDescription
                        ?? response.StatusCode.ToString()));

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginData>>(response.Content, options);
            if (apiResponse == null || apiResponse.Status == null
                || apiResponse.Status.ToLower() != "success"
                || apiResponse.Data == null || string.IsNullOrEmpty(apiResponse.Data.Token))
                throw new InvalidOperationException(
                    "Login failed: unexpected response (status=" + (apiResponse == null ? "<null>" : apiResponse.Status) + ").");

            return apiResponse.Data;
        }

        private string ReadCredential(string registryKey, string appSettingsKey, string defaultValue)
        {
            var fromRegistry = _registryHelper.ReadString(registryKey, null);
            if (!string.IsNullOrEmpty(fromRegistry))
                return fromRegistry;
            var fromConfig = ConfigurationManager.AppSettings[appSettingsKey];
            return string.IsNullOrEmpty(fromConfig) ? defaultValue : fromConfig;
        }

        //  Test seam: clears the in-memory token cache.
        internal static void ClearCache()
        {
            _cachedToken = null;
            _cachedExpiresAt = DateTime.MinValue;
        }
    }

    public class LoginRequest
    {
        public LoginRequest(string userId, string password, string locationId)
        {
            UserId = userId;
            Password = password;
            LocationId = locationId;
        }

        public string UserId { get; set; }
        public string Password { get; set; }
        public string LocationId { get; set; }
    }

    public class LoginData
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string RoleId { get; set; }
        public string LocationId { get; set; }
        public string ServerId { get; set; }
    }
}
