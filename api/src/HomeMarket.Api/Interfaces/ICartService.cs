using HomeMarket.Api.Dtos;

namespace HomeMarket.Api.Interfaces
{
    public enum CartOutcome
    {
        Done,
        NotFound,
        OwnProduct,
        NotEnoughStock,
    }

    // The cart of the account named by the token. Nothing of your own
    // goes in it, and never more than the stock.
    public interface ICartService
    {
        Task<CartDto> GetAsync(int accountId);
        Task<(CartOutcome Outcome, CartDto? Cart)> AddAsync(int accountId, int productId, int quantity);
        Task<(CartOutcome Outcome, CartDto? Cart)> SetQuantityAsync(int accountId, int productId, int quantity);
        Task<(CartOutcome Outcome, CartDto? Cart)> RemoveAsync(int accountId, int productId);
    }
}
