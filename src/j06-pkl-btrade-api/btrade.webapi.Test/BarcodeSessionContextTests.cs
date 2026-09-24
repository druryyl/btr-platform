using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace btrade.webapi.Test;

/// <summary>
/// P4-S05 — integration tests for the BGud barcode routes under the anonymous
/// session-context contract (TD-03/TD-04/TD-05/TD-06/TD-13):
/// GET api/barcodes/sync, POST api/barcode-registration and
/// GET api/BarcodeRegistration/status resolve context from the fixed headers
/// via SessionContextResolver; pending/ack keep their claim-based behavior.
/// </summary>
public class BarcodeSessionContextTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string SyncPath = "/api/barcodes/sync";
    private const string SubmitPath = "/api/barcode-registration";
    private const string StatusPath = "/api/BarcodeRegistration/status";
    private const string PendingPath = "/api/BarcodeRegistration/pending";

    private readonly WebApplicationFactory<Program> _factory;

    public BarcodeSessionContextTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IUserDal, FakeUserDal>();
                services.AddSingleton<IWarehouseMappingDal, FakeWarehouseMappingDal>();
                services.AddSingleton<IBarcodeDal, FakeBarcodeDal>();
                services.AddSingleton<IBarcodeRegistrationDal, FakeBarcodeRegistrationDal>();
            }));
    }

    private void SeedUser(string userId, string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userDal = scope.ServiceProvider.GetRequiredService<IUserDal>();
        ((FakeUserDal)userDal).Insert(
            new UserType(userId, $"Operator {userId}", "hash", "WHS", true, "JOGJA", email));
    }

    private void SeedRegistration(BarcodeRegistrationRequestType row)
    {
        using var scope = _factory.Services.CreateScope();
        var dal = scope.ServiceProvider.GetRequiredService<IBarcodeRegistrationDal>();
        ((FakeBarcodeRegistrationDal)dal).Insert(row);
    }

    private FakeBarcodeRegistrationDal RegistrationDal()
    {
        using var scope = _factory.Services.CreateScope();
        return (FakeBarcodeRegistrationDal)
            scope.ServiceProvider.GetRequiredService<IBarcodeRegistrationDal>();
    }

    private HttpClient SessionClient(string location, string? actor = null)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Session-Location", location);
        if (actor is not null)
            client.DefaultRequestHeaders.Add("X-Session-Actor", actor);
        return client;
    }

    private HttpClient TokenClient(string serverId)
    {
        using var scope = _factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var token = jwtTokenService
            .GenerateToken("SYNC-USER", "SYNC_ROLE", "GAMPING", serverId)
            .Token;
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static BarcodeRegistrationRequestType RegistrationRow(
        string id, string clientRequestId, string serverId, string requestedBy, string status) =>
        new(id, clientRequestId, serverId, "BC-VALUE", "BRG-1", "PCS",
            requestedBy, DateTime.Now, status, null, string.Empty);

    // ---- GET api/barcodes/sync -------------------------------------------

    [Fact]
    public async Task Sync_WithMappedLocation_NoToken_Returns200()
    {
        //  TD-06 — anonymous: no bearer, no 401; tenant from the header.
        var response = await SessionClient("GAMPING").GetAsync(SyncPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Sync_WithMissingLocation_Returns400()
    {
        var response = await _factory.CreateClient().GetAsync(SyncPath);

        //  §9 — missing X-Session-Location is malformed context → 400.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Sync_WithUnmappedLocation_Returns400()
    {
        var response = await SessionClient("UNKNOWN").GetAsync(SyncPath);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---- POST api/barcode-registration -----------------------------------

    [Fact]
    public async Task Submit_WithResolvedSession_RecordsResolvedUserIdAsRequestedBy()
    {
        SeedUser("U-SUBMIT", "submit@gmail.com");
        var client = SessionClient("GAMPING", "submit@gmail.com");

        var response = await client.PostAsJsonAsync(SubmitPath, new
        {
            ClientRequestId = "CR-SUBMIT",
            BarcodeValue = "BC-1",
            BrgId = "BRG-1",
            Satuan = "PCS"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var row = RegistrationDal().Items.Single(x => x.ClientRequestId == "CR-SUBMIT");
        //  TD-05 — the resolved BTR UserId is the stored RequestedBy.
        Assert.Equal("U-SUBMIT", row.RequestedBy);
        //  TD-03 — locationId is resolved to ServerId server-side.
        Assert.Equal("JOGJA", row.ServerId);
    }

    [Fact]
    public async Task Submit_WithUnresolvableActor_Returns409JSendFailure()
    {
        var client = SessionClient("GAMPING", "stranger@gmail.com");

        var response = await client.PostAsJsonAsync(SubmitPath, new
        {
            ClientRequestId = "CR-STRANGER",
            BarcodeValue = "BC-1",
            BrgId = "BRG-1",
            Satuan = "PCS"
        });

        //  TD-13 — actor no longer resolves → 409 Conflict (not 400).
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = System.Text.Json.JsonDocument.Parse(body);
        Assert.Equal("Conflict", doc.RootElement.GetProperty("status").GetString());
        Assert.Equal("409", doc.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Submit_WithMissingLocation_Returns400()
    {
        SeedUser("U-NOLOC", "noloc@gmail.com");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Session-Actor", "noloc@gmail.com");

        var response = await client.PostAsJsonAsync(SubmitPath, new
        {
            ClientRequestId = "CR-NOLOC",
            BarcodeValue = "BC-1",
            BrgId = "BRG-1",
            Satuan = "PCS"
        });

        //  §9 — malformed tenant context wins over actor resolution.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---- GET api/BarcodeRegistration/status ------------------------------

    [Fact]
    public async Task Status_ReturnsOnlyCallersOwnRequests()
    {
        SeedUser("U-STATUS-A", "status-a@gmail.com");
        SeedUser("U-STATUS-B", "status-b@gmail.com");
        SeedRegistration(RegistrationRow("ID-A", "CR-STATUS-A", "JOGJA", "U-STATUS-A", "PENDING"));
        SeedRegistration(RegistrationRow("ID-B", "CR-STATUS-B", "JOGJA", "U-STATUS-B", "PENDING"));

        var response = await SessionClient("GAMPING", "status-a@gmail.com").GetAsync(StatusPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        //  TD-05 — RequestedBy filter preserved: only the caller's row.
        Assert.Contains("CR-STATUS-A", body);
        Assert.DoesNotContain("CR-STATUS-B", body);
    }

    [Fact]
    public async Task Status_WithUnresolvableActor_Returns409()
    {
        var response = await SessionClient("GAMPING", "stranger@gmail.com").GetAsync(StatusPath);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // ---- pending keeps claim-based behavior (token-bearing consumer) -----

    [Fact]
    public async Task Pending_WithBearerToken_UsesClaimBasedServerId()
    {
        SeedRegistration(RegistrationRow("ID-P", "CR-PENDING", "JOGJA", "U-ANY", "PENDING"));

        var response = await TokenClient("JOGJA").GetAsync(PendingPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        //  j07-btrade-sync still sends a bearer; its claims still select the
        //  tenant even though [Authorize] was removed.
        Assert.Contains("CR-PENDING", body);
    }
}
