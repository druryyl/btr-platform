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
            public string LastOperatorName { get; set; } = "Budi Hartono";
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
                LastOperatorName = "Andi Wijaya"
            };

            var fakeClient = new FakeRestClient();
            var client = new BtradeApiClient(fakeRegistry, "http://localhost:5000", fakeClient);

            var request = client.CreateRequest("api/test", Method.Get);

            var locationParam = request.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderLocation);
            var actorParam = request.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderActor);

            Assert.NotNull(locationParam);
            Assert.Equal("GDG-GAMPING", locationParam.Value?.ToString());
            Assert.NotNull(actorParam);
            Assert.Equal("Andi Wijaya", actorParam.Value?.ToString());
        }

        [Fact]
        public void CreateRequest_DynamicallyReflects_ShiftOperatorChange()
        {
            var fakeRegistry = new FakeRegistryHelper
            {
                WarehouseCode = "GDG-GAMPING",
                LastOperatorName = "Operator1"
            };

            var fakeClient = new FakeRestClient();
            var client = new BtradeApiClient(fakeRegistry, "http://localhost:5000", fakeClient);

            var req1 = client.CreateRequest("api/test1", Method.Get);
            var actorParam1 = req1.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderActor);
            Assert.Equal("Operator1", actorParam1?.Value?.ToString());

            // Shift changes
            fakeRegistry.LastOperatorName = "Operator2";

            var req2 = client.CreateRequest("api/test2", Method.Get);
            var actorParam2 = req2.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderActor);
            Assert.Equal("Operator2", actorParam2?.Value?.ToString());
        }

        [Fact]
        public async Task UploadReturnOrderAsync_ExecutesPost_WithHeadersAndPayload()
        {
            var fakeRegistry = new FakeRegistryHelper
            {
                WarehouseCode = "GDG-PST",
                LastOperatorName = "Joko"
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

            var success = await client.UploadReturnOrderAsync(dto);

            Assert.True(success);
            Assert.Single(fakeClient.ExecutedRequests);

            var executed = fakeClient.ExecutedRequests[0];
            Assert.Equal("api/return-order", executed.Resource);
            Assert.Equal(Method.Post, executed.Method);

            var loc = executed.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderLocation);
            var actor = executed.Parameters.FirstOrDefault(p => p.Name == BtradeApiClient.HeaderActor);
            Assert.NotNull(loc);
            Assert.Equal("GDG-PST", loc.Value?.ToString());
            Assert.NotNull(actor);
            Assert.Equal("Joko", actor.Value?.ToString());
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

            var success = await client.UploadBarcodeRegistrationAsync(dto);

            Assert.True(success);
            Assert.Single(fakeClient.ExecutedRequests);

            var executed = fakeClient.ExecutedRequests[0];
            Assert.Equal("api/barcode-registration", executed.Resource);
            Assert.Equal(Method.Post, executed.Method);
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
    ""data"": [
        {
            ""customerId"": ""CUST01"",
            ""customerName"": ""Toko Subur"",
            ""alamat"": ""Jl. Malioboro No. 10""
        }
    ]
}";

            var list = BtradeApiClient.ParseCustomers(json);

            Assert.Single(list);
            Assert.Equal("CUST01", list[0].CustId);
            Assert.Equal("Toko Subur", list[0].CustName);
            Assert.Equal("Jl. Malioboro No. 10", list[0].Alamat);
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
    }
}
