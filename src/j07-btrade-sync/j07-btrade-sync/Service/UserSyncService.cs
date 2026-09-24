using j07_btrade_sync.Model;
using j07_btrade_sync.Shared;
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
    public class UserSyncService
    {
        private readonly RegistryHelper _registryHelper;
        private readonly BtradeAuthService _authService;

        public UserSyncService()
        {
            _registryHelper = new RegistryHelper();
            _authService = new BtradeAuthService();
        }

        //  SYNC USER (credential projection, IR-05).
        //  The projection now carries Email (TD-14); it flows into the
        //  POST api/User payload via the serialized UserType items.
        public async Task<(bool, string)> SyncUser(IEnumerable<UserType> enumUser)
        {
            //  BUILD
            var serverTargetId = _registryHelper.ReadString("ServerTargetID");
            var baseUrl = ConfigurationManager.AppSettings["btrade-cloud-base-url"];
            var endpoint = $"{baseUrl}/api/User";
            var client = new RestClient(endpoint);

            var listUser = enumUser.ToList();
            foreach (var item in listUser)
                item.ServerId = serverTargetId;

            var requestBody = JsonSerializer.Serialize(new UserSyncRequest(listUser));

            //  I-08 is [Authorize] (Arch §9.1): present the service-account
            //  JWT (S3.6). The token is cached; a rejected token forces one
            //  re-login and a single retry — never a login per request.
            try
            {
                var req = new RestRequest()
                    .AddJsonBody(requestBody);
                await _authService.AddAuthHeaderAsync(req).ConfigureAwait(false);

                //  EXECUTE
                var response = await client.ExecutePostAsync(req);
                if (BtradeAuthService.IsUnauthorized(response))
                {
                    var retry = new RestRequest()
                        .AddJsonBody(requestBody);
                    await _authService.AddAuthHeaderAsync(retry, true).ConfigureAwait(false);
                    response = await client.ExecutePostAsync(retry);
                }
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return (false, response.ErrorMessage ?? response.StatusDescription
                        ?? response.Content);
                }

                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }

    public class UserSyncRequest
    {
        public UserSyncRequest(IEnumerable<UserType> listUser)
        {
            ListUser = new List<UserType>(listUser);
        }

        public List<UserType> ListUser { get; set; }
    }
}
