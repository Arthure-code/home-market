namespace HomeMarket.Api.Interfaces
{
    // What a charge is made of: the amount in its currency, what it is for,
    // and the card as the buyer typed it. It goes to the gateway and
    // nowhere else.
    public record PaymentRequest(
        decimal Amount,
        string Currency,
        string Description,
        string CardNumber,
        string CardHolderName,
        string Expiry,
        string SecurityCode);

    // What a charge comes back with: accepted or not, why not, the little
    // of the card an order may keep, and the reference the provider gave
    // the transaction.
    public record PaymentResult(bool Accepted, string Reason, string CardBrand, string CardLast4, string Reference)
    {
        public static PaymentResult Refused(string reason)
        {
            return new PaymentResult(false, reason, string.Empty, string.Empty, string.Empty);
        }
    }

    // The payment provider behind the checkout. One implementation per
    // provider, chosen in Program; the order service knows only this.
    public interface IPaymentGateway
    {
        Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    }
}
