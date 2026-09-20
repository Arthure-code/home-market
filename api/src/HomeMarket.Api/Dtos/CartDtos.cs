using System.ComponentModel.DataAnnotations;

namespace HomeMarket.Api.Dtos
{
    // Adding to the cart: which product and how many. Ten of a thing is
    // the most one order takes.
    public class CartAddRequest
    {
        public const int MaxQuantity = 10;

        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, MaxQuantity)]
        public int Quantity { get; set; } = 1;
    }

    // Changing a line: the new quantity.
    public class CartQuantityRequest
    {
        [Range(1, CartAddRequest.MaxQuantity)]
        public int Quantity { get; set; }
    }

    public class CartLineDto
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public string Seller { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public decimal LineTotal { get; set; }
    }

    // The whole cart with its totals, taxes as charged in Quebec.
    public class CartDto
    {
        public List<CartLineDto> Lines { get; set; } = new();
        public int ItemCount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Gst { get; set; }
        public decimal Qst { get; set; }
        public decimal Total { get; set; }
    }
}
