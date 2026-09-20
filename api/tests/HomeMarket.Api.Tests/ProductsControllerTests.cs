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
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IProductService> _products = new Mock<IProductService>();
        private readonly Mock<IPhotoStore> _photos = new Mock<IPhotoStore>();
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _controller = new ProductsController(_products.Object, _photos.Object);
            _controller.ControllerContext = Caller.Member(Nadia);
        }

        [Fact]
        public async Task List_Visitor_ReturnsTheCatalogueComputedForNobody()
        {
            // Given
            _controller.ControllerContext = Caller.Visitor();
            var catalogue = _fixture.CreateMany<ProductDto>(3).ToList();
            _products.Setup(p => p.ListAsync(null, null, null)).ReturnsAsync(catalogue);

            // When
            var result = await _controller.List(null, null);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(catalogue, ok.Value);
        }

        [Fact]
        public async Task List_MemberWithSearchAndCategory_PassesThemWithTheirAccount()
        {
            // Given
            var found = _fixture.CreateMany<ProductDto>(1).ToList();
            _products.Setup(p => p.ListAsync(Nadia, "kettle", "Kitchen")).ReturnsAsync(found);

            // When
            var result = await _controller.List("kettle", "Kitchen");

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(found, ok.Value);
        }

        [Fact]
        public async Task Categories_ReturnsTheCategoriesWithTheirCounts()
        {
            // Given
            var categories = _fixture.CreateMany<CategoryDto>(8).ToList();
            _products.Setup(p => p.CategoriesAsync()).ReturnsAsync(categories);

            // When
            var result = await _controller.Categories();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(categories, ok.Value);
        }

        [Fact]
        public async Task Get_MissingProduct_Returns404()
        {
            // Given
            _products.Setup(p => p.GetAsync(999, Nadia)).ReturnsAsync((ProductDto?)null);

            // When
            var result = await _controller.Get(999);

            // Then
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_ExistingProduct_ReturnsIt()
        {
            // Given
            var product = _fixture.Create<ProductDto>();
            _products.Setup(p => p.GetAsync(product.Id, Nadia)).ReturnsAsync(product);

            // When
            var result = await _controller.Get(product.Id);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(product, ok.Value);
        }

        [Fact]
        public async Task Mine_ReturnsTheListingsOfTheCaller()
        {
            // Given
            var mine = _fixture.CreateMany<ProductDto>(2).ToList();
            _products.Setup(p => p.MineAsync(Nadia)).ReturnsAsync(mine);

            // When
            var result = await _controller.Mine();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(mine, ok.Value);
        }

        [Fact]
        public async Task Liked_ReturnsTheLikesOfTheCaller()
        {
            // Given
            var liked = _fixture.CreateMany<ProductDto>(2).ToList();
            _products.Setup(p => p.LikedAsync(Nadia)).ReturnsAsync(liked);

            // When
            var result = await _controller.Liked();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(liked, ok.Value);
        }

        [Fact]
        public async Task Create_ValidListing_Returns201PointingAtTheProduct()
        {
            // Given
            var request = _fixture.Create<ProductRequest>();
            var product = _fixture.Create<ProductDto>();
            _products.Setup(p => p.CreateAsync(Nadia, request)).ReturnsAsync((ProductOutcome.Done, product));

            // When
            var result = await _controller.Create(request);

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
            var request = _fixture.Create<ProductRequest>();
            _products.Setup(p => p.CreateAsync(Nadia, request)).ReturnsAsync((ProductOutcome.BadPhoto, null));

            // When
            var result = await _controller.Create(request);

            // Then
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_CategoryNotInTheShop_Returns400()
        {
            // Given
            var request = _fixture.Create<ProductRequest>();
            _products.Setup(p => p.CreateAsync(Nadia, request)).ReturnsAsync((ProductOutcome.BadCategory, null));

            // When
            var result = await _controller.Create(request);

            // Then
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Update_OwnListing_ReturnsTheChangedProduct()
        {
            // Given
            var request = _fixture.Create<ProductRequest>();
            var product = _fixture.Create<ProductDto>();
            _products.Setup(p => p.UpdateAsync(Nadia, product.Id, request)).ReturnsAsync((ProductOutcome.Done, product));

            // When
            var result = await _controller.Update(product.Id, request);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(product, ok.Value);
        }

        [Fact]
        public async Task Update_SomeoneElsesListing_Returns403()
        {
            // Given
            var request = _fixture.Create<ProductRequest>();
            _products.Setup(p => p.UpdateAsync(Nadia, 3, request)).ReturnsAsync((ProductOutcome.NotMine, null));

            // When
            var result = await _controller.Update(3, request);

            // Then
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task Update_MissingListing_Returns404()
        {
            // Given
            var request = _fixture.Create<ProductRequest>();
            _products.Setup(p => p.UpdateAsync(Nadia, 999, request)).ReturnsAsync((ProductOutcome.NotFound, null));

            // When
            var result = await _controller.Update(999, request);

            // Then
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Delete_OwnListing_Returns204()
        {
            // Given
            _products.Setup(p => p.DeleteAsync(Nadia, 3)).ReturnsAsync(ProductOutcome.Done);

            // When
            var result = await _controller.Delete(3);

            // Then
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_SomeoneElsesListing_Returns403()
        {
            // Given
            _products.Setup(p => p.DeleteAsync(Nadia, 3)).ReturnsAsync(ProductOutcome.NotMine);

            // When
            var result = await _controller.Delete(3);

            // Then
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Like_ExistingProduct_Returns204()
        {
            // Given
            _products.Setup(p => p.SetLikeAsync(Nadia, 5, true)).ReturnsAsync(ProductOutcome.Done);

            // When
            var result = await _controller.Like(5);

            // Then
            Assert.IsType<NoContentResult>(result);
            _products.Verify(p => p.SetLikeAsync(Nadia, 5, true), Times.Once);
        }

        [Fact]
        public async Task Like_MissingProduct_Returns404()
        {
            // Given
            _products.Setup(p => p.SetLikeAsync(Nadia, 999, true)).ReturnsAsync(ProductOutcome.NotFound);

            // When
            var result = await _controller.Like(999);

            // Then
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Unlike_ExistingProduct_Returns204()
        {
            // Given
            _products.Setup(p => p.SetLikeAsync(Nadia, 5, false)).ReturnsAsync(ProductOutcome.Done);

            // When
            var result = await _controller.Unlike(5);

            // Then
            Assert.IsType<NoContentResult>(result);
            _products.Verify(p => p.SetLikeAsync(Nadia, 5, false), Times.Once);
        }

        [Fact]
        public async Task UploadPhoto_RealImage_ReturnsTheNameTheServerChoseAndItsUrl()
        {
            // Given
            var file = new Mock<IFormFile>().Object;
            _photos.Setup(p => p.SaveAsync(file)).ReturnsAsync((PhotoOutcome.Saved, "abc123.jpg"));
            _photos.Setup(p => p.UrlFor("abc123.jpg")).Returns("http://localhost:5130/images/abc123.jpg");

            // When
            var result = await _controller.UploadPhoto(file);

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
            _photos.Setup(p => p.SaveAsync(file)).ReturnsAsync((PhotoOutcome.NotAnImage, string.Empty));

            // When
            var result = await _controller.UploadPhoto(file);

            // Then
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UploadPhoto_FileTooLarge_Returns400()
        {
            // Given
            var file = new Mock<IFormFile>().Object;
            _photos.Setup(p => p.SaveAsync(file)).ReturnsAsync((PhotoOutcome.TooLarge, string.Empty));

            // When
            var result = await _controller.UploadPhoto(file);

            // Then
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
