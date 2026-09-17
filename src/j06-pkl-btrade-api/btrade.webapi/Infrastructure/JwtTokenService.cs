using System.Security.Claims;
using System.Text;
using btrade.application.Contract;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace btrade.webapi.Infrastructure;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly JsonWebTokenHandler _tokenHandler = new();

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public JwtTokenResult GenerateToken(string userId, string role, string locationId, string serverId)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId ?? string.Empty),
            new("role", role ?? string.Empty),
            new("locationId", locationId ?? string.Empty),
            new("serverId", serverId ?? string.Empty)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key ?? string.Empty)),
            SecurityAlgorithms.HmacSha256);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Expires = expiresAt,
            SigningCredentials = credentials
        };

        return new JwtTokenResult(_tokenHandler.CreateToken(descriptor), expiresAt);
    }
}
