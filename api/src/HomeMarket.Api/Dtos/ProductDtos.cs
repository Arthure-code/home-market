using System.ComponentModel.DataAnnotations;

namespace HomeMarket.Api.Dtos
{
    // What a seller sends to list or change a product. The category is
    // one of the shop's, the stock how many can be bought, the photo the
    // name the upload route returned, or empty for none.
    public class ProductRequest
    {
        [Required]
        [StringLength(80, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [StringLength(60)]
        public string Brand { get; set; } = string.Empty;

        [StringLength(60)]
        public string Maker { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(40)]
        public string Category { get; set; } = string.Empty;

        [Range(0, 10000)]
        public int Stock { get; set; }

        [StringLength(120)]
        public string Photo { get; set; } = string.Empty;
    }

    // What the upload route answers: the name the server chose for the
    // file, to put in the listing, and where the photo can be seen.
    public class UploadedPhotoDto
    {
        public string Photo { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    // A category of the shop and how many products it holds.
    public class CategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    // A product as the catalogue shows it: with its photo address, its
    // seller, and what it is to the caller (liked, mine).
    public class ProductDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Maker { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Stock { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public string Seller { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Likes { get; set; }
        public bool Liked { get; set; }
        public bool Mine { get; set; }
    }
}
