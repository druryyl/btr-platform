using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace btr.portal.worker.admin.Services
{
    public class PortalApiService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private string _baseUrl;
        private string _token;

        public PortalApiService(string baseUrl, string token, int timeoutSeconds)
        {
            _baseUrl = baseUrl?.TrimEnd('/');
            _token = token;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };

            if (!string.IsNullOrWhiteSpace(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        public async Task<Models.DashboardSnapshotHealthData> GetDashboardSnapshotHealthAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/health/dashboard-snapshots");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonHelper.Deserialize<ApiResponse<Models.DashboardSnapshotHealthData>>(json)?.Data;
        }

        public async Task<Models.DashboardSnapshotHealthData> TriggerRefreshAsync(string domain)
        {
            var request = new Models.RefreshRequest { Domain = domain };
            var json = JsonHelper.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/admin/dashboard/refresh", content);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonHelper.Deserialize<ApiResponse<Models.DashboardSnapshotHealthData>>(responseJson)?.Data;
        }

        public void UpdateToken(string token)
        {
            _token = token;
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public void UpdateBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl?.TrimEnd('/');
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }
}
