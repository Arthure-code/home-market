using Ardalis.Result;
using HomeMarket.Api.Dtos;

namespace HomeMarket.Api.Interfaces
{
    // The cart of the account named by the token. Nothing of your own
    // goes in it (Invalid), nothing missing (NotFound), and never more
    // than the stock (Conflict). Every change answers with the whole cart.
    public interface ICartService
    {
        Task<CartDto> GetAsync(int accountId);
        Task<Result<CartDto>> AddAsync(int accountId, int productId, int quantity);
        Task<Result<CartDto>> SetQuantityAsync(int accountId, int productId, int quantity);
        Task<Result<CartDto>> RemoveAsync(int accountId, int productId);
    }
}
