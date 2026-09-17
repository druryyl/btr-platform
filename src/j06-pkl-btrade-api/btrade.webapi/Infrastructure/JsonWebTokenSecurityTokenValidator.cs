using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace btrade.webapi.Infrastructure;

public class JsonWebTokenSecurityTokenValidator : ISecurityTokenValidator
{
    private readonly JsonWebTokenHandler _handler = new();

    public bool CanValidateToken => _handler.CanValidateToken;

    public int MaximumTokenSizeInBytes
    {
        get => _handler.MaximumTokenSizeInBytes;
        set => _handler.MaximumTokenSizeInBytes = value;
    }

    public bool CanReadToken(string securityToken) => _handler.CanReadToken(securityToken);

    public ClaimsPrincipal ValidateToken(string securityToken,
        TokenValidationParameters validationParameters,
        out SecurityToken validatedToken)
    {
        var result = _handler.ValidateTokenAsync(securityToken, validationParameters)
            .GetAwaiter().GetResult();
        if (!result.IsValid)
            throw result.Exception ?? new SecurityTokenValidationException("Token validation failed.");

        validatedToken = result.SecurityToken;
        return new ClaimsPrincipal(result.ClaimsIdentity);
    }
}
