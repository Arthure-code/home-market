using Ardalis.Result;
using AutoFixture;
using HomeMarket.Api.Data;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using HomeMarket.Api.Services;
using Moq;

namespace HomeMarket.Api.Tests
{
    public class ProductServiceTests
    {
        private static Mock<IPhotoStore> PhotosServing()
        {
            var photos = new Mock<IPhotoStore>();
            photos.Setup(p => p.UrlFor(It.IsAny<string>())).Returns<string>(name => name.Length == 0 ? string.Empty : "http://api/images/" + name);
            return photos;
        }

        private static ProductRequest Listing(Fixture fixture, string category = Category.Kitchen, string photo = "")
        {
            return fixture.Build<ProductRequest>()
                .With(r => r.Price, 25m)
                .With(r => r.Stock, 4)
                .With(r => r.Category, category)
                .With(r => r.Photo, photo)
                .Create();
        }

        [Fact]
        public async Task ListAsync_NoFilter_ReturnsNewestFirstWithWhatTheyAreToTheViewer()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            var older = market.Listing(nadia, "Clock", 19, 3);
            var newer = market.Listing(omar, "Lamp", 65, 6, photo: "abc.jpg");
            newer.LikedBy.Add(nadia);
            newer.CreatedAt = older.CreatedAt.AddMinutes(1);
            market.Context.SaveChanges();
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var products = await service.ListAsync(nadia.Id);

            // Then
            Assert.Equal(new[] { "Lamp", "Clock" }, products.Select(p => p.Title));
            Assert.True(products[0].Liked);
            Assert.False(products[0].Mine);
            Assert.Equal(1, products[0].Likes);
            Assert.Equal("omar", products[0].Seller);
            Assert.Equal("http://api/images/abc.jpg", products[0].PhotoUrl);
            Assert.True(products[1].Mine);
            Assert.False(products[1].Liked);
        }

        [Fact]
        public async Task ListAsync_SearchText_MatchesTitleBrandOrMaker()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            market.Listing(nadia, "Sonora headphones", 249, 2);
            market.Listing(nadia, "Kettle", 45, 8);
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var byTitle = await service.ListAsync(null, "phones");
            var byMaker = await service.ListAsync(null, " sonora co ");

            // Then
            Assert.Equal("Sonora headphones", Assert.Single(byTitle).Title);
            Assert.Equal("Sonora headphones", Assert.Single(byMaker).Title);
        }

        [Fact]
        public async Task ListAsync_Category_KeepsThatCategoryOnly()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            market.Listing(nadia, "Toaster", 39, 5, Category.Kitchen);
            market.Listing(nadia, "Lamp", 65, 6, Category.Home);
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var kitchen = await service.ListAsync(null, category: Category.Kitchen);

            // Then
            Assert.Equal("Toaster", Assert.Single(kitchen).Title);
        }

        [Fact]
        public async Task CategoriesAsync_ReturnsEveryCategoryWithItsCountEmptyOnesIncluded()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            market.Listing(nadia, "Toaster", 39, 5, Category.Kitchen);
            market.Listing(nadia, "Kettle", 45, 8, Category.Kitchen);
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var categories = await service.CategoriesAsync();

            // Then
            Assert.Equal(Category.All, categories.Select(c => c.Name));
            Assert.Equal(2, categories.Single(c => c.Name == Category.Kitchen).Count);
            Assert.Equal(0, categories.Single(c => c.Name == Category.Office).Count);
        }

        [Fact]
        public async Task GetAsync_MissingProduct_ReturnsNull()
        {
            // Given
            using var market = new Market();
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var product = await service.GetAsync(999, null);

            // Then
            Assert.Null(product);
        }

        [Fact]
        public async Task MineAsync_ReturnsTheListingsOfTheViewerOnly()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            market.Listing(nadia, "Clock", 19, 3);
            market.Listing(omar, "Lamp", 65, 6);
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var mine = await service.MineAsync(nadia.Id);

            // Then
            var product = Assert.Single(mine);
            Assert.Equal("Clock", product.Title);
            Assert.True(product.Mine);
        }

        [Fact]
        public async Task LikedAsync_ReturnsWhatTheViewerLiked()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            var lamp = market.Listing(omar, "Lamp", 65, 6);
            market.Listing(omar, "Clock", 19, 3);
            lamp.LikedBy.Add(nadia);
            market.Context.SaveChanges();
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var liked = await service.LikedAsync(nadia.Id);

            // Then
            Assert.Equal("Lamp", Assert.Single(liked).Title);
        }

        [Fact]
        public async Task CreateAsync_ValidListing_StoresItTrimmedAndReturnsIt()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var photos = PhotosServing();
            photos.Setup(p => p.Exists("abc.jpg")).Returns(true);
            var request = Listing(new Fixture(), photo: " abc.jpg ");
            request.Title = "  Kettle ";
            var service = new ProductService(market.Context, photos.Object);

            // When
            var result = await service.CreateAsync(nadia.Id, request);

            // Then
            Assert.True(result.IsSuccess);
            Assert.Equal("Kettle", result.Value.Title);
            Assert.Equal("http://api/images/abc.jpg", result.Value.PhotoUrl);
            Assert.True(result.Value.Mine);
            Assert.Equal("nadia", result.Value.Seller);
            Assert.Equal("Kettle", Assert.Single(market.Context.Products).Title);
        }

        [Fact]
        public async Task CreateAsync_PhotoTheServerNeverStored_IsInvalidAndStoresNothing()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var photos = PhotosServing();
            photos.Setup(p => p.Exists("evil.jpg")).Returns(false);
            var service = new ProductService(market.Context, photos.Object);

            // When
            var result = await service.CreateAsync(nadia.Id, Listing(new Fixture(), photo: "evil.jpg"));

            // Then
            Assert.Equal(ResultStatus.Invalid, result.Status);
            Assert.Equal("Photo", Assert.Single(result.ValidationErrors).Identifier);
            Assert.Empty(market.Context.Products);
        }

        [Fact]
        public async Task CreateAsync_CategoryNotInTheShop_IsInvalid()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var result = await service.CreateAsync(nadia.Id, Listing(new Fixture(), category: "Weapons"));

            // Then
            Assert.Equal(ResultStatus.Invalid, result.Status);
            Assert.Equal("Category", Assert.Single(result.ValidationErrors).Identifier);
        }

        [Fact]
        public async Task UpdateAsync_OwnListing_ChangesItAndKeepsItsPhoto()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var clock = market.Listing(nadia, "Clock", 19, 3, photo: "abc.jpg");
            var request = Listing(new Fixture(), photo: "abc.jpg");
            request.Title = "Alarm clock";
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var result = await service.UpdateAsync(nadia.Id, clock.Id, request);

            // Then
            Assert.True(result.IsSuccess);
            Assert.Equal("Alarm clock", result.Value.Title);
            Assert.Equal(25m, result.Value.Price);
            Assert.Equal("http://api/images/abc.jpg", result.Value.PhotoUrl);
        }

        [Fact]
        public async Task UpdateAsync_SomeoneElsesListing_IsForbiddenAndLeavesIt()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            var lamp = market.Listing(omar, "Lamp", 65, 6);
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var result = await service.UpdateAsync(nadia.Id, lamp.Id, Listing(new Fixture()));

            // Then
            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Equal("Lamp", market.Context.Products.Single().Title);
        }

        [Fact]
        public async Task UpdateAsync_MissingListing_IsNotFound()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var result = await service.UpdateAsync(nadia.Id, 999, Listing(new Fixture()));

            // Then
            Assert.Equal(ResultStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsync_BadCategory_IsInvalidAndLeavesTheListing()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var clock = market.Listing(nadia, "Clock", 19, 3);
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var result = await service.UpdateAsync(nadia.Id, clock.Id, Listing(new Fixture(), category: "Weapons"));

            // Then
            Assert.Equal(ResultStatus.Invalid, result.Status);
            Assert.Equal("Clock", market.Context.Products.Single().Title);
        }

        [Fact]
        public async Task DeleteAsync_OwnListing_RemovesIt()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var clock = market.Listing(nadia, "Clock", 19, 3);
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var result = await service.DeleteAsync(nadia.Id, clock.Id);

            // Then
            Assert.True(result.IsSuccess);
            Assert.Empty(market.Context.Products);
        }

        [Fact]
        public async Task DeleteAsync_SomeoneElsesListing_IsForbidden()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            var lamp = market.Listing(omar, "Lamp", 65, 6);
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var result = await service.DeleteAsync(nadia.Id, lamp.Id);

            // Then
            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Single(market.Context.Products);
        }

        [Fact]
        public async Task DeleteAsync_MissingListing_IsNotFound()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var result = await service.DeleteAsync(nadia.Id, 999);

            // Then
            Assert.Equal(ResultStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task SetLikeAsync_LikingTwiceThenUnliking_CountsOneThenNone()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var omar = market.Member("omar");
            var lamp = market.Listing(omar, "Lamp", 65, 6);
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var first = await service.SetLikeAsync(nadia.Id, lamp.Id, true);
            var second = await service.SetLikeAsync(nadia.Id, lamp.Id, true);
            var afterLiking = await service.GetAsync(lamp.Id, nadia.Id);
            var unliked = await service.SetLikeAsync(nadia.Id, lamp.Id, false);
            var afterUnliking = await service.GetAsync(lamp.Id, nadia.Id);

            // Then
            Assert.True(first.IsSuccess);
            Assert.True(second.IsSuccess);
            Assert.Equal(1, afterLiking!.Likes);
            Assert.True(afterLiking.Liked);
            Assert.True(unliked.IsSuccess);
            Assert.Equal(0, afterUnliking!.Likes);
            Assert.False(afterUnliking.Liked);
        }

        [Fact]
        public async Task SetLikeAsync_UnlikingWhatWasNeverLiked_Succeeds()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var lamp = market.Listing(nadia, "Lamp", 65, 6);
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var result = await service.SetLikeAsync(nadia.Id, lamp.Id, false);

            // Then
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task SetLikeAsync_MissingProduct_IsNotFound()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var result = await service.SetLikeAsync(nadia.Id, 999, true);

            // Then
            Assert.Equal(ResultStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task SetLikeAsync_UnknownViewer_IsNotFound()
        {
            // Given
            using var market = new Market();
            var nadia = market.Member("nadia");
            var lamp = market.Listing(nadia, "Lamp", 65, 6);
            var service = new ProductService(market.Context, PhotosServing().Object);

            // When
            var result = await service.SetLikeAsync(999, lamp.Id, true);

            // Then
            Assert.Equal(ResultStatus.NotFound, result.Status);
        }
    }
}
