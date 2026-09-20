namespace HomeMarket.Api.Models
{
    // One product in one account's cart, with how many. An account has
    // at most one line per product; the cart is the set of its lines.
    public class CartLine
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
