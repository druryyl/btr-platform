using j07_btrade_sync.Model;
using j07_btrade_sync.Shared;
using Nuna.Lib.ValidationHelper;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace j07_btrade_sync.Service
{
    public class ReturnOrderIncrementalDownloadService
    {
        private readonly RegistryHelper _registryHelper;
        private readonly BtradeAuthService _authService;

        public ReturnOrderIncrementalDownloadService()
        {
            _registryHelper = new RegistryHelper();
            _authService = new BtradeAuthService();
        }
        public async Task<(bool, string, List<ReturnOrderModel>)> Execute(Periode periode)
        {
            var serverTargetId = _registryHelper.ReadString("ServerTargetID");
            var baseUrl = ConfigurationManager.AppSettings["btrade-cloud-base-url"];
            var endpoint = $"{baseUrl}/api/ReturnOrder/incremental/{{tgl1}}/{{tgl2}}/{{serverId}}";
            var client = new RestClient(endpoint);

            //  I-RO-02 is [Authorize] (Arch §9.1): present the service-account
            //  JWT (S3.6). The token is cached; a rejected token forces one
            //  re-login and a single retry — never a login per request.
            //  No ServerId is added to any Return Order payload (P-06): the
            //  serverId url segment follows the CheckIn/Order route
            //  convention; the tenant is bound server-side from the JWT.
            RestResponse response;
            try
            {
                var request = new RestRequest()
                    .AddUrlSegment("tgl1", periode.Tgl1.ToString("yyyy-MM-dd"))
                    .AddUrlSegment("tgl2", periode.Tgl2.ToString("yyyy-MM-dd"))
                    .AddUrlSegment("serverId", serverTargetId);
                await _authService.AddAuthHeaderAsync(request).ConfigureAwait(false);
                response = await client.ExecuteGetAsync(request).ConfigureAwait(false);
                if (BtradeAuthService.IsUnauthorized(response))
                {
                    var retry = new RestRequest()
                        .AddUrlSegment("tgl1", periode.Tgl1.ToString("yyyy-MM-dd"))
                        .AddUrlSegment("tgl2", periode.Tgl2.ToString("yyyy-MM-dd"))
                        .AddUrlSegment("serverId", serverTargetId);
                    await _authService.AddAuthHeaderAsync(retry, true).ConfigureAwait(false);
                    response = await client.ExecuteGetAsync(retry).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }

            if (!response.IsSuccessful)
            {
                return (false, response.ErrorMessage ?? response.StatusDescription, null);
            }

            //  TD-15 — the incremental response is PascalCase (§10); the
            //  case-insensitive options bind SubmittedBy (and every other
            //  field) regardless of casing.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ReturnOrderModel>>>(response.Content, options);

            if (apiResponse == null)
            {
                return (false, "Failed to deserialize API response", null);
            }

            if (apiResponse.Status?.ToLower() != "success")
            {
                return (false, $"API returned non-success status: {apiResponse.Status}", null);
            }

            return (true, "", apiResponse.Data ?? new List<ReturnOrderModel>());
        }
    }
}
