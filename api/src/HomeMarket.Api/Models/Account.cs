namespace HomeMarket.Api.Models
{
    // Someone who can sell and like. Only a hash of the password is kept.
    public class Account
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<Product> Listings { get; set; } = new();
        public List<Product> Liked { get; set; } = new();
    }
}
