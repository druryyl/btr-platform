using j07_btrade_sync.Model;
using j07_btrade_sync.Shared;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j07_btrade_sync.Service
{
    public class DriverSyncService
    {
        private readonly RegistryHelper _registryHelper;
        private readonly BtradeAuthService _authService;

        public DriverSyncService()
        {
            _registryHelper = new RegistryHelper();
            _authService = new BtradeAuthService();
        }
        public async Task<(bool, string)> SyncDriver(IEnumerable<Model.DriverType> enumDriver)
        {
            //  BUILD
            var serverTargetId = _registryHelper.ReadString("ServerTargetID");
            var baseUrl = System.Configuration.ConfigurationManager.AppSettings["btrade-cloud-base-url"];
            var endpoint = $"{baseUrl}/api/Driver";
            RestClient client = new RestSharp.RestClient(endpoint);

            //  serialize object cmd to json using System.Text.Json
            var listDriver = enumDriver.ToList();
            foreach(var item in listDriver)
                item.ServerId = serverTargetId;

            var requestBody = System.Text.Json.JsonSerializer.Serialize(new DriverSyncCommand(listDriver, serverTargetId));

            //  I-RO-06 is [Authorize] (Arch §9.1): present the service-account
            //  JWT (S3.6). The token is cached; a rejected token forces one
            //  re-login and a single retry — never a login per request.
            //  The payload shape is unchanged (SalesPerson precedent).
            try
            {
                var req = new RestSharp.RestRequest()
                    .AddJsonBody(requestBody);
                await _authService.AddAuthHeaderAsync(req).ConfigureAwait(false);

                //  EXECUTE
                var response = await client.ExecutePostAsync(req);
                if (BtradeAuthService.IsUnauthorized(response))
                {
                    var retry = new RestSharp.RestRequest()
                        .AddJsonBody(requestBody);
                    await _authService.AddAuthHeaderAsync(retry, true).ConfigureAwait(false);
                    response = await client.ExecutePostAsync(retry);
                }
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return (false, response.ErrorMessage);
                }
                else
                {
                    return (true, "");
                }
            }
            catch (System.Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }

    public class DriverSyncCommand
    {
        public DriverSyncCommand(IEnumerable<DriverType> listDriver, string serverId)
        {
            ListDriver = new List<DriverType>(listDriver);
            ServerId = serverId;
        }
        public List<DriverType> ListDriver { get; set; }
        public string ServerId { get; set; }
    }
}
