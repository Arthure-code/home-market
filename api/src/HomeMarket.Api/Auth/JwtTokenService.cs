using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HomeMarket.Api.Models;
using HomeMarket.Api.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HomeMarket.Api.Auth
{
    // Signs a short-lived token carrying only the account id and name.
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _options;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public (string Token, DateTime ExpiresAt) Issue(Account account)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_options.LifetimeMinutes);
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
                SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                    new Claim(ClaimTypes.Name, account.UserName),
                },
                expires: expiresAt,
                signingCredentials: credentials);
            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
