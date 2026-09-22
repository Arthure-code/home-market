using Ardalis.Result;
using HomeMarket.Api.Interfaces;
using HomeMarket.Api.Services;
using Moq;

namespace HomeMarket.Api.Tests
{
    public class CartServiceTests
    {
        private static Mock<IPhotoStore> PhotosServing()
        {
            var photos = new Mock<IPhotoStore>();
            photos.Setup(p => p.UrlFor(It.IsAny<string>())).Returns<string>(name => name.Length == 0 ? string.Empty : "http://api/images/" + name);
            return photos;
        }

        [Fact]
        public async Task GetAsync_EmptyCart_HasNoLinesAndZeroTotals()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var cart = await service.GetAsync(eve.Id);

            // Then
            Assert.Empty(cart.Lines);
            Assert.Equal(0, cart.ItemCount);
            Assert.Equal(0m, cart.Total);
        }

        [Fact]
        public async Task GetAsync_TwoLines_SumsThemAndAddsBothTaxes()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            market.InCart(eve, market.Listing(omar, "Lamp", 65, 6, photo: "lamp.jpg"), 2);
            market.InCart(eve, market.Listing(omar, "Clock", 19, 3), 1);
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var cart = await service.GetAsync(eve.Id);

            // Then
            Assert.Equal(2, cart.Lines.Count);
            Assert.Equal(3, cart.ItemCount);
            Assert.Equal(149m, cart.Subtotal);
            Assert.Equal(7.45m, cart.Gst);
            Assert.Equal(14.86m, cart.Qst);
            Assert.Equal(171.31m, cart.Total);
            Assert.Equal("http://api/images/lamp.jpg", cart.Lines[0].PhotoUrl);
            Assert.Equal("omar", cart.Lines[0].Seller);
            Assert.Equal(130m, cart.Lines[0].LineTotal);
        }

        [Fact]
        public async Task AddAsync_ProductNotInTheCart_AddsALine()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            var lamp = market.Listing(omar, "Lamp", 65, 6);
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var result = await service.AddAsync(eve.Id, lamp.Id, 2);

            // Then
            Assert.True(result.IsSuccess);
            var line = Assert.Single(result.Value.Lines);
            Assert.Equal(2, line.Quantity);
            Assert.Equal(6, line.Stock);
        }

        [Fact]
        public async Task AddAsync_ProductAlreadyThere_AddsToItsQuantityUpToTen()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            var pens = market.Listing(omar, "Pens", 4, 50);
            market.InCart(eve, pens, 8);
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var result = await service.AddAsync(eve.Id, pens.Id, 5);

            // Then
            Assert.True(result.IsSuccess);
            Assert.Equal(10, Assert.Single(result.Value.Lines).Quantity);
        }

        [Fact]
        public async Task AddAsync_OwnListing_IsInvalid()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var mine = market.Listing(eve, "Lamp", 65, 6);
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var result = await service.AddAsync(eve.Id, mine.Id, 1);

            // Then
            Assert.Equal(ResultStatus.Invalid, result.Status);
            Assert.Empty(market.Context.CartLines);
        }

        [Fact]
        public async Task AddAsync_MissingProduct_IsNotFound()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var result = await service.AddAsync(eve.Id, 999, 1);

            // Then
            Assert.Equal(ResultStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task AddAsync_MoreThanTheStock_IsAConflict()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            var lamp = market.Listing(omar, "Lamp", 65, 2);
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var result = await service.AddAsync(eve.Id, lamp.Id, 3);

            // Then
            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Empty(market.Context.CartLines);
        }

        [Fact]
        public async Task SetQuantityAsync_LineInTheCart_ChangesIt()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            var lamp = market.Listing(omar, "Lamp", 65, 6);
            market.InCart(eve, lamp, 1);
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var result = await service.SetQuantityAsync(eve.Id, lamp.Id, 4);

            // Then
            Assert.True(result.IsSuccess);
            Assert.Equal(4, Assert.Single(result.Value.Lines).Quantity);
        }

        [Fact]
        public async Task SetQuantityAsync_MoreThanTheStock_IsAConflict()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            var lamp = market.Listing(omar, "Lamp", 65, 2);
            market.InCart(eve, lamp, 1);
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var result = await service.SetQuantityAsync(eve.Id, lamp.Id, 3);

            // Then
            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal(1, market.Context.CartLines.Single().Quantity);
        }

        [Fact]
        public async Task SetQuantityAsync_LineNotInTheCart_IsNotFound()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var result = await service.SetQuantityAsync(eve.Id, 999, 1);

            // Then
            Assert.Equal(ResultStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task RemoveAsync_LineInTheCart_RemovesItAndReturnsTheRest()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            var lamp = market.Listing(omar, "Lamp", 65, 6);
            var clock = market.Listing(omar, "Clock", 19, 3);
            market.InCart(eve, lamp, 1);
            market.InCart(eve, clock, 1);
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var result = await service.RemoveAsync(eve.Id, lamp.Id);

            // Then
            Assert.True(result.IsSuccess);
            Assert.Equal("Clock", Assert.Single(result.Value.Lines).Title);
        }

        [Fact]
        public async Task RemoveAsync_LineNotInTheCart_IsNotFound()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var service = new CartService(market.Context, PhotosServing().Object);

            // When
            var result = await service.RemoveAsync(eve.Id, 999);

            // Then
            Assert.Equal(ResultStatus.NotFound, result.Status);
        }
    }
}
