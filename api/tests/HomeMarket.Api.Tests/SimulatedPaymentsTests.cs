using HomeMarket.Api.Interfaces;
using HomeMarket.Api.Services;

namespace HomeMarket.Api.Tests
{
    public class SimulatedPaymentsTests
    {
        private static PaymentRequest Charge(string cardNumber = "4242 4242 4242 4242", string holder = "Eve Martin", string expiry = "12/99", string securityCode = "123")
        {
            return new PaymentRequest(171.31m, "CAD", "home-market order for Eve Martin", cardNumber, holder, expiry, securityCode);
        }

        [Fact]
        public async Task ChargeAsync_GoodVisa_IsAcceptedWithBrandLastFourAndAReference()
        {
            // Given
            var gateway = new SimulatedPayments();

            // When
            var result = await gateway.ChargeAsync(Charge());

            // Then
            Assert.True(result.Accepted);
            Assert.Equal("Visa", result.CardBrand);
            Assert.Equal("4242", result.CardLast4);
            Assert.StartsWith("sim_", result.Reference);
            Assert.Equal(28, result.Reference.Length);
            Assert.Empty(result.Reason);
        }

        [Theory]
        [InlineData("5555-5555-5555-4444", "Mastercard")]
        [InlineData("378282246310005", "American Express")]
        [InlineData("6011111111111117", "Card")]
        public async Task ChargeAsync_OtherNumbers_NameTheBrandByTheFirstDigit(string cardNumber, string brand)
        {
            // Given
            var gateway = new SimulatedPayments();

            // When
            var result = await gateway.ChargeAsync(Charge(cardNumber));

            // Then
            Assert.True(result.Accepted);
            Assert.Equal(brand, result.CardBrand);
        }

        [Theory]
        [InlineData("4242 4242 4242 4241")]
        [InlineData("4242 4242 42")]
        [InlineData("4242.4242.4242.4242")]
        [InlineData("abcd efgh ijkl mnop")]
        public async Task ChargeAsync_NumberThatIsNotACard_IsRefused(string cardNumber)
        {
            // Given
            var gateway = new SimulatedPayments();

            // When
            var result = await gateway.ChargeAsync(Charge(cardNumber));

            // Then
            Assert.False(result.Accepted);
            Assert.Equal("That card number is not valid.", result.Reason);
            Assert.Empty(result.Reference);
        }

        [Fact]
        public async Task ChargeAsync_NoNameOnTheCard_IsRefused()
        {
            // Given
            var gateway = new SimulatedPayments();

            // When
            var result = await gateway.ChargeAsync(Charge(holder: " E "));

            // Then
            Assert.False(result.Accepted);
            Assert.Equal("The name on the card is missing.", result.Reason);
        }

        [Theory]
        [InlineData("01/20")]
        [InlineData("13/99")]
        [InlineData("1299")]
        [InlineData("ab/cd")]
        public async Task ChargeAsync_ExpiredOrUnreadableExpiry_IsRefused(string expiry)
        {
            // Given
            var gateway = new SimulatedPayments();

            // When
            var result = await gateway.ChargeAsync(Charge(expiry: expiry));

            // Then
            Assert.False(result.Accepted);
            Assert.Equal("That card has expired.", result.Reason);
        }

        [Fact]
        public async Task ChargeAsync_CurrentMonth_IsStillValid()
        {
            // Given
            var gateway = new SimulatedPayments();
            var now = DateTime.UtcNow;

            // When
            var result = await gateway.ChargeAsync(Charge(expiry: $"{now.Month:00}/{now.Year % 100:00}"));

            // Then
            Assert.True(result.Accepted);
        }

        [Theory]
        [InlineData("12")]
        [InlineData("12345")]
        [InlineData("12a")]
        public async Task ChargeAsync_SecurityCodeNotThreeOrFourDigits_IsRefused(string securityCode)
        {
            // Given
            var gateway = new SimulatedPayments();

            // When
            var result = await gateway.ChargeAsync(Charge(securityCode: securityCode));

            // Then
            Assert.False(result.Accepted);
            Assert.Equal("The security code is three or four digits.", result.Reason);
        }

        [Fact]
        public async Task ChargeAsync_TheDeclineTestNumber_IsDeclined()
        {
            // Given
            var gateway = new SimulatedPayments();

            // When
            var result = await gateway.ChargeAsync(Charge(SimulatedPayments.DeclinedNumber));

            // Then
            Assert.False(result.Accepted);
            Assert.Equal("Your card was declined.", result.Reason);
        }
    }
}
