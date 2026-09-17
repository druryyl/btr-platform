using System.Security.Claims;

namespace btrade.webapi.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    private const string ServerIdClaim = "serverId";
    private const string SubjectClaim = "sub";

    public static string GetServerId(this ClaimsPrincipal user)
        => user.FindFirst(ServerIdClaim)?.Value ?? string.Empty;

    public static string GetUserId(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? user.FindFirst(SubjectClaim)?.Value
           ?? string.Empty;
}
