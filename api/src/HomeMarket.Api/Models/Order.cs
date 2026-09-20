namespace HomeMarket.Api.Models
{
    // A paid order: who bought, where it ships, what was charged and the
    // lines, each a snapshot of the product as it was sold. Of the card,
    // only the brand and the last four digits are kept, with the reference
    // the payment provider gave the charge.
    public class Order
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public Account? Buyer { get; set; }
        public DateTime PlacedAt { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string CardBrand { get; set; } = string.Empty;
        public string CardLast4 { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Gst { get; set; }
        public decimal Qst { get; set; }
        public decimal Total { get; set; }
        public List<OrderLine> Lines { get; set; } = new();
    }

    // One product of an order, at the price and title of the moment. The
    // product may be removed from the shop later; the line stays.
    public class OrderLine
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public int SellerId { get; set; }
        public Account? Seller { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
