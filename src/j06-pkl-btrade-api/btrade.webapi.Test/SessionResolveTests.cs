using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using btrade.application.Contract;
using btrade.application.UseCase;
using btrade.domain.BarcodeFeature;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace btrade.webapi.Test;

/// <summary>
/// Integration tests for POST api/session/resolve (TD-02, §9 Error Semantics,
/// §10 payload contract) and the POST api/User Email ingest path (EXT-02).
/// DALs are faked per the FakeBarcodeDal convention; no database is required.
/// </summary>
public class SessionResolveTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string ResolvePath = "/api/session/resolve";
    private const string UserSyncPath = "/api/User";

    private readonly WebApplicationFactory<Program> _factory;

    public SessionResolveTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IUserDal, FakeUserDal>();
                services.AddSingleton<IWarehouseMappingDal, FakeWarehouseMappingDal>();
            }));
    }

    private void SeedUser(UserType user)
    {
        using var scope = _factory.Services.CreateScope();
        var userDal = scope.ServiceProvider.GetRequiredService<IUserDal>();
        ((FakeUserDal)userDal).Insert(user);
    }

    private UserType? FindUser(string userId)
    {
        using var scope = _factory.Services.CreateScope();
        var userDal = scope.ServiceProvider.GetRequiredService<IUserDal>();
        var result = userDal.GetData(new UserKey(userId));
        return result.HasValue ? result.Value : null;
    }

    private static UserType ActiveUser(string userId, string email) =>
        new(userId, $"Operator {userId}", "hash", "WHS", true, "JOGJA", email);

    [Fact]
    public async Task Resolve_WithRegisteredActiveEmail_ReturnsIdentityAndWarehouseMapping()
    {
        SeedUser(ActiveUser("U-OK", "operator@gmail.com"));
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(ResolvePath,
            new { Email = "  OPERATOR@Gmail.COM  " });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        //  §10 / TD-02 envelope + PascalCase payload fields, verbatim.
        Assert.Equal("success", doc.RootElement.GetProperty("status").GetString());
        Assert.Equal("200", doc.RootElement.GetProperty("code").GetString());
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal("U-OK", data.GetProperty("UserId").GetString());
        Assert.Equal("Operator U-OK", data.GetProperty("UserName").GetString());
        Assert.Equal("WHS", data.GetProperty("RoleId").GetString());

        var warehouses = data.GetProperty("Warehouses").EnumerateArray()
            .Select(item => (
                LocationId: item.GetProperty("LocationId").GetString(),
                ServerId: item.GetProperty("ServerId").GetString()))
            .ToList();
        Assert.Equal(3, warehouses.Count);
        Assert.Equal("JOGJA", warehouses.Single(w => w.LocationId == "GAMPING").ServerId);
    }

    [Fact]
    public async Task Resolve_WithUnmappedEmail_Returns400JSendFailure()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(ResolvePath,
            new { Email = "stranger@gmail.com" });

        //  §9 — unmapped Google account at POST api/session/resolve → 400.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.Equal("Bad Request", doc.RootElement.GetProperty("status").GetString());
        Assert.Equal("400", doc.RootElement.GetProperty("code").GetString());
        Assert.Contains("administrator", doc.RootElement.GetProperty("data").GetString());
    }

    [Fact]
    public async Task Resolve_WithInactiveAccount_Returns400JSendFailure()
    {
        SeedUser(ActiveUser("U-INACTIVE", "inactive@gmail.com") with { IsAktif = false });
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(ResolvePath,
            new { Email = "inactive@gmail.com" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.Equal("400", doc.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Resolve_WarehouseList_ContainsDistinctGampingAndConcatEntries()
    {
        SeedUser(ActiveUser("U-WH", "warehouse@gmail.com"));
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(ResolvePath,
            new { Email = "warehouse@gmail.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var warehouses = doc.RootElement.GetProperty("data")
            .GetProperty("Warehouses").EnumerateArray()
            .Select(item => (
                LocationId: item.GetProperty("LocationId").GetString(),
                ServerId: item.GetProperty("ServerId").GetString()))
            .ToList();

        //  GAMPING and CONCAT are distinct locations even though both resolve
        //  to ServerId JOGJA (BTR_WarehouseMapping seed); MAGELANG → MGL.
        var locationIds = warehouses.Select(w => w.LocationId).ToList();
        Assert.Equal(3, locationIds.Distinct().Count());
        Assert.Contains("GAMPING", locationIds);
        Assert.Contains("CONCAT", locationIds);
        Assert.Contains("MAGELANG", locationIds);
        Assert.Equal("JOGJA", warehouses.Single(w => w.LocationId == "GAMPING").ServerId);
        Assert.Equal("JOGJA", warehouses.Single(w => w.LocationId == "CONCAT").ServerId);
        Assert.Equal("MGL", warehouses.Single(w => w.LocationId == "MAGELANG").ServerId);
    }

    [Fact]
    public async Task UserSync_ThenResolve_EmailProjectionRoundTrip()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", IssueSyncToken());

        //  Legacy j07-btrade-sync shape (EXT-02): the second user carries no
        //  Email member at all.
        var payload = "{\"ListUser\":[" +
            "{\"UserId\":\"U-PROJ\",\"UserName\":\"Projected\",\"Password\":\"h\"," +
            "\"RoleId\":\"WHS\",\"IsAktif\":true,\"ServerId\":\"\",\"" +
            "Email\":\"projected@gmail.com\"}," +
            "{\"UserId\":\"U-NOEMAIL\",\"UserName\":\"No Email\",\"Password\":\"h\"," +
            "\"RoleId\":\"WHS\",\"IsAktif\":true,\"ServerId\":\"\"}" +
            "]}";

        var syncResponse = await client.PostAsync(UserSyncPath,
            new StringContent(payload, System.Text.Encoding.UTF8, "application/json"));

        //  Ingested without error (EXT-02 tolerance).
        Assert.Equal(HttpStatusCode.OK, syncResponse.StatusCode);
        Assert.Equal(string.Empty, FindUser("U-NOEMAIL")?.Email);
        Assert.Equal("projected@gmail.com", FindUser("U-PROJ")?.Email);

        var resolveResponse = await client.PostAsJsonAsync(ResolvePath,
            new { Email = "PROJECTED@GMAIL.COM" });

        Assert.Equal(HttpStatusCode.OK, resolveResponse.StatusCode);
        var body = await resolveResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.Equal("U-PROJ", doc.RootElement.GetProperty("data")
            .GetProperty("UserId").GetString());
    }

    private string IssueSyncToken()
    {
        using var scope = _factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        return jwtTokenService
            .GenerateToken("SYNC-USER", "SYNC_ROLE", "GAMPING", "JOGJA")
            .Token;
    }
}
