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
    public class CartControllerTests
    {
        private const int Eve = 21;

        [Fact]
        public async Task Get_ReturnsTheCartOfTheCallerWithItsTotals()
        {
            // Given
            var fixture = new Fixture();
            var cart = fixture.Create<CartDto>();
            var carts = new Mock<ICartService>();
            carts.Setup(c => c.GetAsync(Eve)).ReturnsAsync(cart);
            var controller = new CartController(carts.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Get();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(cart, ok.Value);
        }

        [Fact]
        public async Task Add_ProductInStock_ReturnsTheWholeCart()
        {
            // Given
            var fixture = new Fixture();
            var cart = fixture.Create<CartDto>();
            var carts = new Mock<ICartService>();
            carts.Setup(c => c.AddAsync(Eve, 23, 2)).ReturnsAsync(Result<CartDto>.Success(cart));
            var controller = new CartController(carts.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Add(new CartAddRequest { ProductId = 23, Quantity = 2 });

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(cart, ok.Value);
        }

        [Fact]
        public async Task Add_MissingProduct_Returns404()
        {
            // Given
            var carts = new Mock<ICartService>();
            carts.Setup(c => c.AddAsync(Eve, 999, 1)).ReturnsAsync(Result<CartDto>.NotFound());
            var controller = new CartController(carts.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Add(new CartAddRequest { ProductId = 999, Quantity = 1 });

            // Then
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Add_OwnListing_Returns400()
        {
            // Given
            var carts = new Mock<ICartService>();
            carts.Setup(c => c.AddAsync(Eve, 30, 1)).ReturnsAsync(Result<CartDto>.Invalid(new ValidationError("productId", "That is your own listing.")));
            var controller = new CartController(carts.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Add(new CartAddRequest { ProductId = 30, Quantity = 1 });

            // Then
            Refusal.Of(result.Result, StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Add_MoreThanTheStock_Returns409()
        {
            // Given
            var carts = new Mock<ICartService>();
            carts.Setup(c => c.AddAsync(Eve, 29, 5)).ReturnsAsync(Result<CartDto>.Conflict("There are not that many left in stock."));
            var controller = new CartController(carts.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Add(new CartAddRequest { ProductId = 29, Quantity = 5 });

            // Then
            Refusal.Of(result.Result, StatusCodes.Status409Conflict);
        }

        [Fact]
        public async Task SetQuantity_LineOfTheCart_ReturnsTheWholeCart()
        {
            // Given
            var fixture = new Fixture();
            var cart = fixture.Create<CartDto>();
            var carts = new Mock<ICartService>();
            carts.Setup(c => c.SetQuantityAsync(Eve, 23, 4)).ReturnsAsync(Result<CartDto>.Success(cart));
            var controller = new CartController(carts.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.SetQuantity(23, new CartQuantityRequest { Quantity = 4 });

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(cart, ok.Value);
        }

        [Fact]
        public async Task SetQuantity_MoreThanTheStock_Returns409()
        {
            // Given
            var carts = new Mock<ICartService>();
            carts.Setup(c => c.SetQuantityAsync(Eve, 23, 9)).ReturnsAsync(Result<CartDto>.Conflict("There are not that many left in stock."));
            var controller = new CartController(carts.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.SetQuantity(23, new CartQuantityRequest { Quantity = 9 });

            // Then
            Refusal.Of(result.Result, StatusCodes.Status409Conflict);
        }

        [Fact]
        public async Task Remove_LineOfTheCart_ReturnsTheWholeCart()
        {
            // Given
            var fixture = new Fixture();
            var cart = fixture.Create<CartDto>();
            var carts = new Mock<ICartService>();
            carts.Setup(c => c.RemoveAsync(Eve, 23)).ReturnsAsync(Result<CartDto>.Success(cart));
            var controller = new CartController(carts.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Remove(23);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(cart, ok.Value);
            carts.Verify(c => c.RemoveAsync(Eve, 23), Times.Once);
        }

        [Fact]
        public async Task Remove_LineNotInTheCart_Returns404()
        {
            // Given
            var carts = new Mock<ICartService>();
            carts.Setup(c => c.RemoveAsync(Eve, 23)).ReturnsAsync(Result<CartDto>.NotFound());
            var controller = new CartController(carts.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Remove(23);

            // Then
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
