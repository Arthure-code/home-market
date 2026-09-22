using Ardalis.Result;
using HomeMarket.Api.Controllers;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HomeMarket.Api.Tests
{
    public class PhotosControllerTests
    {
        private const int Nadia = 7;

        [Fact]
        public async Task Upload_RealImage_ReturnsTheNameTheServerChoseAndItsUrl()
        {
            // Given
            var file = new Mock<IFormFile>().Object;
            var photos = new Mock<IPhotoStore>();
            photos.Setup(p => p.SaveAsync(file)).ReturnsAsync(Result<string>.Success("abc123.jpg"));
            photos.Setup(p => p.UrlFor("abc123.jpg")).Returns("http://localhost:5130/images/abc123.jpg");
            var controller = new PhotosController(photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Upload(file);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var uploaded = Assert.IsType<UploadedPhotoDto>(ok.Value);
            Assert.Equal("abc123.jpg", uploaded.Photo);
            Assert.Equal("http://localhost:5130/images/abc123.jpg", uploaded.Url);
        }

        [Fact]
        public async Task Upload_FileThatIsNotAnImage_Returns400()
        {
            // Given
            var file = new Mock<IFormFile>().Object;
            var photos = new Mock<IPhotoStore>();
            photos.Setup(p => p.SaveAsync(file)).ReturnsAsync(Result<string>.Invalid(new ValidationError("photo", "That file is not a JPEG, PNG, GIF or WebP image.")));
            var controller = new PhotosController(photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Upload(file);

            // Then
            Refusal.Of(result.Result, StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Upload_FileTooLarge_Returns400()
        {
            // Given
            var file = new Mock<IFormFile>().Object;
            var photos = new Mock<IPhotoStore>();
            photos.Setup(p => p.SaveAsync(file)).ReturnsAsync(Result<string>.Invalid(new ValidationError("photo", "Photos must be 5 MB or less.")));
            var controller = new PhotosController(photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Upload(file);

            // Then
            Refusal.Of(result.Result, StatusCodes.Status400BadRequest);
        }
    }
}
