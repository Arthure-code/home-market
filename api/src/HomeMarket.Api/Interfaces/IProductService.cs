using Ardalis.Result;
using HomeMarket.Api.Dtos;

namespace HomeMarket.Api.Interfaces
{
    // The viewer is the signed-in account, or null for a visitor: the
    // catalogue is public, liked and mine are computed for the viewer.
    // The list can be narrowed by a search text and by a category. What
    // can be refused comes back as a Result, as in the eShopOnWeb
    // reference: NotFound, Forbidden for someone else's listing, Invalid
    // for a photo the server never stored or a category not in the shop.
    public interface IProductService
    {
        Task<IReadOnlyList<ProductDto>> ListAsync(int? viewerId, string? query = null, string? category = null);
        Task<IReadOnlyList<CategoryDto>> CategoriesAsync();
        Task<ProductDto?> GetAsync(int id, int? viewerId);
        Task<IReadOnlyList<ProductDto>> MineAsync(int viewerId);
        Task<IReadOnlyList<ProductDto>> LikedAsync(int viewerId);
        Task<Result<ProductDto>> CreateAsync(int sellerId, ProductRequest request);
        Task<Result<ProductDto>> UpdateAsync(int sellerId, int id, ProductRequest request);
        Task<Result> DeleteAsync(int sellerId, int id);
        Task<Result> SetLikeAsync(int viewerId, int id, bool liked);
    }
}
