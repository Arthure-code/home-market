using HomeMarket.Api.Models;

namespace HomeMarket.Api.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) Issue(Account account);
    }
}
