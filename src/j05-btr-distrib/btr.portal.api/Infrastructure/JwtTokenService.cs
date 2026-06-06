using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace btr.portal.api.Infrastructure
{
    public interface IJwtTokenService
    {
        JwtTokenResult GenerateToken(string userId, string userName, string roleId, string roleName);
        bool TryValidateToken(string token, out ClaimsPrincipal principal);
    }

    public class JwtTokenResult
    {
        public string Token { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _options;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public JwtTokenResult GenerateToken(string userId, string userName, string roleId, string roleName)
        {
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);
            var claims = new List<Claim>
            {
                new Claim("userId", userId ?? string.Empty),
                new Claim("userName", userName ?? string.Empty),
                new Claim("roleId", roleId ?? string.Empty),
                new Claim("roleName", roleName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, userId ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Name, userName ?? string.Empty)
            };

            var signingKey = GetSigningKey();
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new JwtTokenResult
            {
                Token = _tokenHandler.WriteToken(token),
                ExpiresAtUtc = expiresAtUtc
            };
        }

        public bool TryValidateToken(string token, out ClaimsPrincipal principal)
        {
            principal = null;

            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _options.Issuer,
                    ValidAudience = _options.Audience,
                    IssuerSigningKey = GetSigningKey(),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (SecurityTokenException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private SymmetricSecurityKey GetSigningKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key ?? string.Empty));
        }
    }
}
