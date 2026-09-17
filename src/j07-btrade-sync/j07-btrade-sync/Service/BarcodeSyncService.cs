using j07_btrade_sync.Repository;
using j07_btrade_sync.Shared;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace j07_btrade_sync.Service
{
    public class BarcodeSyncService
    {
        private const string WatermarkKey = "BarcodeSyncWatermark";

        private readonly RegistryHelper _registryHelper;
        private readonly BrgBarcodeDal _brgBarcodeDal;
        private readonly BtradeAuthService _authService;

        public BarcodeSyncService()
        {
            _registryHelper = new RegistryHelper();
            _brgBarcodeDal = new BrgBarcodeDal();
            _authService = new BtradeAuthService();
        }

        //  SYNC BARCODE (publish)
        public async Task<(bool, string)> SyncBarcode()
        {
            //  BUILD
            var watermark = ReadWatermark();
            var listChanged = _brgBarcodeDal.ListChanged(watermark).ToList();
            if (!listChanged.Any())
                return (true, "");

            var listUpsert = listChanged
                .Where(x => x.IsAktif)
                .Select(x => new BarcodeDto(x.BrgBarcodeId, x.BarcodeValue, x.BrgId,
                    x.BrgCode, x.BrgName, x.Satuan))
                .ToList();
            var listRemove = listChanged
                .Where(x => !x.IsAktif)
                .Select(x => x.BrgBarcodeId)
                .ToList();

            var baseUrl = ConfigurationManager.AppSettings["btrade-cloud-base-url"];
            var endpoint = $"{baseUrl}/api/Barcode/sync";
            var client = new RestClient(endpoint);

            var requestBody = JsonSerializer.Serialize(new BarcodeSyncCommand(listUpsert, listRemove));

            //  I-01 is [Authorize] (Arch §9.1): present the service-account
            //  JWT (S3.6). The token is cached; a rejected token forces one
            //  re-login and a single retry — never a login per request.
            try
            {
                var req = new RestRequest()
                    .AddJsonBody(requestBody)
                    .AddHeader("Content-Type", "application/json");
                await _authService.AddAuthHeaderAsync(req).ConfigureAwait(false);

                //  EXECUTE
                var response = await client.ExecutePostAsync(req);
                if (BtradeAuthService.IsUnauthorized(response))
                {
                    var retry = new RestRequest()
                        .AddJsonBody(requestBody)
                        .AddHeader("Content-Type", "application/json");
                    await _authService.AddAuthHeaderAsync(retry, true).ConfigureAwait(false);
                    response = await client.ExecutePostAsync(retry);
                }
                if (response.StatusCode != HttpStatusCode.OK)
                    return (false, response.ErrorMessage ?? response.StatusDescription
                        ?? response.Content);

                //  ADVANCE WATERMARK ONLY AFTER HTTP 200
                WriteWatermark(listChanged.Last().RowVer);
                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private byte[] ReadWatermark()
        {
            var value = _registryHelper.ReadString(WatermarkKey, "");
            return string.IsNullOrEmpty(value)
                ? null
                : Convert.FromBase64String(value);
        }

        private void WriteWatermark(byte[] watermark)
        {
            _registryHelper.WriteString(WatermarkKey, Convert.ToBase64String(watermark));
        }
    }

    public class BarcodeSyncCommand
    {
        public BarcodeSyncCommand(IEnumerable<BarcodeDto> listUpsert, IEnumerable<string> listRemove)
        {
            ListUpsert = new List<BarcodeDto>(listUpsert);
            ListRemove = new List<string>(listRemove);
        }

        public List<BarcodeDto> ListUpsert { get; set; }
        public List<string> ListRemove { get; set; }
    }

    public class BarcodeDto
    {
        public BarcodeDto(string brgBarcodeId, string barcodeValue, string brgId,
            string brgCode, string brgName, string satuan)
        {
            BrgBarcodeId = brgBarcodeId;
            BarcodeValue = barcodeValue;
            BrgId = brgId;
            BrgCode = brgCode;
            BrgName = brgName;
            Satuan = satuan;
        }

        public string BrgBarcodeId { get; set; }
        public string BarcodeValue { get; set; }
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; }
    }
}
