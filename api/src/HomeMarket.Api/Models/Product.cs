namespace HomeMarket.Api.Models
{
    // Something for sale, listed by one account, liked by any number of
    // others, in one of the shop's categories, with the number left in
    // stock. The photo is either a public URL or the name of an uploaded
    // file served by the API.
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Maker { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Stock { get; set; }
        public string Photo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int SellerId { get; set; }
        public Account? Seller { get; set; }
        public List<Account> LikedBy { get; set; } = new();
    }
}
