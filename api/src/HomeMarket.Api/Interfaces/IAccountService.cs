using Ardalis.Result;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Models;

namespace HomeMarket.Api.Interfaces
{
    public interface IAccountService
    {
        // Conflict when the user name is taken.
        Task<Result> RegisterAsync(RegisterRequest request);
        Task<Account?> AuthenticateAsync(LoginRequest request);
    }
}
