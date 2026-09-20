using HomeMarket.Api.Dtos;
using HomeMarket.Api.Models;

namespace HomeMarket.Api.Interfaces
{
    public enum RegisterOutcome
    {
        Created,
        NameTaken,
    }

    public interface IAccountService
    {
        Task<RegisterOutcome> RegisterAsync(RegisterRequest request);
        Task<Account?> AuthenticateAsync(LoginRequest request);
    }
}
