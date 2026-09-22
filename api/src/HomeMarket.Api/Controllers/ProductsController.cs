using HomeMarket.Api.Auth;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeMarket.Api.Controllers
{
    // The catalogue is public; selling, changing and liking need a token,
    // and the account named by the token is the only one acted for. A
    // refusal comes back from the service as a Result and leaves here as
    // a Problem Details answer.
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _products;

        public ProductsController(IProductService products)
        {
            _products = products;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> List([FromQuery] string? q, [FromQuery] string? category)
        {
            return Ok(await _products.ListAsync(User.AccountIdOrNull(), q, category));
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Categories()
        {
            return Ok(await _products.CategoriesAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            var product = await _products.GetAsync(id, User.AccountIdOrNull());
            return product is null ? NotFound() : Ok(product);
        }

        [Authorize]
        [HttpGet("mine")]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> Mine()
        {
            return Ok(await _products.MineAsync(User.AccountId()));
        }

        [Authorize]
        [HttpGet("liked")]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> Liked()
        {
            return Ok(await _products.LikedAsync(User.AccountId()));
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create(ProductRequest request)
        {
            var result = await _products.CreateAsync(User.AccountId(), request);
            return result.IsSuccess
                ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
                : this.Refuse(result);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductDto>> Update(int id, ProductRequest request)
        {
            var result = await _products.UpdateAsync(User.AccountId(), id, request);
            return result.IsSuccess ? Ok(result.Value) : this.Refuse(result);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _products.DeleteAsync(User.AccountId(), id);
            return result.IsSuccess ? NoContent() : this.Refuse(result);
        }

        [Authorize]
        [HttpPut("{id:int}/like")]
        public async Task<IActionResult> Like(int id)
        {
            var result = await _products.SetLikeAsync(User.AccountId(), id, true);
            return result.IsSuccess ? NoContent() : this.Refuse(result);
        }

        [Authorize]
        [HttpDelete("{id:int}/like")]
        public async Task<IActionResult> Unlike(int id)
        {
            var result = await _products.SetLikeAsync(User.AccountId(), id, false);
            return result.IsSuccess ? NoContent() : this.Refuse(result);
        }
    }
}
