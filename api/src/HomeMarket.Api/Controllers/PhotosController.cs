using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeMarket.Api.Controllers
{
    // Where a seller sends the photo of a listing before saving it. The
    // file is checked by its bytes and stored under a name the server
    // picks; the answer is that name, to put in the listing, and its URL.
    [ApiController]
    [Route("api/products/photos")]
    [Produces("application/json")]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotoStore _photos;

        public PhotosController(IPhotoStore photos)
        {
            _photos = photos;
        }

        [Authorize]
        [HttpPost]
        [RequestSizeLimit(Services.PhotoStore.MaxBytes + 4096)]
        public async Task<ActionResult<UploadedPhotoDto>> Upload(IFormFile photo)
        {
            var result = await _photos.SaveAsync(photo);
            return result.IsSuccess
                ? Ok(new UploadedPhotoDto { Photo = result.Value, Url = _photos.UrlFor(result.Value) })
                : this.Refuse(result);
        }
    }
}
