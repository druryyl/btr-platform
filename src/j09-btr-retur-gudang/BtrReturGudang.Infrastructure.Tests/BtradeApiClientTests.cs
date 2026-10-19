using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.Infrastructure.Http;
using RestSharp;
using Xunit;

namespace BtrReturGudang.Infrastructure.Tests
{
    public class BtradeApiClientTests
    {
        private class FakeRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "GDG-CONCAT";
            public string ServerTargetId { get; set; } = "SERVER-01";
            public string ApiBaseUrl { get; set; } = BtrReturGudang.Infrastructure.Helpers.RegistryHelper.DefaultApiBaseUrl;
            public string LastOperatorName { get; set; } = "Budi Hartono";
            public string LastOperatorEmail { get; set; } = "budi.hartono@btr.co.id";
            public bool EnableSoundFeedback { get; set; } = true;
        }

        private class FakeRestClient : IRestClient
        {
            public ReadOnlyRestClientOptions Options => new ReadOnlyRestClientOptions(new RestClientOptions("http://localhost:5000"));
            public RestSharp.Serializers.RestSerializers Serializers => throw new NotImplementedException();
            public DefaultParameters DefaultParameters => throw new NotImplementedException();

            public List<RestRequest> ExecutedRequests { get; } = new List<RestRequest>();
            public RestResponse NextResponse { get; set; } = new RestResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                Content = "{\"status\":\"success\",\"data\":\"Done\"}"
            };

            public Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
            {
                ExecutedRequests.Add(request);
                return Task.FromResult(NextResponse);
            }

            public Task<System.IO.Stream> DownloadStreamAsync(RestRequest request, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
            }
        }

        [Fact]
        public void CreateRequest_InjectsSessionHeaders_FromProviders()
        {
            var fakeRegistry = new FakeRegistryHelper
            {
                WarehouseCode = "GDG-GAMPING",
                LastOperatorName = "Andi Wijaya",
                LastOperatorEmail = "andi.wijaya@btr.co.id"
            };

            var fakeClient = new FakeRestClient();
            var client = new BtradeApiClient(fakeRegistry, "http://localhost:5000", fakeClient);

            var request = client.CreateRequest("api/test", Method.Get);

            var locationParam = request.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderLocation);
            var actorParam = request.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderActor);

            Assert.NotNull(locationParam);
            Assert.Equal("GDG-GAMPING", locationParam.Value?.ToString());
            Assert.NotNull(actorParam);

            // TD-007/TD-012: the actor header carries the registered Google email, never the display name.
            Assert.Equal("andi.wijaya@btr.co.id", actorParam.Value?.ToString());
            Assert.NotEqual("Andi Wijaya", actorParam.Value?.ToString());
        }

        [Fact]
        public void CreateRequest_DynamicallyReflects_ShiftOperatorChange()
        {
            var fakeRegistry = new FakeRegistryHelper
            {
                WarehouseCode = "GDG-GAMPING",
                LastOperatorEmail = "operator1@btr.co.id"
            };

            var fakeClient = new FakeRestClient();
            var client = new BtradeApiClient(fakeRegistry, "http://localhost:5000", fakeClient);

            var req1 = client.CreateRequest("api/test1", Method.Get);
            var actorParam1 = req1.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderActor);
            Assert.Equal("operator1@btr.co.id", actorParam1?.Value?.ToString());

            // Shift changes
            fakeRegistry.LastOperatorEmail = "operator2@btr.co.id";

            var req2 = client.CreateRequest("api/test2", Method.Get);
            var actorParam2 = req2.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderActor);
            Assert.Equal("operator2@btr.co.id", actorParam2?.Value?.ToString());
        }

        [Fact]
        public async Task UploadReturnOrderAsync_ExecutesPost_WithHeadersAndPayload()
        {
            var fakeRegistry = new FakeRegistryHelper
            {
                WarehouseCode = "GDG-PST",
                LastOperatorEmail = "joko.gudang@btr.co.id"
            };

            var fakeClient = new FakeRestClient();
            var client = new BtradeApiClient(fakeRegistry, "http://localhost:5000", fakeClient);

            var dto = new ReturnOrderDto
            {
                ReturnOrderId = "01ARZ3NDEKTSV4RRFFQ69G5FAV",
                ReturnOrderDate = new DateTime(2026, 10, 11),
                CustId = "C001",
                CustName = "Toko Berkah",
                WarehouseCode = "GDG-PST",
                Keterangan = "Retur Barang Rusak",
                Items = new List<ReturnOrderItemDto>
                {
                    new ReturnOrderItemDto("01ARZ3NDEKTSV4RRFFQ69G5FAV", 1, "B001", "Indomie Goreng", "8991234567890", "RUSAK", 10, "Dus", "Kardus basah")
                }
            };

            var result = await client.UploadReturnOrderAsync(dto);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Single(fakeClient.ExecutedRequests);

            var executed = fakeClient.ExecutedRequests[0];
            Assert.Equal("api/return-order", executed.Resource);
            Assert.Equal(Method.Post, executed.Method);

            var loc = executed.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderLocation);
            var actor = executed.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderActor);
            Assert.NotNull(loc);
            Assert.Equal("GDG-PST", loc.Value?.ToString());

            // Criterion: outbound requests carry X-Session-Actor == LastOperatorEmail.
            Assert.NotNull(actor);
            Assert.Equal("joko.gudang@btr.co.id", actor.Value?.ToString());
        }

        [Fact]
        public async Task UploadReturnOrderAsync_BlankOptionalIds_StripsOptionalNamesFromPayload()
        {
            var fakeRegistry = new FakeRegistryHelper();
            var fakeClient = new FakeRestClient();
            var client = new BtradeApiClient(fakeRegistry, "http://localhost:5000", fakeClient);

            var dto = new ReturnOrderDto
            {
                ReturnOrderId = "01ARZ3NDEKTSV4RRFFQ69G5FAV",
                ReturnOrderDate = new DateTime(2026, 10, 11),
                CustId = "C001",
                CustName = "Toko Berkah",
                WarehouseCode = "GDG-PST",
                DriverId = null,
                DriverName = "-- Pilih Driver (Opsional) --",
                SalesPersonId = string.Empty,
                SalesPersonName = "-- Pilih Salesman (Opsional) --",
                Items = new List<ReturnOrderItemDto>()
            };

            await client.UploadReturnOrderAsync(dto);

            var executed = Assert.Single(fakeClient.ExecutedRequests);
            var body = executed.Parameters.First(p => p.Type == ParameterType.RequestBody).Value;
            var driverName = body.GetType().GetProperty("DriverName").GetValue(body) as string;
            var salesPersonName = body.GetType().GetProperty("SalesPersonName").GetValue(body) as string;

            // The optional names must never carry the dropdown prompt placeholder onto the wire.
            Assert.True(string.IsNullOrEmpty(driverName));
            Assert.True(string.IsNullOrEmpty(salesPersonName));
        }

        [Fact]
        public async Task UploadBarcodeRegistrationAsync_ExecutesPost_WithPayload()
        {
            var fakeRegistry = new FakeRegistryHelper
            {
                WarehouseCode = "GDG-PST",
                LastOperatorName = "Joko"
            };

            var fakeClient = new FakeRestClient();
            var client = new BtradeApiClient(fakeRegistry, "http://localhost:5000", fakeClient);

            var dto = new BarcodeRegistrationDto
            {
                RequestId = "01ARZ3NDEKTSV4RRFFQ69G5FA9",
                Barcode = "8999999999999",
                BrgId = "BRG-002",
                Satuan = "Pcs",
                WarehouseCode = "GDG-PST",
                OperatorName = "Joko"
            };

            var result = await client.UploadBarcodeRegistrationAsync(dto);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Single(fakeClient.ExecutedRequests);

            var executed = fakeClient.ExecutedRequests[0];
            Assert.Equal("api/barcode-registration", executed.Resource);
            Assert.Equal(Method.Post, executed.Method);
        }

        [Fact]
        public async Task UploadReturnOrderAsync_NormalizesConfiguredBaseUrl_AndBuildsBelajarApiRequestUri()
        {
            var fakeRegistry = new FakeRegistryHelper();
            var fakeClient = new FakeRestClient();

            // Configured WITHOUT a trailing slash: RegistryHelper.NormalizeApiUrl must append one so
            // RestSharp does not strip the '/belajar-api' segment when resolving the relative resource.
            var client = new BtradeApiClient(fakeRegistry, "http://dev.smart-ics.com:8089/belajar-api", fakeClient);

            Assert.Equal("http://dev.smart-ics.com:8089/belajar-api/", client.BaseUrl);

            var dto = new ReturnOrderDto
            {
                ReturnOrderId = "01ARZ3NDEKTSV4RRFFQ69G5FAV",
                ReturnOrderDate = new DateTime(2026, 10, 11),
                CustId = "C001",
                CustName = "Toko Berkah",
                WarehouseCode = "GDG-PST",
                Items = new List<ReturnOrderItemDto>()
            };

            await client.UploadReturnOrderAsync(dto);

            var executed = Assert.Single(fakeClient.ExecutedRequests);

            // Resolve the captured request against the normalized BaseUrl exactly as RestSharp does on the wire.
            var resolvingClient = new RestClient(new RestClientOptions(client.BaseUrl));
            var resolvedUri = resolvingClient.BuildUri(executed);

            Assert.Equal("http://dev.smart-ics.com:8089/belajar-api/api/return-order", resolvedUri.ToString());
        }

        [Fact]
        public async Task UploadReturnOrderAsync_Non2xxResponse_ReturnsFailureWithStatusCodeAndErrorMessage()
        {
            var fakeRegistry = new FakeRegistryHelper();
            var errorBody = "{\"status\":\"error\",\"message\":\"Return order endpoint not found\"}";
            var fakeClient = new FakeRestClient
            {
                NextResponse = new RestResponse
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    ResponseStatus = ResponseStatus.Completed,
                    Content = errorBody
                }
            };

            var client = new BtradeApiClient(fakeRegistry, "http://localhost:5000", fakeClient);

            var dto = new ReturnOrderDto
            {
                ReturnOrderId = "01ARZ3NDEKTSV4RRFFQ69G5FAV",
                ReturnOrderDate = new DateTime(2026, 10, 11),
                CustId = "C001",
                CustName = "Toko Berkah",
                WarehouseCode = "GDG-PST",
                Items = new List<ReturnOrderItemDto>()
            };

            var result = await client.UploadReturnOrderAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(errorBody, result.ErrorMessage);
            Assert.Equal(errorBody, result.RawResponse);
        }

        [Fact]
        public void ParseBarcodes_ExtractsJSendArrayCorrectly()
        {
            var json = @"
{
    ""status"": ""success"",
    ""data"": [
        {
            ""brgBarcodeId"": ""BC001"",
            ""barcodeValue"": ""8991112223334"",
            ""brgId"": ""BRG01"",
            ""brgName"": ""Kopi Kapal Api"",
            ""satuan"": ""Renceng""
        },
        {
            ""brgBarcodeId"": ""BC002"",
            ""barcodeValue"": ""8994445556667"",
            ""brgId"": ""BRG02"",
            ""brgName"": ""Gula Pasir 1kg"",
            ""satuan"": ""Bungkus""
        }
    ]
}";

            var list = BtradeApiClient.ParseBarcodes(json);

            Assert.Equal(2, list.Count);
            Assert.Equal("8991112223334", list[0].BarcodeValue);
            Assert.Equal("BRG01", list[0].BrgId);
            Assert.Equal("Kopi Kapal Api", list[0].BrgName);
            Assert.Equal("Renceng", list[0].Satuan);
        }

        [Fact]
        public void ParseCustomers_ExtractsJSendArrayCorrectly()
        {
            var json = @"
{
    ""status"": ""success"",
    ""code"": ""200"",
    ""data"": [
        {
            ""customerId"": ""CS0001"",
            ""customerCode"": ""32"",
            ""customerName"": ""Toko Subur"",
            ""alamat"": ""Jl. Malioboro No. 10"",
            ""wilayah"": ""YOGYAKARTA""
        }
    ]
}";

            var list = BtradeApiClient.ParseCustomers(json);

            Assert.Single(list);
            Assert.Equal("CS0001", list[0].CustomerId);
            Assert.Equal("CS0001", list[0].CustId);
            Assert.Equal("32", list[0].CustomerCode);
            Assert.Equal("32", list[0].CustCode);
            Assert.Equal("Toko Subur", list[0].CustomerName);
            Assert.Equal("Toko Subur", list[0].CustName);
            Assert.Equal("Jl. Malioboro No. 10", list[0].Alamat);
            Assert.Equal("YOGYAKARTA", list[0].Wilayah);
        }

        [Fact]
        public void ParseCustomers_HandlesMissingOptionalFieldsGracefully()
        {
            var json = @"
{
    ""status"": ""success"",
    ""data"": [
        {
            ""customerId"": ""CS0002"",
            ""customerName"": ""Customer Minimal""
        }
    ]
}";

            var list = BtradeApiClient.ParseCustomers(json);

            Assert.Single(list);
            Assert.Equal("CS0002", list[0].CustomerId);
            Assert.Equal("Customer Minimal", list[0].CustomerName);
            Assert.True(string.IsNullOrEmpty(list[0].CustomerCode));
            Assert.True(string.IsNullOrEmpty(list[0].Alamat));
            Assert.True(string.IsNullOrEmpty(list[0].Wilayah));
        }

        [Fact]
        public void ParseBarang_ExtractsJSendArrayCorrectly()
        {
            var json = @"
{
    ""status"": ""success"",
    ""data"": [
        {
            ""brgId"": ""B001"",
            ""brgName"": ""Indomie Goreng"",
            ""satBesar"": ""Dus"",
            ""satKecil"": ""Pcs""
        }
    ]
}";

            var list = BtradeApiClient.ParseBarang(json);

            Assert.Single(list);
            Assert.Equal("B001", list[0].BrgId);
            Assert.Equal("Indomie Goreng", list[0].BrgName);
            Assert.Equal("Dus", list[0].Satuan1);
            Assert.Equal("Pcs", list[0].Satuan2);
        }

        [Fact]
        public async Task DownloadMasterDataAsync_DefaultsToJOG_WhenServerIdIsEmptyOrNull()
        {
            var fakeRegistry = new FakeRegistryHelper
            {
                ServerTargetId = "",
                ApiBaseUrl = "http://dev.smart-ics.com:8089/belajar-api"
            };

            var fakeClient = new FakeRestClient();
            var client = new BtradeApiClient(fakeRegistry, null, fakeClient);

            var result = await client.DownloadMasterDataAsync("");

            Assert.Equal("JOG", result.ServerId);
            Assert.Contains(fakeClient.ExecutedRequests, r => r.Resource.Contains("Customer/JOG"));
            Assert.Contains(fakeClient.ExecutedRequests, r => r.Resource.Contains("Brg/JOG"));
        }

        [Fact]
        public async Task ResolveSessionAsync_200_ReturnsResolvedIdentityAndWarehouses()
        {
            var fakeRegistry = new FakeRegistryHelper();
            var resolvedBody = @"{
    ""status"": ""success"",
    ""data"": {
        ""UserId"": ""USR-001"",
        ""UserName"": ""Andi Wijaya"",
        ""RoleId"": ""ROLE-GDG"",
        ""Warehouses"": [
            { ""LocationId"": ""GDG-PST"", ""ServerId"": ""SRV-JOG"" },
            { ""LocationId"": ""GDG-SBY"", ""ServerId"": ""SRV-SBY"" }
        ]
    }
}";

            var fakeClient = new FakeRestClient
            {
                NextResponse = new RestResponse
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    ResponseStatus = ResponseStatus.Completed,
                    Content = resolvedBody
                }
            };

            var client = new BtradeApiClient(fakeRegistry, "http://localhost:5000", fakeClient);

            var result = await client.ResolveSessionAsync("andi.wijaya@btr.co.id");

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("USR-001", result.UserId);
            Assert.Equal("Andi Wijaya", result.UserName);
            Assert.Equal("ROLE-GDG", result.RoleId);
            Assert.NotNull(result.Warehouses);
            Assert.Equal(2, result.Warehouses.Count);
            Assert.Equal("GDG-PST", result.Warehouses[0].LocationId);
            Assert.Equal("SRV-JOG", result.Warehouses[0].ServerId);
            Assert.Equal("GDG-SBY", result.Warehouses[1].LocationId);
            Assert.Equal("SRV-SBY", result.Warehouses[1].ServerId);

            // Resolution is anonymous: no X-Session-* headers are attached.
            var executed = Assert.Single(fakeClient.ExecutedRequests);
            Assert.Equal("api/session/resolve", executed.Resource);
            Assert.Equal(Method.Post, executed.Method);
            Assert.DoesNotContain(executed.Parameters, p => p.Name == BtradeApiClient.HeaderActor);
            Assert.DoesNotContain(executed.Parameters, p => p.Name == BtradeApiClient.HeaderLocation);
        }

        [Fact]
        public async Task ResolveSessionAsync_400_ReturnsFailureWithServerMessage()
        {
            var fakeRegistry = new FakeRegistryHelper();
            const string refusal = "Google account is not registered or is not active. Contact your administrator.";
            var errorBody = "{\"status\":\"error\",\"message\":\"" + refusal + "\"}";

            var fakeClient = new FakeRestClient
            {
                NextResponse = new RestResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ResponseStatus = ResponseStatus.Completed,
                    Content = errorBody
                }
            };

            var client = new BtradeApiClient(fakeRegistry, "http://localhost:5000", fakeClient);

            var result = await client.ResolveSessionAsync("tidak.terdaftar@btr.co.id");

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains(refusal, result.ErrorMessage);
            Assert.Equal(errorBody, result.RawResponse);

            var executed = Assert.Single(fakeClient.ExecutedRequests);
            Assert.Equal("api/session/resolve", executed.Resource);
            Assert.Equal(Method.Post, executed.Method);
        }
    }
}
