using Ardalis.Result;
using AutoFixture;
using HomeMarket.Api.Controllers;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HomeMarket.Api.Tests
{
    public class ProductsControllerTests
    {
        private const int Nadia = 7;

        [Fact]
        public async Task List_Visitor_ReturnsTheCatalogueComputedForNobody()
        {
            // Given
            var fixture = new Fixture();
            var catalogue = fixture.CreateMany<ProductDto>(3).ToList();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.ListAsync(null, null, null)).ReturnsAsync(catalogue);
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Visitor() };

            // When
            var result = await controller.List(null, null);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(catalogue, ok.Value);
        }

        [Fact]
        public async Task List_MemberWithSearchAndCategory_PassesThemWithTheirAccount()
        {
            // Given
            var fixture = new Fixture();
            var found = fixture.CreateMany<ProductDto>(1).ToList();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.ListAsync(Nadia, "kettle", "Kitchen")).ReturnsAsync(found);
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.List("kettle", "Kitchen");

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(found, ok.Value);
        }

        [Fact]
        public async Task Categories_ReturnsTheCategoriesWithTheirCounts()
        {
            // Given
            var fixture = new Fixture();
            var categories = fixture.CreateMany<CategoryDto>(8).ToList();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.CategoriesAsync()).ReturnsAsync(categories);
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Visitor() };

            // When
            var result = await controller.Categories();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(categories, ok.Value);
        }

        [Fact]
        public async Task Get_MissingProduct_Returns404()
        {
            // Given
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.GetAsync(999, Nadia)).ReturnsAsync((ProductDto?)null);
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Get(999);

            // Then
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_ExistingProduct_ReturnsIt()
        {
            // Given
            var fixture = new Fixture();
            var product = fixture.Create<ProductDto>();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.GetAsync(product.Id, Nadia)).ReturnsAsync(product);
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Get(product.Id);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(product, ok.Value);
        }

        [Fact]
        public async Task Mine_ReturnsTheListingsOfTheCaller()
        {
            // Given
            var fixture = new Fixture();
            var mine = fixture.CreateMany<ProductDto>(2).ToList();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.MineAsync(Nadia)).ReturnsAsync(mine);
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Mine();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(mine, ok.Value);
        }

        [Fact]
        public async Task Liked_ReturnsTheLikesOfTheCaller()
        {
            // Given
            var fixture = new Fixture();
            var liked = fixture.CreateMany<ProductDto>(2).ToList();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.LikedAsync(Nadia)).ReturnsAsync(liked);
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Liked();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(liked, ok.Value);
        }

        [Fact]
        public async Task Create_ValidListing_Returns201PointingAtTheProduct()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<ProductRequest>();
            var product = fixture.Create<ProductDto>();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.CreateAsync(Nadia, request)).ReturnsAsync(Result<ProductDto>.Success(product));
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Create(request);

            // Then
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(ProductsController.Get), created.ActionName);
            Assert.Equal(product.Id, created.RouteValues!["id"]);
            Assert.Same(product, created.Value);
        }

        [Fact]
        public async Task Create_PhotoTheServerNeverStored_Returns400()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<ProductRequest>();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.CreateAsync(Nadia, request)).ReturnsAsync(Result<ProductDto>.Invalid(new ValidationError("Photo", "Upload the photo first, then save the product.")));
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Create(request);

            // Then
            Refusal.Of(result.Result, StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Create_CategoryNotInTheShop_Returns400()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<ProductRequest>();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.CreateAsync(Nadia, request)).ReturnsAsync(Result<ProductDto>.Invalid(new ValidationError("Category", "Pick one of the shop's categories.")));
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Create(request);

            // Then
            Refusal.Of(result.Result, StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Update_OwnListing_ReturnsTheChangedProduct()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<ProductRequest>();
            var product = fixture.Create<ProductDto>();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.UpdateAsync(Nadia, product.Id, request)).ReturnsAsync(Result<ProductDto>.Success(product));
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Update(product.Id, request);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(product, ok.Value);
        }

        [Fact]
        public async Task Update_SomeoneElsesListing_Returns403()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<ProductRequest>();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.UpdateAsync(Nadia, 3, request)).ReturnsAsync(Result<ProductDto>.Forbidden());
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Update(3, request);

            // Then
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task Update_MissingListing_Returns404()
        {
            // Given
            var fixture = new Fixture();
            var request = fixture.Create<ProductRequest>();
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.UpdateAsync(Nadia, 999, request)).ReturnsAsync(Result<ProductDto>.NotFound());
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Update(999, request);

            // Then
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Delete_OwnListing_Returns204()
        {
            // Given
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.DeleteAsync(Nadia, 3)).ReturnsAsync(Result.Success());
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Delete(3);

            // Then
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_SomeoneElsesListing_Returns403()
        {
            // Given
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.DeleteAsync(Nadia, 3)).ReturnsAsync(Result.Forbidden());
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Delete(3);

            // Then
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Like_ExistingProduct_Returns204()
        {
            // Given
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.SetLikeAsync(Nadia, 5, true)).ReturnsAsync(Result.Success());
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Like(5);

            // Then
            Assert.IsType<NoContentResult>(result);
            products.Verify(p => p.SetLikeAsync(Nadia, 5, true), Times.Once);
        }

        [Fact]
        public async Task Like_MissingProduct_Returns404()
        {
            // Given
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.SetLikeAsync(Nadia, 999, true)).ReturnsAsync(Result.NotFound());
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Like(999);

            // Then
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Unlike_ExistingProduct_Returns204()
        {
            // Given
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            products.Setup(p => p.SetLikeAsync(Nadia, 5, false)).ReturnsAsync(Result.Success());
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.Unlike(5);

            // Then
            Assert.IsType<NoContentResult>(result);
            products.Verify(p => p.SetLikeAsync(Nadia, 5, false), Times.Once);
        }

        [Fact]
        public async Task UploadPhoto_RealImage_ReturnsTheNameTheServerChoseAndItsUrl()
        {
            // Given
            var file = new Mock<IFormFile>().Object;
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            photos.Setup(p => p.SaveAsync(file)).ReturnsAsync(Result<string>.Success("abc123.jpg"));
            photos.Setup(p => p.UrlFor("abc123.jpg")).Returns("http://localhost:5130/images/abc123.jpg");
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.UploadPhoto(file);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var uploaded = Assert.IsType<UploadedPhotoDto>(ok.Value);
            Assert.Equal("abc123.jpg", uploaded.Photo);
            Assert.Equal("http://localhost:5130/images/abc123.jpg", uploaded.Url);
        }

        [Fact]
        public async Task UploadPhoto_FileThatIsNotAnImage_Returns400()
        {
            // Given
            var file = new Mock<IFormFile>().Object;
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            photos.Setup(p => p.SaveAsync(file)).ReturnsAsync(Result<string>.Invalid(new ValidationError("photo", "That file is not a JPEG, PNG, GIF or WebP image.")));
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.UploadPhoto(file);

            // Then
            Refusal.Of(result.Result, StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task UploadPhoto_FileTooLarge_Returns400()
        {
            // Given
            var file = new Mock<IFormFile>().Object;
            var products = new Mock<IProductService>();
            var photos = new Mock<IPhotoStore>();
            photos.Setup(p => p.SaveAsync(file)).ReturnsAsync(Result<string>.Invalid(new ValidationError("photo", "Photos must be 5 MB or less.")));
            var controller = new ProductsController(products.Object, photos.Object) { ControllerContext = Caller.Member(Nadia) };

            // When
            var result = await controller.UploadPhoto(file);

            // Then
            Refusal.Of(result.Result, StatusCodes.Status400BadRequest);
        }
    }
}
