using Ardalis.Result;
using HomeMarket.Api.Data;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using HomeMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeMarket.Api.Services
{
    public class CartService : ICartService
    {
        private const string NotEnoughStock = "There are not that many left in stock.";
        private readonly MarketContext _context;
        private readonly IPhotoStore _photos;

        public CartService(MarketContext context, IPhotoStore photos)
        {
            _context = context;
            _photos = photos;
        }

        public async Task<CartDto> GetAsync(int accountId)
        {
            var lines = await _context.CartLines
                .Where(l => l.AccountId == accountId)
                .OrderBy(l => l.AddedAt)
                .Select(l => new CartLineDto
                {
                    ProductId = l.ProductId,
                    Title = l.Product!.Title,
                    PhotoUrl = l.Product.Photo,
                    Seller = l.Product.Seller!.UserName,
                    UnitPrice = l.Product.Price,
                    Quantity = l.Quantity,
                    Stock = l.Product.Stock,
                    LineTotal = l.Product.Price * l.Quantity,
                })
                .ToListAsync();
            foreach (var line in lines)
            {
                line.PhotoUrl = _photos.UrlFor(line.PhotoUrl);
            }
            return Build(lines);
        }

        // Adding what is already there adds to its quantity, up to the
        // stock and the ten-per-order ceiling.
        public async Task<Result<CartDto>> AddAsync(int accountId, int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product is null) return Result<CartDto>.NotFound();
            if (product.SellerId == accountId) return Result<CartDto>.Invalid(new ValidationError(nameof(productId), "That is your own listing."));

            var line = await _context.CartLines.SingleOrDefaultAsync(l => l.AccountId == accountId && l.ProductId == productId);
            var wanted = Math.Min((line?.Quantity ?? 0) + quantity, CartAddRequest.MaxQuantity);
            if (wanted > product.Stock) return Result<CartDto>.Conflict(NotEnoughStock);

            if (line is null)
            {
                _context.CartLines.Add(new CartLine { AccountId = accountId, ProductId = productId, Quantity = wanted, AddedAt = DateTime.UtcNow });
            }
            else
            {
                line.Quantity = wanted;
            }
            await _context.SaveChangesAsync();
            return Result<CartDto>.Success(await GetAsync(accountId));
        }

        public async Task<Result<CartDto>> SetQuantityAsync(int accountId, int productId, int quantity)
        {
            var line = await _context.CartLines.Include(l => l.Product).SingleOrDefaultAsync(l => l.AccountId == accountId && l.ProductId == productId);
            if (line is null) return Result<CartDto>.NotFound();
            if (quantity > line.Product!.Stock) return Result<CartDto>.Conflict(NotEnoughStock);

            line.Quantity = quantity;
            await _context.SaveChangesAsync();
            return Result<CartDto>.Success(await GetAsync(accountId));
        }

        public async Task<Result<CartDto>> RemoveAsync(int accountId, int productId)
        {
            var line = await _context.CartLines.SingleOrDefaultAsync(l => l.AccountId == accountId && l.ProductId == productId);
            if (line is null) return Result<CartDto>.NotFound();

            _context.CartLines.Remove(line);
            await _context.SaveChangesAsync();
            return Result<CartDto>.Success(await GetAsync(accountId));
        }

        private static CartDto Build(List<CartLineDto> lines)
        {
            var subtotal = lines.Sum(l => l.LineTotal);
            return new CartDto
            {
                Lines = lines,
                ItemCount = lines.Sum(l => l.Quantity),
                Subtotal = subtotal,
                Gst = Taxes.Gst(subtotal),
                Qst = Taxes.Qst(subtotal),
                Total = Taxes.Total(subtotal),
            };
        }
    }
}
