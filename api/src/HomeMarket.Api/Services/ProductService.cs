using Ardalis.Result;
using HomeMarket.Api.Data;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using HomeMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeMarket.Api.Services
{
    public class ProductService : IProductService
    {
        private readonly MarketContext _context;
        private readonly IPhotoStore _photos;

        public ProductService(MarketContext context, IPhotoStore photos)
        {
            _context = context;
            _photos = photos;
        }

        // The search looks at the title, the brand and the maker; the
        // category must match exactly. Both are optional.
        public Task<IReadOnlyList<ProductDto>> ListAsync(int? viewerId, string? query = null, string? category = null)
        {
            var products = _context.Products.AsQueryable();
            var text = query?.Trim() ?? string.Empty;
            if (text.Length > 0)
            {
                var pattern = $"%{text}%";
                products = products.Where(p => EF.Functions.Like(p.Title, pattern) || EF.Functions.Like(p.Brand, pattern) || EF.Functions.Like(p.Maker, pattern));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p => p.Category == category);
            }
            return Project(products.OrderByDescending(p => p.CreatedAt), viewerId);
        }

        // Every category of the shop, with its count, empty ones included.
        public async Task<IReadOnlyList<CategoryDto>> CategoriesAsync()
        {
            var counts = await _context.Products
                .GroupBy(p => p.Category)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Name, g => g.Count);
            return Category.All.Select(name => new CategoryDto { Name = name, Count = counts.GetValueOrDefault(name) }).ToList();
        }

        public async Task<ProductDto?> GetAsync(int id, int? viewerId)
        {
            var found = await Project(_context.Products.Where(p => p.Id == id), viewerId);
            return found.Count == 0 ? null : found[0];
        }

        public Task<IReadOnlyList<ProductDto>> MineAsync(int viewerId)
        {
            return Project(_context.Products.Where(p => p.SellerId == viewerId).OrderByDescending(p => p.CreatedAt), viewerId);
        }

        public Task<IReadOnlyList<ProductDto>> LikedAsync(int viewerId)
        {
            return Project(_context.Products.Where(p => p.LikedBy.Any(a => a.Id == viewerId)).OrderByDescending(p => p.CreatedAt), viewerId);
        }

        // The photo of a listing is empty, a file this API stored, or the
        // address the listing already had: never an address a client sends.
        // The category is one of the shop's.
        public async Task<Result<ProductDto>> CreateAsync(int sellerId, ProductRequest request)
        {
            var now = DateTime.UtcNow;
            var product = new Product { SellerId = sellerId, CreatedAt = now };
            var invalid = Apply(product, request, now);
            if (invalid is not null) return Result<ProductDto>.Invalid(invalid);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Result<ProductDto>.Success((await GetAsync(product.Id, sellerId))!);
        }

        // Only the seller changes or removes a listing. Someone else's
        // listing is reported as such, not as missing: it is public.
        public async Task<Result<ProductDto>> UpdateAsync(int sellerId, int id, ProductRequest request)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return Result<ProductDto>.NotFound();
            if (product.SellerId != sellerId) return Result<ProductDto>.Forbidden();
            var invalid = Apply(product, request, DateTime.UtcNow);
            if (invalid is not null) return Result<ProductDto>.Invalid(invalid);

            await _context.SaveChangesAsync();
            return Result<ProductDto>.Success((await GetAsync(id, sellerId))!);
        }

        public async Task<Result> DeleteAsync(int sellerId, int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return Result.NotFound();
            if (product.SellerId != sellerId) return Result.Forbidden();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        // Liking is idempotent: liking twice or unliking what was never
        // liked changes nothing and succeeds.
        public async Task<Result> SetLikeAsync(int viewerId, int id, bool liked)
        {
            var product = await _context.Products.Include(p => p.LikedBy).SingleOrDefaultAsync(p => p.Id == id);
            if (product is null) return Result.NotFound();

            var already = product.LikedBy.FirstOrDefault(a => a.Id == viewerId);
            if (liked && already is null)
            {
                var viewer = await _context.Accounts.FindAsync(viewerId);
                if (viewer is null) return Result.NotFound();
                product.LikedBy.Add(viewer);
            }
            else if (!liked && already is not null)
            {
                product.LikedBy.Remove(already);
            }
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        // Null when the request went on the product; the validation error
        // otherwise, and the product is left as it was.
        private ValidationError? Apply(Product product, ProductRequest request, DateTime at)
        {
            var photo = request.Photo.Trim();
            if (photo.Length > 0 && photo != product.Photo && !_photos.Exists(photo))
            {
                return new ValidationError(nameof(request.Photo), "Upload the photo first, then save the product.");
            }
            if (!Category.Exists(request.Category))
            {
                return new ValidationError(nameof(request.Category), "Pick one of the shop's categories.");
            }

            product.Title = request.Title.Trim();
            product.Brand = request.Brand.Trim();
            product.Maker = request.Maker.Trim();
            product.Description = request.Description.Trim();
            product.Price = request.Price;
            product.Category = request.Category;
            product.Stock = request.Stock;
            product.Photo = photo;
            product.UpdatedAt = at;
            return null;
        }

        private async Task<IReadOnlyList<ProductDto>> Project(IQueryable<Product> products, int? viewerId)
        {
            var rows = await products
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Brand,
                    p.Maker,
                    p.Description,
                    p.Price,
                    p.Category,
                    p.Stock,
                    p.Photo,
                    Seller = p.Seller!.UserName,
                    p.SellerId,
                    p.CreatedAt,
                    p.UpdatedAt,
                    Likes = p.LikedBy.Count,
                    Liked = viewerId != null && p.LikedBy.Any(a => a.Id == viewerId),
                })
                .ToListAsync();

            return rows.Select(r => new ProductDto
            {
                Id = r.Id,
                Title = r.Title,
                Brand = r.Brand,
                Maker = r.Maker,
                Description = r.Description,
                Price = r.Price,
                Category = r.Category,
                Stock = r.Stock,
                PhotoUrl = _photos.UrlFor(r.Photo),
                Seller = r.Seller,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Likes = r.Likes,
                Liked = r.Liked,
                Mine = viewerId == r.SellerId,
            }).ToList();
        }
    }
}
