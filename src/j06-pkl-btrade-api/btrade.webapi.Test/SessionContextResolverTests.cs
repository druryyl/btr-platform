using System.Text.Json;
using btrade.application.UseCase;
using btrade.domain.BarcodeFeature;
using btrade.domain.SalesFeature;
using Xunit;

namespace btrade.webapi.Test;

/// <summary>
/// SessionContextResolver unit tests (TD-02/TD-04/TD-14) plus the EXT-02
/// serializer-level check that a POST api/User payload without Email binds to
/// an empty string.
/// </summary>
public class SessionContextResolverTests
{
    private static SessionContextResolver BuildResolver(params UserType[] users)
    {
        var userDal = new FakeUserDal();
        foreach (var user in users)
            userDal.Insert(user);
        return new SessionContextResolver(userDal, new FakeWarehouseMappingDal());
    }

    private static UserType ActiveUser(string email, string userId = "U-RESOLVE") =>
        new(userId, "Operator One", "hash", "WHS", true, "JOGJA", email);

    [Fact]
    public void ResolveAccount_RegisteredActiveEmail_ReturnsBtrIdentity()
    {
        var resolver = BuildResolver(ActiveUser("operator@gmail.com"));

        var account = resolver.ResolveAccount("operator@gmail.com");

        Assert.Equal("U-RESOLVE", account.UserId);
        Assert.Equal("Operator One", account.UserName);
        Assert.Equal("WHS", account.RoleId);
    }

    [Fact]
    public void ResolveAccount_TrimsAndIgnoresCase_MatchesStoredEmail()
    {
        var resolver = BuildResolver(ActiveUser("operator@gmail.com"));

        var account = resolver.ResolveAccount("  OPERATOR@Gmail.COM  ");

        Assert.Equal("U-RESOLVE", account.UserId);
    }

    [Fact]
    public void ResolveAccount_UnmappedEmail_ThrowsAccountUnresolvable()
    {
        var resolver = BuildResolver(ActiveUser("operator@gmail.com"));

        var ex = Assert.Throws<SessionAccountUnresolvableException>(
            () => resolver.ResolveAccount("stranger@gmail.com"));

        //  Distinct from location-unmapped (ArgumentException-derived) so a
        //  later slice can map actor-unresolvable to 409 without ambiguity.
        Assert.False(typeof(ArgumentException).IsAssignableFrom(ex.GetType()));
        Assert.Contains("administrator", ex.Message);
    }

    [Fact]
    public void ResolveAccount_InactiveUser_ThrowsAccountUnresolvable()
    {
        var resolver = BuildResolver(
            ActiveUser("inactive@gmail.com") with { IsAktif = false });

        Assert.Throws<SessionAccountUnresolvableException>(
            () => resolver.ResolveAccount("inactive@gmail.com"));
    }

    [Fact]
    public void ResolveAccount_BlankEmail_ThrowsAccountUnresolvable()
    {
        var resolver = BuildResolver(ActiveUser("operator@gmail.com"));

        Assert.Throws<SessionAccountUnresolvableException>(
            () => resolver.ResolveAccount("   "));
        Assert.Throws<SessionAccountUnresolvableException>(
            () => resolver.ResolveAccount(string.Empty));
    }

    [Fact]
    public void ResolveAccount_UnmappedEmptyEmailRow_IsNeverMatchable()
    {
        //  A user with Email = '' (unmapped, EXT-00/§8) must not be resolvable
        //  through an empty lookup.
        var resolver = BuildResolver(
            ActiveUser(string.Empty, userId: "U-UNMAPPED"));

        Assert.Throws<SessionAccountUnresolvableException>(
            () => resolver.ResolveAccount(string.Empty));
    }

    [Fact]
    public void ResolveTenant_MappedLocation_ReturnsServerId()
    {
        var resolver = BuildResolver();

        Assert.Equal("JOGJA", resolver.ResolveTenant("GAMPING"));
        Assert.Equal("JOGJA", resolver.ResolveTenant("CONCAT"));
        Assert.Equal("MGL", resolver.ResolveTenant("MAGELANG"));
    }

    [Fact]
    public void ResolveTenant_UnmappedLocation_ThrowsExplicitly_WithoutFallback()
    {
        var resolver = BuildResolver();

        var ex = Assert.Throws<SessionTenantUnresolvableException>(
            () => resolver.ResolveTenant("UNKNOWN"));

        //  Mirrors IssueTokenCommand's unmapped-warehouse failure verbatim and
        //  stays an ArgumentException so ErrorHandlerMiddleware maps it to 400.
        Assert.Equal("Invalid warehouse (UNKNOWN)", ex.Message);
        Assert.IsAssignableFrom<ArgumentException>(ex);
    }

    [Fact]
    public void ListWarehouses_ReturnsDistinctGampingConcatMappings_AsSeeded()
    {
        var resolver = BuildResolver();

        var warehouses = resolver.ListWarehouses().ToList();

        Assert.Equal(3, warehouses.Count);
        Assert.Equal(3, warehouses.Select(w => w.WarehouseCode).Distinct().Count());
        Assert.Equal("JOGJA", warehouses.Single(w => w.WarehouseCode == "GAMPING").ServerId);
        Assert.Equal("JOGJA", warehouses.Single(w => w.WarehouseCode == "CONCAT").ServerId);
        Assert.Equal("MGL", warehouses.Single(w => w.WarehouseCode == "MAGELANG").ServerId);
    }

    [Fact]
    public void UserType_PayloadWithoutEmail_BindsToEmptyEmail_Ext02Tolerance()
    {
        //  Legacy j07-btrade-sync still deploying (EXT-02): the JSON payload
        //  carries no Email member; the model default keeps the ingest
        //  NOT NULL-safe.
        const string json =
            "{\"UserId\":\"U-LEGACY\",\"UserName\":\"Legacy\",\"Password\":\"p\"," +
            "\"RoleId\":\"WHS\",\"IsAktif\":true,\"ServerId\":\"JOGJA\"}";

        var user = JsonSerializer.Deserialize<UserType>(
            json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(user);
        Assert.Equal(string.Empty, user!.Email);
    }
}
