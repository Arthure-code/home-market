using HomeMarket.Api.Auth;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeMarket.Api.Controllers
{
    // The cart of the account named by the token. Every change answers
    // with the whole cart, totals included, so the client never adds up;
    // every refusal comes from the service as a Result.
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
            var result = await _cart.AddAsync(User.AccountId(), request.ProductId, request.Quantity);
            return result.IsSuccess ? Ok(result.Value) : this.Refuse(result);
        }

        [HttpPut("lines/{productId:int}")]
        public async Task<ActionResult<CartDto>> SetQuantity(int productId, CartQuantityRequest request)
        {
            var result = await _cart.SetQuantityAsync(User.AccountId(), productId, request.Quantity);
            return result.IsSuccess ? Ok(result.Value) : this.Refuse(result);
        }

        [HttpDelete("lines/{productId:int}")]
        public async Task<ActionResult<CartDto>> Remove(int productId)
        {
            var result = await _cart.RemoveAsync(User.AccountId(), productId);
            return result.IsSuccess ? Ok(result.Value) : this.Refuse(result);
        }
    }
}
