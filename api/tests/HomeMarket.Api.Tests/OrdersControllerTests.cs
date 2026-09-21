using AutoFixture;
using HomeMarket.Api.Controllers;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HomeMarket.Api.Tests
{
    public class OrdersControllerTests
    {
        private const int Eve = 21;

        [Fact]
        public async Task Mine_ReturnsTheOrdersOfTheCaller()
        {
            // Given
            var fixture = new Fixture();
            var mine = fixture.CreateMany<OrderDto>(2).ToList();
            var orders = new Mock<IOrderService>();
            orders.Setup(o => o.MineAsync(Eve)).ReturnsAsync(mine);
            var controller = new OrdersController(orders.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Mine();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(mine, ok.Value);
        }

        [Fact]
        public async Task Sales_ReturnsTheLinesOthersBoughtFromTheCaller()
        {
            // Given
            var fixture = new Fixture();
            var sales = fixture.CreateMany<SaleDto>(3).ToList();
            var orders = new Mock<IOrderService>();
            orders.Setup(o => o.SalesAsync(Eve)).ReturnsAsync(sales);
            var controller = new OrdersController(orders.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Sales();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(sales, ok.Value);
        }

        [Fact]
        public async Task Get_OrderOfTheCaller_ReturnsIt()
        {
            // Given
            var fixture = new Fixture();
            var order = fixture.Create<OrderDto>();
            var orders = new Mock<IOrderService>();
            orders.Setup(o => o.GetAsync(Eve, order.Id)).ReturnsAsync(order);
            var controller = new OrdersController(orders.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Get(order.Id);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(order, ok.Value);
        }

        [Fact]
        public async Task Get_OrderOfSomebodyElse_Returns404()
        {
            // Given
            var orders = new Mock<IOrderService>();
            orders.Setup(o => o.GetAsync(Eve, 1)).ReturnsAsync((OrderDto?)null);
            var controller = new OrdersController(orders.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Get(1);

            // Then
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Place_CardAccepted_Returns201PointingAtTheOrder()
        {
            // Given
            var fixture = new Fixture();
            var checkout = fixture.Create<CheckoutRequest>();
            var order = fixture.Create<OrderDto>();
            var orders = new Mock<IOrderService>();
            orders.Setup(o => o.PlaceAsync(Eve, checkout)).ReturnsAsync((OrderOutcome.Done, order, string.Empty));
            var controller = new OrdersController(orders.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Place(checkout);

            // Then
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(OrdersController.Get), created.ActionName);
            Assert.Equal(order.Id, created.RouteValues!["id"]);
            Assert.Same(order, created.Value);
        }

        [Fact]
        public async Task Place_CardRefused_Returns402WithTheReason()
        {
            // Given
            var fixture = new Fixture();
            var checkout = fixture.Create<CheckoutRequest>();
            var orders = new Mock<IOrderService>();
            orders.Setup(o => o.PlaceAsync(Eve, checkout)).ReturnsAsync((OrderOutcome.CardRefused, null, "Your card was declined."));
            var controller = new OrdersController(orders.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Place(checkout);

            // Then
            var refused = Refusal.Of(result.Result, StatusCodes.Status402PaymentRequired);
            Assert.Contains("declined", refused.Detail);
        }

        [Fact]
        public async Task Place_EmptyCart_Returns400()
        {
            // Given
            var fixture = new Fixture();
            var checkout = fixture.Create<CheckoutRequest>();
            var orders = new Mock<IOrderService>();
            orders.Setup(o => o.PlaceAsync(Eve, checkout)).ReturnsAsync((OrderOutcome.EmptyCart, null, "Your cart is empty."));
            var controller = new OrdersController(orders.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Place(checkout);

            // Then
            Refusal.Of(result.Result, StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Place_StockGoneMeanwhile_Returns409()
        {
            // Given
            var fixture = new Fixture();
            var checkout = fixture.Create<CheckoutRequest>();
            var orders = new Mock<IOrderService>();
            orders.Setup(o => o.PlaceAsync(Eve, checkout)).ReturnsAsync((OrderOutcome.NotEnoughStock, null, "Only 0 left of Clock."));
            var controller = new OrdersController(orders.Object) { ControllerContext = Caller.Member(Eve) };

            // When
            var result = await controller.Place(checkout);

            // Then
            Refusal.Of(result.Result, StatusCodes.Status409Conflict);
        }
    }
}
