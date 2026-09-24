using System.Net;
using System.Net.Http.Headers;
using System.Text;
using btrade.application.Contract;
using btrade.webapi.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace btrade.webapi.Test;

/// <summary>
/// JWT issuance/validation tests. Since TD-06 removed [Authorize] from the
/// BGud-consumed controllers, the remaining authenticated endpoint is
/// POST api/User (UserController keeps [Authorize]; j07-btrade-sync still
/// authenticates for it). The BGud routes are now anonymous and are covered by
/// BarcodeSessionContextTests.
/// </summary>
public class JwtAuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string AuthenticatedEndpoint = "/api/User";

    private readonly WebApplicationFactory<Program> _factory;

    public JwtAuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IBarcodeDal, FakeBarcodeDal>();
                services.AddSingleton<IUserDal, FakeUserDal>();
            }));
    }

    private static StringContent EmptyUserSyncPayload() =>
        new("{\"ListUser\":[]}", Encoding.UTF8, "application/json");

    [Fact]
    public async Task AuthenticatedEndpoint_WithNoToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync(AuthenticatedEndpoint, EmptyUserSyncPayload());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedEndpoint_WithInvalidToken_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "not-a-valid-token");

        var response = await client.PostAsync(AuthenticatedEndpoint, EmptyUserSyncPayload());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedEndpoint_WithIssuedToken_Returns200()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", IssueToken());

        var response = await client.PostAsync(AuthenticatedEndpoint, EmptyUserSyncPayload());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void IssuedToken_IsAccepted_And_ExposesApprovedClaims()
    {
        var token = IssueToken();
        var validator = new JsonWebTokenSecurityTokenValidator();

        var principal = validator.ValidateToken(token, BuildValidationParameters(), out _);

        Assert.Equal("USER-1", principal.FindFirst("sub")?.Value);
        Assert.Equal("OFFICE_ADMIN", principal.FindFirst("role")?.Value);
        Assert.Equal("JOGJA", principal.FindFirst("locationId")?.Value);
        Assert.Equal("SRV-1", principal.FindFirst("serverId")?.Value);
    }

    private string IssueToken()
    {
        using var scope = _factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        return jwtTokenService.GenerateToken("USER-1", "OFFICE_ADMIN", "JOGJA", "SRV-1").Token;
    }

    private TokenValidationParameters BuildValidationParameters()
    {
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidIssuer = configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty))
        };
    }
}
