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

        public DriverSyncService()
        {
            _registryHelper = new RegistryHelper();
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
            var req = new RestSharp.RestRequest()
                .AddJsonBody(requestBody);
            
            //  EXECUTE
            var response = await client.ExecutePostAsync(req);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return (false, response.ErrorMessage);
            }
            else
            {
                return (true, "");
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
