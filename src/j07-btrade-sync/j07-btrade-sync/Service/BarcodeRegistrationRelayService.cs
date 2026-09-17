using btr.application.BrgContext.BrgBarcodeAgg;
using j07_btrade_sync.Model;
using j07_btrade_sync.Shared;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace j07_btrade_sync.Service
{
    public class BarcodeRegistrationRelayService
    {
        private readonly MainOfficeCommandExecutor _mainOfficeCommand;

        public BarcodeRegistrationRelayService()
        {
            _mainOfficeCommand = new MainOfficeCommandExecutor();
        }

        //  SYNC REGISTRATION (relay + ack, 8.2 / 8.3)
        public async Task<(bool, string)> SyncRegistration()
        {
            var baseUrl = ConfigurationManager.AppSettings["btrade-cloud-base-url"];

            //  PULL PENDING (I-02)
            var listPending = await GetPending(baseUrl);
            if (!listPending.Item1)
                return (false, listPending.Item2);

            var errorMessages = new List<string>();
            foreach (var item in listPending.Item3)
            {
                //  PROCESS IN-PROCESS VIA THE MAIN OFFICE (IR-08) — accept or
                //  reject is decided by the only authority (BR-011, INV-13)
                ProcessBarcodeRegistrationRequestResponse outcome;
                try
                {
                    outcome = await _mainOfficeCommand.ProcessBarcodeRegistration(
                        item.BarcodeValue, item.BrgId, item.Satuan, item.RequestedBy);
                }
                catch (Exception ex)
                {
                    //  NO OUTCOME — leave the request PENDING in the Cloud and
                    //  let the next run re-relay it (8.2)
                    errorMessages.Add($"{item.BarcodeRegistrationId}: {ex.Message}");
                    continue;
                }

                //  EXPLICIT POST-COMMIT ACK (I-03, GAP-007); the rejection
                //  reason is transported verbatim (8.2)
                var ackResult = await Ack(baseUrl, item.BarcodeRegistrationId,
                    outcome.Status, outcome.Reason);
                if (!ackResult.Item1)
                    errorMessages.Add($"{item.BarcodeRegistrationId}: {ackResult.Item2}");
            }

            return errorMessages.Any()
                ? (false, string.Join("; ", errorMessages))
                : (true, "");
        }

        private static async Task<(bool, string, List<BarcodeRegistrationRequestType>)> GetPending(
            string baseUrl)
        {
            var endpoint = $"{baseUrl}/api/BarcodeRegistration/pending";
            var client = new RestClient(endpoint);

            var request = new RestRequest();
            var response = await client.ExecuteGetAsync(request);
            if (!response.IsSuccessful)
                return (false, response.ErrorMessage ?? response.StatusDescription,
                    new List<BarcodeRegistrationRequestType>());

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<BarcodeRegistrationRequestType>>>(
                response.Content, options);
            if (apiResponse == null)
                return (false, "Failed to deserialize API response",
                    new List<BarcodeRegistrationRequestType>());

            if (apiResponse.Status?.ToLower() != "success")
                return (false, $"API returned non-success status: {apiResponse.Status}",
                    new List<BarcodeRegistrationRequestType>());

            return (true, "", apiResponse.Data ?? new List<BarcodeRegistrationRequestType>());
        }

        private static async Task<(bool, string)> Ack(string baseUrl, string barcodeRegistrationId,
            string status, string processedNote)
        {
            var endpoint = $"{baseUrl}/api/BarcodeRegistration/ack";
            var client = new RestClient(endpoint);

            var requestBody = JsonSerializer.Serialize(new BarcodeRegistrationAckCommand(
                barcodeRegistrationId, status, processedNote ?? string.Empty));
            var request = new RestRequest()
                .AddJsonBody(requestBody)
                .AddHeader("Content-Type", "application/json");
            var response = await client.ExecutePostAsync(request);
            if (!response.IsSuccessful)
                return (false, response.ErrorMessage ?? response.StatusDescription);

            return (true, "");
        }
    }

    public class BarcodeRegistrationAckCommand
    {
        public BarcodeRegistrationAckCommand(string barcodeRegistrationId, string status,
            string processedNote)
        {
            BarcodeRegistrationId = barcodeRegistrationId;
            Status = status;
            ProcessedNote = processedNote;
        }

        public string BarcodeRegistrationId { get; set; }
        public string Status { get; set; }
        public string ProcessedNote { get; set; }
    }
}
