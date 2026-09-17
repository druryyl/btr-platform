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

        public UserSyncService()
        {
            _registryHelper = new RegistryHelper();
        }

        //  SYNC USER (credential projection, IR-05)
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

            var req = new RestRequest()
                .AddJsonBody(requestBody);

            //  EXECUTE
            var response = await client.ExecutePostAsync(req);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return (false, response.ErrorMessage ?? response.Content);
            }

            return (true, "");
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
