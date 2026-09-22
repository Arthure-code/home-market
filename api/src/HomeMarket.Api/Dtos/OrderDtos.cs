using System.ComponentModel.DataAnnotations;

namespace HomeMarket.Api.Dtos
{
    // Checking out: where to ship and how to pay. The card fields are
    // passed to the payment gateway and never stored.
    public class CheckoutRequest
    {
        [Required]
        [StringLength(80, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(120, MinimumLength = 3)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(60, MinimumLength = 2)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(40, MinimumLength = 2)]
        public string Province { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 3)]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [StringLength(60, MinimumLength = 2)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(23, MinimumLength = 12)]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(80, MinimumLength = 2)]
        public string CardHolderName { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(0[1-9]|1[0-2])/[0-9]{2}$", ErrorMessage = "Expiry must be MM/YY.")]
        public string Expiry { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^[0-9]{3,4}$", ErrorMessage = "The security code is three or four digits.")]
        public string SecurityCode { get; set; } = string.Empty;
    }

    public class OrderLineDto
    {
        public int? ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Seller { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    // An order as its buyer sees it: the lines, the totals, the address
    // and the card by its brand and last four digits only.
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime PlacedAt { get; set; }
        public string Buyer { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string CardBrand { get; set; } = string.Empty;
        public string CardLast4 { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Gst { get; set; }
        public decimal Qst { get; set; }
        public decimal Total { get; set; }
        public int ItemCount { get; set; }
        public List<OrderLineDto> Lines { get; set; } = new();
    }

    // One line sold by me: what, to whom, and where it ships. The buyer's
    // card is not part of it.
    public class SaleDto
    {
        public int OrderId { get; set; }
        public DateTime PlacedAt { get; set; }
        public string Buyer { get; set; } = string.Empty;
        public int? ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
        public string ShipTo { get; set; } = string.Empty;
    }
}
