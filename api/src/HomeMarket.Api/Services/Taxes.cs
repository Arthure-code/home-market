namespace HomeMarket.Api.Services
{
    // The two sales taxes charged in Quebec, each on the subtotal.
    public static class Taxes
    {
        public const decimal GstRate = 0.05m;
        public const decimal QstRate = 0.09975m;

        public static decimal Gst(decimal subtotal)
        {
            return Round(subtotal * GstRate);
        }

        public static decimal Qst(decimal subtotal)
        {
            return Round(subtotal * QstRate);
        }

        public static decimal Total(decimal subtotal)
        {
            return subtotal + Gst(subtotal) + Qst(subtotal);
        }

        private static decimal Round(decimal amount)
        {
            return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        }
    }
}
