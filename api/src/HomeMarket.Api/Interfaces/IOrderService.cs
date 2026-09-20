using HomeMarket.Api.Dtos;

namespace HomeMarket.Api.Interfaces
{
    public enum OrderOutcome
    {
        Done,
        EmptyCart,
        NotEnoughStock,
        CardRefused,
    }

    // Orders are placed from the cart of the account named by the token,
    // read back by their buyer, and their lines listed to their sellers.
    public interface IOrderService
    {
        Task<(OrderOutcome Outcome, OrderDto? Order, string Detail)> PlaceAsync(int buyerId, CheckoutRequest request);
        Task<IReadOnlyList<OrderDto>> MineAsync(int buyerId);
        Task<OrderDto?> GetAsync(int buyerId, int id);
        Task<IReadOnlyList<SaleDto>> SalesAsync(int sellerId);
    }
}
