using Ardalis.Result;
using HomeMarket.Api.Data;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using HomeMarket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeMarket.Api.Services
{
    public class OrderService : IOrderService
    {
        private const string Currency = "CAD";
        private readonly MarketContext _context;
        private readonly IPaymentGateway _payments;

        public OrderService(MarketContext context, IPaymentGateway payments)
        {
            _context = context;
            _payments = payments;
        }

        // The cart becomes an order in one transaction: the stock is
        // checked line by line, the card is charged for the total, the
        // lines are copied with their price of the day, the stock goes
        // down and the cart is emptied. Nothing is written if the card is
        // refused; that refusal is an Error carrying the provider's reason.
        public async Task<Result<OrderDto>> PlaceAsync(int buyerId, CheckoutRequest request)
        {
            var lines = await _context.CartLines
                .Include(l => l.Product)
                .Where(l => l.AccountId == buyerId)
                .OrderBy(l => l.AddedAt)
                .ToListAsync();
            if (lines.Count == 0) return Result<OrderDto>.Invalid(new ValidationError("cart", "Your cart is empty."));

            var shortLine = lines.FirstOrDefault(l => l.Quantity > l.Product!.Stock);
            if (shortLine is not null)
            {
                return Result<OrderDto>.Conflict($"Only {shortLine.Product!.Stock} left of {shortLine.Product.Title}.");
            }

            var subtotal = lines.Sum(l => l.Product!.Price * l.Quantity);
            var payment = await _payments.ChargeAsync(new PaymentRequest(
                Taxes.Total(subtotal),
                Currency,
                $"home-market order for {request.FullName.Trim()}",
                request.CardNumber,
                request.CardHolderName,
                request.Expiry,
                request.SecurityCode));
            if (!payment.Accepted) return Result<OrderDto>.Error(payment.Reason);

            var order = new Order
            {
                BuyerId = buyerId,
                PlacedAt = DateTime.UtcNow,
                FullName = request.FullName.Trim(),
                Street = request.Street.Trim(),
                City = request.City.Trim(),
                Province = request.Province.Trim(),
                PostalCode = request.PostalCode.Trim().ToUpperInvariant(),
                Country = request.Country.Trim(),
                CardBrand = payment.CardBrand,
                CardLast4 = payment.CardLast4,
                PaymentReference = payment.Reference,
                Subtotal = subtotal,
                Gst = Taxes.Gst(subtotal),
                Qst = Taxes.Qst(subtotal),
                Total = Taxes.Total(subtotal),
            };
            foreach (var line in lines)
            {
                var product = line.Product!;
                order.Lines.Add(new OrderLine
                {
                    ProductId = product.Id,
                    SellerId = product.SellerId,
                    Title = product.Title,
                    UnitPrice = product.Price,
                    Quantity = line.Quantity,
                });
                product.Stock -= line.Quantity;
            }
            _context.Orders.Add(order);
            _context.CartLines.RemoveRange(lines);
            await _context.SaveChangesAsync();
            return Result<OrderDto>.Success((await GetAsync(buyerId, order.Id))!);
        }

        public Task<IReadOnlyList<OrderDto>> MineAsync(int buyerId)
        {
            return Project(_context.Orders.Where(o => o.BuyerId == buyerId));
        }

        // An order is found for its buyer only.
        public async Task<OrderDto?> GetAsync(int buyerId, int id)
        {
            var found = await Project(_context.Orders.Where(o => o.Id == id && o.BuyerId == buyerId));
            return found.Count == 0 ? null : found[0];
        }

        public async Task<IReadOnlyList<SaleDto>> SalesAsync(int sellerId)
        {
            return await _context.OrderLines
                .Where(l => l.SellerId == sellerId)
                .OrderByDescending(l => l.Order!.PlacedAt)
                .ThenByDescending(l => l.Id)
                .Select(l => new SaleDto
                {
                    OrderId = l.OrderId,
                    PlacedAt = l.Order!.PlacedAt,
                    Buyer = l.Order.Buyer!.UserName,
                    ProductId = l.ProductId,
                    Title = l.Title,
                    UnitPrice = l.UnitPrice,
                    Quantity = l.Quantity,
                    LineTotal = l.UnitPrice * l.Quantity,
                    ShipTo = l.Order.FullName + ", " + l.Order.Street + ", " + l.Order.City + ", " + l.Order.Province + " " + l.Order.PostalCode + ", " + l.Order.Country,
                })
                .ToListAsync();
        }

        private static async Task<IReadOnlyList<OrderDto>> Project(IQueryable<Order> orders)
        {
            return await orders
                .OrderByDescending(o => o.PlacedAt)
                .ThenByDescending(o => o.Id)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    PlacedAt = o.PlacedAt,
                    Buyer = o.Buyer!.UserName,
                    FullName = o.FullName,
                    Street = o.Street,
                    City = o.City,
                    Province = o.Province,
                    PostalCode = o.PostalCode,
                    Country = o.Country,
                    CardBrand = o.CardBrand,
                    CardLast4 = o.CardLast4,
                    Subtotal = o.Subtotal,
                    Gst = o.Gst,
                    Qst = o.Qst,
                    Total = o.Total,
                    ItemCount = o.Lines.Sum(l => l.Quantity),
                    Lines = o.Lines.OrderBy(l => l.Id).Select(l => new OrderLineDto
                    {
                        ProductId = l.ProductId,
                        Title = l.Title,
                        Seller = l.Seller!.UserName,
                        UnitPrice = l.UnitPrice,
                        Quantity = l.Quantity,
                        LineTotal = l.UnitPrice * l.Quantity,
                    }).ToList(),
                })
                .ToListAsync();
        }
    }
}
