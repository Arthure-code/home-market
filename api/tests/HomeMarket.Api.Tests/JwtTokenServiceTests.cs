using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HomeMarket.Api.Auth;
using HomeMarket.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HomeMarket.Api.Tests
{
    public class JwtTokenServiceTests
    {
        private const string Key = "a-test-key-of-at-least-thirty-two-bytes!";

        [Fact]
        public void Issue_CarriesTheIdAndTheNameAndNothingElseAboutTheAccount()
        {
            // Given
            var options = new JwtOptions { Key = Key, Issuer = "home-market", Audience = "home-market", LifetimeMinutes = 60 };
            var service = new JwtTokenService(Options.Create(options));
            var account = new Account { Id = 7, UserName = "nadia", PasswordHash = "hash" };

            // When
            var (token, expiresAt) = service.Issue(account);

            // Then
            var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.Equal("7", parsed.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid").Value);
            Assert.Equal("nadia", parsed.Claims.Single(c => c.Type == ClaimTypes.Name || c.Type == "unique_name").Value);
            Assert.DoesNotContain("hash", token);
            Assert.Equal("home-market", parsed.Issuer);
            Assert.InRange(expiresAt, DateTime.UtcNow.AddMinutes(59), DateTime.UtcNow.AddMinutes(61));
        }

        [Fact]
        public void Issue_SignsWithTheConfiguredKey_SoTheTokenValidatesAgainstIt()
        {
            // Given
            var options = new JwtOptions { Key = Key, LifetimeMinutes = 5 };
            var service = new JwtTokenService(Options.Create(options));
            var account = new Account { Id = 7, UserName = "nadia" };

            // When
            var (token, _) = service.Issue(account);

            // Then
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidIssuer = options.Issuer,
                ValidAudience = options.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            }, out _);
            Assert.Equal("7", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
