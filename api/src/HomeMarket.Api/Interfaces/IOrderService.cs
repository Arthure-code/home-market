using Ardalis.Result;
using HomeMarket.Api.Dtos;

namespace HomeMarket.Api.Interfaces
{
    // Orders are placed from the cart of the account named by the token,
    // read back by their buyer, and their lines listed to their sellers.
    // Placing one is Invalid on an empty cart, Conflict when the stock
    // went meanwhile, and Error, with the provider's reason, when the
    // card is refused.
    public interface IOrderService
    {
        Task<Result<OrderDto>> PlaceAsync(int buyerId, CheckoutRequest request);
        Task<IReadOnlyList<OrderDto>> MineAsync(int buyerId);
        Task<OrderDto?> GetAsync(int buyerId, int id);
        Task<IReadOnlyList<SaleDto>> SalesAsync(int sellerId);
    }
}
