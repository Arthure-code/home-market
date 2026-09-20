using HomeMarket.Api.Dtos;

namespace HomeMarket.Api.Interfaces
{
    public enum ProductOutcome
    {
        Done,
        NotFound,
        NotMine,
        BadPhoto,
        BadCategory,
    }

    // The viewer is the signed-in account, or null for a visitor: the
    // catalogue is public, liked and mine are computed for the viewer.
    // The list can be narrowed by a search text and by a category.
    public interface IProductService
    {
        Task<IReadOnlyList<ProductDto>> ListAsync(int? viewerId, string? query = null, string? category = null);
        Task<IReadOnlyList<CategoryDto>> CategoriesAsync();
        Task<ProductDto?> GetAsync(int id, int? viewerId);
        Task<IReadOnlyList<ProductDto>> MineAsync(int viewerId);
        Task<IReadOnlyList<ProductDto>> LikedAsync(int viewerId);
        Task<(ProductOutcome Outcome, ProductDto? Product)> CreateAsync(int sellerId, ProductRequest request);
        Task<(ProductOutcome Outcome, ProductDto? Product)> UpdateAsync(int sellerId, int id, ProductRequest request);
        Task<ProductOutcome> DeleteAsync(int sellerId, int id);
        Task<ProductOutcome> SetLikeAsync(int viewerId, int id, bool liked);
    }
}
