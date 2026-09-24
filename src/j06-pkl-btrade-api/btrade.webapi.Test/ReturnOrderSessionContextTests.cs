using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using btrade.application.Contract;
using btrade.domain.BarcodeFeature;
using btrade.domain.DriverFeature;
using btrade.domain.ReturnOrderFeature;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace btrade.webapi.Test;

/// <summary>
/// P4-S06 — integration tests for the BGud return-order and driver routes under
/// the anonymous session-context contract (TD-03/TD-04/TD-05/TD-06/TD-13/TD-15):
/// POST api/return-order resolves tenant/actor from the fixed headers and
/// persists the resolved UserId as SubmittedBy; the incremental download emits
/// SubmittedBy; GET api/Driver/{serverId} is an anonymous legacy read; and
/// POST api/Driver keeps its body-bound behavior for token-bearing consumers.
/// </summary>
public class ReturnOrderSessionContextTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string SubmitPath = "/api/return-order";
    private const string IncrementalPath = "/api/ReturnOrder/incremental/2026-01-01/2026-12-31/{0}";
    private const string DriverGetPath = "/api/Driver/{0}";
    private const string DriverPostPath = "/api/Driver";

    private readonly WebApplicationFactory<Program> _factory;

    public ReturnOrderSessionContextTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IUserDal, FakeUserDal>();
                services.AddSingleton<IWarehouseMappingDal, FakeWarehouseMappingDal>();
                services.AddSingleton<IReturnOrderDal, FakeReturnOrderDal>();
                services.AddSingleton<IReturnOrderItemDal, FakeReturnOrderItemDal>();
                services.AddSingleton<IDriverDal, FakeDriverDal>();
            }));
    }

    private void SeedUser(string userId, string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userDal = scope.ServiceProvider.GetRequiredService<IUserDal>();
        ((FakeUserDal)userDal).Insert(
            new UserType(userId, $"Operator {userId}", "hash", "WHS", true, "JOGJA", email));
    }

    private void SeedReturnOrder(ReturnOrderType row)
    {
        using var scope = _factory.Services.CreateScope();
        var dal = scope.ServiceProvider.GetRequiredService<IReturnOrderDal>();
        ((FakeReturnOrderDal)dal).Insert(row);
    }

    private FakeReturnOrderDal ReturnOrderDal()
    {
        using var scope = _factory.Services.CreateScope();
        return (FakeReturnOrderDal)scope.ServiceProvider.GetRequiredService<IReturnOrderDal>();
    }

    private HttpClient SessionClient(string location, string? actor = null)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Session-Location", location);
        if (actor is not null)
            client.DefaultRequestHeaders.Add("X-Session-Actor", actor);
        return client;
    }

    private HttpClient TokenClient()
    {
        using var scope = _factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var token = jwtTokenService
            .GenerateToken("SYNC-USER", "SYNC_ROLE", "GAMPING", "JOGJA")
            .Token;
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static object SubmitBody() => new
    {
        ReturnOrderId = "RO-1",
        ReturnOrderDate = "2026-01-05",
        WarehouseCode = "GAMPING",
        CustomerId = "C1",
        CustomerName = "Customer",
        SalesPersonId = "S1",
        SalesPersonName = "Sales",
        DriverId = "D1",
        DriverName = "Driver",
        Note = "note",
        ListItem = Array.Empty<object>()
    };

    private static ReturnOrderType ReturnOrderRow(string id, string serverId, string submittedBy) =>
        new(id, serverId, "2026-01-05", "GAMPING", "C1", "Customer", "S1", "Sales",
            "D1", "Driver", "note", "TERKIRIM", submittedBy);

    // ---- POST api/return-order -------------------------------------------

    [Fact]
    public async Task Submit_WithResolvedSession_NoToken_RecordsResolvedUserIdAsSubmittedBy()
    {
        //  TD-06 — anonymous: no bearer, no 401; context from the headers.
        SeedUser("U-RO", "ro@gmail.com");
        var client = SessionClient("GAMPING", "ro@gmail.com");

        var response = await client.PostAsJsonAsync(SubmitPath, SubmitBody());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var row = ReturnOrderDal().Items.Single(x => x.ReturnOrderId == "RO-1");
        //  TD-05 — the resolved BTR UserId is the stored SubmittedBy.
        Assert.Equal("U-RO", row.SubmittedBy);
        //  TD-03 — locationId is resolved to ServerId server-side.
        Assert.Equal("JOGJA", row.ServerId);
        Assert.Equal("TERKIRIM", row.StatusSync);
    }

    [Fact]
    public async Task Submit_WithUnresolvableActor_Returns409JSendFailure()
    {
        var client = SessionClient("GAMPING", "stranger@gmail.com");

        var response = await client.PostAsJsonAsync(SubmitPath, SubmitBody());

        //  TD-13 — actor no longer resolves → 409 Conflict (not 400).
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.Equal("Conflict", doc.RootElement.GetProperty("status").GetString());
        Assert.Equal("409", doc.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Submit_WithMissingLocation_Returns400()
    {
        SeedUser("U-NOLOC", "noloc@gmail.com");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Session-Actor", "noloc@gmail.com");

        var response = await client.PostAsJsonAsync(SubmitPath, SubmitBody());

        //  §9 — missing X-Session-Location is malformed context → 400.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_WithUnmappedLocation_Returns400()
    {
        SeedUser("U-BADLOC", "badloc@gmail.com");
        var client = SessionClient("UNKNOWN", "badloc@gmail.com");

        var response = await client.PostAsJsonAsync(SubmitPath, SubmitBody());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---- GET api/ReturnOrder/incremental/... -----------------------------

    [Fact]
    public async Task Incremental_NoToken_Returns200_AndEmitsSubmittedByPascalCase()
    {
        SeedReturnOrder(ReturnOrderRow("RO-INC", "JOGJA", "U-RO"));

        //  No session headers and no bearer: the incremental route keeps its
        //  existing {serverId} path contract and is anonymous (TD-06).
        var response = await _factory.CreateClient()
            .GetAsync(string.Format(IncrementalPath, "JOGJA"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        Assert.Equal("success", doc.RootElement.GetProperty("status").GetString());
        var data = doc.RootElement.GetProperty("data");
        var item = data.EnumerateArray().Single();
        //  TD-15 — the download carries the resolved operator UserId so
        //  j07-btrade-sync can relay it; §10 PascalCase payload fields.
        Assert.Equal("U-RO", item.GetProperty("SubmittedBy").GetString());
        Assert.Equal("RO-INC", item.GetProperty("ReturnOrderId").GetString());
    }

    [Fact]
    public async Task Incremental_WithNoRows_NoToken_Returns200EmptyList()
    {
        var response = await _factory.CreateClient()
            .GetAsync(string.Format(IncrementalPath, "JOGJA"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.Equal(0, doc.RootElement.GetProperty("data").GetArrayLength());
    }

    // ---- Driver routes ---------------------------------------------------

    [Fact]
    public async Task DriverGet_NoToken_Returns200()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var dal = (FakeDriverDal)scope.ServiceProvider.GetRequiredService<IDriverDal>();
            dal.Insert(new DriverType("D1", "Driver", true, "JOGJA"));
        }

        //  TD-06/TD-08 — legacy read route, anonymous, client-supplied serverId.
        var response = await _factory.CreateClient()
            .GetAsync(string.Format(DriverGetPath, "JOGJA"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("D1", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task DriverPost_WithBearerToken_KeepsBodyBoundBehavior()
    {
        //  j07-btrade-sync still sends a bearer; the sync path keeps its
        //  body-bound ServerId behavior (no JWT claim is read).
        var client = TokenClient();

        var response = await client.PostAsJsonAsync(DriverPostPath, new
        {
            ListDriver = new[]
            {
                new { DriverId = "D1", DriverName = "Driver", IsAktif = true, ServerId = "JOGJA" }
            },
            ServerId = "JOGJA"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = _factory.Services.CreateScope();
        var dal = (FakeDriverDal)scope.ServiceProvider.GetRequiredService<IDriverDal>();
        Assert.Contains(dal.Items, x => x.DriverId == "D1" && x.ServerId == "JOGJA");
    }
}
