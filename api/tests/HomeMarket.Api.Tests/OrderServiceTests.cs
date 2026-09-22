using Ardalis.Result;
using AutoFixture;
using HomeMarket.Api.Dtos;
using HomeMarket.Api.Interfaces;
using HomeMarket.Api.Services;
using Moq;

namespace HomeMarket.Api.Tests
{
    public class OrderServiceTests
    {
        private static CheckoutRequest Checkout(Fixture fixture)
        {
            return fixture.Build<CheckoutRequest>()
                .With(r => r.FullName, " Eve Martin ")
                .With(r => r.PostalCode, "h2x 1y4")
                .With(r => r.CardNumber, "4242 4242 4242 4242")
                .Create();
        }

        private static Mock<IPaymentGateway> GatewayAnswering(PaymentResult answer)
        {
            var gateway = new Mock<IPaymentGateway>();
            gateway.Setup(g => g.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(answer);
            return gateway;
        }

        [Fact]
        public async Task PlaceAsync_EmptyCart_IsInvalidAndChargesNothing()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var gateway = GatewayAnswering(new PaymentResult(true, string.Empty, "Visa", "4242", "sim_1"));
            var service = new OrderService(market.Context, gateway.Object);

            // When
            var result = await service.PlaceAsync(eve.Id, Checkout(new Fixture()));

            // Then
            Assert.Equal(ResultStatus.Invalid, result.Status);
            gateway.Verify(g => g.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PlaceAsync_LineBeyondTheStock_IsAConflictNamingTheProduct()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            market.InCart(eve, market.Listing(omar, "Clock", 19, 1), 2);
            var gateway = GatewayAnswering(new PaymentResult(true, string.Empty, "Visa", "4242", "sim_1"));
            var service = new OrderService(market.Context, gateway.Object);

            // When
            var result = await service.PlaceAsync(eve.Id, Checkout(new Fixture()));

            // Then
            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Only 1 left of Clock.", Assert.Single(result.Errors));
            gateway.Verify(g => g.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PlaceAsync_CardRefused_IsAnErrorWithTheReasonAndWritesNothing()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            var clock = market.Listing(omar, "Clock", 19, 3);
            market.InCart(eve, clock, 2);
            var gateway = GatewayAnswering(PaymentResult.Refused("Your card was declined."));
            var service = new OrderService(market.Context, gateway.Object);

            // When
            var result = await service.PlaceAsync(eve.Id, Checkout(new Fixture()));

            // Then
            Assert.Equal(ResultStatus.Error, result.Status);
            Assert.Equal("Your card was declined.", Assert.Single(result.Errors));
            Assert.Empty(market.Context.Orders);
            Assert.Single(market.Context.CartLines);
            Assert.Equal(3, market.Context.Products.Single().Stock);
        }

        [Fact]
        public async Task PlaceAsync_CardAccepted_ChargesTheTotalWithTaxesAndTurnsTheCartIntoAnOrder()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            var lamp = market.Listing(omar, "Lamp", 65, 6);
            var clock = market.Listing(omar, "Clock", 19, 3);
            market.InCart(eve, lamp, 2);
            market.InCart(eve, clock, 1);
            var gateway = GatewayAnswering(new PaymentResult(true, string.Empty, "Visa", "4242", "sim_abc"));
            var request = Checkout(new Fixture());
            var service = new OrderService(market.Context, gateway.Object);

            // When
            var result = await service.PlaceAsync(eve.Id, request);

            // Then
            Assert.True(result.IsSuccess);
            var order = result.Value;
            Assert.Equal("eve", order.Buyer);
            Assert.Equal("Eve Martin", order.FullName);
            Assert.Equal("H2X 1Y4", order.PostalCode);
            Assert.Equal("Visa", order.CardBrand);
            Assert.Equal("4242", order.CardLast4);
            Assert.Equal(149m, order.Subtotal);
            Assert.Equal(7.45m, order.Gst);
            Assert.Equal(14.86m, order.Qst);
            Assert.Equal(171.31m, order.Total);
            Assert.Equal(3, order.ItemCount);
            Assert.Equal(new[] { "Lamp", "Clock" }, order.Lines.Select(l => l.Title));
            Assert.Equal(130m, order.Lines[0].LineTotal);
            Assert.Equal("omar", order.Lines[0].Seller);
            gateway.Verify(g => g.ChargeAsync(
                It.Is<PaymentRequest>(p => p.Amount == 171.31m && p.Currency == "CAD" && p.CardNumber == request.CardNumber && p.Description.Contains("Eve Martin")),
                It.IsAny<CancellationToken>()));
            market.Reload();
            Assert.Empty(market.Context.CartLines);
            Assert.Equal(4, market.Context.Products.Single(p => p.Id == lamp.Id).Stock);
            Assert.Equal(2, market.Context.Products.Single(p => p.Id == clock.Id).Stock);
            Assert.Equal("sim_abc", market.Context.Orders.Single().PaymentReference);
        }

        [Fact]
        public async Task MineAsync_ReturnsTheOrdersOfTheBuyerNewestFirst()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            var clock = market.Listing(omar, "Clock", 19, 9);
            var gateway = GatewayAnswering(new PaymentResult(true, string.Empty, "Visa", "4242", "sim_1"));
            var service = new OrderService(market.Context, gateway.Object);
            market.InCart(eve, clock, 1);
            var first = await service.PlaceAsync(eve.Id, Checkout(new Fixture()));
            market.InCart(eve, clock, 2);
            var second = await service.PlaceAsync(eve.Id, Checkout(new Fixture()));
            market.InCart(omar, market.Listing(eve, "Lamp", 65, 6), 1);
            await service.PlaceAsync(omar.Id, Checkout(new Fixture()));

            // When
            var mine = await service.MineAsync(eve.Id);

            // Then
            Assert.Equal(new[] { second.Value.Id, first.Value.Id }, mine.Select(o => o.Id));
            Assert.Equal(2, mine[0].ItemCount);
        }

        [Fact]
        public async Task GetAsync_SomeoneElsesOrder_ReturnsNull()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            market.InCart(eve, market.Listing(omar, "Clock", 19, 9), 1);
            var service = new OrderService(market.Context, GatewayAnswering(new PaymentResult(true, string.Empty, "Visa", "4242", "sim_1")).Object);
            var placed = await service.PlaceAsync(eve.Id, Checkout(new Fixture()));

            // When
            var asBuyer = await service.GetAsync(eve.Id, placed.Value.Id);
            var asSeller = await service.GetAsync(omar.Id, placed.Value.Id);

            // Then
            Assert.NotNull(asBuyer);
            Assert.Null(asSeller);
        }

        [Fact]
        public async Task SalesAsync_ReturnsTheLinesSoldBySellerWithWhereToShip()
        {
            // Given
            using var market = new Market();
            var eve = market.Member("eve");
            var omar = market.Member("omar");
            var nadia = market.Member("nadia");
            market.InCart(eve, market.Listing(omar, "Clock", 19, 9), 2);
            market.InCart(eve, market.Listing(nadia, "Lamp", 65, 6), 1);
            var service = new OrderService(market.Context, GatewayAnswering(new PaymentResult(true, string.Empty, "Visa", "4242", "sim_1")).Object);
            var request = Checkout(new Fixture());
            request.Street = "12 Rue Nord";
            request.City = "Montreal";
            request.Province = "QC";
            request.Country = "Canada";
            var placed = await service.PlaceAsync(eve.Id, request);

            // When
            var sales = await service.SalesAsync(omar.Id);

            // Then
            var sale = Assert.Single(sales);
            Assert.Equal(placed.Value.Id, sale.OrderId);
            Assert.Equal("eve", sale.Buyer);
            Assert.Equal("Clock", sale.Title);
            Assert.Equal(2, sale.Quantity);
            Assert.Equal(38m, sale.LineTotal);
            Assert.Equal("Eve Martin, 12 Rue Nord, Montreal, QC H2X 1Y4, Canada", sale.ShipTo);
        }
    }
}
