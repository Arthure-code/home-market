using HomeMarket.Api.Auth;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeMarket.Api.Controllers
{
    // The catalogue is public; selling, changing and liking need a token,
    // and the account named by the token is the only one acted for. A
    // refusal with a reason is a Problem Details answer (RFC 9457), as the
    // ASP.NET Core documentation recommends; a bare status is one too,
    // through the status code pages.
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private const string BadPhotoMessage = "Upload the photo first, then save the product.";
        private const string BadCategoryMessage = "Pick one of the shop's categories.";
        private readonly IProductService _products;
        private readonly IPhotoStore _photos;

        public ProductsController(IProductService products, IPhotoStore photos)
        {
            _products = products;
            _photos = photos;
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
            var (outcome, product) = await _products.CreateAsync(User.AccountId(), request);
            return outcome switch
            {
                ProductOutcome.BadPhoto => Problem(BadPhotoMessage, statusCode: StatusCodes.Status400BadRequest),
                ProductOutcome.BadCategory => Problem(BadCategoryMessage, statusCode: StatusCodes.Status400BadRequest),
                _ => CreatedAtAction(nameof(Get), new { id = product!.Id }, product),
            };
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductDto>> Update(int id, ProductRequest request)
        {
            var (outcome, product) = await _products.UpdateAsync(User.AccountId(), id, request);
            return outcome switch
            {
                ProductOutcome.NotFound => NotFound(),
                ProductOutcome.NotMine => Forbid(),
                ProductOutcome.BadPhoto => Problem(BadPhotoMessage, statusCode: StatusCodes.Status400BadRequest),
                ProductOutcome.BadCategory => Problem(BadCategoryMessage, statusCode: StatusCodes.Status400BadRequest),
                _ => Ok(product),
            };
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _products.DeleteAsync(User.AccountId(), id) switch
            {
                ProductOutcome.NotFound => NotFound(),
                ProductOutcome.NotMine => Forbid(),
                _ => NoContent(),
            };
        }

        [Authorize]
        [HttpPut("{id:int}/like")]
        public async Task<IActionResult> Like(int id)
        {
            return await _products.SetLikeAsync(User.AccountId(), id, true) == ProductOutcome.NotFound ? NotFound() : NoContent();
        }

        [Authorize]
        [HttpDelete("{id:int}/like")]
        public async Task<IActionResult> Unlike(int id)
        {
            return await _products.SetLikeAsync(User.AccountId(), id, false) == ProductOutcome.NotFound ? NotFound() : NoContent();
        }

        // The file is checked by its bytes and stored under a name the
        // server picks; the answer is that name, to put in the listing.
        [Authorize]
        [HttpPost("photos")]
        [RequestSizeLimit(Services.PhotoStore.MaxBytes + 4096)]
        public async Task<ActionResult<UploadedPhotoDto>> UploadPhoto(IFormFile photo)
        {
            var (outcome, fileName) = await _photos.SaveAsync(photo);
            return outcome switch
            {
                PhotoOutcome.NotAnImage => Problem("That file is not a JPEG, PNG, GIF or WebP image.", statusCode: StatusCodes.Status400BadRequest),
                PhotoOutcome.TooLarge => Problem("Photos must be 5 MB or less.", statusCode: StatusCodes.Status400BadRequest),
                _ => Ok(new UploadedPhotoDto { Photo = fileName, Url = _photos.UrlFor(fileName) }),
            };
        }
    }
}
