using HomeMarket.Api.Auth;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeMarket.Api.Controllers
{
    // The cart of the account named by the token. Every change answers
    // with the whole cart, totals included, so the client never adds up.
    [Authorize]
    [ApiController]
    [Route("api/cart")]
    [Produces("application/json")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cart;

        public CartController(ICartService cart)
        {
            _cart = cart;
        }

        [HttpGet]
        public async Task<ActionResult<CartDto>> Get()
        {
            return Ok(await _cart.GetAsync(User.AccountId()));
        }

        [HttpPost("lines")]
        public async Task<ActionResult<CartDto>> Add(CartAddRequest request)
        {
            var (outcome, cart) = await _cart.AddAsync(User.AccountId(), request.ProductId, request.Quantity);
            return Answer(outcome, cart);
        }

        [HttpPut("lines/{productId:int}")]
        public async Task<ActionResult<CartDto>> SetQuantity(int productId, CartQuantityRequest request)
        {
            var (outcome, cart) = await _cart.SetQuantityAsync(User.AccountId(), productId, request.Quantity);
            return Answer(outcome, cart);
        }

        [HttpDelete("lines/{productId:int}")]
        public async Task<ActionResult<CartDto>> Remove(int productId)
        {
            var (outcome, cart) = await _cart.RemoveAsync(User.AccountId(), productId);
            return Answer(outcome, cart);
        }

        private ActionResult<CartDto> Answer(CartOutcome outcome, CartDto? cart)
        {
            return outcome switch
            {
                CartOutcome.NotFound => NotFound(),
                CartOutcome.OwnProduct => BadRequest(new { message = "That is your own listing." }),
                CartOutcome.NotEnoughStock => Conflict(new { message = "There are not that many left in stock." }),
                _ => Ok(cart),
            };
        }
    }
}
