using HomeMarket.Api.Services;

namespace HomeMarket.Api.Tests
{
    public class TaxesTests
    {
        [Fact]
        public void Gst_OnASubtotal_IsFivePercentToTheCent()
        {
            // Given
            var subtotal = 149m;

            // When
            var gst = Taxes.Gst(subtotal);

            // Then
            Assert.Equal(7.45m, gst);
        }

        [Fact]
        public void Qst_OnASubtotal_IsNinePointNineSevenFivePercentRoundedAwayFromZero()
        {
            // Given
            var subtotal = 149m;

            // When
            var qst = Taxes.Qst(subtotal);

            // Then
            Assert.Equal(14.86m, qst);
        }

        [Fact]
        public void Qst_HalfACent_RoundsAwayFromZero()
        {
            // Given
            var subtotal = 50m;

            // When
            var qst = Taxes.Qst(subtotal);

            // Then
            Assert.Equal(4.99m, qst);
        }

        [Fact]
        public void Total_IsTheSubtotalPlusBothTaxes()
        {
            // Given
            var subtotal = 149m;

            // When
            var total = Taxes.Total(subtotal);

            // Then
            Assert.Equal(171.31m, total);
        }
    }
}
