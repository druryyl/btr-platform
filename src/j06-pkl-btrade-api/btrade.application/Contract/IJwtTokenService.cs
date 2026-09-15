namespace btrade.application.Contract;

public record JwtTokenResult(string Token, DateTime ExpiresAt);

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(string userId, string role, string locationId, string serverId);
}
